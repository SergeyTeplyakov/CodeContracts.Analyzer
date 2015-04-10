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
                return 
                    p.Argument.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>()
                        .Any(be => be.OperatorToken.ToString() == "!=" &&
                                   // null literal could on both side of the expression
                                   (be.Right.ToString() == "null" || be.Left.ToString() == "null"));
            }

            return false;
        }

        public bool HasNotNullCheckWithContractResult()
        {
            foreach (var p in ParametersInUse.OfType<ContractResultPredicateArgument>())
            {
                // Super naive!! Should be added a check for other side of the expression!
                return
                    p.Argument.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>()
                        .Any(be => be.OperatorToken.ToString() == "!=" &&
                                   // null literal could on both side of the expression
                                   (be.Right.ToString() == "null" || be.Left.ToString() == "null"));
            }

            return false;
        }

        public IReadOnlyList<TypeSyntax> GetContractEnsuresTypes()
        {
            return ParametersInUse.OfType<ContractResultPredicateArgument>().Select(x => x.ResultType).ToList();
        }

        public bool Contains(ParameterSyntax parameterSyntax)
        {
            return ParametersInUse.OfType<ParameterReferenceArgument>().Any(x => x.ReferencedParameter.Equals(parameterSyntax));
        }

        public IReadOnlyList<PredicateArgument> ParametersInUse { get; }
    }
}