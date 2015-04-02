using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Contracts;
using CodeContractor.Refactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts
{
    [TestFixture]
    public class RequiresUtilsTests
    {
        [Test]
        public async Task AddContractNamespaceIfNeededAddsNamespaceOnlyOnce()
        {
            string src =
@"internal class SampleClass
{
    private SampleClass(string s{caret}tr)
    {
    }
}";
            var doc = await ClassTemplate.FromFullSource(src);
            var newRoot = RequiresUtils.AddContractNamespaceIfNeeded(doc.Root);

            Console.WriteLine(newRoot.GetText().ToString());
            
            string expected =
@"using System.Diagnostics.Contracts;
internal class SampleClass
{
    private SampleClass(string str)
    {
    }
}";
            Assert.AreEqual(expected, newRoot.GetText().ToString());

            // Calling the same method once more time
            newRoot = RequiresUtils.AddContractNamespaceIfNeeded(newRoot);
            Assert.AreEqual(expected, newRoot.GetText().ToString(), "Second call to AddContractNamespacedIfNeeded should have no effect");
        }

        [Test]
        public async Task AddContractNamespaceIfNeededDoesNotAddsNamespaceIfAlreadyExists()
        {
            string src =
@"using System;
using System.Diagnostics.Contracts;

internal class SampleClass
{
    private SampleClass(string s{caret}tr)
    {
    }
}";
            var doc = await ClassTemplate.FromFullSource(src);
            var newRoot = RequiresUtils.AddContractNamespaceIfNeeded(doc.Root);

            Console.WriteLine(newRoot.GetText().ToString());

            string expected =
@"using System;
using System.Diagnostics.Contracts;

internal class SampleClass
{
    private SampleClass(string str)
    {
    }
}";
            Assert.AreEqual(expected, newRoot.GetText().ToString());
        }

        [TestCaseSource("NotNullPreconditionSource")]
        public async Task<string> AddNotNullPrecondition(string method)
        {
            var doc = await ClassTemplate.FromMethodAsync(method);

            var parameterSyntax = doc.SelectedNode as ParameterSyntax;
            Assert.IsNotNull(parameterSyntax);

            var newMethod = RequiresUtils.AddRequires(parameterSyntax);

            Console.WriteLine("Original method:\r\n" + method);

            //var rr = Formatter.Format(method, Formatter.Annotation, doc.Document.Project.Solution.Workspace);
            var newMethodString = newMethod.WithLeadingTrivia().GetText().ToString();
            Console.WriteLine("New method (without leading trivia):\r\n" + newMethodString);

            return newMethodString;
        }

        [TestCaseSource("GetMethodsForDefaultValuesCheck")]
        public async Task<bool> TestIsDefaultedToNull(string method)
        {
            var doc = await ClassTemplate.FromMethodAsync(method);

            var parameterSyntax = doc.SelectedNode as ParameterSyntax;
            Assert.IsNotNull(parameterSyntax);

            return parameterSyntax.IsDefaultedToNull();
        }

        [TestCaseSource("GetMethodsForNullableTypeCheck")]
        public async Task<bool> TestIsNullable(string method)
        {
            var doc = await ClassTemplate.FromMethodAsync(method);

            var parameterSyntax = doc.SelectedNode as ParameterSyntax;
            Assert.IsNotNull(parameterSyntax);

            return parameterSyntax.IsNullable(doc.SemanticModel);
        }

        private static IEnumerable<TestCaseData> NotNullPreconditionSource()
        {
            yield return new TestCaseData(
                @"public static void Foo(string s{caret}tr)
{
}")
                .Returns(
                    @"public static void Foo(string str)
{
Contract.Requires(str != null);
}
");

            yield return new TestCaseData(
                @"public static void Foo(string s{caret}tr)
{
    Console.WriteLine(str);
}")
// I don't know why, but in tests formatting is not working properly!!
                .Returns(
                    @"public static void Foo(string str)
{
Contract.Requires(str != null);
    Console.WriteLine(str);
}
");
        }

        private static IEnumerable<TestCaseData> GetMethodsForDefaultValuesCheck()
        {
            yield return new TestCaseData(
                @"public static void Foo(string s{caret}tr = null) {}").Returns(true);

            yield return new TestCaseData(
                @"public static void Foo(string s{caret}tr = """") {}").Returns(false);

            yield return new TestCaseData(
                @"public static void Foo(int? s{caret}tr = null) {}").Returns(true);

            yield return new TestCaseData(
                @"public static void Foo(int? s{caret}tr = 42) {}").Returns(false);

            yield return new TestCaseData(
                @"public static void Foo(int s{caret}tr = 42) {}").Returns(false);

            yield return new TestCaseData(
                @"unsafe static void Foo(int * s{caret}tr = null) {}").Returns(true);
        }

        private static IEnumerable<TestCaseData> GetMethodsForNullableTypeCheck()
        {
            yield return new TestCaseData(
                @"public static void Foo(string s{caret}tr) {}").Returns(true);

            yield return new TestCaseData(
                @"public static void Foo(int s{caret}tr) {}").Returns(false);

            yield return new TestCaseData(
                @"public static void Foo(int? s{caret}tr) {}").Returns(true);

            yield return new TestCaseData(
                @"public static void Foo(Nullable<int> s{caret}tr) {}").Returns(true);

            yield return new TestCaseData(
                @"class Booo {}
                  public static void Foo(Booo s{caret}tr) {}").Returns(true);

            yield return new TestCaseData(
                @"struct Booo {}
                  public static void Foo(Booo s{caret}tr) {}").Returns(false);

            yield return new TestCaseData(
                @"unsafe static void Foo(int * s{caret}tr) {}").Returns(true);
        }
    }
}