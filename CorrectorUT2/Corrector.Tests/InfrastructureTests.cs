using System.Diagnostics;
using System.Text;
using Corrector.Core;
using Xunit;

namespace Corrector.Tests;

/// <summary>
/// Comprueba el propio corrector: compilación, entrada y salida, aislamiento
/// de ejecuciones y límites para entregas que no terminan.
/// </summary>
public sealed class InfrastructureTests
{
    [Theory]
    [InlineData("Console.WriteLine(\"Hola, clase\");")]
    [InlineData("class Program { static void Main() { Console.WriteLine(\"Hola, clase\"); } }")]
    public async Task AceptaProgramasConMainOConInstruccionesDirectas(string source)
    {
        using var program = new StudentProgram(source);

        ExecutionResult result = await program.RunAsync();

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("Hola, clase", result.Output.Trim());
        Assert.Empty(result.Error);
        Assert.False(result.TimedOut, "Un programa que termina debe ejecutarse sin agotar el tiempo.");
        Assert.False(result.OutputLimitExceeded);
    }

    [Fact]
    public async Task ConservaCaracteresUnicodeEnLaEntradaYLaSalida()
    {
        using var program = new StudentProgram("Console.WriteLine(\"¡Hola, \" + Console.ReadLine() + \"!\");");

        ExecutionResult result = await program.RunAsync("María Muñoz 👩‍💻\n");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("¡Hola, María Muñoz 👩‍💻!", result.Output.Trim());
    }

    [Fact]
    public async Task CierraLaEntradaAlTerminarLosDatos()
    {
        using var program = new StudentProgram("""
            int lineas = 0;
            while (Console.ReadLine() != null) lineas++;
            Console.WriteLine(lineas);
            """);

        ExecutionResult result = await program.RunAsync("primera\nsegunda\n");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("2", result.Output.Trim());
        Assert.False(result.TimedOut, "El fin de los datos debe llegar como fin de entrada, sin bloquear Console.ReadLine().");
    }

    [Fact]
    public async Task SeparaLaSalidaNormalDeLosErroresYConservaElCodigoDeSalida()
    {
        using var program = new StudentProgram("""
            Console.WriteLine("resultado");
            Console.Error.WriteLine("diagnóstico");
            Environment.Exit(7);
            """);

        ExecutionResult result = await program.RunAsync();

        Assert.Equal(7, result.ExitCode);
        Assert.Equal("resultado", result.Output.Trim());
        Assert.Equal("diagnóstico", result.Error.Trim());
        Assert.False(result.TimedOut);
    }

    [Fact]
    public async Task InformaDelArchivoYLaLineaCuandoLaEntregaNoCompila()
    {
        using var program = new StudentProgram("Console.WriteLine(\"primera línea\");\nConsole.WriteLine(variableInexistente);");

        CompilationException error = await Assert.ThrowsAsync<CompilationException>(() => program.RunAsync());

        Assert.Contains("Program.cs", error.Message);
        Assert.Contains("línea 2", error.Message);
        Assert.Contains("CS0103", error.Message);
        Assert.Contains("variableInexistente", error.Message);
    }

    [Fact]
    public async Task WithSourceCreaOtraEntregaSinCambiarLaOriginal()
    {
        const string originalSource = "Console.WriteLine(\"original\");";
        using var original = new StudentProgram(originalSource);
        using var replacement = original.WithSource("Console.WriteLine(\"alternativa\");");

        ExecutionResult replacedResult = await replacement.RunAsync();
        ExecutionResult originalResult = await original.RunAsync();

        Assert.Equal(originalSource, original.Source);
        Assert.Equal("original", originalResult.Output.Trim());
        Assert.Equal("alternativa", replacedResult.Output.Trim());
    }

    [Fact]
    public async Task CadaEjecucionRecibeUnDirectorioDeTrabajoLimpio()
    {
        using var program = new StudentProgram("""
            Console.WriteLine(File.Exists("resultado.txt") ? "existía" : "nuevo");
            File.WriteAllText("resultado.txt", "contenido de esta ejecución");
            """);

        ExecutionResult first = await program.RunAsync();
        ExecutionResult second = await program.RunAsync();

        Assert.Equal("nuevo", first.Output.Trim());
        Assert.Equal("nuevo", second.Output.Trim());
    }

