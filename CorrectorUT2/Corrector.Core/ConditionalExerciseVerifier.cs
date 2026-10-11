using System.Globalization;
using System.Text.RegularExpressions;

namespace Corrector.Core;

/// <summary>Behaviour tests for exercises 21–30. Initializers are changed in temporary syntax trees.</summary>
public static class ConditionalExerciseVerifier
{
    private sealed record Scenario(string Name, Dictionary<string, object> Values,
        Func<string, double?, bool> Check, string Expected, bool ProbeSaldo = false);

    public static async Task<IReadOnlyList<CheckResult>> VerifyAsync(int exercise, StudentProgram program,
        IReadOnlyDictionary<string, string>? variableMap = null, CancellationToken ct = default)
    {
        var roles = Roles(exercise);
        if (roles.Count == 0) throw new ArgumentOutOfRangeException(nameof(exercise), "Este verificador comprende los ejercicios 21–30.");
        var results = new List<CheckResult>();
        var analysis = new ConditionalSourceAnalysis(program.Source, roles, variableMap);
        if (analysis.Error is not null)
        {
            results.Add(new CheckResult("Identificar datos del ejercicio", false, analysis.Error));
            return results;
        }
        try
        {
            var original = await program.RunAsync(cancellationToken: ct);
            results.Add(new CheckResult("Compilar y ejecutar el archivo original", IsSuccessful(original),
                IsSuccessful(original) ? "El archivo original compila y finaliza correctamente." : Failure(original)));
        }
        catch (CompilationException exception)
        {
            results.Add(new CheckResult("Compilar el archivo original", false, exception.Message));
            return results;
        }

        foreach (var scenario in Scenarios(exercise))
        {
            ct.ThrowIfCancellationRequested();
            var values = roles.ToDictionary(x => x.Name, x => x.InitialValue, StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in scenario.Values) values[key] = value;
            try
            {
                using var mutated = program.WithSource(analysis.WithValues(values, scenario.ProbeSaldo));
                var execution = await mutated.RunAsync(cancellationToken: ct);
                var (output, balance) = ExtractProbe(execution.Output);
                bool passed = IsSuccessful(execution) && scenario.Check(output, balance);
                var adaptation = analysis.Notes.Count > 0 ? " " + string.Join(" ", analysis.Notes) : "";
                results.Add(new CheckResult(scenario.Name, passed, passed
                    ? "Resultado correcto." + adaptation
                    : !IsSuccessful(execution) ? Failure(execution)
                    : $"Se esperaba: {scenario.Expected}. Salida obtenida: {Display(output)}"
                        + (scenario.ProbeSaldo ? $" Saldo final observado: {balance?.ToString(CultureInfo.InvariantCulture) ?? "no disponible"}." : "")
                        + adaptation));
            }
            catch (CompilationException exception)
            {
                results.Add(new CheckResult(scenario.Name, false, "La variante de prueba no compila: " + exception.Message));
            }
        }
        return results;
    }

    private static bool IsSuccessful(ExecutionResult execution) => execution.ExitCode == 0 && !execution.TimedOut && !execution.OutputLimitExceeded;
    private static string Failure(ExecutionResult execution) => execution.TimedOut ? "El programa no finaliza dentro del tiempo permitido."
        : execution.OutputLimitExceeded ? "El programa genera demasiada salida; comprueba sus bucles."
        : $"El programa termina con error ({execution.ExitCode}): {Display(execution.Error)}";
    private static string Display(string output) => string.IsNullOrWhiteSpace(output) ? "(sin salida)" : output.Trim()[..Math.Min(output.Trim().Length, 900)];

    private static (string Output, double? Balance) ExtractProbe(string output)
    {
        var expression = new Regex(@"(?m)^" + Regex.Escape(ConditionalSourceAnalysis.ProbePrefix) + @"([^\r\n]*)\r?$", RegexOptions.CultureInvariant);
        var match = expression.Matches(output).Cast<Match>().LastOrDefault();
        double? balance = match is not null && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        return (expression.Replace(output, ""), balance);
    }

