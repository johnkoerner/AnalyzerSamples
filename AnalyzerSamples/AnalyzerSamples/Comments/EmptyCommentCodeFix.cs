using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using AnalyzerSamples.Helpers;

namespace AnalyzerSamples.Comments
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EmptyCommentCodeFix)), Shared]
    public class EmptyCommentCodeFix : CodeFixProvider
    {
        public const string DiagnosticId = EmptyCommentAnalyzer.DiagnosticId;

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics.Where(d => FixableDiagnosticIds.Contains(d.Id)))
            {
                context.RegisterCodeFix(CodeAction.Create("Remove the empty comment", token => GetTransformedDocumentAsync(context.Document, diagnostic, token)), diagnostic);
            }

            return Task.FromResult<object>(null);
        }
        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var node = root.FindTrivia(diagnostic.Location.SourceSpan.Start, true);

            int diagnosticIndex = 0;
            var triviaList = TriviaHelper.GetContainingTriviaList(node, out diagnosticIndex);

            var nodesToRemove = new List<SyntaxTrivia>();
            nodesToRemove.Add(node);

            // If there is trialing content on the line, we don't want to remove the leading whitespace
            bool hasTrailingContent = TriviaHasTrailingContentOnLine(root, triviaList);

            if (diagnosticIndex > 0 && !hasTrailingContent)
            {
                var previousStart = triviaList[diagnosticIndex - 1].SpanStart;
                var previousNode = root.FindTrivia(previousStart, true);
                nodesToRemove.Add(previousNode);
            }

            // If there is leading content on the line, then we don't want to remove the trailing end of lines
            bool hasLeadingContent = TriviaHasLeadingContentOnLine(root, triviaList);

            if (diagnosticIndex < triviaList.Count - 1)
            {
                var nextStart = triviaList[diagnosticIndex + 1].SpanStart;
                var nextNode = root.FindTrivia(nextStart, true);

                if (nextNode.IsKind(SyntaxKind.EndOfLineTrivia) && !hasLeadingContent)
                {
                    nodesToRemove.Add(nextNode);
                }
            }

            // Replace all roots with an empty node
            var newRoot = root.ReplaceTrivia(nodesToRemove, (original, rewritten) =>
            {
                return new SyntaxTrivia();
            });

            newRoot = newRoot.NormalizeWhitespace();

            Document updatedDocument = document.WithSyntaxRoot(newRoot);
            return updatedDocument;
        }

        private static bool TriviaHasLeadingContentOnLine(SyntaxNode root, IReadOnlyList<SyntaxTrivia> triviaList)
        {
            var nodeBeforeStart = triviaList[0].SpanStart - 1;
            var nodeBefore = root.FindNode(new Microsoft.CodeAnalysis.Text.TextSpan(nodeBeforeStart, 1));

            if (GetLineSpan(nodeBefore).EndLinePosition.Line == GetLineSpan(triviaList[0]).StartLinePosition.Line)
            {
                return true;
            }

            return false;
        }

        private static bool TriviaHasTrailingContentOnLine(SyntaxNode root, IReadOnlyList<SyntaxTrivia> triviaList)
        {
            var nodeAfterTriviaStart = triviaList[triviaList.Count - 1].SpanStart - 1;
            var nodeAfterTrivia = root.FindNode(new Microsoft.CodeAnalysis.Text.TextSpan(nodeAfterTriviaStart, 1));

            if (GetLineSpan(nodeAfterTrivia).StartLinePosition.Line == GetLineSpan(triviaList[triviaList.Count - 1]).EndLinePosition.Line)
            {
                return true;
            }

            return false;
        }

        internal static FileLinePositionSpan GetLineSpan(SyntaxNode node)
        {
            return node.SyntaxTree.GetLineSpan(node.Span);
        }
        internal static FileLinePositionSpan GetLineSpan(SyntaxTrivia trivia)
        {
            return trivia.SyntaxTree.GetLineSpan(trivia.Span);
        }
    }
}