    [Fact]
    public async Task DetieneUnBucleInfinitoYPermiteUnaEjecucionPosterior()
    {
        using var program = new StudentProgram("""
            if (Console.ReadLine() == "bucle") while (true) { }
            Console.WriteLine("terminado");
            """);
        await program.RunAsync("normal\n"); // Compila antes de medir el tiempo de ejecución.
        var elapsed = Stopwatch.StartNew();

        ExecutionResult looping = await program.RunAsync("bucle\n", TimeSpan.FromMilliseconds(350));
        ExecutionResult following = await program.RunAsync("normal\n");

        Assert.True(looping.TimedOut, "Un bucle infinito debe marcarse como tiempo agotado.");
        Assert.False(looping.OutputLimitExceeded);
        Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(10), "El corrector debe recuperar el control después de detener un bucle.");
        Assert.Equal(0, following.ExitCode);
        Assert.Equal("terminado", following.Output.Trim());
    }

    [Theory]
    [InlineData("Console.Write(texto);")]
    [InlineData("Console.Error.Write(texto);")]
    public async Task LimitaLaSalidaContinuaSinAcumularTextoIndefinidamente(string writeStatement)
    {
        using var program = new StudentProgram("string texto = new string('á', 4096) + \"😀\"; while (true) { " + writeStatement + " }");

        ExecutionResult result = await program.RunAsync(timeout: TimeSpan.FromSeconds(10));

        Assert.True(result.OutputLimitExceeded, "La salida continua debe detener el programa al llegar al límite de salida.");
        Assert.False(result.TimedOut, "Un programa que imprime sin parar debe detenerse por el límite de salida antes del límite de tiempo.");
        string captured = writeStatement.Contains("Error", StringComparison.Ordinal) ? result.Error : result.Output;
        Assert.NotEmpty(captured);
        Assert.True(Encoding.UTF8.GetByteCount(captured) <= 256 * 1024,
            "La salida capturada no debe superar 256 KiB aunque contenga caracteres Unicode.");
        Assert.DoesNotContain("\uFFFD", captured);
        Assert.False(char.IsHighSurrogate(captured[^1]), "El límite no debe partir un carácter Unicode representado por dos caracteres de C#.");
    }

    [Fact]
    public async Task CancelaUnaEjecucionActivaSinInvalidarLaEntrega()
    {
        using var program = new StudentProgram("""
            if (Console.ReadLine() == "bucle") while (true) { }
            Console.WriteLine("listo");
            """);
        await program.RunAsync("normal\n");
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(350));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            program.RunAsync("bucle\n", TimeSpan.FromSeconds(10), cancellation.Token));
        ExecutionResult following = await program.RunAsync("normal\n");

        Assert.Equal(0, following.ExitCode);
        Assert.Equal("listo", following.Output.Trim());
    }

    [Fact]
    public async Task RespetaUnaCancelacionAnteriorALaEjecucion()
    {
        using var program = new StudentProgram("Console.WriteLine(\"no debería ejecutarse\");");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            program.RunAsync(cancellationToken: cancellation.Token));
    }

    [Fact]
    public async Task LiberaLosArchivosTemporalesAlFinalizar()
    {
        string workspace = Path.Combine(Path.GetTempPath(), "corrector-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        try
        {
            using (var program = new StudentProgram("Console.WriteLine(42);", workspace))
            {
                ExecutionResult result = await program.RunAsync();
                Assert.Equal(0, result.ExitCode);
                Assert.NotEmpty(Directory.EnumerateDirectories(workspace));
            }

            Assert.Empty(Directory.EnumerateFileSystemEntries(workspace));
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    [Fact]
    public async Task RechazaUnTiempoDeEsperaNoPositivo()
    {
        using var program = new StudentProgram("Console.WriteLine(42);");

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => program.RunAsync(timeout: TimeSpan.Zero));
    }
}