    private static bool Number(string output, double value) => OutputChecks.ContainsNumber(output, value);
    private static bool Any(string output, params string[] phrases) => OutputChecks.ContainsAny(output, phrases);
    private static bool Balance(double? actual, double expected) => actual.HasValue && Math.Abs(actual.Value - expected) < 0.0001;
    private static Dictionary<string, object> V(params (string Name, object Value)[] values) => values.ToDictionary(x => x.Name, x => x.Value);
    private static Scenario S(string name, Dictionary<string, object> values, Func<string, double?, bool> check, string expected, bool probe = false)
        => new(name, values, check, expected, probe);
    private static ConditionalRole R(string name, string kind, object initial, params string[] aliases) => new(name, kind, initial, aliases);

    private static IReadOnlyList<ConditionalRole> Roles(int exercise) => exercise switch
    {
        21 => [R("compra", "number", 85.5, "importeCompra", "totalCompra", "importe", "total"), R("productos", "number", 6, "cantidadProductos", "numeroProductos", "numProductos"), R("esSocio", "bool", true, "socio", "clienteSocio", "miembro")],
        22 => [R("saldo", "number", 45.0, "saldoDisponible", "dinero", "balance"), R("precioVideojuego", "number", 39.99, "precio", "precioJuego", "costeVideojuego", "coste")],
        23 => [R("usuario", "string", "mikel", "nombreUsuario", "username", "user"), R("password", "string", "abc123", "contrasena", "clave", "pass"), R("cuentaActiva", "bool", true, "activa", "esActiva", "activo")],
        24 => [R("temperatura", "number", 27.5, "temp", "grados", "temperature")],
        25 => [R("nota", "number", 7.5, "calificacion", "notaFinal", "media"), R("asistencia", "number", 85, "porcentajeAsistencia", "asistenciaPorcentaje"), R("proyectoEntregado", "bool", true, "entregado", "entregaProyecto", "proyecto")],
        26 => [R("opcion", "number", 2, "opcionMenu", "eleccion", "seleccion")],
        27 => [R("numero1", "number", 20, "num1", "n1", "primerNumero", "a", "operando1"), R("numero2", "number", 5, "num2", "n2", "segundoNumero", "b", "operando2"), R("operacion", "char", '*', "operador", "simbolo", "op")],
        28 => [R("edad", "number", 20, "age"), R("estudiante", "bool", true, "esEstudiante", "alumno", "student"), R("familiaNumerosa", "bool", true, "esFamiliaNumerosa", "familia", "largeFamily")],
        29 => [R("autenticado", "bool", true, "estaAutenticado", "usuarioAutenticado", "logueado"), R("rol", "string", "editor", "role", "tipoUsuario"), R("accion", "string", "editar", "action", "operacion"), R("propietario", "bool", false, "esPropietario", "dueno", "owner")],
        30 => [R("tarjetaActiva", "bool", true, "activa", "tarjetaValida"), R("pinCorrecto", "number", 1234, "pinValido", "pinGuardado", "pinReal"), R("pinIntroducido", "number", 1234, "pinIngresado", "pinUsuario", "pinEntrada"), R("intentosFallidos", "number", 1, "intentos", "fallos", "numeroIntentos"), R("saldo", "number", 850.0, "saldoDisponible", "balance"), R("cantidad", "number", 200.0, "importe", "monto", "cantidadOperacion"), R("operacion", "number", 2, "opcion", "opcionMenu", "eleccion")],
        _ => []
    };

    private static IEnumerable<Scenario> Scenarios(int exercise) => exercise switch
    {
        21 => Promotions(), 22 => VideoGame(), 23 => Access(), 24 => Temperature(), 25 => Assessment(),
        26 => Restaurant(), 27 => Calculator(), 28 => Ticket(), 29 => Permissions(), 30 => Atm(),
        _ => []
    };

