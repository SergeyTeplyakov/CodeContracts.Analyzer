using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Refactorings;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts
{
    [TestFixture]
    public class AddNotNullRequiresRefactoringTests : CodeContractRefactoringBase
    {
        [Test]
        public async Task AddPreconditionToIndexer()
        {
            string src =
@"abstract class A
{  
  public object this[string inde{caret}x]
  {
    get
    {
      return new object();
    }
    set
    {
      Consonle.WriteLine(42);
    }
  }
}";
            var newDocumentString = await ApplyRefactoring(src);

            string expected =
@"using System.Diagnostics.Contracts;
abstract class A
{  
  public object this[string inde{caret}x]
  {
    get
    {
      Contract.Requires(indexer != null);
      return new object();
    }
    set
    {
      Contract.Requires(indexer != null);
      Consonle.WriteLine(42);
    }
  }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

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

        internal override async Task<ICodeContractRefactoring> CreateRefactoringAsync(ClassTemplate doc)
        {
            return await AddNotNullRequiresRefactoring.Create(doc.SelectedNode, doc.Document);
        }
    }
}