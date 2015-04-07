using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using CodeContractor.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts.Assertions
{
    /// <summary>
    /// Represents condition for any contract assertions like requires, ensures or old-fasion if-throw preconditions.
    /// </summary>
    public sealed class PredicateExpression
    {
        private PredicateExpression(IReadOnlyList<PredicateArgument> predicates)
        {
            ParametersInUse = predicates;
        }

        public static PredicateExpression Create(ArgumentSyntax argumentSyntax, SemanticModel semanticModel)
        {
            Contract.Requires(argumentSyntax != null);
            Contract.Requires(semanticModel != null);

            return new PredicateExpression(PredicateArgument.Create(argumentSyntax, semanticModel));
        }

        public bool HasNotNullCheck(ParameterSyntax parameterSyntax)
        {
            foreach (var p in ParametersInUse.OfType<ParameterReferenceArgument>().Where(x => x.ReferencedParameter.Equals(parameterSyntax)))
            {
                var binaryExpression = p.ReferencedParameter.Parent.As(x => x as BinaryExpressionSyntax);
                if (binaryExpression?.OperatorToken.ToString() == "!=" &&
                    // null literal could on both side of the expression
                    (binaryExpression?.Right.ToString() == "null" || binaryExpression?.Left.ToString() == "null"))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(ParameterSyntax parameterSyntax)
        {
            return ParametersInUse.OfType<ParameterReferenceArgument>().Any(x => x.ReferencedParameter.Equals(parameterSyntax));
        }

        public IReadOnlyList<PredicateArgument> ParametersInUse { get; }
    }
}