using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CodeContractor.Contracts.Assertions.Messages;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts.Assertions
{
    /// <summary>
    /// Represents Contract.Ensures expression.
    /// </summary>
    public sealed class ContractEnsures : CodeContractAssertion
    {
        // TODO: change to sequence of result types!!
        //private readonly ContractResultPredicateArgument _contractResultArgument;

        internal ContractEnsures(ExpressionStatementSyntax statement, PredicateExpression predicateExpression, Message message) 
            : base(statement, predicateExpression, message)
        {
            //_contractResultArgument =
            //    predicateExpression.Predicates.Select(p => p.Argument)
            //        .OfType<ContractResultPredicateArgument>()
            //        .FirstOrDefault();
        }

        public override CodeContractAssertionType CodeContractAssertionType => CodeContractAssertionType.Ensures;

        //public IList<IDeclaredType> ContractResultTypes
        //{
        //    get 
        //    { 
        //        return 
        //            Predicates
        //            .Select(p => p.Argument)
        //            .OfType<ContractResultPredicateArgument>()
        //            .Select(pa => pa.ResultType)
        //            .ToList(); 
        //    }
        //}

        //[CanBeNull]
        //public IDeclaredType DeclaredResultType
        //{
        //    get { return _contractResultArgument.Return(x => x.ResultType); }
        //}

        public void SetContractResultType(object contractResultType)
        {
            Contract.Requires(contractResultType != null);
            //Contract.Requires(DeclaredResultType != null, "Nothing to change!");

            //foreach (var contractResult in Predicates.Select(p => p.Argument).OfType<ContractResultPredicateArgument>())
            //{
            //    contractResult.SetResultType(contractResultType);
            //}
        }

        //[CanBeNull]
        //public IClrTypeName ResultType
        //{
        //    get { return DeclaredResultType.Return(x => x.GetClrName()); }
        //}
    }
}