    private static bool Promotion(string output, int index) => index switch
    {
        0 => Any(output, "envio gratuito", "envio gratis", "portes gratis"),
        1 => Any(output, "regalo promocional", "regalo incluido", "obtienes un regalo"),
        2 => Any(output, "acumulas puntos", "acumula puntos", "obtienes puntos", "puntos con esta compra"),
        3 => Any(output, "cupon de 10", "cupon 10", "cupon descuento de 10"),
        _ => false
    };

    private static IEnumerable<Scenario> Promotions()
    {
        (double Buy, int Products, bool Member)[] inputs = [(85.5, 6, true), (50, 4, false), (50.01, 4, false), (49, 5, false), (0, 0, true), (100, 5, false), (100.01, 6, true)];
        foreach (var (buy, products, member) in inputs)
        {
            bool[] expected = [buy > 50, products >= 5, member, buy > 100];
            yield return S($"Promociones: compra {buy}, {products} productos, socio {member}", V(("compra", buy), ("productos", products), ("esSocio", member)),
                (output, _) => Enumerable.Range(0, 4).All(i => Promotion(output, i) == expected[i]),
                "exclusivamente las promociones aplicables: " + string.Join(", ", new[] { "envío gratuito", "regalo", "puntos", "cupón de 10 €" }.Where((_, i) => expected[i])));
        }
    }

    private static bool Purchased(string output) => Any(output, "compra realizada", "compra correcta", "compra completada", "comprado correctamente", "has comprado", "compra exitosa");
    private static bool Insufficient(string output) => Any(output, "saldo insuficiente", "dinero insuficiente", "fondos insuficientes", "no tienes suficiente", "no hay suficiente", "saldo no es suficiente");
    private static IEnumerable<Scenario> VideoGame()
    {
        (double Money, double Price)[] inputs = [(45, 39.99), (20, 39.99), (39.99, 39.99), (39.98, 39.99), (45, 10), (45, 50)];
        foreach (var (money, price) in inputs)
        {
            var enough = money >= price;
            var final = enough ? money - price : money;
            yield return S($"Compra: saldo {money}, precio {price}", V(("saldo", money), ("precioVideojuego", price)),
                (output, balance) => (enough ? Purchased(output) && !Insufficient(output) : Insufficient(output) && !Purchased(output))
                    && Number(output, final) && Balance(balance, final),
                $"{(enough ? "compra realizada" : "saldo insuficiente")} y saldo final {final}", true);
        }
    }

    private static bool AllowedAccess(string output) => Any(output, "acceso concedido", "acceso permitido", "acceso autorizado", "puedes acceder", "bienvenido");
    private static bool DeniedAccess(string output) => Any(output, "acceso denegado", "acceso rechazado", "acceso bloqueado", "no puedes acceder", "acceso no permitido", "credenciales incorrectas");
    private static IEnumerable<Scenario> Access()
    {
        (string User, string Password, bool Active)[] inputs = [("mikel", "abc123", true), ("ane", "abc123", true), ("mikel", "incorrecta", true), ("mikel", "abc123", false), ("ane", "incorrecta", false)];
        foreach (var (user, password, active) in inputs)
        {
            var allowed = user == "mikel" && password == "abc123" && active;
            yield return S($"Acceso: usuario {user}, contraseña {(password == "abc123" ? "válida" : "incorrecta")}, cuenta activa {active}",
                V(("usuario", user), ("password", password), ("cuentaActiva", active)),
                (output, _) => AllowedAccess(output) == allowed && DeniedAccess(output) != allowed,
                allowed ? "acceso concedido" : "acceso denegado");
        }
    }

