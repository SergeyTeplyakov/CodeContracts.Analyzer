using System;
using System.Collections.Immutable;
using CodeContractor.Refactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using CodeContractor.Contracts;
using CodeContractor.Utils;

namespace CodeContractor.Analyzers.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodArgumentNotNullAnalyzer : DiagnosticAnalyzer
    {
        public const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;
        public const string DiagnosticId = "CC001";
        internal const string Title = "Argument not-null analyzer";
        public const string MessageFormatForRequires = "Lack of argument validation for nullable parameter '{0}'.";
        internal const string Category = "CodeSmell";

        private static readonly DiagnosticDescriptor AddRequiresRule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormatForRequires, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AddRequiresRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
        }

        private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
        {
            var enclosingMetod = context.Node.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();

            if (enclosingMetod == null || !enclosingMetod.IsPublicOrProtected())
            {
                return;
            }

            var requiresRefactoring = AddNotNullRequiresRefactoring.Create(context.Node as ParameterSyntax, context.SemanticModel, context.CancellationToken).Result;

            if (requiresRefactoring.IsAvailableAsync(context.CancellationToken).Result)
            {
                var diagnostic = Diagnostic.Create(AddRequiresRule, context.Node.GetLocation(), requiresRefactoring.Parameter.Value?.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}