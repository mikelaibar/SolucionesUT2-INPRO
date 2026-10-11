using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Corrector.Core;

internal sealed record ConditionalRole(string Name, string Kind, object InitialValue, params string[] Aliases);

/// <summary>Identifies input declarations without assuming that the student copied our variable names.</summary>
internal sealed class ConditionalSourceAnalysis
{
    private readonly CompilationUnitSyntax root;
    private readonly Dictionary<string, VariableDeclaratorSyntax> bindings = new(StringComparer.OrdinalIgnoreCase);
    public string? Error { get; private set; }
    public IReadOnlyList<string> Notes => notes;
    private readonly List<string> notes = [];
    public const string ProbePrefix = "__CORRECTOR_SALDO__:";

    public ConditionalSourceAnalysis(string source, IReadOnlyList<ConditionalRole> roles,
        IReadOnlyDictionary<string, string>? variableMap)
    {
        root = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
        var candidates = root.DescendantNodes().OfType<VariableDeclaratorSyntax>()
            .Where(IsUsableDeclaration).OrderBy(x => x.SpanStart).ToList();
        var used = new HashSet<int>();

        foreach (var role in roles)
        {
            var explicitName = variableMap?.FirstOrDefault(x => NormalizeName(x.Key) == NormalizeName(role.Name)).Value;
            if (explicitName is not null)
            {
                var matches = candidates.Where(x => x.Identifier.ValueText == explicitName).ToList();
                if (matches.Count != 1 || used.Contains(matches[0].SpanStart))
                {
                    Error = $"El mapa {role.Name}={explicitName} no identifica una declaración inicial única y disponible.";
                    return;
                }
                Bind(role, matches[0], used);
                continue;
            }
            var canonical = candidates.Where(x => !used.Contains(x.SpanStart)
                && NormalizeName(x.Identifier.ValueText) == NormalizeName(role.Name)).ToList();
            var aliases = role.Aliases.Select(NormalizeName).ToHashSet();
            var byName = canonical.Count > 0 ? canonical
                : candidates.Where(x => !used.Contains(x.SpanStart) && aliases.Contains(NormalizeName(x.Identifier.ValueText))).ToList();
            if (byName.Count == 1) Bind(role, byName[0], used);
            else if (byName.Count > 1)
            {
                Error = MappingHelp(role.Name, byName);
                return;
            }
        }

        // Only infer from a default value when it uniquely identifies both the declaration and its role.
        bool changed;
        do
        {
            changed = false;
            var pending = roles.Where(x => !bindings.ContainsKey(x.Name)).ToList();
            foreach (var role in pending)
            {
                var matches = candidates.Where(x => !used.Contains(x.SpanStart) && HasDefault(x, role)).ToList();
                if (matches.Count != 1) continue;
                if (pending.Count(other => HasDefault(matches[0], other)) != 1) continue;
                Bind(role, matches[0], used);
                notes.Add($"{role.Name} se ha asociado a {matches[0].Identifier.ValueText} por su tipo y valor inicial.");
                changed = true;
            }
        } while (changed);

        foreach (var role in roles.Where(x => !bindings.ContainsKey(x.Name)))
        {
            var matches = candidates.Where(x => !used.Contains(x.SpanStart) && HasDefault(x, role)).ToList();
            Error = MappingHelp(role.Name, matches);
            return;
        }
    }

    private static string MappingHelp(string role, IReadOnlyList<VariableDeclaratorSyntax> matches)
    {
        var details = matches.Count == 0 ? "No se ha localizado una declaración inicial compatible."
            : "Hay varias declaraciones compatibles: " + string.Join(", ", matches.Select(x => x.Identifier.ValueText)) + ".";
        return $"No se puede identificar con certeza el dato «{role}». {details} Indica el mapa de variables {role}=nombreDeTuVariable; también puedes conservar los nombres del enunciado.";
    }

    private void Bind(ConditionalRole role, VariableDeclaratorSyntax node, HashSet<int> used)
    {
        bindings[role.Name] = node;
        used.Add(node.SpanStart);
    }

    public string WithValues(IReadOnlyDictionary<string, object> values, bool probeSaldo = false)
    {
        var mutations = new Dictionary<int, ExpressionSyntax>();
        foreach (var (role, value) in values)
        {
            if (!bindings.TryGetValue(role, out var node)) throw new InvalidOperationException($"Dato no asociado: {role}");
            mutations[node.SpanStart] = Literal(value, node);
        }
        var updated = (CompilationUnitSyntax)new InitializerRewriter(mutations).Visit(root)!;
        if (probeSaldo && bindings.TryGetValue("saldo", out var saldo))
            updated = AddProbe(updated, saldo);
        return updated.ToFullString();
    }

