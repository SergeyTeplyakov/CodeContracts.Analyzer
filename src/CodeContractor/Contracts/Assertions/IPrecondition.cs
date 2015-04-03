using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts.Assertions

{
    public enum PreconditionType
    {
        GenericContractRequires,
        ContractRequires,
        IfThrowStatement
    }

    /// <summary>
    /// Marker interface for any type of precondition: including if-throw, guards and Contract.Requires.
    /// </summary>
    public interface IPrecondition
    {
        ExpressionStatementSyntax CSharpStatement { get; }

        PreconditionType PreconditionType { get; }

        /// <summary>
        /// Returns true if current Assertion checks for null something with specified <paramref name="parameter"/>.
        /// </summary>
        bool ChecksForNotNull(ParameterSyntax parameter);

        /// <summary>
        /// Returns true if parameter is used by current precondition.
        /// </summary>
        bool UsesParameter(ParameterSyntax parameter);
    }
}