using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts.Assertions.Messages
{
    public sealed class ParameterReferenceMessage : Message
    {
        public ParameterReferenceMessage(IdentifierNameSyntax originalExpression, ParameterSyntax referencedParameter)
            : base(originalExpression)
        {
            //Contract.Requires(reference != null);
            ReferencedParameter = referencedParameter;
        }

        public ParameterSyntax ReferencedParameter { get; }
    }
}