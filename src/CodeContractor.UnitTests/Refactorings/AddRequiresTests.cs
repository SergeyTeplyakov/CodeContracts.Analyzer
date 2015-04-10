using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.UnitTests.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Refactorings
{
    [TestFixture]
    public class AddRequiresTests
    {
        [Test]
        public async Task Test_Reverse()
        {
            string method =
@"public static void Foo(stirng s{caret}tr) {}";

            var doc = await ClassTemplate.FromMethodAsync(method);

            var actions = new List<CodeAction>();
            var context = new CodeRefactoringContext(doc.Document, doc.SelectedNode.Span, (a) => actions.Add(a), CancellationToken.None);
            var provider = new CodeContractor.Refactorings.AddRequiresRefactoringProvider();
            provider.ComputeRefactoringsAsync(context).Wait();

            if (actions.Count != 0)
            {
                var operations = await actions[0].GetOperationsAsync(CancellationToken.None);

                Assert.IsNotEmpty(operations);
                ApplyChangesOperation operation = operations.First() as ApplyChangesOperation;

                var changedSolution = operation.ChangedSolution;
            }
        }

        //public void VerifyRefactoringDiabled(CodeRefactoringProvider codeRefactoring)
        //{
        //    var document = RoslynTestsUtils.CreateDocument("foo");

        //    var span = new TextSpan(1, 10);
        //    var actions = new List<CodeAction>();
        //    var context = new CodeRefactoringContext(document, span, (a) => actions.Add(a), CancellationToken.None);
        //    var provider = new CodeContractor.Refactorings.AddRequiresRefactoringProvider();
        //    provider.ComputeRefactoringsAsync(context).Wait();


        //}
    }
}