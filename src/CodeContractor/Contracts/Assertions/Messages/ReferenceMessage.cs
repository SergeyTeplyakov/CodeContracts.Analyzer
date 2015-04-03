using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts.Assertions.Messages
{
    public sealed class ReferenceMessage : Message
    {
        public ReferenceMessage(IdentifierNameSyntax originalExpression)
            : base(originalExpression)
        {
            //Contract.Requires(reference != null);
            //_reference = reference;
        }

        //public IReferenceExpression Reference
        //{
        //    get
        //    {
        //        Contract.Ensures(Contract.Result<IReferenceExpression>() != null);
        //        return _reference;
        //    }
        //}
    }
}