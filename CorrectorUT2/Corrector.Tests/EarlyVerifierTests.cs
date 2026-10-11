using Corrector.Core;
using Xunit;

namespace Corrector.Tests;

public sealed class EarlyVerifierTests
{
    [Fact]
    public async Task AceptaUnaFichaConOtrosDatosMainYBooleanoSiNo()
    {
        using var program = new StudentProgram("""
            class Program
            {
                static void Main()
                {
                    string persona = "Nerea";
                    int anos = 20;
                    float estatura = 1.65f;
                    char apellido = 'Z';
                    bool conduce = true;
                    Console.WriteLine($"{persona}: {anos} años, {estatura} metros, {apellido}");
                    Console.WriteLine(conduce ? "Sí" : "No");
                }
            }
            """);

        var checks = await EarlyExerciseVerifier.VerifyAsync(1, program);

        Assert.All(checks, check => Assert.True(check.Passed, check.Name + ": " + check.Message));
    }

    [Fact]
    public async Task RechazaDatosQueSoloAparecenEscritosComoTextoFijo()
    {
        using var program = new StudentProgram("""
            string nombre = "Ane";
            int edad = 19;
            double altura = 1.72;
            char inicial = 'G';
            bool carnet = false;
            Console.WriteLine("Nombre: Ane, edad: 19, altura: 1.72, inicial: G, carnet: False");
            """);

        var checks = await EarlyExerciseVerifier.VerifyAsync(1, program);

        Assert.Contains(checks, check => !check.Passed && check.Name == "Datos de texto");
    }

    [Fact]
    public async Task AceptaCalculadoraConNombresYOrdenDeSalidaDistintos()
    {
        using var program = new StudentProgram("""
            int primero = 17, segundo = 5;
            int diferencia = primero - segundo;
            int cociente = primero / segundo;
            int producto = primero * segundo;
            int total = primero + segundo;
            int sobrante = primero % segundo;
            Console.WriteLine(sobrante);
            Console.WriteLine(producto);
            Console.WriteLine(total);
            Console.WriteLine(cociente);
            Console.WriteLine(diferencia);
            class TipoAdicional { }
            """);

        var checks = await EarlyExerciseVerifier.VerifyAsync(11, program);

        Assert.All(checks, check => Assert.True(check.Passed, check.Name + ": " + check.Message));
    }

    [Fact]
    public async Task LosDatosAlternativosDetectanUnaCalculadoraConResultadosFijos()
    {
        using var program = new StudentProgram("""
            int numero1 = 17, numero2 = 5;
            int suma = 22, resta = 12, multiplicacion = 85, division = 3, resto = 2;
            Console.WriteLine($"{suma}, {resta}, {multiplicacion}, {division}, {resto}");
            """);

        var checks = await EarlyExerciseVerifier.VerifyAsync(11, program);

        Assert.Contains(checks, check => !check.Passed && check.Name == "Suma");
    }

    [Fact]
    public async Task AceptaUnBoolMostradoConSiNoEnLaCalculadoraDeNotas()
    {
        using var program = new StudentProgram("""
            double examen1 = 6.5, examen2 = 8, examen3 = 5.5;
            double media = (examen1 + examen2 + examen3) / 3;
            bool aprobado = media >= 5;
            Console.WriteLine($"La media es {media:F2}");
            Console.WriteLine(aprobado ? "Sí" : "No");
            """);

        var checks = await EarlyExerciseVerifier.VerifyAsync(15, program);

        Assert.All(checks, check => Assert.True(check.Passed, check.Name + ": " + check.Message));
    }

    [Fact]
    public async Task UnMapaDeVariablesResuelveEntradasBooleanasReordenadas()
    {
        using var program = new StudentProgram("""
            int anos = 17;
            bool c = true, a = true, b = true;
            double tiempo = 52.30;
            bool adulto = anos >= 18;
            bool edadValida = adulto || a;
            bool tramites = b && c;
            bool participa = edadValida && tramites;
            bool rapido = tiempo < 55;
            bool avanzado = participa && rapido;
            Console.WriteLine($"{adulto}, {edadValida}, {tramites}, {participa}, {rapido}, {avanzado}");
            """);
        var map = new Dictionary<string, string>
        {
            ["edad"] = "anos", ["tieneAutorizacion"] = "a", ["inscripcionPagada"] = "b",
            ["documentacionEntregada"] = "c", ["marcaAnterior"] = "tiempo"
        };

        var checks = await EarlyExerciseVerifier.VerifyAsync(18, program, map);

        Assert.All(checks, check => Assert.True(check.Passed, check.Name + ": " + check.Message));
    }
}
