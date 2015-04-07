using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Refactorings;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts
{
    [TestFixture]
    public class AddNotNullRequiresForIndexers : AddNotNullRequiresTestBase
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
        public async Task AddPreconditionToSetter()
        {
            string src =
@"abstract class A
{  
  public object this[string index]
  {
    se{caret}t
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
  public object this[string index]
  {
    se{caret}t
    {
      Contract.Requires(value != null);
      Consonle.WriteLine(42);
    }
  }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }
    }
}