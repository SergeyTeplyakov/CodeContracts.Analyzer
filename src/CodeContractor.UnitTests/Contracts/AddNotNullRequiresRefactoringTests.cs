using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Refactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts
{
    [TestFixture]
    public class AddNotNullRequiresRefactoringTests
    {
        [Test]
        public async Task AddPreconditionAddsAppropriateUsingStatementWithSelectedParameter()
        {
            string src =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class SampleClass
{
    public static void Foo(string s{caret}tr)
    {
        Console.WriteLine(str);
    }
}";
            var newDocumentString = await ApplyRefactoring(src);

            string expected =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

internal class SampleClass
{
    public static void Foo(string str)
    {
Contract.Requires(str != null);
        Console.WriteLine(str);
    }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        [Test]
        public async Task AddPreconditionAddsAppropriateUsingStatementWithSelectedArgument()
        {
            string src =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class SampleClass
{
    public static void Foo(string str)
    {
        Console.WriteLine(s{caret}tr);
    }
}";
            var newDocumentString = await ApplyRefactoring(src);

            string expected =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

internal class SampleClass
{
    public static void Foo(string str)
    {
Contract.Requires(str != null);
        Console.WriteLine(str);
    }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        [Test]
        public async Task AddPreconditionToConstructorBody()
        {
            string src =
@"internal class SampleClass
{
    private SampleClass(string s{caret}tr)
    {
    }
}";
            var newDocumentString = await ApplyRefactoring(src);

            string expected =
@"using System.Diagnostics.Contracts;
internal class SampleClass
{
    private SampleClass(string str)
    {
Contract.Requires(str != null);
    }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        [TestCaseSource("RefactoringAvailabilitySource")]
        public async Task<bool> Test_Refactoring_Availability(string method)
        {
            var doc = await ClassTemplate.FromMethodAsync(method);
            var refactoring = await AddNotNullRequiresRefactoring.Create(doc.SelectedNode, doc.Document);

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
        }

        private async Task<string> ApplyRefactoring(string fullSource)
        {
            var doc = await ClassTemplate.FromFullSource(fullSource);

            var refactoring = await AddNotNullRequiresRefactoring.Create(doc.SelectedNode, doc.Document);

            var newDocument = await refactoring.ApplyRefactoringAsync(CancellationToken.None);
            var newDocumentString = (await newDocument.GetTextAsync()).ToString();

            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Console.WriteLine("Refactored document: \r\n" + newDocumentString);

            return newDocumentString;
        }

    }
}