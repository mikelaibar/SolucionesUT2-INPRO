using Corrector.Core;
using Xunit;
using Xunit.Abstractions;
using System.Text.RegularExpressions;

namespace Corrector.Tests;

public sealed class SubmissionTests(ITestOutputHelper output)
{
    public static IEnumerable<object[]> SelectedExercises()
    {
        if (Environment.GetEnvironmentVariable("CORRECTOR_REFERENCIAS") == "1")
        {
            for (int exercise = 1; exercise <= 50; exercise++)
                yield return [exercise];
            yield break;
        }
        // La ausencia de configuración se comunica mediante una prueba fallida,
        // para que nunca parezca que una ejecución sin entrega ha corregido algo.
        int selected = 0;
        try { selected = SubmissionConfig.Load(ConfigPath()).Exercise; }
        catch (Exception) { }
        yield return [selected];
    }

    [Theory]
    [MemberData(nameof(SelectedExercises))]
    [Trait("Categoria", "Entrega")]
    public async Task El_programa_cumple_las_comprobaciones_del_ejercicio(int exercise)
    {
        SubmissionConfig config;
        if (Environment.GetEnvironmentVariable("CORRECTOR_REFERENCIAS") == "1")
        {
            string projectRoot = FindRoot();
            string solutions = Path.GetFullPath(Path.Combine(projectRoot, "..", "Ejercicios1"));
            string source = Directory.GetFiles(solutions, "Program.cs", SearchOption.AllDirectories)
                .Single(path => Regex.IsMatch(Path.GetFileName(Path.GetDirectoryName(path))!,
                    "^UT2Ejer" + exercise + "(?:[^0-9]|$)"));
            config = new SubmissionConfig { Exercise = exercise, SourcePath = source };
        }
        else
        {
            config = SubmissionConfig.Load(ConfigPath());
            Assert.Equal(exercise, config.Exercise);
        }

        CorrectionReport report = await CorrectionEngine.EvaluateAsync(config);
        foreach (CheckResult check in report.Checks)
            output.WriteLine($"{(check.Passed ? "OK" : "FALLO")} · {check.Name}: {check.Message}");
        output.WriteLine($"Ejercicio {exercise}: {report.PassedCount}/{report.Checks.Count} comprobaciones superadas.");
        Assert.True(report.Passed, $"Ejercicio {exercise}: {report.PassedCount}/{report.Checks.Count} comprobaciones superadas.\n" +
            string.Join("\n", report.Checks.Where(check => !check.Passed).Select(check => $"• {check.Name}: {check.Message}")));
    }

    private static string ConfigPath() => Environment.GetEnvironmentVariable("CORRECTOR_ENTREGA")
        ?? Path.Combine(FindRoot(), "entrega.json");

    private static string FindRoot()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Corrector.Tests", "Corrector.Tests.csproj")))
                return directory.FullName;
        throw new InvalidOperationException("No se encuentra la carpeta del proyecto CorrectorUT2.");
    }
}
