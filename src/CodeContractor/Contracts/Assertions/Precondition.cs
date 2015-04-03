using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeContractor.Utils;

namespace CodeContractor.Contracts.Assertions
{
    public sealed class ContractRequires : IPrecondition
    {
        private ContractRequires(ExpressionStatementSyntax originalExpression, PredicateExpression predicateExpression)
        {
            Contract.Requires(originalExpression != null);
            Contract.Requires(predicateExpression != null);

            CSharpStatement = originalExpression;
            PredicateExpression = predicateExpression;
            PreconditionType = PreconditionType.ContractRequires;
        }

        public static Optional<ContractRequires> FromExpressionStatement(ExpressionStatementSyntax expression, SemanticModel semanticModel)
        {
            Contract.Requires(expression != null);
            Contract.Requires(semanticModel != null);

            var memberAccess =
                expression
                .Expression?.As(x => x as InvocationExpressionSyntax)
                ?.Expression.As(x => x as MemberAccessExpressionSyntax);

            var memberSymbol = memberAccess.As(x => semanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol);

            // Looking for Contract.Requires
            if (memberAccess?.Name.ToString() != "Requires" || memberSymbol?.ToString().StartsWith(typeof(Contract).FullName) == false)
            {
                return new Optional<ContractRequires>();
            }

            // Looking for condition expression
            ArgumentSyntax predicate = expression.Expression.As(x => x as InvocationExpressionSyntax).ArgumentList.Arguments.First();

            return new Optional<ContractRequires>(
                new ContractRequires(expression, PredicateExpression.Create(predicate, semanticModel)));
        }

        public bool ChecksForNotNull(ParameterSyntax parameter)
        {
            return PredicateExpression.HasNotNullCheck(parameter);
        }

        public bool UsesParameter(ParameterSyntax parameter)
        {
            return PredicateExpression.Contains(parameter);
        }

        internal PredicateExpression PredicateExpression { get; }

        public ExpressionStatementSyntax CSharpStatement { get; }

        public PreconditionType PreconditionType { get; }
    }
}