using System;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Refactorings;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Contracts
{
    public abstract class AddNotNullRequiresTestBase
    {
        protected async Task<string> ApplyRefactoring(string fullSource)
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