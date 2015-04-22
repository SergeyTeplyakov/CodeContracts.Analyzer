using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CodeContractor.Utils
{
    /// <summary>
    /// Custom method-like syntax that abstract away method or property syntax.
    /// </summary>
    internal sealed class MethodLikeSyntax
    {
        private readonly IndexerDeclarationSyntax _indexer;
        private readonly BaseMethodDeclarationSyntax _method;

        private MethodLikeSyntax(BaseMethodDeclarationSyntax method)
        {
            _method = method;
        }

        public MethodLikeSyntax(IndexerDeclarationSyntax indexer)
        {
            _indexer = indexer;
        }

        public static MethodLikeSyntax GetEnclosingMethod(ParameterSyntax parameter)
        {
            var method = parameter.AncestorsAndSelf().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
            if (method != null)
            {
                return new MethodLikeSyntax(method);
            }

            // Maybe parameter declared in the indexer
            var indexer = parameter.AncestorsAndSelf().OfType<IndexerDeclarationSyntax>().FirstOrDefault();
            if (indexer == null)
            {
                // That's strage!
                Contract.Assert(false, "This should never happend!! Right?");
                throw new InvalidOperationException("Unknown enclosing declaration for paramter.");
            }

            return new MethodLikeSyntax(indexer);
        }

        public bool IsAbstract()
        {
            if (_method != null)
            {
                return _method.IsAbstract();
            }

            var indexerDeclaration = _indexer.AccessorList.Accessors.FirstOrDefault();
            return indexerDeclaration?.IsAbstract() ?? false;
        }

        public async Task<bool> CheckedInMethodContract(ParameterSyntax parameter, SemanticModel semanticModel, CancellationToken token = default(CancellationToken))
        {
            if (_method != null)
            {
                ContractBlock contractBlock = await ContractBlock.CreateForMethodAsync(_method, semanticModel, token);

                return contractBlock.Preconditions.Any(p => p.ChecksForNotNull(parameter));
            }

            return _indexer.AccessorList.Accessors.All(a =>
            {
                ContractBlock contractBlock = ContractBlock.CreateForMethodAsync(a, semanticModel, token).Result;
                return contractBlock.Preconditions.Any(p => p.ChecksForNotNull(parameter));
            });
        }

        private Option<ParameterSyntax> GetPreviousParameter(ParameterSyntax parameter)
        {
            var parameters = parameter.Parent.As(x => x as ParameterListSyntax)?.Parameters;
            
            // Possible for indexer
            if (parameters == null)
            {
                return null;
            }

            bool currentFound = false;
            for (int idx = parameters.Value.Count; idx > 0; idx--)
            {
                if (!currentFound)
                {
                    if (parameters.Value[idx - 1] == parameter)
                    {
                        currentFound = true;
                    }
                }
                else
                {
                    if (parameters.Value[idx - 1] != parameter)
                    {
                        return parameters.Value[idx - 1];
                    }
                }
            }

            return new Option<ParameterSyntax>();
        }

        private Option<ExpressionStatementSyntax> GetParentForCurrentParameter(ParameterSyntax parameter, ContractBlock contractBlock)
        {
            Option<ParameterSyntax> previousParameter = GetPreviousParameter(parameter);

            if (!previousParameter.HasValue)
            {
                return new Option<ExpressionStatementSyntax>();
            }

            return contractBlock.Preconditions.LastOrDefault(p => p.UsesParameter(previousParameter.Value))?.CSharpStatement;
        }

        public CSharpSyntaxNode CurrentMethod => (CSharpSyntaxNode)_method ?? _indexer;

        public CSharpSyntaxNode AddRequires(ParameterSyntax parameter, SemanticModel semanticModel)
        {
            Contract.Requires(parameter != null);
            Contract.Requires(semanticModel != null);

            if (_method != null)
            {
                var anchor = GetParentForCurrentParameter(parameter, ContractBlock.CreateForMethodAsync(_method, semanticModel).Result);
                return RequiresUtils.AddRequires(parameter, _method, anchor);
            }

            var accessors = new SyntaxList<AccessorDeclarationSyntax>();
            foreach (var accessor in _indexer.AccessorList.Accessors)
            {
                var contractBlock = ContractBlock.CreateForMethodAsync(accessor, semanticModel).Result;
                var anchor = GetParentForCurrentParameter(parameter, contractBlock);
                if (contractBlock.Preconditions.Any(x => x.ChecksForNotNull(parameter)))
                {
                    accessors = accessors.Add(accessor);
                }
                else
                {
                    var body = RequiresUtils.AddRequires(parameter, accessor.Body, anchor);
                    accessors = accessors.Add(accessor.WithBody(body));
                }
            }

            return _indexer.WithAccessorList(_indexer.AccessorList.WithAccessors(accessors));
        }

        public IEnumerable<ContractBlock> GetMethodContracts(SemanticModel semanticModel, CancellationToken token)
        {
            if (_method != null)
            {
                yield return ContractBlock.CreateForMethodAsync(_method, semanticModel, token).Result;
            }

            if (_indexer != null)
            {
                foreach (var accessor in _indexer.AccessorList.Accessors)
                {
                    yield return ContractBlock.CreateForMethodAsync(accessor, semanticModel, token).Result;
                }
            }
        }
    }
}