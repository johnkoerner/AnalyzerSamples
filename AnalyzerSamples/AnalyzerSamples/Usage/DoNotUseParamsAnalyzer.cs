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
    public class DoNotUseParamsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DoNotUseParamsAnalyzer";
        internal static readonly LocalizableString Title = "DoNotUseParamsAnalyzer Title";
        internal static readonly LocalizableString MessageFormat = "DoNotUseParamsAnalyzer '{0}'";
        internal const string Category = "DoNotUseParamsAnalyzer Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction((syntaxContext) =>
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Rule, syntaxContext.Node.GetLocation()));
            }, SyntaxKind.ParamsKeyword);
        }
    }
}