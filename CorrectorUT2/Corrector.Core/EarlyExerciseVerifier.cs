using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Corrector.Core;

/// <summary>Comprueba tipos, uso real de variables y resultados con datos alternativos.</summary>
public static class EarlyExerciseVerifier
{
    public static async Task<IReadOnlyList<CheckResult>> VerifyAsync(int exercise, StudentProgram program, IReadOnlyDictionary<string, string>? variableMap = null, CancellationToken ct = default)
    {
        if (exercise is < 1 or > 20) throw new ArgumentOutOfRangeException(nameof(exercise));
        var analysis = new EarlySourceAnalysis(program.Source);
        if (exercise <= 10) return await VerifyDataAsync(exercise, analysis, program, ct);
        var plan = GetPlan(exercise);
        var results = new List<CheckResult>();
        var bindings = ResolveInputs(analysis, plan.Inputs, variableMap, results);
        if (bindings == null) return results;
        results.Add(new("Datos iniciales identificados", true, string.Join(", ", plan.Inputs.Select(i => i.Role + " → " + bindings[i.Role].Name))));
        if (exercise == 20) results.Add(new("Operaciones sin if", !analysis.HasIf, analysis.HasIf ? "El ejercicio pide obtener los resultados mediante operaciones, sin if." : "Las comprobaciones se obtienen sin if."));
        var snapshots = new List<EarlySnapshot>();
        foreach (var variant in plan.Variants)
        {
            var replacement = plan.Inputs.Select((input, index) => (bindings[input.Role].Id, variant.Values[index])).ToDictionary(p => p.Id, p => p.Item2);
            var mutated = new EarlySourceAnalysis(analysis.WithInitialValues(replacement));
            var snapshot = await ExecuteAsync(mutated, program, "Ejecución: " + variant.Name, results, ct);
            if (snapshot == null) return results;
            snapshots.Add(snapshot);
        }
        var expectedRows = plan.Expected(plan.Variants[0].Values);
        var expectations = plan.Variants.Select(v => plan.Expected(v.Values)).ToArray();
        var inputIds = bindings.Values.Select(v => v.Id).ToHashSet();
        var candidates = analysis.Variables.Where(v => exercise == 13 || !inputIds.Contains(v.Id)).ToArray();
        var options = expectedRows.Select((expected, row) => candidates.Where(variable => variable.Kind == expected.Kind && !variable.Nullable &&
            snapshots.Select((snapshot, index) => snapshot.Values.TryGetValue(variable.Id, out var value) && Equal(value, expectations[index][row].Value) &&
                (!expected.MustShow || snapshot.Shown.Contains(variable.Id))).All(ok => ok)).Select(v => v.Id).ToArray()).ToArray();
        var matching = new Dictionary<string, int>();
        bool Match(int row, HashSet<string> visited)
        {
            foreach (var id in options[row])
                if (visited.Add(id) && (!matching.TryGetValue(id, out var previous) || Match(previous, visited)))
                { matching[id] = row; return true; }
            return false;
        }
        for (var row = 0; row < expectedRows.Count; row++)
        {
            var passed = Match(row, new HashSet<string>());
            results.Add(new(expectedRows[row].Name, passed, passed ? $"Resultado correcto con {plan.Variants.Count} juegos de datos." :
                ExplainFailure(expectedRows[row], row, candidates, options[row], plan.Variants, expectations, snapshots)));
        }
        return results;
    }