    private static string[] TemperatureMessages => ["temperatura muy baja", "hace frio", "temperatura agradable", "hace calor", "temperatura muy alta"];
    private static IEnumerable<Scenario> Temperature()
    {
        double[] temperatures = [5, 9.99, 10, 17.99, 18, 25, 25.01, 27.5, 30, 35, 35.01, 40];
        foreach (var temperature in temperatures)
        {
            var expected = temperature < 10 ? 0 : temperature < 18 ? 1 : temperature <= 25 ? 2 : temperature <= 35 ? 3 : 4;
            yield return S($"Temperatura {temperature}: límites de los rangos", V(("temperatura", temperature)),
                (output, _) => Enumerable.Range(0, 5).All(i => Any(output, TemperatureMessages[i]) == (i == expected)),
                "únicamente «" + TemperatureMessages[expected] + "»");
        }
    }

    private static string[] AssessmentMessages => ["proyecto pendiente", "no cumple la asistencia minima", "suspenso", "aprobado", "notable", "sobresaliente"];
    private static bool AssessmentMessage(string output, int index) => index == 1
        ? Any(output, AssessmentMessages[1], "asistencia insuficiente", "no cumple asistencia", "asistencia inferior al minimo")
        : Any(output, AssessmentMessages[index]);
    private static IEnumerable<Scenario> Assessment()
    {
        (double Grade, int Attendance, bool Delivered)[] inputs = [(7.5, 85, true), (3, 60, false), (9, 79, true), (4.99, 80, true), (5, 80, true), (6.99, 80, true), (7, 80, true), (8.99, 80, true), (9, 80, true), (10, 80, true), (10, 100, false)];
        foreach (var (grade, attendance, delivered) in inputs)
        {
            var expected = !delivered ? 0 : attendance < 80 ? 1 : grade < 5 ? 2 : grade < 7 ? 3 : grade < 9 ? 4 : 5;
            yield return S($"Evaluación: nota {grade}, asistencia {attendance} %, proyecto {delivered}", V(("nota", grade), ("asistencia", attendance), ("proyectoEntregado", delivered)),
                (output, _) => Enumerable.Range(0, 6).All(i => AssessmentMessage(output, i) == (i == expected)), "únicamente «" + AssessmentMessages[expected] + "»");
        }
    }

    private static IEnumerable<Scenario> Restaurant()
    {
        string[] dishes = ["hamburguesa", "pizza", "ensalada", "bocadillo"];
        double[] prices = [9.5, 11, 8, 7.5];
        foreach (var option in new[] { 1, 2, 3, 4, 5, 0, -1, 6 })
        {
            var selected = option - 1;
            bool Check(string output, double? _)
            {
                bool chosen = Any(output, "has elegido", "has seleccionado", "has pedido");
                bool farewell = Any(output, "hasta pronto", "adios", "hasta luego");
                bool wrong = Any(output, "opcion incorrecta", "opcion no valida", "opcion invalida");
                if (option == 5) return farewell && !chosen && !wrong;
                if (option < 1 || option > 5) return wrong && !chosen && !farewell;
                return chosen && !farewell && !wrong && Number(output, prices[selected])
                    && Any(output, "has elegido " + dishes[selected], "has seleccionado " + dishes[selected], "has pedido " + dishes[selected])
                    && Enumerable.Range(0, 4).Where(i => i != selected).All(i => !Any(output, "has elegido " + dishes[i], "has seleccionado " + dishes[i], "has pedido " + dishes[i]));
            }
            yield return S($"Restaurante: opción {option}", V(("opcion", option)), Check,
                option is >= 1 and <= 4 ? $"plato {dishes[selected]}, precio {prices[selected]}" : option == 5 ? "despedida" : "opción incorrecta");
        }
    }

