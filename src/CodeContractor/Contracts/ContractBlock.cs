using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Contracts.Assertions;
using Microsoft.CodeAnalysis;

namespace CodeContractor.Contracts
{
    /// <summary>
    /// Represents contract block for the method.
    /// </summary>
    public class ContractBlock
    {
        private ContractBlock(IReadOnlyList<IPrecondition> preconditions)
        {
            Contract.Requires(preconditions != null);
            Preconditions = preconditions;
        }

        public static Task<ContractBlock> CreateForMethodAsync(BaseMethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, CancellationToken token = default(CancellationToken))
        {
            Contract.Requires(methodDeclaration != null);
            Contract.Requires(semanticModel != null);

            var preconditions =
                methodDeclaration.DescendantNodes()
                    .OfType<ExpressionStatementSyntax>()
                    .Select(e => ContractRequires.FromExpressionStatement(e, semanticModel))
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();

            return Task.FromResult(new ContractBlock(preconditions));
        }

        public IReadOnlyList<IPrecondition> Preconditions { get; }
    }
}