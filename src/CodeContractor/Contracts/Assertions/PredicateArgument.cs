using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.Contracts;
using System.Linq;
using CodeContractor.Utils;
using Microsoft.CodeAnalysis;

namespace CodeContractor.Contracts.Assertions
{
    /// <summary>
    /// Represents argument for the <seealso cref="PredicateExpression"/>.
    /// </summary>
    public abstract class PredicateArgument
    {
        protected PredicateArgument(ArgumentSyntax argument)
        {
            Contract.Requires(argument != null);
            Argument = argument;
        }

        internal static IReadOnlyList<PredicateArgument> Create(ArgumentSyntax argumentSyntax, SemanticModel semanticModel)
        {
            return
                argumentSyntax.DescendantNodes()
                    .Select(i => Create(argumentSyntax, i, semanticModel))
                    .Where(p => p.HasValue)
                    .Select(p => p.Value)
                    .ToList();
        }

        private static Option<PredicateArgument> Create(ArgumentSyntax argument, SyntaxNode node, SemanticModel semanticModel)
        {
            Contract.Requires(argument != null);
            Contract.Requires(node != null);
            Contract.Requires(semanticModel != null);

            var identifier = node as IdentifierNameSyntax;
            if (identifier != null)
            {
                return
                    identifier.FindCorrespondingParameter(semanticModel)
                    .Bind(parameter => (PredicateArgument)new ParameterReferenceArgument(argument, identifier, parameter));
            }

            var invocationExpression = node as InvocationExpressionSyntax;
            if (invocationExpression != null)
            {
                var memberAccess = invocationExpression.Expression.As(x => x as MemberAccessExpressionSyntax);
                var memberSymbol =
                    invocationExpression
                    .Expression.As(x => x as MemberAccessExpressionSyntax)
                    ?.As(x => semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol);

                // Looking for Contract.Requires
                if (memberSymbol?.ToString().StartsWith(typeof(Contract).FullName) == false)
                {
                    return null;
                }

                var result = memberAccess.Name.As(x => x as GenericNameSyntax);
                if (result == null || result.Identifier.ToString() != "Result")
                {
                    return null;
                }

                TypeSyntax type = result.TypeArgumentList.Arguments.First();
                
                return new Option<PredicateArgument>(new ContractResultPredicateArgument(argument, type));
            }

            return null;
        }

        public ArgumentSyntax Argument { get; }
    }

    /// <summary>
    /// Represents an absence of the argument (this is some kind of Null Object Pattern).
    /// </summary>
    public sealed class EmptyPredicateArgument : PredicateArgument
    {
        public EmptyPredicateArgument(ArgumentSyntax argument) : base(argument)
        {}
    }

    /// <summary>
    /// Represents "reference" argument that contains a name for the argument/field/property.
    /// </summary>
    public sealed class ParameterReferenceArgument : PredicateArgument
    {
        public ParameterReferenceArgument(ArgumentSyntax argument, IdentifierNameSyntax identifier, ParameterSyntax referencedParameter)
            : base(argument)
        {
            Contract.Requires(identifier != null);
            Contract.Requires(referencedParameter != null);

            Identifier = identifier;
            ReferencedParameter = referencedParameter;
        }

        public IdentifierNameSyntax Identifier { get; }

        public ParameterSyntax ReferencedParameter { get; }
    }

    /// <summary>
    /// Represents Contract.Result&lt;T&gt; predicate argument.
    /// </summary>
    public sealed class ContractResultPredicateArgument : PredicateArgument
    {
        public ContractResultPredicateArgument(ArgumentSyntax argument, TypeSyntax genericResultType)
            : base(argument)
        {
            Contract.Requires(genericResultType != null);

            ResultType = genericResultType;
        }

        public TypeSyntax ResultType { get; }
    }

    //    public IDeclaredType ResultType
    //    {
    //        get { return _resultTypeName; }
    //    }

    //    public void SetResultType(IType contractResultType)
    //    {
    //        Contract.Requires(contractResultType != null);

    //        _contractResultReference.SetTypeArguments(new[] { contractResultType });
    //    }

    //    public IClrTypeName ResultTypeName
    //    {
    //        get { return _resultTypeName.GetClrName(); }
    //    }
    //}
}