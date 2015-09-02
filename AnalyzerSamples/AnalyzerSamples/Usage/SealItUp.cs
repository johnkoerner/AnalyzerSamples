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
    public class SealItUp : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SealItUp";
        internal static readonly LocalizableString Title = "Seal classes that do not have any virtual or abstract methods, properties, events, or indexers";
        internal static readonly LocalizableString MessageFormat = "Seal classes that do not have any virtual or abstract methods, properties, events, or indexers";
        internal const string Category = "Usage";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction((syntaxNodeContext) =>
            {
                var node = syntaxNodeContext.Node as ClassDeclarationSyntax;

                // We don't care about sealing static classes
                if (node.Modifiers.Where(x => x.IsKind(SyntaxKind.StaticKeyword)).Any())
                    return;

                var methods = node.Members.Where(x => x.IsKind(SyntaxKind.MethodDeclaration));
                var props = node.Members.Where(x => x.IsKind(SyntaxKind.PropertyDeclaration));
                var events = node.Members.Where(x => x.IsKind(SyntaxKind.EventDeclaration));
                var indexers = node.Members.Where(x => x.IsKind(SyntaxKind.IndexerDeclaration));

                foreach (var m in methods)
                {
                    var modifiers = (m as MethodDeclarationSyntax)?.Modifiers.Where(x=>x.IsKind(SyntaxKind.AbstractKeyword) || x.IsKind(SyntaxKind.VirtualKeyword));
                    if (modifiers != null && modifiers.Any())
                        return;
                }

                foreach (var p in props)
                {
                    var modifiers = (p as PropertyDeclarationSyntax)?.Modifiers.Where(x => x.IsKind(SyntaxKind.AbstractKeyword) || x.IsKind(SyntaxKind.VirtualKeyword));
                    if (modifiers != null && modifiers.Any())
                        return;
                }

                foreach (var e in events)
                {
                    var modifiers = (e as EventDeclarationSyntax)?.Modifiers.Where(x => x.IsKind(SyntaxKind.AbstractKeyword) || x.IsKind(SyntaxKind.VirtualKeyword));
                    if (modifiers != null && modifiers.Any())
                        return;
                }

                foreach (var i in indexers)
                {
                    var modifiers = (i as IndexerDeclarationSyntax)?.Modifiers.Where(x => x.IsKind(SyntaxKind.AbstractKeyword) || x.IsKind(SyntaxKind.VirtualKeyword));
                    if (modifiers != null && modifiers.Any())
                        return;
                }

                // We got here, so there are no abstract or virtual methods/properties/events/indexers
                syntaxNodeContext.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));

            } , SyntaxKind.ClassDeclaration);
        }
    }

public abstract class C
    {
        public virtual string X { get; set; }

    }
}

