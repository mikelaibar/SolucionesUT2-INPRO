using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Corrector.Core;

/// <summary>
/// Compila una copia de Program.cs y ejecuta cada prueba en otro proceso.
/// La primera compilación se reutiliza en las siguientes pruebas.
/// </summary>
public sealed class StudentProgram : IDisposable
{
    private const int MaximumOutputBytes = 256 * 1024;
    private static readonly UTF8Encoding Utf8 = new(false);
    private static readonly Lazy<MetadataReference[]> References = new(CreateReferences);
    private const string ImplicitUsings = """
        global using System;
        global using System.Collections.Generic;
        global using System.IO;
        global using System.Linq;
        global using System.Net.Http;
        global using System.Threading;
        global using System.Threading.Tasks;
        """;

    private readonly string _workspace;
    private readonly string _temporaryDirectory;
    private readonly object _lifetimeLock = new();
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private readonly Lazy<Task<string>> _compilation;
    private int _activeRuns;
    private bool _disposed;

    public StudentProgram(string source, string? workspace = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        Source = source;
        _workspace = Path.GetFullPath(workspace ?? Path.GetTempPath());
        _temporaryDirectory = Path.Combine(_workspace, "corrector-ut2-" + Guid.NewGuid().ToString("N"));
        _compilation = new Lazy<Task<string>>(
            () => Task.Run(Compile), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string Source { get; }

    public StudentProgram WithSource(string source) => new(source, _workspace);

    public async Task<ExecutionResult> RunAsync(
        string input = "",
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        TimeSpan executionTimeout = timeout ?? TimeSpan.FromSeconds(5);
        if (executionTimeout <= TimeSpan.Zero || executionTimeout.TotalMilliseconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(timeout), "El tiempo máximo debe ser positivo.");
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lifetimeLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _activeRuns++;
        }

        string? runDirectory = null;
        try
        {
            // Una cancelación de una prueba no invalida la compilación compartida.
            string assemblyPath = await _compilation.Value.ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            _lifetimeCancellation.Token.ThrowIfCancellationRequested();
            runDirectory = Path.Combine(_temporaryDirectory, "ejecucion-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(runDirectory);

            var startInfo = new ProcessStartInfo
            {
                FileName = FindDotnetHost(),
                WorkingDirectory = runDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardInputEncoding = Utf8,
                StandardOutputEncoding = Utf8,
                StandardErrorEncoding = Utf8
            };
            startInfo.ArgumentList.Add(assemblyPath);
            using var process = new Process { StartInfo = startInfo };
            if (!process.Start())
                throw new InvalidOperationException("No se ha podido iniciar el programa del alumno.");

            using var timeoutCancellation = new CancellationTokenSource(executionTimeout);
            using var waitingCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                timeoutCancellation.Token, cancellationToken, _lifetimeCancellation.Token);
            int outputLimitExceeded = 0;
            void StopForOutputLimit()
            {
                Interlocked.Exchange(ref outputLimitExceeded, 1);
                KillProcess(process);
            }

            using var readingCancellation = new CancellationTokenSource();
            Task<string> outputTask = ReadBoundedAsync(process.StandardOutput, StopForOutputLimit, readingCancellation.Token);
            Task<string> errorTask = ReadBoundedAsync(process.StandardError, StopForOutputLimit, readingCancellation.Token);
            Task inputTask = WriteInputAsync(process.StandardInput, input, waitingCancellation.Token);
            bool timedOut = false;
            try
            {
                await process.WaitForExitAsync(waitingCancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                timedOut = timeoutCancellation.IsCancellationRequested
                    && !cancellationToken.IsCancellationRequested
                    && !_lifetimeCancellation.IsCancellationRequested;
                KillProcess(process);
                await process.WaitForExitAsync().ConfigureAwait(false);
            }
            finally
            {
                // También desbloquea una escritura pendiente si el programa dejó de leer.
                waitingCancellation.Cancel();
                await inputTask.ConfigureAwait(false);
                await FinishReadingAsync(process, readingCancellation, outputTask, errorTask).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            _lifetimeCancellation.Token.ThrowIfCancellationRequested();
            return new ExecutionResult(process.ExitCode,
                await outputTask.ConfigureAwait(false),
                await errorTask.ConfigureAwait(false),
                timedOut, Volatile.Read(ref outputLimitExceeded) != 0);
        }
        finally
        {
            if (runDirectory is not null) TryDeleteDirectory(runDirectory);
            lock (_lifetimeLock)
            {
                _activeRuns--;
                if (_disposed && _activeRuns == 0)
                {
                    TryDeleteDirectory(_temporaryDirectory);
                    _lifetimeCancellation.Dispose();
                }
            }
        }
    }

    private string Compile()
    {
        CancellationToken cancellationToken = _lifetimeCancellation.Token;
        cancellationToken.ThrowIfCancellationRequested();
        string buildDirectory = Path.Combine(_temporaryDirectory, "compilacion");
        Directory.CreateDirectory(buildDirectory);
        string sourcePath = Path.Combine(buildDirectory, "Program.cs");
        File.WriteAllText(sourcePath, Source, Utf8);
        File.WriteAllText(Path.Combine(buildDirectory, "GlobalUsings.g.cs"), ImplicitUsings, Utf8);
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(Source, parseOptions, "Program.cs", Utf8, cancellationToken),
            CSharpSyntaxTree.ParseText(ImplicitUsings, parseOptions, "GlobalUsings.g.cs", Utf8, cancellationToken)
        };
        var compilation = CSharpCompilation.Create("EntregaAlumno", syntaxTrees, References.Value,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                optimizationLevel: OptimizationLevel.Release,
                deterministic: true,
                nullableContextOptions: NullableContextOptions.Disable));
        string assemblyPath = Path.Combine(buildDirectory, "EntregaAlumno.dll");
        using (var assembly = File.Create(assemblyPath))
        {
            var result = compilation.Emit(assembly, cancellationToken: cancellationToken);
            if (!result.Success)
            {
                string errors = string.Join(Environment.NewLine,
                    result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                        .Select(FormatDiagnostic));
                throw new CompilationException("No se ha podido compilar Program.cs:" + Environment.NewLine + errors);
            }
        }

        // Las referencias proceden del runtime del corrector, que puede estar instalado
        // con un major posterior a .NET 8. Ejecutamos con ese mismo runtime.
        int runtimeMajor = Math.Max(8, Environment.Version.Major);
        var runtimeConfiguration = new
        {
            runtimeOptions = new
            {
                tfm = $"net{runtimeMajor}.0",
                rollForward = "LatestMajor",
                framework = new { name = "Microsoft.NETCore.App", version = $"{runtimeMajor}.0.0" }
            }
        };
        File.WriteAllText(Path.Combine(buildDirectory, "EntregaAlumno.runtimeconfig.json"),
            JsonSerializer.Serialize(runtimeConfiguration), Utf8);
        return assemblyPath;
    }

    private static string FormatDiagnostic(Diagnostic diagnostic)
    {
        string location = "";
        if (diagnostic.Location.IsInSource)
        {
            FileLinePositionSpan span = diagnostic.Location.GetLineSpan();
            location = $"{Path.GetFileName(span.Path)} (línea {span.StartLinePosition.Line + 1}, columna {span.StartLinePosition.Character + 1}): ";
        }
        return location + diagnostic.Id + ": " + diagnostic.GetMessage(CultureInfo.InvariantCulture);
    }

    private static MetadataReference[] CreateReferences()
    {
        string? paths = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrEmpty(paths))
            throw new InvalidOperationException("El runtime no proporciona las referencias de .NET necesarias para compilar.");
        return paths.Split(Path.PathSeparator)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }

