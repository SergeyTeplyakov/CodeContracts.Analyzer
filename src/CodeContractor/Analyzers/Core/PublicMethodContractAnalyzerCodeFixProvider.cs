using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Diagnostics.Contracts;

namespace CodeContractor.Analyzers.Core
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class PublicMethodContractAnalyzerCodeFixProvider : CodeFixProvider
    {
        public const string FixText = @"Add not-null method contract";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PublicMethodContractAnalyzer.DiagnosticId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Contract.Ensures(Contract.Result<Task>() != null);

            // Create a new block with a list that contains a throw statement.
            var throwStatement = SyntaxFactory.ThrowStatement();

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var token = root.FindToken(diagnosticSpan.Start); // This is catch keyword.

            var catchBlock = token.Parent as CatchClauseSyntax;

            //var blockWithThrow = catchBlock.Block.Statements.Add(throwStatement);
            //var newBlock = SyntaxFactory.Block().WithStatements(blockWithThrow).WithAdditionalAnnotations(Formatter.Annotation);
            var newBlock = catchBlock.Block.AddStatements(throwStatement).WithAdditionalAnnotations(Formatter.Annotation);
            var newRoot = root.ReplaceNode(catchBlock, catchBlock.WithBlock(newBlock));

            var codeAction = CodeAction.Create(FixText, ct => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)));
            context.RegisterCodeFix(codeAction, diagnostic);
        }


    }
}