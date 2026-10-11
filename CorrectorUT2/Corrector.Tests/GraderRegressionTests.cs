using Corrector.Core;
using Xunit;

namespace Corrector.Tests;

public sealed class GraderRegressionTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(21)]
    [InlineData(28)]
    [InlineData(31)]
    [InlineData(35)]
    [InlineData(43)]
    [InlineData(50)]
    public async Task ImprimirUnSaludoNoResuelveUnEjercicio(int exercise)
    {
        CorrectionReport report = await Check(exercise, "Console.WriteLine(\"Hola Mundo\");");
        Assert.False(report.Passed, $"El ejercicio {exercise} ha aceptado un programa sin su lógica.");
        Assert.NotEmpty(report.Checks);
    }

    [Fact]
    public async Task AceptaUnaFichaConNombresYTextosDistintos()
    {
        CorrectionReport report = await Check(1, """
            string persona = "Luz";
            int anios = 20;
            float medida = 1.63f;
            char letra = 'L';
            bool licencia = true;
            Console.WriteLine($"{persona} / {anios} / {medida} / {letra} / {licencia}");
            """);
        Assert.True(report.Passed, Messages(report));
    }

    [Fact]
    public async Task AceptaCalculosConOtrosNombresYUnaSalidaDistinta()
    {
        CorrectionReport report = await Check(11, """
            int primero = 17;
            int segundo = 5;
            int a = primero + segundo;
            int b = primero - segundo;
            int c = primero * segundo;
            int d = primero / segundo;
            int e = primero % segundo;
            Console.WriteLine($"{a} | {b} | {c} | {d} | {e}");
            """);
        Assert.True(report.Passed, Messages(report));
    }

    [Fact]
    public async Task RechazaResultadosDeCalculadoraEscritosAmano()
    {
        CorrectionReport report = await Check(11, """
            int numero1 = 17;
            int numero2 = 5;
            int suma = 22;
            int resta = 12;
            int multiplicacion = 85;
            int division = 3;
            int resto = 2;
            Console.WriteLine($"{numero1} {numero2} {suma} {resta} {multiplicacion} {division} {resto}");
            """);
        Assert.False(report.Passed, "Cambiar los datos iniciales debe detectar resultados escritos a mano.");
    }

    [Fact]
    public async Task DetectaUnaTarifaQueDaPrioridadAlDescuentoIncorrecto()
    {
        CorrectionReport report = await Check(28, """
            int edad = 20;
            bool estudiante = true;
            bool familiaNumerosa = true;
            double precio;
            if (edad < 6) precio = 0;
            else if (edad <= 17) precio = 8;
            else if (estudiante) precio = 10;
            else if (familiaNumerosa) precio = 9;
            else if (edad >= 65) precio = 6;
            else precio = 15;
            Console.WriteLine($"Precio: {precio}");
            """);
        Assert.False(report.Passed, "Cuando se cumplen varias tarifas debe elegirse la más barata.");
    }

    [Fact]
    public async Task UnErrorDeCompilacionSeInformaComoFalloDeLaEntrega()
    {
        CorrectionReport report = await Check(43, "Console.WriteLine(variableQueNoExiste);");
        Assert.False(report.Passed);
        Assert.Contains("CS0103", Messages(report));
    }

    private static string Messages(CorrectionReport report) =>
        string.Join("\n", report.Checks.Select(check => check.Name + ": " + check.Message));

    private static async Task<CorrectionReport> Check(int exercise, string source)
    {
        string path = Path.Combine(Path.GetTempPath(), "regresion-ut2-" + Guid.NewGuid().ToString("N") + ".cs");
        try
        {
            await File.WriteAllTextAsync(path, source);
            return await CorrectionEngine.EvaluateAsync(new SubmissionConfig { Exercise = exercise, SourcePath = path });
        }
        finally { File.Delete(path); }
    }
}
