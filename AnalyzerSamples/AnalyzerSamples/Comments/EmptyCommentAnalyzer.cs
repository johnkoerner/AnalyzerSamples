using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using AnalyzerSamples.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzerSamples.Comments
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyCommentAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EmptyCommentAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        public static readonly LocalizableString Title = "Comments Cannot be Empty";
        public static readonly LocalizableString MessageFormat = "Comment Cannot Be Empty";
        internal static readonly LocalizableString Description = "No really, comments cannot be empty.";
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(this.HandleSyntaxTree);
        }

        // If you want a full implementation of this analyzer with system tests and a code fix, go to 
        // https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/StyleCop.Analyzers/StyleCop.Analyzers/ReadabilityRules/SA1120CommentsMustContainText.cs

        private void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);

            foreach (var node in root.DescendantTrivia())
            {
                switch (node.Kind())
                {
                    case SyntaxKind.SingleLineCommentTrivia:
                        // Remove the leading // from the comment
                        var commentText = node.ToString().Substring(2);
                        int index = 0;

                        var list = TriviaHelper.GetContainingTriviaList(node, out index);
                        bool isFirst = IsFirstComment(list, index);
                        bool isLast = IsLastComment(list, index);

                        if (string.IsNullOrWhiteSpace(commentText) && (isFirst || isLast))
                        {
                            var diagnostic = Diagnostic.Create(Rule, node.GetLocation());
                            context.ReportDiagnostic(diagnostic);
                        }

                        break;
                }
            }
        }


        bool IsWhiteSpace(SyntaxTrivia triviaNode)
        {
            switch (triviaNode.Kind())
            {
                case SyntaxKind.EndOfLineTrivia:
                case SyntaxKind.WhitespaceTrivia:
                    return true;

                default:
                    return false;
            }
        }

        bool IsFirstComment(IReadOnlyList<SyntaxTrivia> triviaList, int commentIndex)
        {
            for (var i = 0; i < commentIndex; i++)
            {
                if (IsWhiteSpace(triviaList[i]))
                    continue;
                return false;
            }
            return true;
        }

        bool IsLastComment(IReadOnlyList<SyntaxTrivia> triviaList , int commentIndex)
        {
            for (var i = commentIndex + 1; i < triviaList.Count - 1; i++)
            {
                if (IsWhiteSpace(triviaList[i]))
                    continue;
                return false;
            }
            return true;
        }
    }
}