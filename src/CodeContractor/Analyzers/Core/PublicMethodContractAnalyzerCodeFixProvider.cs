using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using CodeContractor.Refactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace CodeContractor.Analyzers.Core
{
    [ExportCodeFixProvider("PublicMethodContractAnalyzerCodeFixProvider", LanguageNames.CSharp), Shared]
    public class PublicMethodContractAnalyzerCodeFixProvider : CodeFixProvider
    {
        public const string FixText = @"Add not-null assertion(s)";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodArgumentNotNullAnalyzer.DiagnosticId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            return Task.FromResult(42);
            //var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            //var diagnostic = context.Diagnostics.First();

            //var diagnosticSpan = diagnostic.Location.SourceSpan;
            //var token = root.FindToken(diagnosticSpan.Start);

            //// Looks terribly complicated! TODO: think about other approaches!
            //// Unfortunately, there is no API to link diagnostic with some additional custom object!
            //Func<SyntaxNode, SemanticModel, Task<ICodeContractRefactoring>> factory =
            //    MethodArgumentNotNullAnalyzer.GetRefactoring(diagnostic);

            //ICodeContractRefactoring fix = await factory(token.Parent, await context.Document.GetSemanticModelAsync(context.CancellationToken));

            //if (await fix.IsAvailableAsync(context.CancellationToken))
            //{
            //    var codeAction = CodeAction.Create(FixText, ct => fix.ApplyRefactoringAsync(context.Document, context.CancellationToken));
            //    context.RegisterCodeFix(codeAction, diagnostic);
            //}
        }
    }
}