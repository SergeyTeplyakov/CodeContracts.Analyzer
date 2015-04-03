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
    /// Represents condition for <see cref="ContractAssertion"/>
    /// </summary>
    public sealed class PredicateExpression
    {
        private PredicateExpression(IReadOnlyList<Tuple<IdentifierNameSyntax, ParameterSyntax>> parametersInUse)
        {
            Contract.Requires(parametersInUse != null);

            ParametersInUse = parametersInUse;
        }

        public static PredicateExpression Create(ArgumentSyntax argumentSyntax, SemanticModel semanticModel)
        {
            Contract.Requires(argumentSyntax != null);
            Contract.Requires(semanticModel != null);

            var parameters =
                argumentSyntax.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Select(i => new {Identifier = i, Parameter = i.FindCorrespondingParameter(semanticModel)})
                    .Where(p => p.Parameter.HasValue)
                    .Select(p => Tuple.Create(p.Identifier, p.Parameter.Value))
                    .ToList();

            return new PredicateExpression(parameters);
        }

        public bool HasNotNullCheck(ParameterSyntax parameterSyntax)
        {
            foreach (var kvp in ParametersInUse.Where(x => x.Item2.Equals(parameterSyntax)))
            {
                var binaryExpression = kvp.Item1.Parent.As(x => x as BinaryExpressionSyntax);
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
            return ParametersInUse.Any(x => x.Item2.Equals(parameterSyntax));
        }

        public IReadOnlyList<Tuple<IdentifierNameSyntax, ParameterSyntax>> ParametersInUse { get; }
    }
}