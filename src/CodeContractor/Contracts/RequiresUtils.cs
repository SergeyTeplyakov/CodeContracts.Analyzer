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

        public static BlockSyntax AddRequires(ParameterSyntax parameter, BlockSyntax block, Option<ExpressionStatementSyntax> anchor)
        {
            StatementSyntax notNullRequiresStatement = CreateNotNullRequiresFor(parameter).WithAdditionalAnnotations(Formatter.Annotation);

            int index = 0;

            // Looking for an anchor in the method body!
            if (anchor.HasValue)
            {
                // New statement should be added after anchor.
                // If anchor is not find, index would be 0
                index = block.Statements.IndexOf(anchor.Value) + 1;
            }

            SyntaxList<StatementSyntax> newStatements = block.Statements.Insert(index, notNullRequiresStatement);
            return block.WithStatements(newStatements).WithAdditionalAnnotations(Formatter.Annotation);
        }

        public static BaseMethodDeclarationSyntax AddRequires(ParameterSyntax parameter, BaseMethodDeclarationSyntax baseMethodDeclaration, Option<ExpressionStatementSyntax> anchor = default(Option<ExpressionStatementSyntax>))
        {
            Contract.Requires(parameter != null);
            Contract.Requires(baseMethodDeclaration != null);
            Contract.Requires(baseMethodDeclaration.Body != null);

            var body = AddRequires(parameter, baseMethodDeclaration.Body, anchor);

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

        public static MethodDeclarationSyntax AddEnsures(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, Option<ExpressionStatementSyntax> anchor = default(Option<ExpressionStatementSyntax>))
        {
            Contract.Requires(methodDeclaration != null);
            Contract.Requires(methodDeclaration.Body != null);

            StatementSyntax notNullEnsures =
                CreateNotNullEnsuresFor(methodDeclaration.UnwrapReturnTypeIfNeeded(semanticModel))
                    .WithAdditionalAnnotations(Formatter.Annotation);

            int index = 0;

            // Looking for an anchor in the method body!
            if (anchor.HasValue)
            {
                // New statement should be added after anchor.
                // If anchor is not find, index would be 0
                index = methodDeclaration.Body.Statements.IndexOf(anchor.Value) + 1;
            }

            SyntaxList<StatementSyntax> newStatements = methodDeclaration.Body.Statements.Insert(index, notNullEnsures);
            BlockSyntax body = methodDeclaration.Body.WithStatements(newStatements).WithAdditionalAnnotations(Formatter.Annotation);

            return methodDeclaration.WithBody(body).WithAdditionalAnnotations(Formatter.Annotation);
        }

        public static SyntaxNode AddContractNamespaceIfNeeded(SyntaxNode tree)
        {
            Contract.Ensures(Contract.Result<SyntaxNode>() != null);
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
                        SyntaxFactory.IdentifierName(typeof(Contract).Namespace))
                        .NormalizeWhitespace()
                        .WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.CarriageReturnLineFeed)));

            return tree.ReplaceNode(compilation, compilation.WithUsings(newUsings));
        }

        private static StatementSyntax CreateNotNullEnsuresFor(TypeSyntax returnType)
        {
            Contract.Ensures(Contract.Result<StatementSyntax>() != null);

            var contractType = typeof(Contract);
            var ensures = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(
                        @"Contract"),
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier(
                            @"Result"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    returnType))))).NormalizeWhitespace();

            var arguments = SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            ensures,
                            SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))
                    ));

            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(contractType.Name),
                        SyntaxFactory.IdentifierName(@"Ensures")))
                    .WithArgumentList(arguments))
                    .NormalizeWhitespace()
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)
                    .WithAdditionalAnnotations(Formatter.Annotation);
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