using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Refactorings;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts.AddNotNullEnsures
{
    [TestFixture]
    public class AddNotNullEnsuresAvailabilityTests
    {
        [TestCaseSource("RefactoringAvailabilitySource")]
        public async Task<bool> Test_Refactoring_Availability(string method)
        {
            var doc = await ClassTemplate.FromMethodAsync(method);
            var refactoring = await AddNotNullEnsuresRefactoring.Create(doc.SelectedNode, doc.SemanticModel);

            return await refactoring.IsAvailableAsync(CancellationToken.None);
        }

        private static IEnumerable<TestCaseData> RefactoringAvailabilitySource()
        {
            yield return new TestCaseData(
@"public static st{caret}ring Foo(string sr)
{
  return string.Empty;
}")
.Returns(true);

            yield return new TestCaseData(
@"public static string Foo(string sr)
{
  re{caret}turn string.Empty;
}")
.Returns(true);

            yield return new TestCaseData(
@"public static object F{caret}oo(string sr)
{
  return string.Empty;
}")
.Returns(true);

            yield return new TestCaseData(
@"public static int? Foo(string sr)
{
  re{caret}turn 42;
}")
.Returns(true);

            yield return new TestCaseData(
@"public static int Foo(string sr)
{
  re{caret}turn 42;
}")
.Returns(false);
        }
    }
}