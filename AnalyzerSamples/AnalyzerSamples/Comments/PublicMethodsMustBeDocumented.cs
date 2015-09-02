using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzerSamples.Comments
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PublicMethodsMustBeDocumented : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PublicMethodsMustBeDocumented";
        internal static readonly LocalizableString Title = "PublicMethodsMustBeDocumented Title";
        internal static readonly LocalizableString MessageFormat = "PublicMethodsMustBeDocumented '{0}'";
        internal const string Category = "PublicMethodsMustBeDocumented Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(CheckMethods, SyntaxKind.MethodDeclaration);
        }

        private void CheckMethods(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var node = syntaxNodeAnalysisContext.Node as MethodDeclarationSyntax;

            if (!node.Modifiers.Any(SyntaxKind.PublicKeyword))
                return;

            var xmlTrivia = node.GetLeadingTrivia()
                .Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            var hasSummary = xmlTrivia.ChildNodes()
                .OfType<XmlElementSyntax>()
                .Any(i => i.StartTag.Name.ToString().Equals("summary"));

            if (!hasSummary)
            {
                syntaxNodeAnalysisContext.ReportDiagnostic(Diagnostic.Create(Rule, node.Identifier.GetLocation(), "Missing Summary"));
            }

            var allParamNameAttributes = xmlTrivia.ChildNodes()
                .OfType<XmlElementSyntax>()
                .Where(i => i.StartTag.Name.ToString().Equals("param"))
                .SelectMany(i => i.StartTag.Attributes.OfType<XmlNameAttributeSyntax>());
            
            foreach (var param in node.ParameterList.Parameters)
            {
                var existsInXmlTrivia = allParamNameAttributes
                            .Any(i=>i.Identifier.ToString().Equals(param.Identifier.Text)) ;// ()

                if (!existsInXmlTrivia)
                {
                    syntaxNodeAnalysisContext.ReportDiagnostic(Diagnostic.Create(Rule, param.GetLocation(), "Parameter Not Documented"));
                }
            }
        }
    }
}