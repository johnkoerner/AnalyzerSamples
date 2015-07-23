using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace AnalyzerSamples.VB
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class OptionExplicitAndOptionStrictShouldNotBeTurnedOff : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "OptionExplicitAndOptionStrictShouldNotBeTurnedOff";
        internal static readonly LocalizableString Title = "Option strict and option explicit should not be turned off";
        internal static readonly LocalizableString MessageFormat = "Option {0} should not be truned off.";
        internal const string Category = "Visual Basic";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.OptionStatement);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var optionStatement = context.Node as OptionStatementSyntax;

            if (optionStatement == null)
                return;

            if (optionStatement.NameKeyword.IsMissing || optionStatement.ValueKeyword.IsMissing)
                return;

            // Look to see if the keyword is Strict or Explicit.
            if (!(optionStatement.NameKeyword.IsKind(SyntaxKind.StrictKeyword) || optionStatement.NameKeyword.IsKind(SyntaxKind.ExplicitKeyword)))
                return;

            // We only care if it is set to Off
            if (!optionStatement.ValueKeyword.IsKind(SyntaxKind.OffKeyword))
                return;

            // For all such symbols, produce a diagnostic.
            var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), optionStatement.NameKeyword.ValueText);

            context.ReportDiagnostic(diagnostic);

        }
    }
}