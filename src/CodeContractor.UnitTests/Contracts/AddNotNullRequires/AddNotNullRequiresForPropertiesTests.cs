using System.Threading.Tasks;
using CodeContractor.Refactorings;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts.AddNotNullRequires
{
    [TestFixture]
    public class AddNotNullRequiresForIndexers : CodeContractRefactoringBase
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
  public object this[string index]
  {
    get
    {
Contract.Requires(index != null);
      return new object();
    }
    set
    {
Contract.Requires(index != null);
      Consonle.WriteLine(42);
    }
  }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        [Test]
        public async Task AddPreconditionToSetterIndexer()
        {
            string src =
@"using System.Diagnostics.Contracts;
abstract class A
{  
  public object this[string inde{caret}x]
  {
    get
    {
      Contract.Requires(index != null);
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
  public object this[string index]
  {
    get
    {
      Contract.Requires(index != null);
      return new object();
    }
    set
    {
Contract.Requires(index != null);
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
    set
    {
Contract.Requires(value != null);
      Consonle.WriteLine(42);
    }
  }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        internal override async Task<ICodeContractRefactoring> CreateRefactoringAsync(ClassTemplate doc)
        {
            return await AddNotNullRequiresRefactoring.Create(doc.SelectedNode, doc.SemanticModel);
        }
    }
}