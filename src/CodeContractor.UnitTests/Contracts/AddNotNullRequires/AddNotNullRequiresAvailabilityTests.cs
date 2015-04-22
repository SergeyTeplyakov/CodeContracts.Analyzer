using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Refactorings;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts.AddNotNullRequires
{
    [TestFixture]
    public class AddNotNullRequiresAvailabilityTests
    {
        [TestCaseSource(nameof(RefactoringAvailabilitySource))]
        public async Task<bool> Test_Refactoring_Availability(string method)
        {
            var doc = await ClassTemplate.FromMethodAsync(method);
            var refactoring = await AddNotNullRequiresRefactoring.Create(doc.SelectedNode, doc.SemanticModel);

            return await refactoring.IsAvailableAsync(CancellationToken.None);
        }

        private static IEnumerable<TestCaseData> RefactoringAvailabilitySource()
        {
            yield return new TestCaseData(
@"public static void Foo(string s{caret}tr)
{
}")
.Returns(true);

            yield return new TestCaseData(
@"public static void Foo(string str)
{
    Console.WriteLine(s{caret}tr);
}")
.Returns(true);

            yield return new TestCaseData(
@"public abstract void Foo(string s{caret}tr);")
.Returns(false);

            yield return new TestCaseData(
@"public void Foo(string str)
{
    if (s{caret}tr == null)
    {
        Console.WriteLine(42);
    }
}")
.Returns(true);

            yield return new TestCaseData(
@"public void Foo(string s{caret}tr)
{
    Contract.Requires(str != null);
}")
.Returns(false);

            // Not Implemented yet!
            yield return new TestCaseData(
@"public void Foo(string str)
{
    Contract.Requires(!string.IsNullOrEmpty(str));
}")
.Returns(false).Ignore();

            yield return new TestCaseData(
@"public void Foo(string str)
{
    Contract.Requires(string.IsNullOrEmpty(str));
}")
.Returns(false).Ignore();

            // Not Implemented yet!
            yield return new TestCaseData(
@"public void Foo(string str)
{
    if (str == null) throw new ArgumentNullException(""str"");
}")
.Returns(false).Ignore();

            yield return new TestCaseData(
@"public void EnabledOnParamsArguments(params object[] argume{caret}nts)
  {}")
.Returns(true);

            yield return new TestCaseData(
@"public void DisabledBecauseAlreadyCheckedInFirstComplex(string s1, string s2{caret}) 
  {
    Contract.Requires((s1 != null || s1.Length == 0) && s2 != null);
  }")
.Returns(false);
        }
    }
}