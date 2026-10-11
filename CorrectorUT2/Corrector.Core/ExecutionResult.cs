namespace Corrector.Core;

/// <summary>Resultado de una ejecución del programa entregado por el alumno.</summary>
public sealed record ExecutionResult(
    int ExitCode,
    string Output,
    string Error,
    bool TimedOut,
    bool OutputLimitExceeded);

/// <summary>El archivo entregado no se puede compilar como programa de consola.</summary>
public sealed class CompilationException : Exception
{
    public CompilationException(string message) : base(message) { }
}
