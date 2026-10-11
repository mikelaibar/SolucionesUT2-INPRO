using System.Globalization;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Corrector.Core;

/// <summary>Análisis e instrumentación de las fichas y operaciones de los ejercicios 1–20.</summary>
public sealed class EarlySourceAnalysis
{
    public const string ValuesMarker = "__CORRECTOR_VALORES__";
    public const string ShownMarker = "__CORRECTOR_MOSTRADOS__";
    private static readonly Lazy<MetadataReference[]> ReferencesCache = new(() =>
        ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "")
        .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p)).ToArray());
    private readonly CompilationUnitSyntax root;
    private readonly SemanticModel semantic;
    private readonly MethodDeclarationSyntax? main;
    private readonly Dictionary<ISymbol, EarlyVariable> bySymbol = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<string, HashSet<string>> dependencies = new();

    public IReadOnlyList<EarlyVariable> Variables { get; }

    public EarlySourceAnalysis(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        root = tree.GetCompilationUnitRoot();
        var imports = CSharpSyntaxTree.ParseText("global using System; global using System.Collections.Generic; global using System.Linq; global using System.Threading.Tasks;");
        var compilation = CSharpCompilation.Create("Inspection", new[] { tree, imports }, ReferencesCache.Value, new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        semantic = compilation.GetSemanticModel(tree);
        main = root.Members.OfType<GlobalStatementSyntax>().Any() ? null : root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(m => m.Identifier.ValueText == "Main" && m.Body != null);
        var declarations = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Where(IsMainVariable).ToArray();
        var variables = new List<EarlyVariable>();
        foreach (var declaration in declarations)
        {
            if (semantic.GetDeclaredSymbol(declaration) is not ILocalSymbol symbol) continue;
            var type = symbol.Type;
            var nullable = type is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
            if (nullable) type = ((INamedTypeSymbol)type).TypeArguments[0];
            var kind = type.SpecialType switch
            {
                SpecialType.System_Int32 => "int",
                SpecialType.System_Double or SpecialType.System_Single or SpecialType.System_Decimal => "real",
                SpecialType.System_Boolean => "bool",
                SpecialType.System_Char => "char",
                SpecialType.System_String => "string",
                _ => "other"
            };
            var constant = declaration.Initializer == null ? default : semantic.GetConstantValue(declaration.Initializer.Value);
            var variable = new EarlyVariable("v" + variables.Count, symbol.Name, kind, nullable, symbol.Type.ToDisplayString(), constant.HasValue, constant.HasValue ? constant.Value : null, declaration);
            variables.Add(variable);
            bySymbol[symbol] = variable;
        }
        Variables = variables;
        foreach (var variable in variables)
        {
            dependencies[variable.Id] = variable.Declaration.Initializer == null ? new() : References(variable.Declaration.Initializer.Value);
        }
        foreach (var assignment in root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            var symbol = semantic.GetSymbolInfo(assignment.Left).Symbol;
            if (symbol != null && bySymbol.TryGetValue(symbol, out var variable)) dependencies[variable.Id].UnionWith(References(assignment.Right));
        }
    }

    private bool IsMainVariable(VariableDeclaratorSyntax declaration)
    {
        if (declaration.Parent?.Parent is not LocalDeclarationStatementSyntax statement) return false;
        if (statement.Parent is GlobalStatementSyntax) return true;
        return main?.Body != null && statement.Parent == main.Body;
    }

    private HashSet<string> References(SyntaxNode node)
    {
        var ids = new HashSet<string>();
        foreach (var identifier in node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            var symbol = semantic.GetSymbolInfo(identifier).Symbol;
            if (symbol != null && bySymbol.TryGetValue(symbol, out var variable)) ids.Add(variable.Id);
        }
        return ids;
    }

    private HashSet<string> Expand(HashSet<string> ids)
    {
        var pending = new Queue<string>(ids);
        while (pending.TryDequeue(out var id))
            if (dependencies.TryGetValue(id, out var values))
                foreach (var value in values) if (ids.Add(value)) pending.Enqueue(value);
        return ids;
    }

    public string WithInitialValues(IReadOnlyDictionary<string, object?> values)
    {
        var nodes = Variables.Where(v => values.ContainsKey(v.Id) && v.Declaration.Initializer != null).Select(v => v.Declaration).ToArray();
        return root.ReplaceNodes(nodes, (old, _) =>
        {
            var variable = Variables.First(v => v.Declaration == old);
            return old.WithInitializer(SyntaxFactory.EqualsValueClause(Literal(values[variable.Id], variable))).WithTriviaFrom(old);
        }).ToFullString();
    }

    private static ExpressionSyntax Literal(object? value, EarlyVariable variable)
    {
        string expression;
        if (value == null) expression = "null";
        else if (value is bool boolean) expression = boolean ? "true" : "false";
        else if (value is string text) expression = SymbolDisplay.FormatLiteral(text, true);
        else if (value is char character) expression = SymbolDisplay.FormatLiteral(character, true);
        else
        {
            expression = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            if (variable.Kind == "real") expression += variable.TypeName.TrimEnd('?') switch { "float" => "f", "decimal" => "m", _ => "d" };
        }
        return SyntaxFactory.ParseExpression(expression);
    }

    /// <summary>Conserva stdout; los datos de comprobación se escriben por stderr.</summary>
    public string Instrument()
    {
        var shown = new Dictionary<ExpressionStatementSyntax, HashSet<string>>();
        foreach (var statement in root.DescendantNodes().OfType<ExpressionStatementSyntax>())
        {
            var ids = new HashSet<string>();
            foreach (var call in statement.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var method = semantic.GetSymbolInfo(call).Symbol as IMethodSymbol;
                if (method?.ContainingType.ToDisplayString() == "System.Console" && (method.Name == "WriteLine" || method.Name == "Write"))
                    ids.UnionWith(Expand(References(call.ArgumentList)));
            }
            if (ids.Count > 0) shown[statement] = ids;
        }
        var rewritten = root.ReplaceNodes(shown.Keys, (original, rewrittenStatement) => SyntaxFactory.Block(rewrittenStatement,
            SyntaxFactory.ParseStatement("global::System.Console.Error.WriteLine(" + SymbolDisplay.FormatLiteral(ShownMarker + string.Join(",", shown[original]), true) + ");")));
        var array = string.Join(",", Variables.Select(v => "new object[] { " + SymbolDisplay.FormatLiteral(v.Id, true) + ", " + v.Declaration.Identifier.Text + " }"));
        var snapshot = SyntaxFactory.ParseStatement("global::System.Console.Error.WriteLine(" + SymbolDisplay.FormatLiteral(ValuesMarker, true) + " + global::System.Text.Json.JsonSerializer.Serialize(new object[] { " + array + " }));");
        if (root.Members.OfType<GlobalStatementSyntax>().Any())
        {
            var lastGlobal = rewritten.Members.Select((member, index) => (member, index)).Last(pair => pair.member is GlobalStatementSyntax).index;
            rewritten = rewritten.WithMembers(rewritten.Members.Insert(lastGlobal + 1, SyntaxFactory.GlobalStatement(snapshot)));
        }
        else
        {
            var target = rewritten.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(m => m.Identifier.ValueText == "Main" && m.Body != null);
            if (target != null) rewritten = rewritten.ReplaceNode(target, target.WithBody(target.Body!.AddStatements(snapshot)));
        }
        return rewritten.NormalizeWhitespace().ToFullString();
    }

    public static EarlySnapshot ReadSnapshot(string error)
    {
        var values = new Dictionary<string, JsonElement>();
        var shown = new HashSet<string>();
        foreach (var line in error.Split('\n'))
        {
            if (line.StartsWith(ShownMarker, StringComparison.Ordinal)) shown.UnionWith(line[ShownMarker.Length..].Trim().Split(',', StringSplitOptions.RemoveEmptyEntries));
            if (!line.StartsWith(ValuesMarker, StringComparison.Ordinal)) continue;
            try
            {
                using var document = JsonDocument.Parse(line[ValuesMarker.Length..].Trim());
                foreach (var row in document.RootElement.EnumerateArray()) values[row[0].GetString()!] = row[1].Clone();
            }
            catch (JsonException) { }
        }
        return new EarlySnapshot(values, shown);
    }

    public bool HasIf => root.DescendantNodes().OfType<IfStatementSyntax>().Any();
}

public sealed record EarlyVariable(string Id, string Name, string Kind, bool Nullable, string TypeName, bool HasConstant, object? InitialValue, VariableDeclaratorSyntax Declaration);
public sealed record EarlySnapshot(IReadOnlyDictionary<string, JsonElement> Values, IReadOnlySet<string> Shown);
