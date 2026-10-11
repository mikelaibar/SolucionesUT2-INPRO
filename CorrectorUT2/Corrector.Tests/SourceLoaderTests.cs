using System.Text;
using Corrector.Cli;
using Xunit;

namespace Corrector.Tests;

/// <summary>Valida la lectura de entregas sin hacer peticiones de red.</summary>
public sealed class SourceLoaderTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "corrector-source-tests-" + Guid.NewGuid().ToString("N"));

    public SourceLoaderTests() => Directory.CreateDirectory(_directory);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConservaElTextoUnicodeYLosBytesOriginalesConOSinBom(bool bom)
    {
        const string source = "// María Muñoz, π y 🦊\r\nConsole.WriteLine(\"¡Hola, clase!\");\r\n";
        string path = Path.Combine(_directory, "Program.cs");
        await File.WriteAllTextAsync(path, source, new UTF8Encoding(bom));
        byte[] originalBytes = await File.ReadAllBytesAsync(path);

        string loaded = await SourceLoader.FromFileAsync(path);

        Assert.Equal(source, loaded);
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(path));
        Assert.Equal(bom, originalBytes.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()));
    }

    [Fact]
    public async Task AceptaLaExtensionCsEnMayusculas()
    {
        string path = await WriteAsync("Console.WriteLine(42);", "Programa.CS");

        Assert.Equal("Console.WriteLine(42);", await SourceLoader.FromFileAsync(path));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    public async Task RechazaUnaEntregaVacia(string source)
    {
        string path = await WriteAsync(source);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => SourceLoader.FromFileAsync(path));

        Assert.Contains("vacío", error.Message);
    }

    [Theory]
    [InlineData("<!DOCTYPE html><html><body>Una página web</body></html>")]
    [InlineData(" \n <HTML><body>Una página web</body></HTML>")]
    public async Task RechazaUnaPaginaHtmlGuardadaComoProgramCs(string source)
    {
        string path = await WriteAsync(source);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => SourceLoader.FromFileAsync(path));

        Assert.Contains("no es un archivo de C#", error.Message);
    }

    [Fact]
    public async Task RechazaContenidoBinarioConCaracterNulo()
    {
        string path = await WriteAsync("Console.WriteLine(42);\0");

        await Assert.ThrowsAsync<InvalidOperationException>(() => SourceLoader.FromFileAsync(path));
    }

    [Theory]
    [InlineData("Program.txt")]
    [InlineData("Program.cs.html")]
    [InlineData("Program")]
    public async Task RechazaUnaExtensionQueNoSeaCs(string name)
    {
        string path = await WriteAsync("Console.WriteLine(42);", name);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => SourceLoader.FromFileAsync(path));

        Assert.Contains("extensión .cs", error.Message);
    }

    [Fact]
    public async Task RechazaUnArchivoInexistente()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => SourceLoader.FromFileAsync(Path.Combine(_directory, "inexistente.cs")));
    }

    [Fact]
    public async Task AceptaExactamenteUnMegabyteYRechazaUnByteMas()
    {
        string path = await WriteAsync("//" + new string('a', SourceLoader.MaximumBytes - 2));
        byte[] originalBytes = await File.ReadAllBytesAsync(path);

        string source = await SourceLoader.FromFileAsync(path);

        Assert.Equal(SourceLoader.MaximumBytes, Encoding.UTF8.GetByteCount(source));
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(path));
        await File.AppendAllTextAsync(path, "a", new UTF8Encoding(false));
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => SourceLoader.FromFileAsync(path));
        Assert.Contains("1 MB", error.Message);
    }

    [Fact]
    public async Task CuentaBytesUtf8EnVezDeCaracteresParaElLimite()
    {
        // Cada á ocupa dos bytes aunque sea un solo carácter en el archivo.
        string path = await WriteAsync("//" + new string('á', SourceLoader.MaximumBytes / 2));

        await Assert.ThrowsAsync<InvalidOperationException>(() => SourceLoader.FromFileAsync(path));
    }

    [Fact]
    public async Task RechazaBytesQueNoSonTextoUtf8Valido()
    {
        string path = Path.Combine(_directory, "Program.cs");
        await File.WriteAllBytesAsync(path, [0xC3, 0x28]);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => SourceLoader.FromFileAsync(path));

        Assert.Contains("UTF-8", error.Message);
    }

    [Fact]
    public async Task RespetaLaCancelacionDeLaLecturaLocal()
    {
        string path = await WriteAsync("Console.WriteLine(42);");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SourceLoader.FromFileAsync(path, cancellation.Token));
    }

    [Theory]
    [InlineData("https://github.com/alumno/practicas/blob/main/Program.cs")]
    [InlineData("https://github.com/alumno/practicas/raw/main/Program.cs")]
    [InlineData("https://GITHUB.COM/alumno/practicas/blob/main/Program.cs?raw=true")]
    public void ConvierteElEnlaceDeGithubEnElArchivoRaw(string address)
    {
        Uri result = SourceLoader.NormalizeUrl(address);

        Assert.Equal("https://raw.githubusercontent.com/alumno/practicas/main/Program.cs", result.AbsoluteUri);
    }

    [Fact]
    public void ConservaLasSubcarpetasYLosEspaciosEscapadosDelEnlaceGithub()
    {
        Uri result = SourceLoader.NormalizeUrl("https://github.com/alumno/practicas/blob/main/Mis%20ejercicios/UT2/Program.cs");

        Assert.Equal("https://raw.githubusercontent.com/alumno/practicas/main/Mis%20ejercicios/UT2/Program.cs", result.AbsoluteUri);
    }

    [Theory]
    [InlineData("https://raw.githubusercontent.com/alumno/practicas/main/Program.cs")]
    [InlineData("https://ejemplo.test/Program.cs?descarga=1")]
    public void ConservaElEnlaceHttpsDirecto(string address)
    {
        Assert.Equal(address, SourceLoader.NormalizeUrl(address).AbsoluteUri);
    }

    [Fact]
    public void NoTrataUnDominioParecidoAGithubComoGithub()
    {
        const string address = "https://github.com.ejemplo.test/alumno/practicas/blob/main/Program.cs";

        Assert.Equal(address, SourceLoader.NormalizeUrl(address).AbsoluteUri);
    }

    [Theory]
    [InlineData("http://ejemplo.test/Program.cs")]
    [InlineData("ftp://ejemplo.test/Program.cs")]
    [InlineData("file:///tmp/Program.cs")]
    [InlineData("https://alumno:contraseña@ejemplo.test/Program.cs")]
    [InlineData("https://alumno@github.com/practicas/repositorio/blob/main/Program.cs")]
    [InlineData("Program.cs")]
    [InlineData("//ejemplo.test/Program.cs")]
    [InlineData("")]
    public void RechazaEnlacesSinHttpsAbsolutoOLosQueContienenCredenciales(string address)
    {
        Assert.Throws<InvalidOperationException>(() => SourceLoader.NormalizeUrl(address));
    }

    private async Task<string> WriteAsync(string text, string name = "Program.cs")
    {
        string path = Path.Combine(_directory, name);
        await File.WriteAllTextAsync(path, text, new UTF8Encoding(false));
        return path;
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);
}