    private static string ExplainFailure(Expected expected, int row, IReadOnlyList<EarlyVariable> candidates, string[] options,
        IReadOnlyList<Variant> variants, IReadOnlyList<Expected>[] expectations, IReadOnlyList<EarlySnapshot> snapshots)
    {
        var expectedCases = "Resultados esperados: " + string.Join("; ", variants.Select((variant, index) => variant.Name + " → " + Display(expectations[index][row].Value))) + ".";
        if (options.Length > 0) return "Crea una variable diferente para guardar cada resultado del ejercicio. " + expectedCases;
        var best = candidates.Where(v => v.Kind == expected.Kind && !v.Nullable)
            .OrderByDescending(v => snapshots.Select((snapshot, index) => snapshot.Values.TryGetValue(v.Id, out var value) && Equal(value, expectations[index][row].Value)).Count(ok => ok)).FirstOrDefault();
        if (best == null) return "Guarda este resultado en una variable " + (expected.Kind == "real" ? "numérica que conserve decimales" : expected.Kind) + ". " + expectedCases;
        var differences = new List<string>();
        for (var index = 0; index < variants.Count && differences.Count < 4; index++)
        {
            var hasValue = snapshots[index].Values.TryGetValue(best.Id, out var value);
            if (!hasValue || !Equal(value, expectations[index][row].Value))
                differences.Add(variants[index].Name + ": se esperaba " + Display(expectations[index][row].Value) + ", se obtuvo " + (hasValue ? value.ToString() : "ningún dato"));
            else if (expected.MustShow && !snapshots[index].Shown.Contains(best.Id))
                differences.Add(variants[index].Name + ": el valor es correcto, pero no se muestra por consola");
        }
        return "Revisa la variable " + best.Name + ". " + string.Join("; ", differences) + ". " + expectedCases;
    }

