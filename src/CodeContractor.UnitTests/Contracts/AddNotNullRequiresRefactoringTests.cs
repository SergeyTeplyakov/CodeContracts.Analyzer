using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Refactorings;
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

        [Test]
        public async Task AddPreconditionToSecondArgument()
        {
            string src =
@"using System.Diagnostics.Contracts;
internal class SampleClass
{
    private SampleClass(string str1, string s{caret}tr)
    {
        Contract.Requires(str1 != null);
    }
}";
            var newDocumentString = await ApplyRefactoring(src);

            string expected =
@"using System.Diagnostics.Contracts;
internal class SampleClass
{
    private SampleClass(string str1, string str)
    {
        Contract.Requires(str1 != null);
Contract.Requires(str != null);
    }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        [Test]
        public async Task AddPreconditionToConstructorBodyForGenericStruct()
        {
            string src =
@"internal struct Option<T> where T : class
{
    public Option(T v{caret}alue)
    {
    }
}";
            var newDocumentString = await ApplyRefactoring(src);

            string expected =
@"using System.Diagnostics.Contracts;
internal struct Option<T> where T : class
{
    public Option(T value)
    {
Contract.Requires(value != null);
    }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        [Test]
        public async Task AddPreconditionToConstructorWithGenericList()
        {
            string src =
@"using System.Collections.Generic;
using System.Collections.Immutable;
internal class Sample
{
    public Sample(IReadOnlyList<Precondition> pre{caret}conditions)
    {
    }
}";
            var newDocumentString = await ApplyRefactoring(src);

            string expected =
@"using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
internal class Sample
{
    public Sample(IReadOnlyList<Precondition> preconditions)
    {
Contract.Requires(preconditions != null);
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
        }

        private async Task<string> ApplyRefactoring(string fullSource)
        {
            var doc = await ClassTemplate.FromFullSource(fullSource);

            var refactoring = await AddNotNullRequiresRefactoring.Create(doc.SelectedNode, doc.Document);

            bool isAvailable = await refactoring.IsAvailableAsync(CancellationToken.None);
            Assert.IsTrue(isAvailable, "Refactoring should be awailable!");

            var newDocument = await refactoring.ApplyRefactoringAsync(CancellationToken.None);
            var newDocumentString = (await newDocument.GetTextAsync()).ToString();

            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Console.WriteLine("Refactored document: \r\n" + newDocumentString);

            return newDocumentString;
        }

    }
}