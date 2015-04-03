using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts.Assertions.Messages
{
    public sealed class LiteralMessage : Message
    {
        public LiteralMessage(LiteralExpressionSyntax originalExpression)
            : base(originalExpression)
        {
            Contract.Requires(originalExpression != null);
            Literal = originalExpression.ToString();
        }

        public string Literal { get; }
    }
}