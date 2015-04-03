using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts.Assertions.Messages
{
    public sealed class InvocationMessage : Message
    {
        public InvocationMessage(InvocationExpressionSyntax originalExpression)
            : base(originalExpression)
        {
            InvocationExpression = originalExpression;
        }

        public InvocationExpressionSyntax InvocationExpression { get; }
    }
}