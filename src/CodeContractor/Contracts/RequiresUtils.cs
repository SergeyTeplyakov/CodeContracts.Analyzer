using System;
using System.Diagnostics.Contracts;
using System.Linq;
using CodeContractor.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CodeContractor.Contracts
{
    public static class RequiresUtils
    {
        public static BaseMethodDeclarationSyntax AddRequires(ParameterSyntax parameter)
        {
            Contract.Requires(parameter != null);
            Contract.Requires(parameter != null);
            var method = parameter.AncestorsAndSelf().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();

            return AddRequires(parameter, method);
        }

        public static BaseMethodDeclarationSyntax AddRequires(ParameterSyntax parameter, BaseMethodDeclarationSyntax baseMethodDeclaration)
        {
            Contract.Requires(parameter != null);
            Contract.Requires(baseMethodDeclaration != null);

            StatementSyntax notNullRequiresStatement = CreateNotNullRequiresFor(parameter).WithAdditionalAnnotations(Formatter.Annotation);

            var newStatements = baseMethodDeclaration.Body.Statements.Insert(0, notNullRequiresStatement);
            BlockSyntax body = baseMethodDeclaration.Body.WithStatements(newStatements).WithAdditionalAnnotations(Formatter.Annotation);

            var methodDeclaration = baseMethodDeclaration as MethodDeclarationSyntax;
            if (methodDeclaration != null)
            {
                return methodDeclaration.WithBody(body).WithAdditionalAnnotations(Formatter.Annotation);
            }

            var constructorDeclaration = baseMethodDeclaration as ConstructorDeclarationSyntax;
            if (constructorDeclaration != null)
            {
                return constructorDeclaration.WithBody(body).WithAdditionalAnnotations(Formatter.Annotation);
            }

            var message = "Unsupported method declaration syntax! Type: " + baseMethodDeclaration.GetType();
            Contract.Assert(false, message);
            throw new InvalidOperationException(message);
        }

        public static SyntaxNode AddContractNamespaceIfNeeded(SyntaxNode tree)
        {
            // TODO: super naive pproach
            // Getting all using statements and looking for System there
            var root = tree;
            var usings =
                root.DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>().SelectMany(nd => nd.Usings)
                .Union(
                    root.DescendantNodesAndSelf().OfType<CompilationUnitSyntax>().SelectMany(nd => nd.Usings)).ToArray();

            bool contractNamespaceUsingExists =
                usings.Any(x => x.Name.GetText().ToString() == typeof(Contract).Namespace);

            if (contractNamespaceUsingExists)
            {
                return tree;
            }

            var compilation = root.DescendantNodesAndSelf().OfType<CompilationUnitSyntax>().First();

            var newUsings =
                compilation.Usings.Add(
                    SyntaxFactory.UsingDirective(
                        SyntaxFactory.IdentifierName(typeof (Contract).Namespace))
                        .NormalizeWhitespace()
                        .WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.CarriageReturnLineFeed)));

            return tree.ReplaceNode(compilation, compilation.WithUsings(newUsings));
        }

        private static StatementSyntax CreateNotNullRequiresFor(ParameterSyntax parameter)
        {
            var contractType = typeof (Contract);

            var arguments = SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            SyntaxFactory.IdentifierName(parameter.Identifier),
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NullLiteralExpression)))));

            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(contractType.Name),
                        SyntaxFactory.IdentifierName(@"Requires")))
                    .WithArgumentList(arguments))
                    .NormalizeWhitespace()
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)
                    .WithAdditionalAnnotations(Formatter.Annotation);
        }
    }
}