using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace CodeContractor.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CodeContractorCodeRefactoringProvider)), Shared]
    public sealed class CodeContractorCodeRefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            SyntaxNode node = root.FindNode(context.Span);

            if (node == null)
            {
                return;
            }

            var refactoring = await AddNotNullRequiresRefactoring.Create(node, context.Document);
            bool isAwailable = await refactoring.IsAvailableAsync(context.CancellationToken);

            if (isAwailable)
            {
                // For any type declaration node, create a code action to reverse the identifier text.
                var action = CodeAction.Create("Add Requires", c => refactoring.ApplyRefactoringAsync(context.CancellationToken));

                // Register this code action.
                context.RegisterRefactoring(action);
            }
        }
    }
}