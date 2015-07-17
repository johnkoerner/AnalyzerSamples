using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzerSamples.Usage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DictionaryWithKeyStringMustSpecifyIComparer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DictionaryWithKeyStringMustSpecifyIComparer";
        internal static readonly LocalizableString Title = "Dictionaries with keys of type string need to specify the equality comparer.";
        internal static readonly LocalizableString MessageFormat = "Dictionary does not specify the string comparer.";
        internal const string Category = "Usage";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var dictionaryTokenType = compilationContext.Compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2");
                var equalityComparerInterfaceType = compilationContext.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEqualityComparer`1");

                if (dictionaryTokenType != null)
                {
                    compilationContext.RegisterSyntaxNodeAction(symbolContext =>
                    {
                        var creationNode = (ObjectCreationExpressionSyntax)symbolContext.Node;
                        var variableTypeInfo = symbolContext.SemanticModel.GetTypeInfo(symbolContext.Node).ConvertedType as INamedTypeSymbol;

                        if (variableTypeInfo == null)
                            return;

                        if (!variableTypeInfo.OriginalDefinition.Equals(dictionaryTokenType))
                            return;

                        // We only care about dictionaries who use a string as the key
                        if (variableTypeInfo.TypeArguments.First().SpecialType != SpecialType.System_String)
                            return;

                        var arguments = creationNode.ArgumentList?.Arguments;

                        if (arguments == null || arguments.Value.Count == 0)
                        {
                            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, symbolContext.Node.GetLocation()));
                            return;
                        }

                        bool hasEqualityComparer = false;
                        foreach (var argument in arguments)
                        {
                            var argumentType = symbolContext.SemanticModel.GetTypeInfo(argument.Expression);

                            if (argumentType.ConvertedType == null)
                                return;

                            if (argumentType.ConvertedType.OriginalDefinition.Equals(equalityComparerInterfaceType))
                            {
                                hasEqualityComparer = true;
                                break;
                            }
                        }

                        if (!hasEqualityComparer)
                        {
                            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, symbolContext.Node.GetLocation()));
                        }
                    }, SyntaxKind.ObjectCreationExpression);
                }
            });
        }
    }
}
