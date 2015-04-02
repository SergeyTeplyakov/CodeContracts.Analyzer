using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeContractor.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeContractor.Contracts
{
    public sealed class Parameter
    {
        public static Parameter Create(SyntaxNode selectedNode)
        {
            throw new System.Exception();
        }
    }

    public static class ParameterSyntaxEx
    {
        public static bool IsNullable(this ParameterSyntax parameter, SemanticModel semanticModel)
        {
            Contract.Requires(parameter != null);
            Contract.Requires(semanticModel != null);

            var typeInfo = ModelExtensions.GetSymbolInfo(semanticModel, parameter.Type).Symbol as ITypeSymbol;

            if (typeInfo == null)
            {
                // Pessimistic behavior. If symbol is undefined, type is not nullable.
                return false;
            }

            // Both reference types and pointers are nullable
            if (typeInfo.IsReferenceType || typeInfo.TypeKind == TypeKind.Pointer)
            {
                return true;
            }

            // If parameter type is a value type, System.Nulable should be considered
            var systemNullable = semanticModel.Compilation.GetTypeByMetadataName(typeof (System.Nullable<>).FullName);
            return typeInfo.OriginalDefinition.Equals(systemNullable);
        }

        public static bool IsDefaultedToNull(this ParameterSyntax parameter)
        {
            return parameter?.Default?.Value.Kind() == SyntaxKind.NullLiteralExpression;
        }

        public static ParameterSyntax FindCorrespondingParameterSyntax(this ArgumentSyntax argument, SemanticModel semanticModel)
        {
            Contract.Requires(argument != null);
            Contract.Requires(semanticModel != null);
            
            var identifier = argument.Expression as IdentifierNameSyntax;
            if (identifier == null)
            {
                return null;
            }

            var parameterSymbol = semanticModel.GetSymbolInfo(identifier).Symbol as IParameterSymbol;

            if (parameterSymbol == null || parameterSymbol.Locations.Length == 0)
            {
                return null;
            }

            var location = parameterSymbol.Locations[0].SourceSpan;

            return argument.SyntaxTree.GetRoot().FindNode(location) as ParameterSyntax;
        }

    }
}
