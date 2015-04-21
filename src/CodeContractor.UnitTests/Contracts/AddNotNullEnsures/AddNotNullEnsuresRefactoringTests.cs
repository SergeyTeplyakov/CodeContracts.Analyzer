using System;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Refactorings;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts.AddNotNullEnsures
{
    [TestFixture]
    public class AddNotNullEnsuresRefactoringTests : CodeContractRefactoringBase
    {
        [Test]
        public async Task Add_Ensures_Added_Contract_Namespace_Using()
        {
            string src =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class SampleClass
{
    public static str{caret}ing Foo()
    {
        return string.Empty;
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
    public static string Foo()
    {
Contract.Ensures(Contract.Result<string>() != null);
        return string.Empty;
    }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        [Test]
        public async Task Ensures_Is_Added_After_Requires()
        {
            string src =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

internal class SampleClass
{
    public static str{caret}ing Foo(string str)
    {
        Contract.Requires(str != null);
        return string.Empty;
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
    public static string Foo(string str)
    {
        Contract.Requires(str != null);
Contract.Ensures(Contract.Result<string>() != null);
        return string.Empty;
    }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        [Test]
        public async Task Add_Ensures_For_Generic_Return_Type()
        {
            string src =
@"using System.Collections.Generic;
internal class SampleClass<T>
{
    private List<T> GetList()
    {
        ret{caret}urn new List<T>();
    }
}";
            var newDocumentString = await ApplyRefactoring(src);
            
            string expected =
@"using System.Collections.Generic;
using System.Diagnostics.Contracts;
internal class SampleClass<T>
{
    private List<T> GetList()
    {
Contract.Ensures(Contract.Result<List<T>>() != null);
        return new List<T>();
    }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        [Test]
        public async Task Add_Ensures_For_Nullable_Int()
        {
            string src =
@"internal class SampleClass
{
    private int? GetFoo()
    {
        ret{caret}urn 42;
    }
}";
            var newDocumentString = await ApplyRefactoring(src);
            
            string expected =
@"using System.Diagnostics.Contracts;
internal class SampleClass
{
    private int? GetFoo()
    {
Contract.Ensures(Contract.Result<int ? >() != null);
        return 42;
    }
}";
            // Please note, that during IDE run Contract.Requires would have required leading trivia
            Assert.AreEqual(expected, newDocumentString);
        }

        internal override async Task<ICodeContractRefactoring> CreateRefactoringAsync(ClassTemplate doc)
        {
            return await AddNotNullEnsuresRefactoring.Create(doc.SelectedNode, doc.SemanticModel);
        }
    }
}