    private static bool InvalidOperation(string output) => Any(output, "operacion no valida", "operacion invalida", "operacion incorrecta", "opcion no valida", "opcion incorrecta", "operador no valido", "operador invalido");
    private static IEnumerable<Scenario> Calculator()
    {
        (double A, double B, char Operator)[] inputs = [(20, 5, '+'), (20, 5, '-'), (20, 5, '*'), (20, 5, '/'), (7, 2, '/'), (-4, 2, '*'), (1.5, 2.25, '+')];
        foreach (var (a, b, operation) in inputs)
        {
            var result = operation switch { '+' => a + b, '-' => a - b, '*' => a * b, _ => a / b };
            yield return S($"Calculadora: {a} {operation} {b}", V(("numero1", a), ("numero2", b), ("operacion", operation)),
                (output, _) => Number(output, result) && !InvalidOperation(output), $"resultado {result}");
        }
        yield return S("Calculadora: operador desconocido", V(("operacion", '?')), (output, _) => InvalidOperation(output), "operación no válida");
        // Division by zero is the book's optional extension, so it does not determine the mandatory result.
    }

    private static IEnumerable<Scenario> Ticket()
    {
        (int Age, bool Student, bool Family)[] inputs = [(20, true, true), (0, false, false), (5, true, true), (6, true, true), (17, true, true), (18, true, true), (20, true, false), (20, false, true), (20, false, false), (64, true, false), (65, true, true), (65, false, false)];
        foreach (var (age, student, family) in inputs)
        {
            var price = 15;
            if (age < 6) price = 0;
            else
            {
                if (age <= 17) price = Math.Min(price, 8);
                if (student) price = Math.Min(price, 10);
                if (family) price = Math.Min(price, 9);
                if (age >= 65) price = Math.Min(price, 6);
            }
            yield return S($"Entrada: edad {age}, estudiante {student}, familia numerosa {family}", V(("edad", age), ("estudiante", student), ("familiaNumerosa", family)),
                (output, _) => Any(output, "entrada") && Any(output, "precio final", "precio de la entrada", "precio entrada", "precio a pagar") && Number(output, price),
                $"la tarifa más barata, {price} €, en el informe de la entrada");
        }
    }

    private static bool PermissionAllowed(string output) => Any(output, "accion permitida", "accion autorizada", "operacion permitida");
    private static bool PermissionDenied(string output) => Any(output, "accion denegada", "accion no permitida", "accion rechazada", "operacion denegada");
    private static IEnumerable<Scenario> Permissions()
    {
        foreach (bool authenticated in new[] { true, false })
        foreach (var role in new[] { "admin", "editor", "usuario" })
        foreach (var action in new[] { "leer", "editar", "eliminar" })
        foreach (bool owner in new[] { false, true })
        {
            var allowed = authenticated && (role == "admin" || role == "editor" && (action != "eliminar" || owner)
                || role == "usuario" && (action == "leer" || action == "editar" && owner));
            yield return S($"Permisos: {role}, {action}, propietario {owner}, autenticado {authenticated}", V(("autenticado", authenticated), ("rol", role), ("accion", action), ("propietario", owner)),
                (output, _) => PermissionAllowed(output) == allowed && PermissionDenied(output) != allowed,
                allowed ? "únicamente acción permitida" : "únicamente acción denegada");
        }
    }

