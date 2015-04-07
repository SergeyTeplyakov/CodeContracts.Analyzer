using System.Diagnostics.Contracts;
using CodeContractor.Contracts.Assertions.Messages;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts.Assertions
{
    /// <summary>
    /// Base class for every contract statements like Contract.Requires, Contract.Ensures etc,
    /// if-throw precondition, guard-base preconditions etc.
    /// </summary>
    public abstract class ContractAssertion
    {
        protected ContractAssertion(ExpressionStatementSyntax statement, PredicateExpression predicateExpression, Message message)
        {
            Contract.Requires(statement != null);
            Contract.Requires(predicateExpression != null);
            Contract.Requires(message != null);

            CSharpStatement = statement;
            PredicateExpression = predicateExpression;
            Message = message;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            
        }

        /// <summary>
        /// Returns true if current assertion checks for null something with specified <paramref name="parameter"/>.
        /// </summary>
        public bool ChecksForNotNull(ParameterSyntax parameter)
        {
            Contract.Requires(parameter != null);
            return PredicateExpression.HasNotNullCheck(parameter);
        }

        /// <summary>
        /// Returns true if current assertion uses parameter <paramref name="parameter"/>.
        /// </summary>
        public bool UsesParameter(ParameterSyntax parameter)
        {
            Contract.Requires(parameter != null);
            return PredicateExpression.Contains(parameter);
        }

        internal PredicateExpression PredicateExpression { get; }

        public ExpressionStatementSyntax CSharpStatement { get; }

        public Message Message { get; }

        //protected static Message ExtractMessage(ICSharpArgument argument)
        //{
        //    Contract.Requires(argument != null);
        //    Contract.Ensures(Contract.Result<Message>() != null);

        //    return argument.Expression.With(Message.Create);
        //}
    }
}