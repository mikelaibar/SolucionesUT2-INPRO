using System.Globalization;

namespace Corrector.Core;

public static class CorrectionEngine
{
    public static async Task<CorrectionReport> EvaluateAsync(SubmissionConfig config,
        CancellationToken cancellationToken = default)
    {
        if (config.Exercise is < 1 or > 50)
            throw new ArgumentOutOfRangeException(nameof(config), "El ejercicio debe estar entre 1 y 50.");
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(config.Culture);
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
            string source = await File.ReadAllTextAsync(config.SourcePath, cancellationToken);
            // La cultura se configura en la copia de trabajo, sin editar la entrega.
            string prepared = SetCulture(source, config.Culture);
            using var program = new StudentProgram(prepared);
            IReadOnlyList<CheckResult> checks = config.Exercise switch
            {
                <= 20 => await EarlyExerciseVerifier.VerifyAsync(config.Exercise, program, config.VariableMap, cancellationToken),
                <= 30 => await ConditionalExerciseVerifier.VerifyAsync(config.Exercise, program, config.VariableMap, cancellationToken),
                _ => await InteractiveExerciseVerifier.VerifyAsync(config.Exercise, program, cancellationToken)
            };
            return new CorrectionReport(config.Exercise, checks);
        }
        catch (CompilationException ex)
        {
            return new CorrectionReport(config.Exercise, [new("Compilación", false, ex.Message)]);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new CorrectionReport(config.Exercise, [new("Preparación de la comprobación", false, ex.Message)]);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    private static string SetCulture(string source, string culture)
    {
        // ModuleInitializer funciona tanto con instrucciones superiores como con Main.
        // Se ejecuta antes del código del alumno, que puede escoger otra cultura.
        return source + "\ninternal static class __CorrectorCulture\n{\n" +
            "[global::System.Runtime.CompilerServices.ModuleInitializer]\n" +
            "internal static void Initialize() { global::System.Globalization.CultureInfo.CurrentCulture = " +
            "global::System.Globalization.CultureInfo.GetCultureInfo(\"" + culture.Replace("\"", "") + "\"); }\n}\n";
    }
}
