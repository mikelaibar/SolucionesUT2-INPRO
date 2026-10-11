using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Corrector.Core;

/// <summary>Pruebas de entrada/salida para los ejercicios de bucles e interacción (31–50).</summary>
public static class InteractiveExerciseVerifier
{
    private sealed record Scenario(string Name, string Input, Func<string, string?> Evaluate);
    private sealed record Field(string Pattern, double Value, string Description, double Tolerance = 0.011);

    public static async Task<IReadOnlyList<CheckResult>> VerifyAsync(
        int exercise, StudentProgram program, CancellationToken ct = default)
    {
        if (exercise is < 31 or > 50)
            throw new ArgumentOutOfRangeException(nameof(exercise));

        var results = new List<CheckResult>();
        var syntax = CSharpSyntaxTree.ParseText(program.Source).GetRoot();
        if (exercise == 31)
        {
            bool singleFor = syntax.DescendantNodes().OfType<ForStatementSyntax>().Count() == 1;
            results.Add(new("Utilizar un único bucle for", singleFor,
                singleFor ? "Se utiliza un único for." : "El enunciado pide un único bucle for."));
        }
        if (exercise == 33)
        {
            bool hasContinue = syntax.DescendantNodes().OfType<ContinueStatementSyntax>().Any();
            results.Add(new("Descartar mediante continue", hasContinue,
                hasContinue ? "Se utiliza continue." : "El enunciado exige utilizar continue para descartar números."));
        }
        if (exercise is 32 or 33)
        {
            bool hasFor = syntax.DescendantNodes().OfType<ForStatementSyntax>().Any();
            results.Add(new("Utilizar un bucle for", hasFor,
                hasFor ? "Se utiliza for." : "El enunciado pide utilizar un bucle for."));
        }
        if (exercise == 34)
        {
            bool hasWhile = syntax.DescendantNodes().OfType<WhileStatementSyntax>().Any();
            results.Add(new("Simular mediante while", hasWhile,
                hasWhile ? "Se utiliza while." : "El enunciado pide resolver la simulación con while."));
        }

        foreach (var scenario in Scenarios(exercise))
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var run = await program.RunAsync(scenario.Input, cancellationToken: ct);
                string? failure = run.TimedOut ? "El programa no terminó con la entrada suministrada."
                    : run.OutputLimitExceeded ? "El programa produjo demasiada salida; revisa la condición del bucle."
                    : run.ExitCode != 0 ? $"El programa terminó con un error: {run.Error.Trim()}"
                    : scenario.Evaluate(run.Output);
                results.Add(new(scenario.Name, failure is null, failure ?? "Resultado correcto."));
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception exception)
            {
                results.Add(new(scenario.Name, false, exception.Message));
            }
        }
        return results;
    }

    private static IEnumerable<Scenario> Scenarios(int exercise) => exercise switch
    {
        31 => Training(), 32 => Savings(), 33 => Filter(), 34 => Sales(),
        35 => Password(), 36 => Guess(), 37 => ApplicationMenu(), 38 => Atm(),
        39 => Register(), 40 => Combat(), 41 => Classrooms(), 42 => Invoice(),
        43 => Parking(), 44 => Tickets(), 45 => Tables(), 46 => Figures(),
        47 => Grades(), 48 => Hotel(), 49 => Championship(), 50 => Restaurant(),
        _ => throw new ArgumentOutOfRangeException(nameof(exercise))
    };

    private static IEnumerable<Scenario> Training()
    {
        yield return new("Diez vueltas, hidratación y finalización", "", output =>
        {
            var turns = Regex.Matches(Fold(output), @"\bvuelta\s*[:=]?\s*(\d+)")
                .Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)).ToArray();
            if (!turns.SequenceEqual(Enumerable.Range(1, 10)))
                return "Deben mostrarse exactamente las vueltas 1 a 10, en orden.";
            if (Count(output, "hidrat") != 3)
                return "Debe haber tres avisos de hidratación: vueltas 3, 6 y 9.";
            foreach (int turn in new[] { 3, 6, 9 })
                if (!Has(BetweenTurns(output, turn, turn + 1), "hidrat"))
                    return $"Falta el aviso de hidratación en la vuelta {turn}.";
            if (!Has(BetweenTurns(output, 5, 6), "mitad"))
                return "La vuelta 5 debe avisar de la mitad del entrenamiento.";
            return Require(Has(output, "entrenamiento completado", "entrenamiento terminado", "entrenamiento finalizado"),
                "Falta el mensaje de entrenamiento completado.");
        });
    }

    private static IEnumerable<Scenario> Savings()
    {
        yield return new("Doce meses de ahorro, con o sin ampliación", "", output =>
        {
            var pairs = Pairs(output, "mes");
            if (pairs.Count != 12 || !pairs.Select(p => p.Index).SequenceEqual(Enumerable.Range(1, 12)))
                return "Deben mostrarse los meses 1 a 12 con sus ahorros acumulados.";
            var basic = Enumerable.Range(1, 12).Select(month => 100 + 50.0 * month).ToArray();
            var extension = Enumerable.Range(1, 12).Select(month => 100 + 50.0 * month + 100 * (month / 6)).ToArray();
            var actual = pairs.Select(p => p.Value).ToArray();
            bool baseVersion = EqualNumbers(actual, basic);
            bool extendedVersion = EqualNumbers(actual, extension);
            if (!baseVersion && !extendedVersion)
                return "La secuencia mensual debe sumar 50 € desde los 100 € iniciales; se admite la paga extra opcional de 100 € en los meses 6 y 12.";
            return Summary(output,
                new Field(@"(?:dinero\s*)?total\s*(?:acumulado|ahorrado)?|ahorros\s*(?:finales|totales)",
                    baseVersion ? 700 : 900, "total ahorrado"));
        });
    }

    private static IEnumerable<Scenario> Filter()
    {
        yield return new("Pares del 1 al 50 excluyendo múltiplos de tres", "", output =>
        {
            double[] expected = Enumerable.Range(1, 50).Where(n => n % 2 == 0 && n % 3 != 0)
                .Select(n => (double)n).ToArray();
            string folded = Fold(output);
            var summaryStart = Regex.Match(folded, @"cantidad|numeros\s*mostrados|suma\s*(?:total)?");
            string body = summaryStart.Success ? folded[..summaryStart.Index] : folded;
            var numbers = OutputChecks.Numbers(body).ToArray();
            if (!EqualNumbers(numbers, expected))
                return "Deben mostrarse exactamente 2, 4, 8, 10, …, 46, 50: los pares que no sean múltiplos de 3.";
            return Summary(output,
                new Field(@"cantidad(?:\s+de\s+numeros)?(?:\s+mostrados)?|numeros\s*mostrados|contador", 17, "cantidad de números"),
                new Field(@"suma(?:\s+total)?", 434, "suma total"));
        });
    }

    private static IEnumerable<Scenario> Sales()
    {
        yield return new("Alcanzar el objetivo en siete días", "", output =>
        {
            var pairs = Pairs(output, "dia");
            // El resumen puede incluir «días», pero no debe convertirse en otra fila diaria.
            if (pairs.Count != 7 || !pairs.Select(p => p.Index).SequenceEqual(Enumerable.Range(1, 7)))
                return "El objetivo se alcanza en siete días: deben mostrarse los días 1 a 7.";
            double[] expected = Enumerable.Range(1, 7).Select(day => 1000 + 650.0 * day).ToArray();
            if (!EqualNumbers(pairs.Select(p => p.Value).ToArray(), expected))
                return "Las ventas deben aumentar 650 € al día: 1650, 2300, …, 5550 €.";
            if (!Has(output, "objetivo alcanzado", "objetivo conseguido", "objetivo cumplido"))
                return "Falta indicar que se ha alcanzado el objetivo.";
            return Summary(output, new Field(@"dias\s*(?:necesarios|transcurridos|totales)|total\s*(?:de\s*)?dias", 7, "días necesarios"));
        });
    }

    private static IEnumerable<Scenario> Password()
    {
        yield return PasswordCase("Contraseña correcta a la primera", new[] { "CSharp123" }, 0, true);
        yield return PasswordCase("Error y acierto al segundo intento", new[] { "no", "CSharp123" }, 1, true);
        yield return PasswordCase("La contraseña distingue mayúsculas", new[] { "csharp123", "CSharp123" }, 1, true);
        yield return PasswordCase("Acierto en el quinto intento", new[] { "a", "b", "c", "d", "CSharp123" }, 4, true);
        yield return PasswordCase("Bloquear tras cinco errores sin aceptar un sexto intento", new[] { "a", "b", "c", "d", "e", "CSharp123" }, 5, false);
    }

    private static Scenario PasswordCase(string name, string[] attempts, int errors, bool granted) =>
        new(name, Input(attempts), output =>
        {
            bool success = Has(output, "acceso concedido", "acceso autorizado", "acceso permitido");
            bool blocked = Has(output, "cuenta bloqueada", "acceso bloqueado", "cuenta ha sido bloqueada");
            if (success != granted || blocked == granted)
                return granted ? "Debe concederse acceso y terminar al acertar la contraseña." : "Debe bloquearse la cuenta después de cinco errores.";
            if (Count(output, "incorrect") < errors)
                return $"Deben avisarse los {errors} intentos incorrectos.";
            for (int i = 1; i <= errors; i++)
                if (!OutputChecks.ContainsNumber(output, 5 - i))
                    return $"Tras el error {i} deben indicarse {5 - i} intentos restantes.";
            return null;
        });

    private static IEnumerable<Scenario> Guess()
    {
        yield return GuessCase("Acierto a la primera", new[] { 37 }, 1, 0, 0, true);
        yield return GuessCase("Pistas de mayor y menor antes del acierto", new[] { 20, 50, 37 }, 3, 1, 1, true);
        yield return GuessCase("Acierto en el séptimo intento", new[] { 1, 2, 3, 4, 5, 6, 37 }, 7, 6, 0, true);
        yield return GuessCase("Siete intentos fallidos y fin de partida", new[] { 38, 39, 40, 41, 42, 43, 44, 37 }, 7, 0, 7, false);
        yield return GuessCase("Alternar pistas y contar todos los intentos", new[] { 100, 0, 90, 36, 37 }, 5, 2, 2, true);
    }

    private static Scenario GuessCase(string name, int[] attempts, int used, int higher, int lower, bool won) =>
        new(name, Input(attempts.Select(n => n.ToString(CultureInfo.InvariantCulture))), output =>
        {
            if (Has(output, "acertado", "has ganado", "numero correcto") != won)
                return won ? "Debe reconocer el número secreto 37." : "No se puede aceptar un octavo intento.";
            if (!won && !Has(output, "perdido", "agotado", "sin intentos"))
                return "Debe indicar la derrota al agotar los siete intentos.";
            if (Count(output, "mayor") < higher || Count(output, "menor") < lower)
                return "Las pistas deben indicar mayor para números menores de 37 y menor para números mayores de 37.";
            return Summary(output, new Field(@"intentos\s*(?:utilizados|usados|realizados|totales)", used, "intentos utilizados"));
        });

    private static IEnumerable<Scenario> ApplicationMenu()
    {
        yield return new("Mostrar cada acción y volver al menú", "1\n2\n3\n4\n", output => Validate(
            (Has(output, "hola", "bienvenido", "buenos dias", "buenas tardes", "saludos"), "La opción 1 debe mostrar un saludo."),
            (Regex.IsMatch(Fold(output), @"2026\s*[/\-]\s*2027|2026\s+2027"), "La opción 2 debe mostrar el curso 2026/2027."),
            (Has(output, "iniciacion a la programacion"), "La opción 3 debe mostrar el nombre del módulo."),
            (Count(output, "menu") >= 4, "El menú debe aparecer para las cuatro opciones."),
            (Closing(output), "La opción 4 debe cerrar el programa.")));
        yield return new("Opción incorrecta y continuación", "9\n2\n4\n", output => Validate(
            (InvalidOption(output), "Una opción inexistente debe avisar del error."),
            (OutputChecks.ContainsNumber(output, 2026) && OutputChecks.ContainsNumber(output, 2027), "Después del error debe poder consultarse el curso."),
            (Closing(output), "La opción 4 debe cerrar el programa.")));
        yield return new("Salir inmediatamente", "4\n1\n", output => Validate(
            (Closing(output), "Debe indicar que se cierra el programa."),
            (!Has(output, "hola", "bienvenido"), "No debe ejecutarse una opción posterior a salir.")));
    }

    private static IEnumerable<Scenario> Atm()
    {
        yield return AtmCase("Consultar saldo inicial y salir", "1\n4\n", 1000);
        yield return AtmCase("Retirada e ingreso consecutivos", "2\n250\n3\n120\n1\n4\n", 870);
        yield return AtmCase("Retirar todo el saldo", "2\n1000\n1\n4\n", 0);
        yield return AtmCase("No retirar más dinero del disponible", "2\n1001\n1\n4\n", 1000, "insuficiente");
        yield return AtmCase("Retiradas cero y negativas sin alterar saldo", "2\n0\n2\n-5\n4\n", 1000, "cantidad");
        yield return AtmCase("Ingresos cero y negativos sin alterar saldo", "3\n0\n3\n-5\n4\n", 1000, "cantidad");
        yield return AtmCase("Opción incorrecta y otra retirada", "9\n2\n175\n4\n", 825, "opcion");
        yield return AtmCase("Conservar saldo entre varias operaciones", "3\n500\n2\n1200\n2\n301\n3\n7\n4\n", 307, "insuficiente");
    }

    private static Scenario AtmCase(string name, string input, double balance, string? error = null) =>
        new(name, input, output =>
        {
            if (error == "insuficiente" && !Has(output, "saldo insuficiente", "fondos insuficientes", "no hay suficiente", "no tienes suficiente"))
                return "Debe avisar de saldo insuficiente y conservar el saldo.";
            if (error == "cantidad" && !InvalidAmount(output))
                return "Las cantidades iguales o inferiores a cero deben rechazarse.";
            if (error == "opcion" && !InvalidOption(output))
                return "Debe avisar de la opción de menú incorrecta.";
            return Summary(output, new Field(@"saldo\s*(?:final|al\s*finalizar)|saldo\s*restante", balance, "saldo final"));
        });

    private static IEnumerable<Scenario> Register()
    {
        yield return RegisterCase("Ejemplo del libro y precio negativo ignorado", "25\n-5\n120\n40\n0\n", 3, 185, 1, true);
        yield return RegisterCase("Cerrar sin registrar productos", "0\n", 0, 0, 0);
        yield return RegisterCase("Precio 100 no es un producto elevado", "100\n0\n", 1, 100, 0);
        yield return RegisterCase("Avisar del precio superior a cien", "101\n2\n0\n", 2, 103, 1);
        yield return RegisterCase("Varios negativos y acumulación de tres productos", "-7\n3\n-2\n8\n17\n0\n", 3, 28, 0, true);
    }

    private static Scenario RegisterCase(string name, string input, int products, double total, int expensive, bool invalid = false) =>
        new(name, input, output =>
        {
            if (invalid && !Has(output, "precio no valido", "precio incorrecto", "precio invalido"))
                return "Debe rechazarse un precio negativo.";
            if (Count(output, "precio elevado") != expensive)
                return "Sólo los productos con precio superior a 100 € deben mostrar el aviso de precio elevado.";
            return Summary(output,
                new Field(@"productos\s*(?:registrados|comprados|validos|totales)|cantidad\s*(?:de\s*)?productos", products, "productos registrados"),
                new Field(@"total\s*(?:de\s*la\s*)?compra|importe\s*(?:final|total)|total\s*(?:final)?", total, "total de la compra"));
        });

    private static IEnumerable<Scenario> Combat()
    {
        yield return CombatCase("Huir sin jugar un turno", "4\n", 0, 100, 100, 3, "huir");
        yield return CombatCase("Fuerte impar y opción incorrecta no consumen turno", "2\n9\n1\n2\n4\n", 2, 80, 55, 3, "huir", true);
        yield return CombatCase("Siete ataques normales y ningún contraataque final", "1\n1\n1\n1\n1\n1\n1\n", 7, 40, -5, 3, "ganar");
        yield return CombatCase("Ataques fuertes en turnos pares", "1\n2\n1\n2\n1\n", 5, 60, -5, 3, "ganar");
        // La curación empieza por debajo de 80: ambas versiones del reto opcional producen el mismo resultado.
        yield return CombatCase("Curarse con una poción después de cuatro ataques", "1\n1\n1\n1\n3\n4\n", 5, 70, 40, 2, "huir");
        yield return CombatCase("Agotar pociones y no curarse con una cuarta", "1\n1\n1\n1\n3\n3\n3\n3\n4\n", 8, 80, 40, 0, "huir", false, true);
        yield return CombatCase("Derrota al no poder recuperar más vida", "1\n1\n1\n1\n" + string.Concat(Enumerable.Repeat("3\n", 12)),
            16, 0, 40, 0, "perder", false, true);
    }

    private static Scenario CombatCase(string name, string input, int turns, int player, int enemy, int potions,
        string ending, bool invalid = false, bool noPotions = false) => new(name, input, output =>
    {
        bool won = Has(output, "has ganado", "ganado el combate", "victoria");
        bool lost = Has(output, "has sido derrotado", "has perdido", "derrota");
        bool fled = Has(output, "abandonado", "has huido", "huido del combate");
        if (won != (ending == "ganar") || lost != (ending == "perder") || fled != (ending == "huir"))
            return "El mensaje final debe corresponder a victoria, derrota o huida, sin mostrar otros resultados.";
        if (invalid && (!Has(output, "ataque fuerte no disponible", "fuerte no disponible", "turno impar") || !InvalidAction(output)))
            return "El ataque fuerte impar y la acción inexistente deben rechazarse sin consumir turno.";
        if (noPotions && !Has(output, "no quedan pociones", "sin pociones", "no tienes pociones"))
            return "Debe avisar cuando no quedan pociones.";
        // Se admite presentar cero en vez de un valor negativo para el enemigo derrotado.
        double actualEnemy = enemy;
        var enemyValues = Values(output, @"vida\s*(?:restante\s*)?(?:del\s*)?enemigo");
        if (enemy <= 0 && enemyValues.Count > 0 && enemyValues[^1] == 0)
            actualEnemy = 0;
        return Summary(output,
            new Field(@"turnos\s*(?:jugados|realizados|completados|totales)", turns, "turnos jugados"),
            new Field(@"vida\s*(?:restante\s*)?(?:del\s*)?jugador|vida\s*restante(?!\s*(?:del\s*)?enemigo)|tu\s*vida", player, "vida restante del jugador"),
            new Field(@"vida\s*(?:restante\s*)?(?:del\s*)?enemigo", actualEnemy, "vida restante del enemigo"),
            new Field(@"pociones\s*(?:restantes|disponibles)?", potions, "pociones restantes"));
    });

    private static IEnumerable<Scenario> Classrooms()
    {
        yield return ClassroomCase("Límites de las cuatro aulas y minúsculas", "a\nf\ng\nj\nk\nn\no\nz\n0\n", 2, 2, 2, 2);
        yield return ClassroomCase("Caracteres inválidos ignorados y ENTER para terminar", "B\n3\n?\nM\n\n", 1, 0, 1, 0, true);
        yield return ClassroomCase("ENTER inicial sin estudiantes", "\n", 0, 0, 0, 0);
        yield return ClassroomCase("Contadores independientes para aulas desiguales", "C\nd\nE\nI\nL\nP\nY\n0\n", 3, 1, 1, 2);
    }

    private static Scenario ClassroomCase(string name, string input, int a, int b, int c, int d, bool invalid = false) =>
        new(name, input, output =>
        {
            if (invalid && !Has(output, "no es valido", "no valido", "incorrecto", "error", "invalido"))
                return "Debe avisar cuando se introduce un carácter fuera de A–Z.";
            return Summary(output,
                new Field(@"aula\s*1", a, "alumnos del aula 1"), new Field(@"aula\s*2", b, "alumnos del aula 2"),
                new Field(@"aula\s*3", c, "alumnos del aula 3"), new Field(@"aula\s*4", d, "alumnos del aula 4"),
                new Field(@"total(?:\s*(?:de\s*)?alumnos)?", a + b + c + d, "total de alumnos"));
        });

    private static IEnumerable<Scenario> Invoice()
    {
        foreach (int amount in new[] { 49, 50, 99, 100, 199, 200 })
        {
            double rate = amount >= 200 ? .15 : amount >= 100 ? .10 : amount >= 50 ? .05 : 0;
            yield return InvoiceCase($"Tramo de descuento para {amount} €", $"1\nArticulo\n{amount}\n1\n", 1, amount, amount * rate);
        }
        yield return InvoiceCase("Sumar productos y cantidades antes de descontar", "2\nTeclado\n25\n2\nRaton\n50\n1\n", 2, 100, 10);
        yield return InvoiceCase("Tres productos con varias unidades", "3\nCable\n3\n2\nMonitor\n70\n3\nFunda\n4\n2\n", 3, 224, 33.6);
    }

    private static Scenario InvoiceCase(string name, string input, int products, double amount, double discount) =>
        new(name, input, output => Summary(output,
            new Field(@"productos\s*(?:comprados|diferentes|totales)|numero\s*(?:de\s*)?productos", products, "productos comprados"),
            new Field(@"importe\s*(?:sin\s*descuento|bruto)|subtotal\s*(?:compra|final|total)?", amount, "importe sin descuento"),
            new Field(@"descuento(?:\s*aplicado)?", discount, "importe del descuento"),
            new Field(@"(?<!sub)\btotal(?:\s*(?:a\s*pagar|final))?", amount - discount, "total de la factura")));

    private static IEnumerable<Scenario> Parking()
    {
        yield return ParkingCase("Secuencia del libro con entradas y salidas rechazadas", "1\n10\n1\n7\n1\n5\n3\n2\n4\n2\n20\n1\n3\n3\n4\n", 16, true);
        yield return ParkingCase("Vacío, lleno a veinte vehículos y vaciado completo", "3\n1\n20\n3\n1\n1\n2\n20\n3\n4\n", 0, true, true, true);
        yield return ParkingCase("Cantidades no positivas conservan el estado", "1\n0\n1\n-2\n2\n0\n2\n-2\n9\n4\n", 0, false, false, false, true);
        yield return ParkingCase("No pueden salir vehículos de un aparcamiento vacío", "2\n1\n4\n", 0, true);
        yield return ParkingCase("Operaciones alternadas y consulta sin modificar estado", "1\n6\n2\n2\n3\n1\n7\n2\n3\n3\n4\n", 8);
    }

    private static Scenario ParkingCase(string name, string input, int vehicles, bool rejected = false,
        bool empty = false, bool full = false, bool invalid = false) => new(name, input, output =>
    {
        if (rejected && !Has(output, "no hay suficientes", "solamente hay", "insuficiente", "no pueden salir", "no hay vehiculos", "no hay plazas", "supera"))
            return "Debe avisar de la operación que no puede realizarse.";
        if (empty && !Has(output, "vacio"))
            return "La consulta del aparcamiento vacío debe indicar que está vacío.";
        if (full && !Has(output, "completo", "lleno"))
            return "La consulta de las veinte plazas ocupadas debe indicar que está completo.";
        if (invalid && (!InvalidAmount(output) || !InvalidOption(output)))
            return "Deben rechazarse cantidades no positivas y opciones inexistentes.";
        return Summary(output,
            new Field(@"vehiculos\s*(?:actualmente\s*)?(?:aparcados|restantes|al\s*cierre)|vehiculos\s*finales", vehicles, "vehículos al cierre"),
            new Field(@"plazas\s*(?:disponibles|libres|restantes)", 20 - vehicles, "plazas disponibles al cierre"));
    });

    private static IEnumerable<Scenario> Tickets()
    {
        yield return TicketCase("Cerrar sin vender entradas", "0\n", 0, 0);
        yield return TicketCase("Cuatro entradas sin descuento", "4\n0\n", 4, 48);
        yield return TicketCase("Cinco entradas con descuento del diez por ciento", "5\n0\n", 5, 54);
        yield return TicketCase("Vender todas las entradas y finalizar automáticamente", "50\n", 50, 540);
        yield return TicketCase("Rechazar negativo y exceso antes de acumular ventas", "-1\n51\n4\n5\n0\n", 9, 102, true);
        yield return TicketCase("Agotar entradas con compras pequeñas y grandes", "3\n7\n40\n", 50, 543.6);
    }

    private static Scenario TicketCase(string name, string input, int sold, double income, bool rejected = false) =>
        new(name, input, output =>
        {
            if (Has(output, "entradas agotadas", "entradas se han agotado") != (sold == 50))
                return "Debe anunciarse ENTRADAS AGOTADAS únicamente cuando se han vendido las cincuenta entradas.";
            if (rejected && !Has(output, "no hay suficientes", "insuficientes", "no valido", "negativ", "error"))
                return "Las solicitudes negativas o superiores a las entradas disponibles deben rechazarse.";
            return Summary(output,
                new Field(@"entradas\s*(?:vendidas|compradas)|total\s*(?:de\s*)?entradas\s*vendidas", sold, "entradas vendidas"),
                new Field(@"entradas\s*(?:restantes|disponibles|sin\s*vender)", 50 - sold, "entradas restantes"),
                new Field(@"recaudacion(?:\s*total)?|ingresos(?:\s*totales)?", income, "recaudación total"));
        });

    private static IEnumerable<Scenario> Tables()
    {
        foreach (int last in new[] { 1, 3, 5 })
            yield return new($"Todas las multiplicaciones de las tablas 1 a {last}", $"{last}\n", output =>
            {
                var rows = Regex.Matches(Fold(output), @"(\d+)\s*(?:x|\*|×)\s*(\d+)\s*=\s*(\d+)")
                    .Select(m => new[] { int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value) }).ToArray();
                if (rows.Length != last * 10)
                    return $"Deben mostrarse {last * 10} multiplicaciones, diez por tabla.";
                for (int index = 0; index < rows.Length; index++)
                {
                    int table = index / 10 + 1, factor = index % 10 + 1;
                    if (!rows[index].SequenceEqual(new[] { table, factor, table * factor }))
                        return $"Falta o es incorrecta la multiplicación {table} × {factor} = {table * factor}.";
                }
                return null;
            });
        foreach (int invalid in new[] { 0, -2 })
            yield return new($"No generar tablas para {invalid}", $"{invalid}\n", output => Validate(
                (Has(output, "error", "no valido", "mayor", "incorrect"), "Debe avisar de que el límite de tablas no es válido."),
                (!Regex.IsMatch(Fold(output), @"\d+\s*(?:x|\*|×)\s*\d+\s*="), "No deben mostrarse tablas con un límite menor que uno.")));
    }

    private static IEnumerable<Scenario> Figures()
    {
        foreach (int height in new[] { 1, 4, 7 })
        foreach (int choice in new[] { 1, 2, 3 })
        {
            int[] expected = choice == 1 ? Enumerable.Range(1, height).ToArray()
                : choice == 2 ? Enumerable.Range(1, height).Reverse().ToArray()
                : Enumerable.Repeat(height, height).ToArray();
            yield return new($"Figura {choice} de altura {height}", $"{height}\n{choice}\n", output =>
            {
                // El libro primero dibuja un triángulo y después pregunta la figura. También se admite preguntar antes.
                int[] rows = output.Replace("\r", "").Split('\n')
                    .Select(line => Regex.Match(line, @"(\*+(?:\s*\*)*)\s*$"))
                    .Where(match => match.Success).Select(match => match.Groups[1].Value.Count(c => c == '*')).ToArray();
                int[] introductoryTriangle = Enumerable.Range(1, height).ToArray();
                if (rows.SequenceEqual(expected) || rows.SequenceEqual(introductoryTriangle.Concat(expected)))
                    return null;
                return $"La figura debe tener {height} filas con estas cantidades de asteriscos: {string.Join(", ", expected)}. Se admite el triángulo creciente inicial del enunciado.";
            });
        }
    }

    private static IEnumerable<Scenario> Grades()
    {
        yield return GradeCase("Notas inválidas repiten alumno y límites de calificación", "5\n-1\n11\n0\n5\n7\n9\n10\n", 6.2, 4, 1, 10, 0, true,
            new[] { "suspenso", "aprobado", "notable", "sobresaliente" });
        yield return GradeCase("Una sola nota y extremos inicializados correctamente", "1\n6\n", 6, 1, 0, 6, 6);
        yield return GradeCase("Media de suspenso y notable", "2\n4\n8\n", 6, 1, 1, 8, 4);
        yield return GradeCase("Todos suspenden sin aprobar ninguna nota", "3\n1\n2\n3\n", 2, 0, 3, 3, 1);
        foreach (var boundary in new[] { (4, "suspenso"), (5, "aprobado"), (7, "notable"), (9, "sobresaliente"), (10, "sobresaliente") })
            yield return GradeCase($"Calificación de la nota {boundary.Item1}", $"1\n{boundary.Item1}\n", boundary.Item1,
                boundary.Item1 >= 5 ? 1 : 0, boundary.Item1 < 5 ? 1 : 0, boundary.Item1, boundary.Item1,
                classifications: new[] { boundary.Item2 });
    }

    private static Scenario GradeCase(string name, string input, double average, int pass, int fail, double highest, double lowest,
        bool invalid = false, string[]? classifications = null) => new(name, input, output =>
    {
        if (invalid && !Has(output, "nota no valida", "nota incorrecta", "nota invalida", "entre 0 y 10"))
            return "Una nota fuera de 0–10 debe rechazarse y volver a pedirse para el mismo alumno.";
        if (classifications is not null && classifications.Any(c => !Regex.IsMatch(Words(output), @"\b" + Regex.Escape(c) + @"\b")))
            return "Deben mostrarse las calificaciones Suspenso, Aprobado, Notable y Sobresaliente en sus tramos correctos.";
        return Summary(output,
            new Field(@"nota\s*media|media(?:\s*(?:de\s*)?notas)?|promedio", average, "nota media"),
            new Field(@"aprobados|alumnos\s*(?:que\s*)?aprueban", pass, "aprobados"),
            new Field(@"suspensos|suspendidos|alumnos\s*(?:que\s*)?suspenden", fail, "suspensos"),
            new Field(@"nota\s*(?:mas\s*alta|maxima)|maxima(?:\s*nota)?", highest, "nota más alta"),
            new Field(@"nota\s*(?:mas\s*baja|minima)|minima(?:\s*nota)?", lowest, "nota más baja"));
    });

    private static IEnumerable<Scenario> Hotel()
    {
        yield return HotelCase("Dos plantas y reintento de la misma habitación", "2\n2\nx\ns\nn\ns\ns\n", 4, 3, true);
        yield return HotelCase("Todas las habitaciones libres", "1\n3\nN\nN\nN\n", 3, 0);
        yield return HotelCase("Ocupación de un tercio con división decimal", "1\n3\nS\nN\nN\n", 3, 1);
        yield return HotelCase("Tres plantas completamente ocupadas", "3\n2\nS\nS\nS\nS\nS\nS\n", 6, 6);
    }

    private static Scenario HotelCase(string name, string input, int rooms, int occupied, bool invalid = false) =>
        new(name, input, output =>
        {
            if (invalid && !Has(output, "error", "no valid", "incorrect", "introduce s o n"))
                return "Una respuesta distinta de S/N debe rechazarse sin pasar a otra habitación.";
            return Summary(output,
                new Field(@"habitaciones\s*(?:totales|total)|total\s*(?:de\s*)?habitaciones", rooms, "habitaciones totales"),
                new Field(@"(?:habitaciones\s*)?ocupadas", occupied, "habitaciones ocupadas"),
                new Field(@"(?:habitaciones\s*)?libres", rooms - occupied, "habitaciones libres"),
                new Field(@"porcentaje(?:\s*de\s*)?(?:ocupacion)?|ocupacion\s*(?:porcentual|total)", occupied * 100.0 / rooms, "porcentaje de ocupación"));
        });

    private static IEnumerable<Scenario> Championship()
    {
        yield return ChampionshipCase("Repetir puntuación negativa y actualizar mejor jugador", "2\n3\nAne\n-1\n120\n85\n140\nMikel\n100\n100\n200\n",
            new[] { ("Ane", 345, 115.0, 2), ("Mikel", 400, 400.0 / 3, 3) }, "Mikel", 400, true);
        yield return ChampionshipCase("Cero válido y umbral inclusivo de cien puntos", "1\n3\nAna\n0\n99\n100\n",
            new[] { ("Ana", 199, 199.0 / 3, 1) }, "Ana", 199);
        yield return ChampionshipCase("El primer jugador puede seguir siendo el mejor", "2\n2\nNora\n150\n160\nBeni\n10\n20\n",
            new[] { ("Nora", 310, 155.0, 2), ("Beni", 30, 15.0, 0) }, "Nora", 310);
        yield return ChampionshipCase("Reiniciar total y contador de partidas en cada jugador", "3\n1\nLara\n80\nIvan\n100\nSara\n101\n",
            new[] { ("Lara", 80, 80.0, 0), ("Ivan", 100, 100.0, 1), ("Sara", 101, 101.0, 1) }, "Sara", 101);
    }

    private static Scenario ChampionshipCase(string name, string input, (string Name, int Total, double Average, int Hundred)[] players,
        string winner, int best, bool invalid = false) => new(name, input, output =>
    {
        if (invalid && !Has(output, "error", "no negativo", "negativ", "no valid", "incorrect"))
            return "Una puntuación negativa debe rechazarse y volver a pedirse para la misma partida.";
        string folded = Fold(output);
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            string block = PlayerBlock(folded, player.Name, i + 1 < players.Length ? players[i + 1].Name : null);
            if (block.Length == 0)
                return $"Debe mostrarse el resultado del jugador {player.Name}.";
            string? failure = Summary(block,
                new Field(@"puntos\s*(?:totales|total)|puntuacion\s*total|total\s*(?:de\s*)?puntos", player.Total, $"puntos de {player.Name}"),
                new Field(@"media|promedio", player.Average, $"media de {player.Name}"),
                new Field(@"partidas\s*(?:con\s*)?100\s*(?:puntos\s*)?(?:o\s*mas)?|partidas\s*(?:destacadas|superiores)", player.Hundred, $"partidas con 100 o más de {player.Name}"));
            if (failure is not null)
                return failure;
        }
        string end = LastSection(folded, @"campeonato\s*finalizado|mejor\s*jugador|ganador");
        if (!Has(end, winner))
            return $"El mejor jugador debe ser {winner}.";
        return Summary(end, new Field(@"puntuacion\s*total|puntos\s*totales|mejor\s*puntuacion", best, "puntuación del ganador"));
    });

    private static IEnumerable<Scenario> Restaurant()
    {
        yield return RestaurantCase("Pedido del ejemplo con varios productos", "Ane\n1\n2\n5\n2\n6\nN\n",
            new[] { ("Ane", 4, 24.0, 1.2) }, 22.8);
        yield return RestaurantCase("Un producto y ninguna rebaja", "Ana\n3\n1\n6\nN\n",
            new[] { ("Ana", 1, 8.0, 0.0) }, 8);
        yield return RestaurantCase("Opciones y cantidades inválidas conservan el pedido", "Ane\n1\n0\n3\n-2\n9\n2\n1\n6\nX\nN\n",
            new[] { ("Ane", 1, 12.0, 0.0) }, 12, true);
        yield return RestaurantCase("Rechazar cero unidades antes de otro producto", "Zoe\n1\n0\n3\n1\n6\nN\n",
            new[] { ("Zoe", 1, 8.0, 0.0) }, 8, rejectedAmount: true);
        yield return RestaurantCase("Rechazar unidades negativas antes de otro producto", "Bruno\n2\n-1\n5\n1\n6\nN\n",
            new[] { ("Bruno", 1, 2.5, 0.0) }, 2.5, rejectedAmount: true);
        var boundaries = new[]
        {
            ("Antes de 20", "2\n1\n4\n1\n", 2, 19.5, 0.0),
            ("20 exactos", "5\n8\n", 8, 20.0, 1.0),
            ("Antes de 40", "1\n1\n4\n4\n", 5, 39.5, 1.975),
            ("40 exactos", "3\n5\n", 5, 40.0, 4.0),
            ("Antes de 60", "1\n1\n5\n20\n", 21, 59.5, 5.95),
            ("60 exactos", "2\n5\n", 5, 60.0, 9.0)
        };
        foreach (var boundary in boundaries)
            yield return RestaurantCase($"Descuento: {boundary.Item1}", "Cliente\n" + boundary.Item2 + "6\nN\n",
                new[] { ("Cliente", boundary.Item3, boundary.Item4, boundary.Item5) }, boundary.Item4 - boundary.Item5);
        yield return RestaurantCase("Reiniciar cada pedido y acumular dos clientes", "Ane\n1\n2\n5\n2\n6\ns\nMikel\n4\n1\n6\nn\n",
            new[] { ("Ane", 4, 24.0, 1.2), ("Mikel", 1, 7.5, 0.0) }, 30.3);
        yield return RestaurantCase("Repetir respuesta distinta de S/N y atender al siguiente cliente", "Ada\n5\n2\n6\nX\ns\nLuis\n3\n1\n6\nn\n",
            new[] { ("Ada", 2, 5.0, 0.0), ("Luis", 1, 8.0, 0.0) }, 13);
        yield return RestaurantCase("Tres clientes y todos los precios de la carta", "Lara\n1\n1\n2\n1\n6\nS\nIvan\n3\n1\n4\n1\n6\nS\nSara\n5\n2\n6\nN\n",
            new[] { ("Lara", 2, 21.5, 1.075), ("Ivan", 2, 15.5, 0.0), ("Sara", 2, 5.0, 0.0) }, 40.925);
    }

    private static Scenario RestaurantCase(string name, string input, (string Name, int Units, double Subtotal, double Discount)[] orders,
        double income, bool invalid = false, bool rejectedAmount = false) => new(name, input, output =>
    {
        if ((invalid || rejectedAmount) && !InvalidAmount(output))
            return "Las cantidades no positivas deben rechazarse sin añadir productos.";
        if (invalid && !InvalidOption(output))
            return "Las cantidades no positivas y las opciones inexistentes deben rechazarse sin añadir productos.";
        string folded = Fold(output);
        for (int i = 0; i < orders.Length; i++)
        {
            var order = orders[i];
            string block = TicketBlock(folded, order.Name, i + 1 < orders.Length ? orders[i + 1].Name : null);
            if (block.Length == 0)
                return $"Debe mostrarse el ticket del cliente {order.Name}.";
            string? failure = Summary(block,
                new Field(@"unidades\s*(?:pedidas|totales)?|cantidad\s*(?:total\s*)?(?:de\s*)?unidades", order.Units, $"unidades de {order.Name}"),
                new Field(@"subtotal|importe\s*sin\s*descuento", order.Subtotal, $"subtotal de {order.Name}"),
                new Field(@"descuento(?!\s*aplicado)", order.Discount, $"descuento de {order.Name}"),
                new Field(@"(?<!sub)\btotal(?:\s*(?:a\s*pagar|final))?", order.Subtotal - order.Discount, $"total de {order.Name}"));
            if (failure is not null)
                return failure;
        }
        string end = LastSection(folded, @"cierre\s*(?:del\s*)?restaurante|pedidos\s*realizados");
        return Summary(end,
            new Field(@"pedidos\s*(?:realizados|totales)|numero\s*(?:de\s*)?pedidos", orders.Length, "pedidos de la jornada"),
            new Field(@"recaudacion(?:\s*total)?|ingresos(?:\s*totales)?", income, "recaudación total"));
    });

    private static string Input(IEnumerable<string> values) => string.Join('\n', values) + "\n";

    private static string? Summary(string output, params Field[] fields)
    {
        bool allRecognized = true;
        foreach (var field in fields)
        {
            var values = Values(output, field.Pattern);
            if (values.Count == 0)
            {
                allRecognized = false;
                continue;
            }
            if (Math.Abs(values[^1] - field.Value) > field.Tolerance)
                return $"{field.Description}: se esperaba {field.Value.ToString("0.##", CultureInfo.InvariantCulture)} y se obtuvo {values[^1].ToString("0.##", CultureInfo.InvariantCulture)}.";
        }
        if (allRecognized)
            return null;

        // Etiquetas distintas también son válidas si el tramo final contiene el resumen numérico esperado.
        var tail = OutputChecks.Numbers(output).TakeLast(fields.Length + 2).ToArray();
        int next = 0;
        foreach (double actual in tail)
            if (Math.Abs(actual - fields[next].Value) <= fields[next].Tolerance && ++next == fields.Length)
                return null;
        return "No se encuentra un resumen correcto. Se esperaba: " + string.Join("; ", fields.Select(f =>
            $"{f.Description} = {f.Value.ToString("0.##", CultureInfo.InvariantCulture)}")) + ".";
    }

    private static IReadOnlyList<double> Values(string output, string pattern)
    {
        const string number = @"(?<number>-?\d+(?:[.,]\d+)?)";
        return Regex.Matches(Fold(output), @"(?:" + pattern + @")[^\d\r\n+\-]{0,45}" + number)
            .Select(m => double.Parse(m.Groups["number"].Value.Replace(',', '.'), CultureInfo.InvariantCulture)).ToArray();
    }

    private static IReadOnlyList<(int Index, double Value)> Pairs(string output, string label) =>
        Regex.Matches(Fold(output), @"\b" + label + @"\s*[:=]?\s*(\d+)[^\d\r\n]*(-?\d+(?:[.,]\d+)?)")
            .Select(m => (int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                double.Parse(m.Groups[2].Value.Replace(',', '.'), CultureInfo.InvariantCulture))).ToArray();

    private static bool EqualNumbers(IReadOnlyList<double> actual, IReadOnlyList<double> expected) =>
        actual.Count == expected.Count && actual.Zip(expected).All(pair => Math.Abs(pair.First - pair.Second) <= .011);

    private static string BetweenTurns(string output, int first, int next)
    {
        string folded = Fold(output);
        var start = Regex.Match(folded, @"\bvuelta\s*[:=]?\s*" + first + @"\b");
        if (!start.Success) return "";
        var end = Regex.Match(folded[(start.Index + start.Length)..], @"\bvuelta\s*[:=]?\s*" + next + @"\b");
        return end.Success ? folded.Substring(start.Index, start.Length + end.Index) : folded[start.Index..];
    }

    private static string PlayerBlock(string output, string name, string? nextName)
    {
        var matches = Regex.Matches(output, @"\b" + Regex.Escape(Fold(name)) + @"\b");
        if (matches.Count == 0) return "";
        int start = matches[0].Index;
        int end = output.Length;
        if (nextName is not null)
        {
            var next = Regex.Match(output[(start + matches[0].Length)..], @"\b" + Regex.Escape(Fold(nextName)) + @"\b");
            if (next.Success) end = start + matches[0].Length + next.Index;
        }
        else
        {
            var final = Regex.Match(output[start..], @"campeonato\s*finalizado|mejor\s*jugador");
            if (final.Success) end = start + final.Index;
        }
        return output[start..end];
    }

    private static string TicketBlock(string output, string name, string? nextName)
    {
        var match = Regex.Match(output, @"cliente\s*[:=]?\s*" + Regex.Escape(Fold(name)) + @"\b");
        if (!match.Success)
            match = Regex.Match(output, @"\b" + Regex.Escape(Fold(name)) + @"\b");
        if (!match.Success) return "";
        int start = match.Index;
        var terminator = Regex.Match(output[(start + match.Length)..], @"(?:deseas|quieres|registrar)\s*(?:registrar\s*)?(?:otro|mas)\s*pedido|cierre\s*(?:del\s*)?restaurante");
        int end = terminator.Success ? start + match.Length + terminator.Index : output.Length;
        if (nextName is not null)
        {
            var next = Regex.Match(output[(start + match.Length)..], @"cliente\s*[:=]?\s*" + Regex.Escape(Fold(nextName)) + @"\b");
            if (next.Success) end = Math.Min(end, start + match.Length + next.Index);
        }
        return output[start..end];
    }

    private static string LastSection(string output, string heading)
    {
        var matches = Regex.Matches(output, heading);
        return matches.Count == 0 ? output : output[matches[0].Index..];
    }

    private static string Fold(string value)
    {
        string decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (char character in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(character));
        return builder.ToString().Normalize(NormalizationForm.FormC).Replace("\r", "");
    }

    private static string Words(string value) => Regex.Replace(Regex.Replace(Fold(value), @"[^\p{L}\p{N}]+", " "), @"\s+", " ").Trim();
    private static bool Has(string output, params string[] alternatives)
    {
        string words = Words(output);
        return alternatives.Any(alternative => words.Contains(Words(alternative), StringComparison.Ordinal));
    }
    private static int Count(string output, string part) => Regex.Matches(Fold(output), Regex.Escape(Fold(part))).Count;
    private static bool InvalidOption(string output) => Has(output, "opcion no valida", "opcion incorrecta", "opcion invalida", "opcion no es valida", "opcion erronea");
    private static bool InvalidAction(string output) => Has(output, "accion no valida", "accion incorrecta", "accion invalida", "opcion no valida", "opcion incorrecta");
    private static bool InvalidAmount(string output) => Has(output, "cantidad no valida", "cantidad incorrecta", "cantidad invalida", "mayor que 0", "mayor que cero", "cantidad debe ser positiva");
    private static bool Closing(string output) => Has(output, "cerrando", "programa finalizado", "programa cerrado", "hasta pronto", "hasta luego", "adios");
    private static string? Require(bool condition, string failure) => condition ? null : failure;
    private static string? Validate(params (bool Condition, string Failure)[] checks) => checks.FirstOrDefault(check => !check.Condition).Failure;
}