    private static string FindDotnetHost()
    {
        string? processPath = Environment.ProcessPath;
        if (processPath is not null &&
            (Path.GetFileName(processPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase)
             || Path.GetFileName(processPath).Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase)))
            return processPath;
        string? configuredHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
        if (!string.IsNullOrWhiteSpace(configuredHost)) return configuredHost;
        string hostName = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
        string? configuredRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            string candidate = Path.Combine(configuredRoot, hostName);
            if (File.Exists(candidate)) return candidate;
        }
        // dotnet run puede iniciar un apphost sin DOTNET_HOST_PATH. La ruta del
        // runtime permite encontrar su host también en instalaciones locales.
        var runtimeDirectory = new DirectoryInfo(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory());
        string? runtimeRoot = runtimeDirectory.Parent?.Parent?.Parent?.FullName;
        if (runtimeRoot is not null)
        {
            string candidate = Path.Combine(runtimeRoot, hostName);
            if (File.Exists(candidate)) return candidate;
        }
        return "dotnet";
    }

    private static async Task WriteInputAsync(StreamWriter writer, string input, CancellationToken cancellationToken)
    {
        try
        {
            await writer.WriteAsync(input.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (IOException) { } // El alumno puede terminar sin consumir todas las líneas.
        catch (ObjectDisposedException) { }
        finally
        {
            try { writer.Close(); }
            catch (IOException) { }
            catch (ObjectDisposedException) { }
        }
    }

    private static async Task<string> ReadBoundedAsync(
        StreamReader reader, Action outputLimitReached, CancellationToken cancellationToken)
    {
        var result = new StringBuilder();
        var buffer = new char[4096];
        bool limitReached = false;
        try
        {
            int count;
            while ((count = await reader.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false)) != 0)
            {
                if (limitReached) continue;
                result.Append(buffer, 0, count);
                string text = result.ToString();
                if (Utf8.GetByteCount(text) <= MaximumOutputBytes) continue;

                int low = 0;
                int high = text.Length;
                while (low < high)
                {
                    int middle = low + (high - low + 1) / 2;
                    if (Utf8.GetByteCount(text.AsSpan(0, middle)) <= MaximumOutputBytes)
                        low = middle;
                    else
                        high = middle - 1;
                }
                if (low > 0 && char.IsHighSurrogate(text[low - 1])) low--;
                result.Length = low;
                limitReached = true;
                outputLimitReached();
            }
        }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
        catch (OperationCanceledException) { }
        return result.ToString();
    }

    private static async Task FinishReadingAsync(
        Process process, CancellationTokenSource readingCancellation, Task<string> outputTask, Task<string> errorTask)
    {
        Task drains = Task.WhenAll(outputTask, errorTask);
        try
        {
            await drains.WaitAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // Si un descendiente conserva las tuberías abiertas, cerrarlas evita
            // bloquear al corrector después de que el programa haya terminado.
            KillProcess(process);
            readingCancellation.Cancel();
            process.StandardOutput.Dispose();
            process.StandardError.Dispose();
            await drains.ConfigureAwait(false);
        }
    }

    private static void KillProcess(Process process)
    {
        try { process.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) { } // El proceso ya ha terminado.
        catch (System.ComponentModel.Win32Exception) { }
    }

    private static void TryDeleteDirectory(string path)
    {
        try { if (Directory.Exists(path)) Directory.Delete(path, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    public void Dispose()
    {
        lock (_lifetimeLock)
        {
            if (_disposed) return;
            _disposed = true;
            _lifetimeCancellation.Cancel();
            if (_activeRuns == 0)
            {
                TryDeleteDirectory(_temporaryDirectory);
                _lifetimeCancellation.Dispose();
            }
        }
    }
}
