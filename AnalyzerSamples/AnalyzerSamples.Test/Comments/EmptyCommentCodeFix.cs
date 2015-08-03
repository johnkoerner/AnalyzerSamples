using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AnalyzerSamples.Comments;
using AnalyzerSamples.Usage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AnalyzerSamples.Test.Comments
{
    [TestClass]
    public class EmptyCommentCodeFix : CodeFixVerifier
    {

        [TestMethod]
        public void TestBasicEmptyComment()
        {
            var testCode = @"
class Foo
{
    public void Bar()
    {
        //
    }
}";

            var expected = new DiagnosticResult
            {
                Id = EmptyCommentAnalyzer.DiagnosticId,
                Message = EmptyCommentAnalyzer.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 9)
                        }
            };
            this.VerifyCSharpDiagnostic(testCode, expected);

            var expectedFixedCode = @"
class Foo
{
    public void Bar()
    {
    }
}";
            this.VerifyCSharpFix(testCode, expectedFixedCode);
        }


        

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AnalyzerSamples.Comments.EmptyCommentCodeFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EmptyCommentAnalyzer();
        }
    }
}