    private static bool BlockedCard(string output) => Any(output, "tarjeta bloqueada", "tarjeta inactiva", "tarjeta desactivada");
    private static bool TooManyAttempts(string output) => Any(output, "demasiados intentos", "limite de intentos", "maximo de intentos", "tres intentos", "3 intentos");
    private static bool WrongPin(string output) => Any(output, "pin incorrecto", "pin no valido", "pin invalido", "pin erroneo");
    private static bool InvalidAmount(string output) => Any(output, "cantidad debe ser mayor", "cantidad mayor que", "cantidad positiva", "importe positivo", "importe debe ser mayor", "cantidad no valida", "cantidad invalida", "importe no valido", "importe invalido", "cantidad incorrecta", "importe incorrecto", "cantidad no puede", "importe no puede");
    private static bool SuccessfulTransaction(string output) => Any(output, "nuevo saldo", "saldo final", "retirada realizada", "retiro realizado", "ingreso realizado", "retirada correcta", "ingreso correcto");
    private static IEnumerable<Scenario> Atm()
    {
        foreach (var (name, operation, amount, money, expected) in new (string, int, double, double, double)[]
        {
            ("Consulta de saldo", 1, 200, 850, 850), ("Retirada del libro", 2, 200, 850, 650),
            ("Retirada de todo el saldo", 2, 850, 850, 0), ("Ingreso del libro", 3, 200, 850, 1050),
            ("Ingreso con otro saldo e importe", 3, 20, 10, 30), ("Retirada decimal", 2, 10.5, 40.75, 30.25)
        })
            yield return S("Cajero: " + name, V(("operacion", operation), ("cantidad", amount), ("saldo", money)),
                (output, balance) => Number(output, expected) && Balance(balance, expected)
                    && !BlockedCard(output) && !WrongPin(output) && !Insufficient(output) && !InvalidAmount(output), $"saldo mostrado y conservado correctamente: {expected} €", true);

        yield return S("Cajero: retirada superior al saldo", V(("cantidad", 850.01)),
            (output, balance) => Insufficient(output) && !SuccessfulTransaction(output) && Balance(balance, 850), "saldo insuficiente y saldo sin modificar", true);
        foreach (var operation in new[] { 2, 3 })
        foreach (var amount in new[] { 0.0, -1.0 })
            yield return S($"Cajero: {(operation == 2 ? "retirada" : "ingreso")} de {amount} €", V(("operacion", operation), ("cantidad", amount)),
                (output, balance) => InvalidAmount(output) && !SuccessfulTransaction(output) && Balance(balance, 850), "rechazo de una cantidad no positiva y saldo sin modificar", true);

        yield return S("Cajero: salir", V(("operacion", 4)),
            (output, balance) => Any(output, "gracias por utilizar el cajero", "gracias por usar el cajero", "hasta pronto", "hasta luego") && Balance(balance, 850), "despedida y saldo sin modificar", true);
        foreach (var operation in new[] { 0, 9 })
            yield return S($"Cajero: operación desconocida {operation}", V(("operacion", operation)),
                (output, balance) => InvalidOperation(output) && !SuccessfulTransaction(output) && Balance(balance, 850), "operación no válida y saldo sin modificar", true);
        yield return S("Cajero: PIN incorrecto", V(("pinIntroducido", 9999)),
            (output, balance) => WrongPin(output) && !SuccessfulTransaction(output) && Balance(balance, 850), "PIN incorrecto, sin operar", true);
        foreach (var attempts in new[] { 3, 4 })
            yield return S($"Cajero: {attempts} intentos fallidos", V(("intentosFallidos", attempts), ("pinIntroducido", 9999)),
                (output, balance) => TooManyAttempts(output) && BlockedCard(output) && !WrongPin(output) && !SuccessfulTransaction(output) && Balance(balance, 850), "demasiados intentos y tarjeta bloqueada antes de comprobar PIN", true);
        yield return S("Cajero: tarjeta inactiva tiene prioridad", V(("tarjetaActiva", false), ("intentosFallidos", 3), ("pinIntroducido", 9999)),
            (output, balance) => BlockedCard(output) && !TooManyAttempts(output) && !WrongPin(output) && !SuccessfulTransaction(output) && Balance(balance, 850), "tarjeta bloqueada antes de todas las demás comprobaciones", true);
        yield return S("Cajero: dos intentos todavía permiten retirar", V(("intentosFallidos", 2)),
            (output, balance) => Number(output, 650) && Balance(balance, 650) && !BlockedCard(output), "retirada correcta con menos de tres intentos fallidos", true);
        yield return S("Cajero: el PIN correcto procede de la variable", V(("pinCorrecto", 6789), ("pinIntroducido", 6789)),
            (output, balance) => Number(output, 650) && Balance(balance, 650) && !WrongPin(output), "retirada correcta cuando ambos PIN coinciden aunque no sean 1234", true);
    }
}
