using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Refactorings
{
    public sealed class AddNotNullRequiresRefactoring
    {
        private readonly ParameterSyntax _parameter;
        private readonly Document _document;

        private AddNotNullRequiresRefactoring(ParameterSyntax parameter, Document document)
        {
            _parameter = parameter;
            _document = document;
        }

        public static async Task<AddNotNullRequiresRefactoring> Create(SyntaxNode node, Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(node != null);
            Contract.Requires(document != null);

            var parameter = node as ParameterSyntax;
            if (parameter == null)
            {
                var argument = node as ArgumentSyntax;
                if (argument != null)
                {
                    SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                    parameter = argument.FindCorrespondingParameterSyntax(semanticModel);
                }
            }

            return new AddNotNullRequiresRefactoring(parameter, document);
        }

        public async Task<bool> IsAvailableAsync(CancellationToken token)
        {
            if (_parameter == null)
            {
                return false;
            }

            var semaniticModel = await _document.GetSemanticModelAsync(token);
            return _parameter.IsNullable(semaniticModel) && !_parameter.IsDefaultedToNull();
        }

        public async Task<Document> ApplyRefactoringAsync(CancellationToken token)
        {
            Contract.Assert(_parameter != null);

            var method = _parameter.AncestorsAndSelf().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();

            if (method == null)
            {
                return _document;
            }

            SyntaxNode root = await _document.GetSyntaxRootAsync(token);

            var rootWithRequires = root.ReplaceNode(method, RequiresUtils.AddRequires(_parameter, method));
            var rootWithUsings = RequiresUtils.AddContractNamespaceIfNeeded(rootWithRequires);

            return _document.WithSyntaxRoot(rootWithUsings);
        }
    }
}