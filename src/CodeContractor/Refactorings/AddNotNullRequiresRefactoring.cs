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
    public sealed class AddNotNullRequiresRefactoring
    {
        private readonly Optional<ParameterSyntax> _parameter;
        private readonly Document _document;

        private AddNotNullRequiresRefactoring(Optional<ParameterSyntax> parameter, Document document)
        {
            _parameter = parameter;
            _document = document;
        }

        public static async Task<AddNotNullRequiresRefactoring> Create(SyntaxNode node, Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(node != null);
            Contract.Requires(document != null);

            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken);

            return new AddNotNullRequiresRefactoring(node.FindCorrespondingParameter(model), document);
        }

        public async Task<bool> IsAvailableAsync(CancellationToken token)
        {
            if (!_parameter.HasValue || _parameter.Value.Type == null)
            {
                return false;
            }

            var semaniticModel = await _document.GetSemanticModelAsync(token);
            return _parameter.Value.IsNullable(semaniticModel) && 
                  !_parameter.Value.IsDefaultedToNull() && 
                  _parameter.Value.DeclaredMethodIsNotAbstract() &&
                  (!await _parameter.Value.NotCheckedInMethodContract(semaniticModel, token));
        }

        public async Task<Document> ApplyRefactoringAsync(CancellationToken token)
        {
            Contract.Assert(_parameter.HasValue);

            var method = _parameter.Value.AncestorsAndSelf().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();

            if (method == null)
            {
                return _document;
            }

            SyntaxNode root = await _document.GetSyntaxRootAsync(token);

            var parent = await GetParentForCurrentParameter(method, token);

            var rootWithRequires = root.ReplaceNode(method, RequiresUtils.AddRequires(_parameter.Value, method, parent));
            var rootWithUsings = RequiresUtils.AddContractNamespaceIfNeeded(rootWithRequires);

            return _document.WithSyntaxRoot(rootWithUsings);
        }

        private async Task<Optional<ExpressionStatementSyntax>> GetParentForCurrentParameter(BaseMethodDeclarationSyntax method, CancellationToken token)
        {
            Optional<ParameterSyntax> previousParameter = GetPreviousParameter(_parameter.Value);

            if (!previousParameter.HasValue)
            {
                return new Optional<ExpressionStatementSyntax>();
            }

            var semanticModel = await _document.GetSemanticModelAsync(token);
            var contractBlock = await ContractBlock.CreateForMethodAsync(method, semanticModel, token);

            return contractBlock.Preconditions.LastOrDefault(p => p.UsesParameter(previousParameter.Value))?.CSharpStatement;
        }

        private Optional<ParameterSyntax> GetPreviousParameter(ParameterSyntax parameter)
        {
            var parameters = parameter.Parent.As(x => x as ParameterListSyntax).Parameters;
            bool currentFound = false;
            for (int idx = parameters.Count; idx > 0; idx--)
            {
                if (!currentFound)
                {
                    if (parameters[idx-1] == parameter)
                    {
                        currentFound = true;
                    }
                }
                else
                {
                    if (parameters[idx-1] != parameter)
                    {
                        return parameters[idx-1];
                    }
                }
            }

            return new Optional<ParameterSyntax>();
        }
    }
}