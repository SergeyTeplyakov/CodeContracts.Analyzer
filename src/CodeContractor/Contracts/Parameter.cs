using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
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
            Contract.Requires(selectedNode != null);
            throw new System.Exception();
        }
    }

    public static class ParameterSyntaxUtils
    {
        public static bool IsNullable(this TypeSyntax type, SemanticModel semanticModel)
        {
            Contract.Requires(type != null);
            Contract.Requires(semanticModel != null);

            var typeInfo = semanticModel.GetSymbolInfo(type).Symbol as ITypeSymbol;

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
            var systemNullable = semanticModel.Compilation.GetTypeByMetadataName(typeof(Nullable<>).FullName);
            return typeInfo.OriginalDefinition.Equals(systemNullable);

        }

        public static bool IsNullable(this ParameterSyntax parameter, SemanticModel semanticModel)
        {
            Contract.Requires(parameter != null);
            Contract.Requires(parameter.Type != null);
            Contract.Requires(semanticModel != null);

            return parameter.Type.IsNullable(semanticModel);
        }

        public static bool IsDefaultedToNull(this ParameterSyntax parameter)
        {
            Contract.Requires(parameter != null);

            return parameter?.Default?.Value.Kind() == SyntaxKind.NullLiteralExpression;
        }

        public static bool DeclaredMethodIsAbstract(this ParameterSyntax parameter)
        {
            Contract.Requires(parameter != null);

            var method = parameter.AncestorsAndSelf().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();

            return method.IsAbstract();
        }

        public static bool IsAbstract(this BaseMethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration?.Body == null;
        }

        public static async Task<bool> EnsuresReturnValueIsNotNull(
            this BaseMethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken token)
        {
            Contract.Requires(method != null);
            Contract.Requires(semanticModel != null);

            ContractBlock contractBlock = await ContractBlock.CreateForMethodAsync(method, semanticModel, token);

            return contractBlock.Postconditions.Any(p => p.HasNotNullCheck());
        }

        public static async Task<bool> CheckedInMethodContract(this ParameterSyntax parameter, SemanticModel semanticModel, CancellationToken token)
        {
            BaseMethodDeclarationSyntax method = parameter.AncestorsAndSelf().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
            ContractBlock contractBlock = await ContractBlock.CreateForMethodAsync(method, semanticModel, token);

            return contractBlock.Preconditions.Any(p => p.ChecksForNotNull(parameter));
        }
    }

    public static class ParameterSyntaxEx
    {
        public static Option<ParameterSyntax> FindCorrespondingParameter(this SyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            Contract.Requires(syntaxNode != null);
            var parameter = syntaxNode as ParameterSyntax;
            if (parameter != null)
            {
                return parameter;
            }

            var argument = syntaxNode as ArgumentSyntax;
            if (argument != null)
            {
                return argument.FindCorrespondingParameter(semanticModel);
            }

            var identifier = syntaxNode as IdentifierNameSyntax;
            if (identifier != null)
            {
                return identifier.FindCorrespondingParameter(semanticModel);
            }

            return new Option<ParameterSyntax>();
        }

        public static Option<ParameterSyntax> FindCorrespondingParameter(
            this IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            var parameterSymbol = semanticModel.GetSymbolInfo(identifier).Symbol as IParameterSymbol;

            if (parameterSymbol == null || parameterSymbol.Locations.Length == 0)
            {
                return new Option<ParameterSyntax>();
            }

            var location = parameterSymbol.Locations[0].SourceSpan;

            return identifier.SyntaxTree.GetRoot().FindNode(location) as ParameterSyntax;
        }

        private static Option<ParameterSyntax> FindCorrespondingParameter(this ArgumentSyntax argument, SemanticModel semanticModel)
        {
            Contract.Requires(argument != null);
            Contract.Requires(semanticModel != null);

            var identifier = argument.Expression as IdentifierNameSyntax;
            if (identifier == null)
            {
                return new Option<ParameterSyntax>();
            }

            return FindCorrespondingParameter(identifier, semanticModel);
        }
    }
}
