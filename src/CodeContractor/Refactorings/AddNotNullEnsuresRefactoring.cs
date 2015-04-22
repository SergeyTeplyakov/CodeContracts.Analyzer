using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Contracts;
using CodeContractor.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Refactorings
{
    /// <summary>
    /// Helper class that adds <see cref="Contract.Ensures(bool)"/> for nullable return values.
    /// </summary>
    public sealed class AddNotNullEnsuresRefactoring : ICodeContractRefactoring
    {
        private readonly SyntaxNode _selectedNode;
        private readonly SemanticModel _semanticModel;

        private AddNotNullEnsuresRefactoring(SyntaxNode selectedNode, SemanticModel semanticModel)
        {
            Contract.Requires(selectedNode != null);
            Contract.Requires(semanticModel != null);

            _selectedNode = selectedNode;
            _semanticModel = semanticModel;
        }

        public static Task<AddNotNullEnsuresRefactoring> Create(SyntaxNode selectedNode, SemanticModel semanticModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(selectedNode != null);
            Contract.Requires(semanticModel != null);

            return Task.FromResult(new AddNotNullEnsuresRefactoring(selectedNode, semanticModel));
        }

        public async Task<bool> IsAvailableAsync(CancellationToken token)
        {
            // Refactoring should be available only on selected nodes, like return statement or return type of the method
            if (!IsSelectedNodeSuiteableForRefactoring(_selectedNode))
            {
                return false;
            }

            var methodDeclaration = _selectedNode.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();

            if (methodDeclaration == null)
            {
                // Could be null for constructors, for instance
                return false;
            }

            return methodDeclaration.UnwrapReturnTypeIfNeeded(_semanticModel).IsNullable(_semanticModel) &&
                   // Task is a special case. Ignore it! There is an assumption that Task should not be null. Adding
                   // this ensures will just pollute the code
                   !methodDeclaration.ReturnType.TypeEquals(typeof(Task), _semanticModel) &&
                   !methodDeclaration.IsAbstract() &&
                   (!await methodDeclaration.EnsuresReturnValueIsNotNull(_semanticModel, token));
        }

        public async Task<Document> ApplyRefactoringAsync(Document document, CancellationToken token)
        {
            MethodDeclarationSyntax method = _selectedNode.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            Option<ExpressionStatementSyntax> lastRequires = await GetLastRequiresStatement(document, method, token);

            SyntaxNode root = await document.GetSyntaxRootAsync(token);

            SyntaxNode rootWithRequires = root.ReplaceNode(method, RequiresUtils.AddEnsures(method, _semanticModel, lastRequires));
            SyntaxNode rootWithUsings = RequiresUtils.AddContractNamespaceIfNeeded(rootWithRequires);

            return document.WithSyntaxRoot(rootWithUsings);
        }

        private static bool IsSelectedNodeSuiteableForRefactoring(SyntaxNode selectedNode)
        {
            // Naive implementation for now.
            // Add Ensures would be available in amy place in the method.
            return selectedNode.AncestorsAndSelf().OfType<MethodDeclarationSyntax>() != null;
        }

        private async Task<Option<ExpressionStatementSyntax>> GetLastRequiresStatement(Document document, MethodDeclarationSyntax method, CancellationToken token)
        {
            var semanticModel = await document.GetSemanticModelAsync(token);
            var contractBlock = await ContractBlock.CreateForMethodAsync(method, semanticModel, token);

            return contractBlock.Preconditions.LastOrDefault()?.CSharpStatement;
        }
    }
}