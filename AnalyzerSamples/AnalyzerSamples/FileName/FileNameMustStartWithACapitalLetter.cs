using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace AnalyzerSamples.FileName
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileNameMustStartWithACapitalLetter : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FileNameMustStartWithACapitalLetter";
        internal static readonly LocalizableString Title = "FileNameMustStartWithACapitalLetter Title";
        internal static readonly LocalizableString MessageFormat = "FileNameMustStartWithACapitalLetter '{0}'";
        internal const string Category = "FileNameMustStartWithACapitalLetter Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction((syntaxTreeContext) =>
            {
                var filePath = syntaxTreeContext.Tree.FilePath;

                if (filePath == null)
                    return;

                var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                
                if (Char.IsLower(fileName.ToCharArray()[0]))
                    syntaxTreeContext.ReportDiagnostic(Diagnostic.Create(Rule, Location.Create(syntaxTreeContext.Tree, TextSpan.FromBounds(0, 0)), fileName));
            });
        }
    }
}