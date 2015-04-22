﻿using System;
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
    /// Helper class that adds <see cref="Contract.Requires(bool)"/> for nullable parameters values.
    /// </summary>
    public sealed class AddNotNullRequiresRefactoring : ICodeContractRefactoring
    {
        private readonly Option<ParameterSyntax> _parameter;
        private readonly SemanticModel _semanticModel;

        private AddNotNullRequiresRefactoring(Option<ParameterSyntax> parameter, SemanticModel semanticModel)
        {
            _parameter = parameter;
            _semanticModel = semanticModel;
        }

        public static Task<AddNotNullRequiresRefactoring> Create(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires(node != null);
            Contract.Requires(semanticModel != null);
            Contract.Ensures(Contract.Result<AddNotNullRequiresRefactoring>() != null);

            return Task.FromResult(new AddNotNullRequiresRefactoring(node.FindCorrespondingParameter(semanticModel), semanticModel));
        }

        public async Task<bool> IsAvailableAsync(CancellationToken token)
        {
            if (!_parameter.HasValue || _parameter.Value.Type == null)
            {
                return false;
            }

            return _parameter.Value.IsNullable(_semanticModel) && 
                  !_parameter.Value.IsDefaultedToNull() && 
                  !_parameter.Value.DeclaredMethodIsAbstract() &&
                  (!await _parameter.Value.CheckedInMethodContract(_semanticModel, token));
        }

        public async Task<Document> ApplyRefactoringAsync(Document document, CancellationToken token)
        {
            Contract.Assert(_parameter.HasValue);

            var method = _parameter.Value.AncestorsAndSelf().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();

            if (method == null)
            {
                return document;
            }

            SyntaxNode root = await document.GetSyntaxRootAsync(token);

            var parent = await GetParentForCurrentParameter(document, method, token);

            var rootWithRequires = root.ReplaceNode(method, RequiresUtils.AddRequires(_parameter.Value, method, parent));
            var rootWithUsings = RequiresUtils.AddContractNamespaceIfNeeded(rootWithRequires);

            return document.WithSyntaxRoot(rootWithUsings);
        }

        public Option<ParameterSyntax> Parameter => _parameter;

        private async Task<Option<ExpressionStatementSyntax>> GetParentForCurrentParameter(Document document, BaseMethodDeclarationSyntax method, CancellationToken token)
        {
            Option<ParameterSyntax> previousParameter = GetPreviousParameter(_parameter.Value);

            if (!previousParameter.HasValue)
            {
                return new Option<ExpressionStatementSyntax>();
            }

            var semanticModel = await document.GetSemanticModelAsync(token);
            var contractBlock = await ContractBlock.CreateForMethodAsync(method, semanticModel, token);

            return contractBlock.Preconditions.LastOrDefault(p => p.UsesParameter(previousParameter.Value))?.CSharpStatement;
        }

        private Option<ParameterSyntax> GetPreviousParameter(ParameterSyntax parameter)
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

            return new Option<ParameterSyntax>();
        }
    }
}