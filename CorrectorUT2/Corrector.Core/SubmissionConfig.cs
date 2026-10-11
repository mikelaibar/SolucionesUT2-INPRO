using System.Globalization;
using System.Text.Json;

namespace Corrector.Core;

public sealed class SubmissionConfig
{
    public int Exercise { get; init; }
    public string SourcePath { get; init; } = "";
    public Dictionary<string, string> VariableMap { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public string Culture { get; init; } = "es-ES";

    public static SubmissionConfig Load(string path)
    {
        if (!File.Exists(path))
            throw new InvalidOperationException("Primero prepara tu entrega con 'dotnet run --project Corrector.Cli'.");
        var config = JsonSerializer.Deserialize<SubmissionConfig>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("El archivo entrega.json está vacío.");
        if (config.Exercise is < 1 or > 50)
            throw new InvalidOperationException("El número de ejercicio debe estar entre 1 y 50.");
        if (string.IsNullOrWhiteSpace(config.SourcePath))
            throw new InvalidOperationException("Falta la ruta de Program.cs en entrega.json.");
        _ = CultureInfo.GetCultureInfo(config.Culture);
        string resolved = Path.GetFullPath(config.SourcePath, Path.GetDirectoryName(Path.GetFullPath(path))!);
        if (!File.Exists(resolved))
            throw new InvalidOperationException("No se encuentra el archivo de la entrega: " + resolved);
        if (new FileInfo(resolved).Length > 1024 * 1024)
            throw new InvalidOperationException("Program.cs debe ocupar como máximo 1 MB.");
        return new SubmissionConfig { Exercise = config.Exercise, SourcePath = resolved,
            Culture = config.Culture, VariableMap = new(config.VariableMap, StringComparer.OrdinalIgnoreCase) };
    }
}
