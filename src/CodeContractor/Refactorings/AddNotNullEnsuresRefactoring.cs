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
        private readonly Document _document;

        private AddNotNullEnsuresRefactoring(SyntaxNode selectedNode, Document document)
        {
            Contract.Requires(selectedNode != null);
            Contract.Requires(document != null);

            _selectedNode = selectedNode;
            _document = document;
        }

        public static async Task<AddNotNullEnsuresRefactoring> Create(SyntaxNode selectedNode, Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(selectedNode != null);
            Contract.Requires(document != null);

            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken);

            return new AddNotNullEnsuresRefactoring(selectedNode, document);
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

            var semanticModel = await _document.GetSemanticModelAsync(token);

            return methodDeclaration.ReturnType.IsNullable(semanticModel) &&
                   !methodDeclaration.IsAbstract() &&
                   (!await methodDeclaration.EnsuresReturnValueIsNotNull(semanticModel, token));
        }

        private static bool IsSelectedNodeSuiteableForRefactoring(SyntaxNode selectedNode)
        {
            // Naive implementation for now.
            // Add Ensures would be available in amy place in the method.
            return selectedNode.AncestorsAndSelf().OfType<MethodDeclarationSyntax>() != null;
        }

        public async Task<Document> ApplyRefactoringAsync(CancellationToken token)
        {
            MethodDeclarationSyntax method = _selectedNode.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            Option<ExpressionStatementSyntax> lastRequires = await GetLastRequiresStatement(method, token);

            SyntaxNode root = await _document.GetSyntaxRootAsync(token);

            SyntaxNode rootWithRequires = root.ReplaceNode(method, RequiresUtils.AddEnsures(method, lastRequires));
            SyntaxNode rootWithUsings = RequiresUtils.AddContractNamespaceIfNeeded(rootWithRequires);

            return _document.WithSyntaxRoot(rootWithUsings);
        }

        private async Task<Option<ExpressionStatementSyntax>> GetLastRequiresStatement(MethodDeclarationSyntax method, CancellationToken token)
        {
            var semanticModel = await _document.GetSemanticModelAsync(token);
            var contractBlock = await ContractBlock.CreateForMethodAsync(method, semanticModel, token);

            return contractBlock.Preconditions.LastOrDefault()?.CSharpStatement;
        }
    }
}