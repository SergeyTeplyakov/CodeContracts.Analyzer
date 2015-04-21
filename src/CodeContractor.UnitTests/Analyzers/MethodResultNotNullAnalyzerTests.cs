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
    public class MethodResultNotNullargumentsAnalyzerTests 
        : CodeFixVerifier<MethodResultNotNullAnalyzer, PublicMethodContractAnalyzerCodeFixProvider>
    {
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
                @"public class Bla{} public Bl{on}a Foo() {return new Bla();}");

            // Act
            var diagnostics = GetSortedDiagnostics(template.Source);

            // Assert
            string message = @"Lack of not-null ensures for nullable return type 'Bla'.";
            Assert.AreEqual(diagnostics[0].GetMessage(), message);
        }

        private static IEnumerable<TestCaseData> AvailabilityTestCases()
        {
            // TODO: add "{off}" case for explicit check! In this case error message would be more descriptive!
            yield return new TestCaseData(@"public i{off}nt Foo() {return 42;}");

            yield return new TestCaseData(@"public st{on}ring Foo() {return string.Empty;}");

            yield return new TestCaseData(@"public class FooClass{} public Foo{on}Class Foo() {return new FooClass();}");

            yield return new TestCaseData(@"private st{off}ring Foo() {return string.Empty;}");
        }
    }
}