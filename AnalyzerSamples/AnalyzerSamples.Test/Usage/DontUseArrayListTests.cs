using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalyzerSamples.Usage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AnalyzerSamples.Test.Usage
{
    [TestClass]
    public class DontUseArrayListTests : DiagnosticVerifier
    {
        //No diagnostics expected to show up
        [TestMethod]
        public void EmptyCode()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void ValidateDiagnosticFires()
        {
            var test = @"
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           var a = new System.Collections.ArrayList();
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = DontUseArrayList.DiagnosticId,
                Message = "The type ArrayList should not be used",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 20)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DontUseArrayList();
        }

    }
}
