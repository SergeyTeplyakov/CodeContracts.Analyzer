using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using CodeContractor.Contracts.Assertions.Messages;
using CodeContractor.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeContractor.Contracts.Assertions
{
    /// <summary>
    /// Represents one Assertion from Code Contract library, like Contract.Requires, Contract.Invariant etc.
    /// </summary>
    /// <remarks>
    /// Every valid precondtion contains following:
    /// Contract.Method(originalPredicates, message).
    /// 
    /// Note that this class is not suitable for Contract.Ensures because it has slightly 
    /// different internal structure.
    /// </remarks>
    public abstract class CodeContractAssertion : ContractAssertion, ICodeContractAssertion
    {
        private delegate CodeContractAssertion FactoryMethod(
            ExpressionStatementSyntax expression, PredicateExpression predicateExpression, Message message);

        private static Dictionary<CodeContractAssertionType, FactoryMethod> _factoryMethods = new Dictionary<CodeContractAssertionType, FactoryMethod>()
        {
            [CodeContractAssertionType.Requires] = (expression, predicateExpression, message) => new ContractRequires(expression, predicateExpression, message),
            [CodeContractAssertionType.Ensures] = (expression, predicateExpression, message) => new ContractEnsures(expression, predicateExpression, message)
        };
        
        protected CodeContractAssertion(ExpressionStatementSyntax statement, PredicateExpression predicateExpression, Message message)
            : base(statement, predicateExpression, message)
        {}

        //public IExpression OriginalPredicateExpression
        //{
        //    get
        //    {
        //        Contract.Ensures(Contract.Result<IExpression>() != null);
        //        return _predicateExpression.OriginalPredicateExpression;
        //    }
        //}

        public abstract CodeContractAssertionType CodeContractAssertionType { get; }

        internal static Option<CodeContractAssertion> Create(ExpressionStatementSyntax expression, SemanticModel semanticModel)
        {
            Contract.Requires(expression != null);
            Contract.Requires(semanticModel != null);

            CodeContractAssertionType? assertionType = GetContractAssertionType(expression, semanticModel);
            if (assertionType == null)
                return null;

            // Looking for condition expression
            var arguments = expression.Expression.As(x => x as InvocationExpressionSyntax).ArgumentList.Arguments;

            if (arguments.Count == 0)
                return null;

            PredicateExpression predicate = PredicateExpression.Create(arguments[0], semanticModel);
            Message message = Message.Create(arguments.Skip(1).FirstOrDefault());

            var factory = _factoryMethods[assertionType.Value];
            return factory(expression, predicate, message);
        }

        //[CanBeNull]
        //internal static CodeContractAssertionBase FromInvocationExpression(IInvocationExpression invocationExpression)
        //{
        //    Contract.Requires(invocationExpression != null);

        //    var statement = invocationExpression.GetContainingStatement();
        //    Contract.Assert(statement != null);

        //    CodeContractAssertionType? assertionType = GetContractAssertionType(invocationExpression);
        //    if (assertionType == null)
        //        return null;

        //    Contract.Assert(invocationExpression.Arguments.Count != 0, "Invocation expression should have at least one argument!");

        //    IExpression originalPredicateExpression = invocationExpression.Arguments[0].Expression;

        //    var predicateExpression = PredicateExpression.Create(originalPredicateExpression);
        //    var message = ExtractMessage(invocationExpression);

        //    // TODO: switch to dictionary of factory methods?
        //    switch (assertionType.Value)
        //    {
        //        case CodeContractAssertionType.Requires:
        //            return new ContractRequires(statement, invocationExpression, 
        //                predicateExpression, message);
        //        case CodeContractAssertionType.Ensures:
        //            return new ContractEnsures(statement, predicateExpression, message);
        //        case CodeContractAssertionType.Invariant:
        //            return new ContractInvariant(statement, predicateExpression, message);
        //        case CodeContractAssertionType.Assert:
        //            return new ContractAssert(statement, predicateExpression, message);
        //        case CodeContractAssertionType.Assume:
        //            return new ContractAssume(statement, predicateExpression, message);
        //        default:
        //            Contract.Assert(false, "Unknown assertion type: " + assertionType.Value);
        //            return null;
        //    }
        //}

        private static CodeContractAssertionType? GetContractAssertionType(ExpressionStatementSyntax expression, SemanticModel semanticModel)
        {
            var memberAccess =
                expression
                .Expression?.As(x => x as InvocationExpressionSyntax)
                ?.Expression.As(x => x as MemberAccessExpressionSyntax);

            var memberSymbol = memberAccess?.As(x => semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol);

            // Looking for Contract.Requires
            if (memberSymbol?.ToString().StartsWith(typeof(Contract).FullName) == false)
            {
                return null;
            }

            var method = memberAccess?.Name.ToString();

            return ParseAssertionType(method);
        }

        private static CodeContractAssertionType? ParseAssertionType(string method)
        {
            CodeContractAssertionType result;
            if (Enum.TryParse(method, out result))
                return result;
            return null;
        }

        //private static Message ExtractMessage(IInvocationExpression invocationExpression)
        //{
        //    Contract.Requires(invocationExpression != null);
        //    Contract.Ensures(Contract.Result<Message>() != null);
        //    Contract.Assert(invocationExpression.Arguments.Count != 0);

        //    return invocationExpression.Arguments.Skip(1).FirstOrDefault()
        //        .With(x => x.Expression)
        //        .Return(Message.Create, NoMessage.Instance);
        //}
    }
}