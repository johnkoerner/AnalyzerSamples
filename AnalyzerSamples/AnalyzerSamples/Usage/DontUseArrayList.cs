using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzerSamples.Usage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DontUseArrayList : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DontUseArrayList";
        internal static readonly LocalizableString Title = "The type ArrayList should not be used";
        internal static readonly LocalizableString MessageFormat = "The type ArrayList should not be used";
        internal const string Category = "Usage";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(AnalyzeArrayList);
        }

        private static void AnalyzeArrayList(CompilationStartAnalysisContext compilationContext)
        {
            var arrayListType = compilationContext.Compilation.GetTypeByMetadataName("System.Collections.ArrayList");

            compilationContext.RegisterSyntaxNodeAction(syntaxContext =>
            {
                var variableTypeInfo = syntaxContext.SemanticModel.GetTypeInfo(syntaxContext.Node).Type as INamedTypeSymbol;

                if (variableTypeInfo.SpecialType == SpecialType.System_String)
                {

                }

                if (variableTypeInfo == null)
                    return;

                if (variableTypeInfo.Equals(arrayListType))
                {
                    syntaxContext.ReportDiagnostic(Diagnostic.Create(Rule, syntaxContext.Node.GetLocation()));
                }
            }, SyntaxKind.ObjectCreationExpression);


        }
    }
}