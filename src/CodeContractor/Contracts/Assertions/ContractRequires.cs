using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeContractor.Utils;

namespace CodeContractor.Contracts.Assertions
{
    public sealed class ContractRequires : CodeContractAssertion, IPrecondition
    {
        internal ContractRequires(ExpressionStatementSyntax originalExpression, PredicateExpression predicateExpression, Messages.Message message)
            : base(originalExpression, predicateExpression, message)
        {}

        /// <summary>
        ///  Returns generic argument type for generic version of the Contract.Requires&lt;ArgumentNullException&gt;.
        /// </summary>
        public Option<object> GenericArgumentType => null;

        public PreconditionType PreconditionType => PreconditionType.ContractRequires;

        public override CodeContractAssertionType CodeContractAssertionType => CodeContractAssertionType.Requires;
    }
}