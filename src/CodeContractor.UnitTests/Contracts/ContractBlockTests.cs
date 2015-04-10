using System.Linq;
using System.Threading.Tasks;
using CodeContractor.Contracts;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts
{
    [TestFixture]
    public class ContractBlockTests
    {
        [Test]
        public async Task TestContractBlockWithRequires()
        {
            // Arrange
            string method =
@"public static void Foo(string s{caret}tr, string anotherStr)
{
    Contract.Requires(str != null);
    Contract.Requires(str.Length == null);
    Contract.Requires(anotherStr != null);
    Contract.Requires(anotherStr.Length == 42);
}";
            var doc = await ClassTemplate.FromMethodAsync(method, withContractUsings: true);

            // Act
            var contractBlock = await ContractBlock.CreateForMethodAsync(doc.SelectedMethod(), doc.SemanticModel);

            // Assert
            Assert.AreEqual(4, contractBlock.Preconditions.Count, "Method should have only one precondition.");

            var first = contractBlock.Preconditions[0];
            Assert.IsTrue(first.ChecksForNotNull(doc.SelectedNode as ParameterSyntax));

            var second = contractBlock.Preconditions[1];
            Assert.IsFalse(second.ChecksForNotNull(doc.SelectedNode as ParameterSyntax));

            var third = contractBlock.Preconditions[2];
            Assert.IsFalse(third.ChecksForNotNull(doc.SelectedNode as ParameterSyntax));
        }

        [Test]
        public async Task TestContractBlockWithRequiresAndEnsures()
        {
            // Arrange
            string method =
@"public static string F{caret}oo(string str)
{
    Contract.Requires(str != null);
    Contract.Ensures(Contract.Result<string>() != null, ""Message"");
    Contract.Ensures(Contract.Result<string>().Length != 0, ""Message"");
    Contract.Ensures(Contract.Result<string>() != null || Contract.Result<string>().Length != 0, ""Message"");
}";
            var doc = await ClassTemplate.FromMethodAsync(method, withContractUsings: true);
            
            // Act
            var contractBlock = await ContractBlock.CreateForMethodAsync(doc.SelectedMethod(), doc.SemanticModel);

            // Assert
            Assert.AreEqual(3, contractBlock.Postconditions.Count);
            Assert.IsTrue(contractBlock.Postconditions[0].HasNotNullCheck());
            Assert.IsFalse(contractBlock.Postconditions[1].HasNotNullCheck());
            Assert.IsTrue(contractBlock.Postconditions[2].HasNotNullCheck());
        }
    }
}