    private static CompilationUnitSyntax AddProbe(CompilationUnitSyntax updated, VariableDeclaratorSyntax originalSaldo)
    {
        var identifier = originalSaldo.Identifier.Text;
        StatementSyntax probe = SyntaxFactory.ParseStatement(
            $"global::System.Console.WriteLine(\"\\n{ProbePrefix}\" + global::System.Convert.ToString({identifier}, global::System.Globalization.CultureInfo.InvariantCulture));\n");
        var originalMethod = originalSaldo.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        var hasGlobals = updated.Members.OfType<GlobalStatementSyntax>().Any();
        if (originalMethod is not null || !hasGlobals)
        {
            var method = updated.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == "Main" && m.Body is not null);
            if (method?.Body is null) return updated;
            var block = (BlockSyntax)new ExitProbeRewriter(probe).Visit(method.Body)!;
            var newMethod = method.WithBody(block.AddStatements(probe));
            return updated.ReplaceNode(method, newMethod);
        }
        updated = (CompilationUnitSyntax)new ExitProbeRewriter(probe, topLevel: true).Visit(updated)!;
        var globals = updated.Members.OfType<GlobalStatementSyntax>().ToArray();
        var index = updated.Members.IndexOf(globals[^1]) + 1;
        return updated.WithMembers(updated.Members.Insert(index, SyntaxFactory.GlobalStatement(probe)));
    }

    private static bool IsUsableDeclaration(VariableDeclaratorSyntax node)
    {
        if (node.Initializer is null) return false;
        if (node.Parent?.Parent is FieldDeclarationSyntax field)
            return field.Modifiers.Any(SyntaxKind.StaticKeyword) || field.Modifiers.Any(SyntaxKind.ConstKeyword);
        if (node.Parent?.Parent is not LocalDeclarationStatementSyntax statement) return false;
        if (statement.Parent is GlobalStatementSyntax) return true;
        return statement.Parent is BlockSyntax block && block.Parent is MethodDeclarationSyntax method
            && method.Identifier.ValueText == "Main";
    }

    private static bool HasDefault(VariableDeclaratorSyntax candidate, ConditionalRole role)
    {
        if (!TryConstant(candidate.Initializer!.Value, out var kind, out var value) || kind != role.Kind) return false;
        if (kind == "number")
            return Math.Abs(Convert.ToDouble(value, CultureInfo.InvariantCulture) - Convert.ToDouble(role.InitialValue, CultureInfo.InvariantCulture)) < 0.0000001;
        return Equals(value, role.InitialValue);
    }

    private static bool TryConstant(ExpressionSyntax expression, out string kind, out object? value)
    {
        while (expression is ParenthesizedExpressionSyntax parentheses) expression = parentheses.Expression;
        if (expression is PrefixUnaryExpressionSyntax unary && (unary.IsKind(SyntaxKind.UnaryMinusExpression) || unary.IsKind(SyntaxKind.UnaryPlusExpression))
            && TryConstant(unary.Operand, out kind, out value) && kind == "number")
        {
            value = Convert.ToDouble(value, CultureInfo.InvariantCulture) * (unary.IsKind(SyntaxKind.UnaryMinusExpression) ? -1 : 1);
            return true;
        }
        if (expression is LiteralExpressionSyntax literal)
        {
            value = literal.Token.Value;
            kind = literal.Kind() switch
            {
                SyntaxKind.NumericLiteralExpression => "number",
                SyntaxKind.StringLiteralExpression => "string",
                SyntaxKind.CharacterLiteralExpression => "char",
                SyntaxKind.TrueLiteralExpression or SyntaxKind.FalseLiteralExpression => "bool",
                _ => "unknown"
            };
            return kind != "unknown";
        }
        kind = "unknown";
        value = null;
        return false;
    }

    private static ExpressionSyntax Literal(object value, VariableDeclaratorSyntax declaration)
    {
        if (value is string text) return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(text));
        if (value is char character) return SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(character));
        if (value is bool boolean) return SyntaxFactory.LiteralExpression(boolean ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
        var number = Convert.ToString(value, CultureInfo.InvariantCulture)!;
        var declaredType = (declaration.Parent as VariableDeclarationSyntax)?.Type.ToString();
        var originalNumber = declaration.Initializer!.Value.DescendantNodesAndSelf().OfType<LiteralExpressionSyntax>()
            .FirstOrDefault(x => x.IsKind(SyntaxKind.NumericLiteralExpression))?.Token.Value;
        // Keep inferred var types and allow equivalent float/decimal solutions; changing var's type would change division semantics.
        var suffix = declaredType switch
        {
            "decimal" or "System.Decimal" or "global::System.Decimal" => "m",
            "float" or "System.Single" or "global::System.Single" => "f",
            "var" when originalNumber is decimal => "m",
            "var" when originalNumber is float => "f",
            "var" when originalNumber is double => "d",
            "var" when originalNumber is long => "L",
            "var" when originalNumber is ulong => "UL",
            "var" when originalNumber is uint => "U",
            _ => ""
        };
        return SyntaxFactory.ParseExpression(number + suffix);
    }

    private static string NormalizeName(string value) => new(value.Normalize(NormalizationForm.FormD)
        .Where(x => char.IsLetterOrDigit(x) && CharUnicodeInfo.GetUnicodeCategory(x) != UnicodeCategory.NonSpacingMark)
        .Select(char.ToLowerInvariant).ToArray());

    private sealed class InitializerRewriter(Dictionary<int, ExpressionSyntax> replacements) : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitVariableDeclarator(VariableDeclaratorSyntax node) =>
            replacements.TryGetValue(node.SpanStart, out var expression)
                ? node.WithInitializer(node.Initializer!.WithValue(expression.WithTriviaFrom(node.Initializer.Value)))
                : base.VisitVariableDeclarator(node);
    }

    private sealed class ExitProbeRewriter(StatementSyntax probe, bool topLevel = false) : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node)
        {
            if (node.Ancestors().Any(x => x is LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax)) return node;
            if (topLevel && node.Ancestors().Any(x => x is MethodDeclarationSyntax)) return node;
            return SyntaxFactory.Block(probe, node).WithTriviaFrom(node);
        }
        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node) => topLevel ? node : base.VisitMethodDeclaration(node);
    }
}