    private static string Display(object? value) => value switch
    {
        null => "null", bool flag => flag ? "true" : "false", double number => number.ToString("0.#####", CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null"
    };

    private static async Task<IReadOnlyList<CheckResult>> VerifyDataAsync(int exercise, EarlySourceAnalysis analysis, StudentProgram program, CancellationToken ct)
    {
        var checks = new List<CheckResult>();
        var snapshot = await ExecuteAsync(analysis, program, "Compilación y ejecución", checks, ct);
        if (snapshot == null) return checks;
        var shown = analysis.Variables.Where(v => snapshot.Shown.Contains(v.Id) && v.Declaration.Initializer != null).ToArray();
        void Require(string name, Func<EarlyVariable, bool> filter, int count)
        {
            var found = shown.Count(filter);
            checks.Add(new(name, found >= count, $"Se necesitan {count}; se encontraron {found} variables inicializadas y utilizadas en la salida."));
        }
        bool Plain(EarlyVariable v, string kind) => v.Kind == kind && !v.Nullable;
        bool Null(EarlyVariable v, string kind) => v.Kind == kind && v.Nullable && v.HasConstant && v.InitialValue == null;
        if (exercise == 8)
        {
            var expected = new (string Name, string Kind, bool Nullable, object? Value)[]
            {
                ("edad", "int", false, 24), ("altura", "real", false, 1.78), ("inicial", "char", false, 'M'),
                ("nombre", "string", false, "Mario"), ("activo", "bool", false, true), ("puntos", "int", true, null),
                ("temperatura", "real", false, 36.7), ("telefono", "string", false, "654123987")
            };
            foreach (var item in expected)
            {
                var variable = shown.FirstOrDefault(v => v.Name == item.Name);
                var passed = variable != null && variable.Kind == item.Kind && variable.Nullable == item.Nullable && variable.HasConstant && ConstantEqual(variable.InitialValue, item.Value);
                checks.Add(new("Declaración y salida de " + item.Name, passed, passed ? "Tipo y valor corregidos; la variable se muestra." : "Conserva esta variable, corrige su tipo/valor y úsala al mostrar la ficha."));
            }
            return checks;
        }
        if (exercise == 10)
        {
            Require("Diez variables como mínimo", _ => true, 10);
            Require("Una variable int", v => Plain(v, "int"), 1);
            Require("Una variable double o float", v => Plain(v, "real") && (v.TypeName == "double" || v.TypeName == "float"), 1);
            Require("Una variable bool", v => Plain(v, "bool"), 1);
            Require("Una variable char", v => Plain(v, "char"), 1);
            Require("Dos variables string", v => Plain(v, "string"), 2);
            Require("Una variable nullable", v => v.Nullable, 1);
            var all = analysis.Variables.Where(v => v.Declaration.Initializer != null).All(v => snapshot.Shown.Contains(v.Id));
            checks.Add(new("Mostrar toda la ficha", all, all ? "Todas las variables inicializadas participan en la salida." : "Hay variables inicializadas que no se muestran."));
            return checks;
        }
        var requirements = exercise switch
        {
            1 => new[] { 1, 1, 1, 1, 1 },
            2 => new[] { 1, 1, 2, 1, 1 },
            3 => new[] { 2, 1, 1, 1, 1 },
            4 => new[] { 1, 1, 2, 1, 1 },
            5 => new[] { 2, 1, 1, 1, 0 },
            6 => new[] { 3, 2, 1, 1, 0 },
            7 => new[] { 2, 1, 1, 1, 1 },
            9 => new[] { 2, 1, 1, 1, 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(exercise))
        };
        var kinds = new[] { "string", "int", "real", "bool", "char" };
        var names = new[] { "Datos de texto", "Cantidades enteras", "Datos con decimales", "Datos booleanos", "Datos de una letra" };
        for (var i = 0; i < kinds.Length; i++) if (requirements[i] > 0) { var kind = kinds[i]; Require(names[i], v => Plain(v, kind), requirements[i]); }
        if (exercise is 5 or 6 or 9) Require("Dato decimal desconocido iniciado a null", v => Null(v, "real"), 1);
        if (exercise is 6 or 7) Require("Dato entero desconocido iniciado a null", v => Null(v, "int"), 1);
        if (exercise == 9) Require("Paquete todavía no entregado", v => Plain(v, "bool") && v.HasConstant && v.InitialValue is false, 1);
        return checks;
    }

    private static async Task<EarlySnapshot?> ExecuteAsync(EarlySourceAnalysis analysis, StudentProgram program, string name, List<CheckResult> checks, CancellationToken ct)
    {
        try
        {
            using var instrumented = program.WithSource(analysis.Instrument());
            var execution = await instrumented.RunAsync(cancellationToken: ct);
            var snapshot = EarlySourceAnalysis.ReadSnapshot(execution.Error);
            var passed = execution.ExitCode == 0 && !execution.TimedOut && !execution.OutputLimitExceeded && snapshot.Values.Count == analysis.Variables.Count;
            var error = string.Join('\n', execution.Error.Split('\n').Where(line => !line.StartsWith(EarlySourceAnalysis.ValuesMarker) && !line.StartsWith(EarlySourceAnalysis.ShownMarker))).Trim();
            checks.Add(new(name, passed, passed ? "El programa se ejecuta correctamente." : execution.TimedOut ? "Se agotó el tiempo de ejecución." : execution.OutputLimitExceeded ? "La salida supera el límite permitido." :
                string.IsNullOrEmpty(error) ? "No se pudo capturar el estado final; utiliza variables locales en el programa principal y evita terminar antes del final." : error));
            return passed ? snapshot : null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            checks.Add(new(name, false, exception.Message));
            return null;
        }
    }

    private static Dictionary<string, EarlyVariable>? ResolveInputs(EarlySourceAnalysis analysis, IReadOnlyList<Input> inputs, IReadOnlyDictionary<string, string>? variableMap, List<CheckResult> checks)
    {
        var result = new Dictionary<string, EarlyVariable>();
        var available = analysis.Variables.Where(v => v.Declaration.Initializer != null && v.HasConstant && !v.Nullable).ToList();
        foreach (var input in inputs)
        {
            EarlyVariable? variable = null;
            if (variableMap != null && variableMap.TryGetValue(input.Role, out var mapped))
            {
                variable = available.FirstOrDefault(v => v.Name == mapped && v.Kind == input.Kind);
                if (variable == null)
                {
                    checks.Add(new("Mapa de " + input.Role, false, $"La variable '{mapped}' debe existir, tener tipo {input.Kind} y disponer de un inicializador constante."));
                    return null;
                }
            }
            else
            {
                var aliases = input.Aliases.Append(input.Role).Select(Normalize).ToHashSet();
                variable = available.FirstOrDefault(v => v.Kind == input.Kind && aliases.Contains(Normalize(v.Name)));
                if (variable == null && input.Original != null)
                {
                    var literalMatches = available.Where(v => v.Kind == input.Kind && ConstantEqual(v.InitialValue, input.Original)).ToArray();
                    if (literalMatches.Length == 1) variable = literalMatches[0];
                }
                variable ??= available.FirstOrDefault(v => v.Kind == input.Kind);
            }
            if (variable == null)
            {
                checks.Add(new("Dato inicial " + input.Role, false, $"No se encontró un dato de tipo {input.Kind}. Usa nombres descriptivos o indica el mapa de variables."));
                return null;
            }
            result[input.Role] = variable;
            available.Remove(variable);
        }
        return result;
    }

    private static string Normalize(string value) => new(value.Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c)).Select(char.ToLowerInvariant).ToArray());
    private static bool ConstantEqual(object? actual, object? expected)
    {
        if (actual == null || expected == null) return actual == null && expected == null;
        if (actual is char || expected is char || actual is string || expected is string || actual is bool || expected is bool) return Equals(actual, expected);
        try { return Math.Abs(Convert.ToDouble(actual, CultureInfo.InvariantCulture) - Convert.ToDouble(expected, CultureInfo.InvariantCulture)) < 0.00001; } catch { return false; }
    }
    private static bool Equal(JsonElement actual, object? expected)
    {
        if (expected == null) return actual.ValueKind == JsonValueKind.Null;
        if (expected is bool boolean) return actual.ValueKind is JsonValueKind.True or JsonValueKind.False && actual.GetBoolean() == boolean;
        if (expected is string text) return actual.ValueKind == JsonValueKind.String && actual.GetString() == text;
        if (actual.ValueKind != JsonValueKind.Number || !actual.TryGetDouble(out var number)) return false;
        var target = Convert.ToDouble(expected, CultureInfo.InvariantCulture);
        return Math.Abs(number - target) <= 0.00001 * Math.Max(1, Math.Abs(target));
    }

    private sealed record Input(string Role, string Kind, object? Original, params string[] Aliases);
    private sealed record Variant(string Name, object?[] Values);
    private sealed record Expected(string Name, string Kind, object? Value, bool MustShow = true);
    private sealed record Plan(IReadOnlyList<Input> Inputs, IReadOnlyList<Variant> Variants, Func<object?[], IReadOnlyList<Expected>> Expected);
    private static Input I(string role, object? original, params string[] aliases) => new(role, "int", original, aliases);
    private static Input R(string role, object? original, params string[] aliases) => new(role, "real", original, aliases);
    private static Input B(string role, object? original, params string[] aliases) => new(role, "bool", original, aliases);
    private static int N(object? value) => Convert.ToInt32(value, CultureInfo.InvariantCulture);
    private static double D(object? value) => Convert.ToDouble(value, CultureInfo.InvariantCulture);
    private static bool T(object? value) => Convert.ToBoolean(value, CultureInfo.InvariantCulture);
    private static Expected Integer(string name, int value, bool show = true) => new(name, "int", value, show);
    private static Expected Real(string name, double value, bool show = true) => new(name, "real", value, show);
    private static Expected Boolean(string name, bool value, bool show = true) => new(name, "bool", value, show);
    private static Variant V(string name, params object?[] values) => new(name, values);

    private static Plan GetPlan(int exercise) => exercise switch
    {
        11 => new(new[] { I("numero1", 17, "num1", "a", "primerNumero"), I("numero2", 5, "num2", "b", "segundoNumero") },
            new[] { V("17 y 5", 17, 5), V("22 y 4", 22, 4), V("División con negativo", -7, 3), V("Divisor mayor", 5, 11) }, x => new[] {
                Integer("Suma", N(x[0]) + N(x[1])), Integer("Resta", N(x[0]) - N(x[1])), Integer("Multiplicación", N(x[0]) * N(x[1])), Integer("División entera", N(x[0]) / N(x[1])), Integer("Resto", N(x[0]) % N(x[1])) }),
        12 => new(new[] { I("monedas", 23, "totalMonedas"), I("jugadores", 4, "numeroJugadores") },
            new[] { V("23 monedas", 23, 4), V("26 monedas", 26, 4), V("Reparto exacto", 20, 4), V("Menos monedas que jugadores", 2, 5) }, x => new[] {
                Integer("Monedas por jugador", N(x[0]) / N(x[1])), Integer("Monedas sobrantes", N(x[0]) % N(x[1])) }),
        13 => new(new[] { I("puntos", 100, "puntuacion"), I("vidas", 3, "numeroVidas"), I("monedas", 10, "totalMonedas") },
            new[] { V("Partida inicial", 100, 3, 10), V("Una vida y cero monedas", 35, 1, 0), V("Cero puntos", 0, 6, 17) }, x => new[] {
                Integer("Puntos finales", (N(x[0]) + 50) * 2), Integer("Vidas finales", N(x[1])), Integer("Monedas finales", N(x[2]) + 2) }),
        14 => new(new[] { R("precioCamiseta", 19.95, "precioCamisetas"), I("cantidadCamisetas", 2, "camisetas", "numCamisetas"), R("precioPantalon", 35.5, "precioPantalones"), I("cantidadPantalones", 1, "pantalones", "numPantalones") },
            new[] { V("Compra del enunciado", 19.95, 2, 35.5, 1), V("Precios y cantidades alternativos", 7.25, 3, 14.5, 2), V("Productos económicos", 0.99, 5, 2.25, 4) }, x => new[] {
                Real("Coste de las camisetas", D(x[0]) * N(x[1])), Real("Coste de los pantalones", D(x[2]) * N(x[3])), Real("Total de la compra", D(x[0]) * N(x[1]) + D(x[2]) * N(x[3])) }),
        15 => new(new[] { R("examen1", 6.5, "nota1", "primerExamen"), R("examen2", 8d, "nota2", "segundoExamen"), R("examen3", 5.5, "nota3", "tercerExamen") },
            new[] { V("Notas del enunciado", 6.5, 8d, 5.5), V("Media cinco", 4d, 5d, 6d), V("Suspenso", 1d, 2d, 3d), V("Media con decimales", 10d, 10d, 9d) }, x => new[] {
                Real("Nota media", (D(x[0]) + D(x[1]) + D(x[2])) / 3), Boolean("Aprobado desde cinco", (D(x[0]) + D(x[1]) + D(x[2])) / 3 >= 5) }),
        16 => new(new[] { I("edad", 14), R("altura", 1.55), B("tieneEntrada", true, "entrada"), B("vaAcompanado", false, "acompanado") },
            new[] { V("Datos del enunciado", 14, 1.55, true, false), V("Límites inclusivos", 12, 1.5, true, false), V("Edad y altura insuficientes", 11, 1.49, true, false), V("Acompañado sin requisitos", 11, 1.5, false, true), V("Altura insuficiente", 12, 1.49, true, false), V("Sin entrada", 12, 1.5, false, false), V("Acompañado sin entrada", 12, 1.5, false, true) }, Attraction),
        17 => new(new[] { R("totalCompra", 67.5, "compra", "importeCompra"), B("esPremium", false, "premium"), B("tieneCupon", true, "cupon"), I("productos", 4, "numeroProductos", "cantidadProductos") },
            new[] { V("Datos del enunciado", 67.5, false, true, 4), V("50 exactos y tres productos", 50d, false, true, 3), V("30 exactos", 30d, false, true, 3), V("Más de 30 y dos productos", 30.01, false, true, 2), V("Premium sin compra ni cupón", 0d, true, false, 0), V("Más de 50 sin cupón", 51d, false, false, 3), V("Cupón y tres productos sin importe", 0d, false, true, 3), V("Sin promoción", 50d, false, false, 2) }, Shop),
        18 => new(new[] { I("edad", 17), B("tieneAutorizacion", true, "autorizacion"), B("inscripcionPagada", true, "pagada", "haPagado"), B("documentacionEntregada", true, "documentacion", "haEntregadoDocumentacion"), R("marcaAnterior", 52.3, "marca", "tiempoAnterior") },
            new[] { V("Datos del enunciado", 17, true, true, true, 52.3), V("Mayor de edad y 55 minutos", 18, false, true, true, 55d), V("Menor sin autorización", 17, false, true, true, 54d), V("Inscripción sin pagar", 18, false, false, true, 54d), V("Documentación sin entregar", 18, false, true, false, 54d), V("Grupo avanzado", 18, false, true, true, 54d) }, Race),
        19 => new(new[] { I("edad", 22), I("intentosFallidos", 2, "intentos"), B("usuarioActivo", true, "activo"), B("cuentaBloqueada", false, "bloqueada"), B("esAdministrador", false, "administrador", "esAdmin", "admin"), B("dobleFactorCorrecto", true, "dobleFactor", "factorCorrecto") },
            new[] { V("Datos del enunciado", 22, 2, true, false, false, true), V("Edad límite", 18, 2, true, false, false, true), V("Menor de edad", 17, 2, true, false, false, true), V("Tres intentos fallidos", 22, 3, true, false, false, true), V("Usuario inactivo", 22, 2, false, false, false, true), V("Cuenta bloqueada", 22, 2, true, true, false, true), V("Doble factor incorrecto", 22, 2, true, false, false, false), V("Administrador sin acceso normal", 17, 3, false, true, true, false) }, Security),
        20 => new(new[] { new Input("nombreJugador", "string", null, "jugador", "nombre"), I("partidasJugadas", null, "partidas", "jugadas"), I("partidasGanadas", null, "victorias", "ganadas"), I("puntosObtenidos", null, "puntos", "puntuacion"), I("penalizaciones", null, "numeroPenalizaciones"), B("inscripcionPagada", null, "haPagado", "pagada"), B("cuentaActiva", null, "activo", "usuarioActivo"), B("sancionado", null, "haSidoSancionado", "estaSancionado") },
            new[] { V("Clasificación con media decimal", "Prueba A", 10, 6, 1255, 2, true, true, false), V("Cinco victorias y media decimal", "Prueba B", 7, 5, 701, 2, true, true, false), V("Cuatro victorias", "Prueba C", 10, 4, 1100, 2, true, true, false), V("Media inferior a cien", "Prueba D", 10, 5, 999, 2, true, true, false), V("Tres penalizaciones", "Prueba E", 10, 5, 1000, 3, true, true, false), V("Inscripción sin pagar", "Prueba F", 10, 5, 1000, 2, false, true, false), V("Cuenta inactiva", "Prueba G", 10, 5, 1000, 2, true, false, false), V("Sanción", "Prueba H", 10, 5, 1000, 2, true, true, true) }, Championship),
        _ => throw new ArgumentOutOfRangeException(nameof(exercise))
    };

    private static IReadOnlyList<Expected> Attraction(object?[] x)
    {
        var age = N(x[0]) >= 12; var height = D(x[1]) >= 1.5; var both = age && height; var all = T(x[2]) && both;
        return new[] { Boolean("Edad suficiente", age), Boolean("Altura suficiente", height), Boolean("Edad y altura", both), Boolean("Entrada y requisitos", all), Boolean("Acceso o acompañamiento", all || T(x[3])) };
    }
    private static IReadOnlyList<Expected> Shop(object?[] x) => new[] {
        Boolean("Compra superior a 50", D(x[0]) > 50), Boolean("Cliente Premium", T(x[1])), Boolean("Cupón y tres productos", T(x[2]) && N(x[3]) >= 3),
        Boolean("Envío gratuito", D(x[0]) > 50 || T(x[1])), Boolean("Promoción especial", T(x[1]) || (T(x[2]) && D(x[0]) > 30)), Boolean("Sin promoción exclusiva", !T(x[1])) };
    private static IReadOnlyList<Expected> Race(object?[] x)
    {
        var adult = N(x[0]) >= 18; var age = adult || T(x[1]); var paperwork = T(x[2]) && T(x[3]); var participate = age && paperwork; var mark = D(x[4]) < 55;
        return new[] { Boolean("Mayor de edad", adult), Boolean("Requisito de edad", age), Boolean("Trámites completos", paperwork), Boolean("Puede participar", participate), Boolean("Marca inferior a 55", mark), Boolean("Grupo avanzado", participate && mark) };
    }
    private static IReadOnlyList<Expected> Security(object?[] x)
    {
        var adult = N(x[0]) >= 18; var tries = N(x[1]) < 3; var available = T(x[2]) && !T(x[3]); var security = available && T(x[5]); var normal = adult && security && tries;
        return new[] { Boolean("Mayor de edad", adult), Boolean("Intentos disponibles", tries), Boolean("Cuenta disponible", available), Boolean("Seguridad básica", security), Boolean("Acceso normal", normal), Boolean("Acceso al sistema", normal || T(x[4])) };
    }
    private static IReadOnlyList<Expected> Championship(object?[] x)
    {
        var mean = (double)N(x[3]) / N(x[1]); var wins = N(x[2]) >= 5; var average = mean >= 100; var penalties = N(x[4]) < 3;
        var participate = T(x[5]) && T(x[6]) && !T(x[7]); var sport = wins && average && penalties;
        return new[] { Integer("Partidas no ganadas", N(x[1]) - N(x[2])), Real("Media de puntos con decimales", mean), Boolean("Al menos cinco victorias", wins, false),
            Boolean("Media de cien o superior", average, false), Boolean("Menos de tres penalizaciones", penalties, false), Boolean("Puede participar", participate), Boolean("Requisitos deportivos", sport), Boolean("Puede clasificarse", participate && sport) };
    }
}
