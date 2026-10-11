namespace Corrector.Core;

public sealed record CheckResult(string Name, bool Passed, string Message);

public sealed record CorrectionReport(int Exercise, IReadOnlyList<CheckResult> Checks)
{
    public bool Passed => Checks.Count > 0 && Checks.All(check => check.Passed);
    public int PassedCount => Checks.Count(check => check.Passed);
}
