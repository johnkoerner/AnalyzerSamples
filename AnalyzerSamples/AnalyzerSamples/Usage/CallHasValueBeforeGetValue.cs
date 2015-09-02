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
    public class CallHasValueBeforeGetValue : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CallHasValueBeforeGetValue";
        internal static readonly LocalizableString Title = "CallHasValueBeforeGetValue Title";
        internal static readonly LocalizableString MessageFormat = "CallHasValueBeforeGetValue '{0}'";
        internal const string Category = "CallHasValueBeforeGetValue Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            //context.RegisterCodeBlockAction((blockContext) =>
            //{
                


            //    blockContext.SemanticModel.AnalyzeDataFlow()
            //});


            context.RegisterSyntaxNodeAction((syntaxContext) =>
            {
                var expr = (InvocationExpressionSyntax)syntaxContext.Node;

                var memberAccess = expr.Expression as MemberAccessExpressionSyntax;

                if (memberAccess?.Name.ToString() != "GetValue")
                    return;


            }, SyntaxKind.InvocationExpression);

        }
    }
}