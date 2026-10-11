using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Corrector.Cli;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly string[] ExerciseNames =
    [
        "Ficha personal", "Videojuego", "Producto", "Estación meteorológica", "Expediente",
        "Vehículo", "Cita médica", "Rescata el programa", "Seguimiento de paquete", "Mi sistema",
        "Calculadora", "Reparto de monedas", "Partida", "Ticket de compra", "Calculadora de notas",
        "Atracción", "Tienda online", "Inscripción en una carrera", "Sistema de acceso", "Campeonato",
        "Promociones", "Compra de videojuego", "Control de acceso", "Temperatura", "Evaluación del alumno",
        "Menú de restaurante", "Calculadora con switch", "Precio de entrada", "Permisos", "Cajero automático",
        "Entrenamiento", "Plan de ahorro", "Filtro de números", "Objetivo de ventas", "Acceso con contraseña",
        "Adivina el número", "Menú de aplicación", "Cajero", "Caja registradora", "Torneo de combate",
        "Asignación de aulas", "Factura de tienda", "Aparcamiento", "Venta de entradas", "Tablas de multiplicar",
        "Generador de figuras", "Gestión de notas", "Ocupación de hotel", "Campeonato", "Restaurante"
    ];

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        try
        {
            Options options = ParseArguments(args);
            if (options.Help)
            {
                PrintHelp();
                return 0;
            }
            if (options.List)
            {
                for (int index = 0; index < ExerciseNames.Length; index++)
                    Console.WriteLine($"{index + 1,2}. {ExerciseNames[index]}");
                return 0;
            }

            if (options.Exercise is null && options.File is null && options.Url is null)
                ReadInteractiveOptions(options);
            ValidateOptions(options);

            string root = FindCorrectorRoot();
            string source = options.Url is null
                ? await SourceLoader.FromFileAsync(options.File!, cancellation.Token)
                : await SourceLoader.FromUrlAsync(options.Url, cancellation.Token);
            string submissionDirectory = Path.Combine(root, "entregas");
            Directory.CreateDirectory(submissionDirectory);
            string sourcePath = Path.Combine(submissionDirectory, "Program.cs");
            string configurationPath = Path.Combine(root, "entrega.json");
            await SaveAtomicallyAsync(sourcePath, source, cancellation.Token);
            var submission = new SavedSubmission(options.Exercise!.Value, sourcePath, options.VariableMap, options.Culture);
            await SaveAtomicallyAsync(configurationPath, JsonSerializer.Serialize(submission, JsonOptions) + Environment.NewLine, cancellation.Token);

            Console.WriteLine($"\nEntrega preparada: ejercicio {options.Exercise} ({ExerciseNames[options.Exercise.Value - 1]}).");
            Console.WriteLine($"Copia del código: {sourcePath}");
            if (options.PrepareOnly)
            {
                Console.WriteLine("Ahora puedes ejecutar la tarea «Pruebas de la entrega preparada» o dotnet test.");
                return 0;
            }

            return await RunUnitTestsAsync(root, configurationPath, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Comprobación cancelada.");
            return 2;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or
            IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
        {
            Console.Error.WriteLine($"Error: {exception.Message}");
            return 2;
        }
    }

    private static Options ParseArguments(string[] args)
    {
        var options = new Options();
        for (int index = 0; index < args.Length; index++)
        {
            string option = args[index];
            string NextValue()
            {
                if (++index >= args.Length)
                    throw new ArgumentException($"Falta el valor de {option}. Usa --ayuda para ver ejemplos.");
                return args[index];
            }

            switch (option)
            {
                case "--ejercicio":
                    if (!int.TryParse(NextValue(), out int exercise))
                        throw new ArgumentException("El ejercicio debe ser un número del 1 al 50.");
                    options.Exercise = exercise;
                    break;
                case "--archivo": options.File = CleanPath(NextValue()); break;
                case "--url": options.Url = NextValue().Trim(); break;
                case "--cultura": options.Culture = NextValue(); break;
                case "--mapa":
                    string mapping = NextValue();
                    int separator = mapping.IndexOf('=');
                    if (separator <= 0 || separator == mapping.Length - 1)
                        throw new ArgumentException("Cada mapa debe tener el formato --mapa nombreDelCorrector=nombreDelAlumno.");
                    string role = mapping[..separator].Trim();
                    string variable = mapping[(separator + 1)..].Trim();
                    if (!Regex.IsMatch(variable, @"^@?[\p{L}_][\p{L}\p{Nd}_]*$") || string.IsNullOrWhiteSpace(role))
                        throw new ArgumentException("El mapa debe indicar el nombre de una variable de C#.");
                    options.VariableMap[role] = variable;
                    break;
                case "--solo-preparar": options.PrepareOnly = true; break;
                case "--listar": options.List = true; break;
                case "--ayuda": case "-h": case "--help": options.Help = true; break;
                default: throw new ArgumentException($"Opción desconocida: {option}. Usa --ayuda para ver ejemplos.");
            }
        }
        return options;
    }

    private static void ReadInteractiveOptions(Options options)
    {
        Console.WriteLine("Corrector de los 50 ejercicios de UT2");
        Console.WriteLine("Entrega tu Program.cs como archivo local o mediante una URL pública HTTPS.");
        Console.WriteLine("Las pruebas mostrarán qué requisitos se cumplen y cuáles debes revisar.\n");
        while (true)
        {
            Console.Write("Número del ejercicio (1–50): ");
            string? answer = Console.ReadLine();
            if (answer is null)
                throw new InvalidOperationException("No se ha indicado el ejercicio. También puedes usar --ejercicio N --archivo Program.cs.");
            if (int.TryParse(answer, out int exercise) && exercise is >= 1 and <= 50)
            {
                options.Exercise = exercise;
                break;
            }
            Console.WriteLine("Introduce un número del 1 al 50.");
        }

        Console.Write("Ruta de Program.cs o URL HTTPS: ");
        string input = CleanPath(Console.ReadLine() ?? "");
        if (Uri.TryCreate(input, UriKind.Absolute, out Uri? uri) &&
            uri.Scheme is "http" or "https")
            options.Url = input;
        else
            options.File = input;
    }

    private static void ValidateOptions(Options options)
    {
        if (options.Exercise is null or < 1 or > 50)
            throw new ArgumentException("Indica --ejercicio con un número del 1 al 50.");
        bool filePresent = !string.IsNullOrWhiteSpace(options.File);
        bool urlPresent = !string.IsNullOrWhiteSpace(options.Url);
        if (filePresent == urlPresent)
            throw new ArgumentException("Indica exactamente una entrega: --archivo Program.cs o --url https://…/Program.cs.");
        if (options.Culture is not ("es-ES" or "en-US"))
            throw new ArgumentException("La cultura debe ser es-ES o en-US.");
    }

    private static async Task<int> RunUnitTestsAsync(string root, string configurationPath, CancellationToken cancellationToken)
    {
        string testsProject = Path.Combine(root, "Corrector.Tests", "Corrector.Tests.csproj");
        if (!File.Exists(testsProject))
            throw new InvalidOperationException("No se encuentra Corrector.Tests. Descarga la carpeta CorrectorUT2 completa.");

        var startInfo = new ProcessStartInfo
        {
            FileName = FindDotnet(),
            WorkingDirectory = root,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add(testsProject);
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add("Categoria=Entrega");
        startInfo.ArgumentList.Add("--logger");
        startInfo.ArgumentList.Add("console;verbosity=detailed");
        startInfo.Environment["CORRECTOR_ENTREGA"] = configurationPath;
        startInfo.Environment.Remove("CORRECTOR_REFERENCIAS");
        startInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "es";
        Console.WriteLine("\nEjecutando las pruebas unitarias…\n");
        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("No se pudo iniciar dotnet test. Instala el SDK de .NET 8 o posterior.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(10));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
            if (cancellationToken.IsCancellationRequested)
                throw;
            throw new InvalidOperationException("Las pruebas no terminaron en 10 minutos. Revisa la conexión y vuelve a intentarlo.");
        }

        if (process.ExitCode == 0)
        {
            Console.WriteLine("\nTu entrega supera todas las comprobaciones disponibles para este ejercicio.");
            return 0;
        }
        Console.WriteLine("\nLa comprobación no ha terminado correctamente. Revisa los mensajes de las pruebas o los errores de preparación que aparecen arriba.");
        return 1;
    }

    private static string FindDotnet()
    {
        string? processPath = Environment.ProcessPath;
        if (processPath is not null && string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase))
            return processPath;
        string? hostPath = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
        if (!string.IsNullOrWhiteSpace(hostPath) && File.Exists(hostPath))
            return hostPath;
        return "dotnet";
    }

    private static string FindCorrectorRoot()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            DirectoryInfo? directory = new(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Corrector.Cli", "Corrector.Cli.csproj")))
                    return directory.FullName;
                string nested = Path.Combine(directory.FullName, "CorrectorUT2");
                if (File.Exists(Path.Combine(nested, "Corrector.Cli", "Corrector.Cli.csproj")))
                    return nested;
                directory = directory.Parent;
            }
        }
        throw new InvalidOperationException("No se encuentra la carpeta CorrectorUT2. Descarga el corrector completo y abre esa carpeta.");
    }

    private static async Task SaveAtomicallyAsync(string path, string contents, CancellationToken cancellationToken)
    {
        string temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporaryPath, contents, new UTF8Encoding(false), cancellationToken);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static string CleanPath(string input)
    {
        string clean = input.Trim();
        if (clean.Length >= 2 && ((clean[0] == '"' && clean[^1] == '"') || (clean[0] == '\'' && clean[^1] == '\'')))
            clean = clean[1..^1];
        return clean;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            Corrector UT2 — pruebas visibles para los ejercicios 1–50.

            Abre CorrectorUT2 y ejecuta:
              dotnet run --project Corrector.Cli

            O indica directamente la entrega:
              dotnet run --project Corrector.Cli -- --ejercicio 31 --archivo "../MiEjercicio/Program.cs"
              dotnet run --project Corrector.Cli -- --ejercicio 31 --url "https://github.com/usuario/repo/blob/main/Program.cs"

            Opciones:
              --ejercicio N          Número del ejercicio, del 1 al 50.
              --archivo RUTA         Archivo local .cs (pon la ruta entre comillas si tiene espacios).
              --url URL              Enlace HTTPS público al código; admite enlaces blob y Raw de GitHub.
              --mapa ROL=VARIABLE    Asocia un nombre del corrector al nombre usado por el alumno.
                                     Puede repetirse. Consulta los roles de cada ejercicio en las pruebas.
              --cultura es-ES|en-US  Cultura del programa entregado; por defecto es-ES.
              --solo-preparar        Guarda la entrega sin ejecutar sus pruebas.
              --listar               Muestra los 50 ejercicios.
              --ayuda                Muestra esta ayuda.

            Las pruebas se ejecutan con dotnet test. No se exige que los textos ni los
            nombres de las variables sean iguales a los de las soluciones de ejemplo.
            """);
    }

    private sealed class Options
    {
        public int? Exercise { get; set; }
        public string? File { get; set; }
        public string? Url { get; set; }
        public string Culture { get; set; } = "es-ES";
        public Dictionary<string, string> VariableMap { get; } = new(StringComparer.OrdinalIgnoreCase);
        public bool PrepareOnly { get; set; }
        public bool List { get; set; }
        public bool Help { get; set; }
    }

    private sealed record SavedSubmission(int Exercise, string SourcePath, Dictionary<string, string> VariableMap, string Culture);
}
