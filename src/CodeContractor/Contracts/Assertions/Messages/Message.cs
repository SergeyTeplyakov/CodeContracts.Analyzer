using System;
using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts.Assertions.Messages
{
    /// <summary>
    /// Represents message in the contract expression.
    /// </summary>
    /// <remarks>
    /// This hierarchy is an OO implementation of the descriminated union with a list of following cases:
    /// <see cref="NoMessage"/>, <see cref="LiteralMessage"/>, <see cref="ReferenceMessage"/> and 
    /// <see cref="InvocationMessage"/>.
    /// </remarks>
    public abstract class Message
    {
        protected Message(Optional<ExpressionSyntax> originalExpression)
        {
            OriginalExpression = originalExpression;
        }

        public static Message Create(ExpressionSyntax expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<Message>() != null);

            //var literal = expression as ICSharpLiteralExpression;
            //if (literal != null)
            //    return new LiteralMessage(expression, literal.Literal.GetText());

            //var reference = expression as IReferenceExpression;
            //if (reference != null)
            //    return new ReferenceMessage(expression, reference);

            //var invocationExpression = expression as IInvocationExpression;
            //if (invocationExpression != null)
            //    return new InvocationMessage(expression, invocationExpression);

            //return NoMessage.Instance;
            throw new NotImplementedException();
        }

        public Optional<ExpressionSyntax> OriginalExpression { get; }
    }
}