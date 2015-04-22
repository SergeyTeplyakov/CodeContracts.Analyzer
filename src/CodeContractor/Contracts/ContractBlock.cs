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
        private ContractBlock(IReadOnlyList<IPrecondition> preconditions, IReadOnlyList<ContractEnsures> postconditions)
        {
            Contract.Requires(preconditions != null);
            Contract.Requires(postconditions != null);

            Preconditions = preconditions;
            Postconditions = postconditions;
        }

        public static Task<ContractBlock> CreateForMethodAsync(AccessorDeclarationSyntax methodDeclaration,
            SemanticModel semanticModel, CancellationToken token = default(CancellationToken))
        {
            Contract.Requires(methodDeclaration != null);
            Contract.Requires(semanticModel != null);
            Contract.Ensures(Contract.Result<ContractBlock>() != null);

            var contractStatements =
                methodDeclaration.DescendantNodes()
                    .OfType<ExpressionStatementSyntax>()
                    .Select(e => CodeContractAssertion.Create(e, semanticModel))
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();

            var preconditions = contractStatements.OfType<IPrecondition>().ToList();
            var postconditions = contractStatements.OfType<ContractEnsures>().ToList();

            return Task.FromResult(new ContractBlock(preconditions, postconditions));
        }

        public static Task<ContractBlock> CreateForMethodAsync(BaseMethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, CancellationToken token = default(CancellationToken))
        {
            Contract.Requires(methodDeclaration != null);
            Contract.Requires(semanticModel != null);
            Contract.Ensures(Contract.Result<ContractBlock>() != null);

            var contractStatements =
                methodDeclaration.DescendantNodes()
                    .OfType<ExpressionStatementSyntax>()
                    .Select(e => CodeContractAssertion.Create(e, semanticModel))
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();

            var preconditions = contractStatements.OfType<IPrecondition>().ToList();
            var postconditions = contractStatements.OfType<ContractEnsures>().ToList();

            return Task.FromResult(new ContractBlock(preconditions, postconditions));
        }

        public IReadOnlyList<IPrecondition> Preconditions { get; }
        public IReadOnlyList<ContractEnsures> Postconditions { get; }
    }
}