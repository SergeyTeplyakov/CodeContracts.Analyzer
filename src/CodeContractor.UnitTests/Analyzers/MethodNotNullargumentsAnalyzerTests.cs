using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeContractor.Analyzers.Core;
using CodeContractor.UnitTests.Common;
using CodeContractor.UnitTests.Contracts;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Analyzers
{
    [TestFixture]
    public class MethodNotNullargumentsAnalyzerTests
        : CodeFixVerifier<MethodArgumentNotNullAnalyzer, PublicMethodContractAnalyzerCodeFixProvider>
    {
        [TestCase]
        public async Task Test_PublicMethodContractAnalyzer()
        {
            ClassTemplate template = await ClassTemplate.FromMethodAsync(
                @"public void Foo(string s{on}) {}");

            ValidateDiagnostics(template);
        }

        [TestCaseSource(nameof(AvailabilityTestCases))]
        public async Task TestAnalyzerAvailability(string source)
        {
            ClassTemplate template = await ClassTemplate.FromMethodAsync(source);

            ValidateDiagnostics(template);
        }

        [Test]
        public async Task TestDiagnosticMessage()
        {
            // Arrange
            ClassTemplate template = await ClassTemplate.FromMethodAsync(
                @"public void Foo(string s{on}tr) {}");
            
            // Act
            var diagnostics = GetSortedDiagnostics(template.Source);

            // Assert
            string message = @"Lack of argument validation for nullable parameter 'str'.";
            Assert.AreEqual(diagnostics[0].GetMessage(), message);
        }

        private static IEnumerable<TestCaseData> AvailabilityTestCases()
        {
            yield return new TestCaseData(@"public void Foo(string s{on}) {}");
            yield return new TestCaseData(@"protected void Foo(string s{on}) {}");
            yield return new TestCaseData(@"internal void Foo(string s{off}) {}");
            yield return new TestCaseData(@"private void Foo(string s{off}) {}");

            //yield return new TestCaseData(@"public void Foo(string s{on}, string st{on}r, int? n{on}n, int? = null) {}");
        }
    }
}