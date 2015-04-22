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
    public sealed class MethodResultNotNullAnalyzer : DiagnosticAnalyzer
    {
        public const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;
        public const string DiagnosticId = "CC002";
        public const string Title = "Method result not-null analyzer";
        public const string MessageFormatForEnsures = "Lack of not-null ensures for nullable return type '{0}'.";
        internal const string Category = "CodeSmell";

        private static readonly DiagnosticDescriptor AddEnsuresRule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormatForEnsures, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AddEnsuresRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyseReturnType, SyntaxKind.IdentifierToken);
            context.RegisterSyntaxNodeAction(AnalyseReturnType, SyntaxKind.IdentifierName);
            context.RegisterSyntaxNodeAction(AnalyseReturnType, SyntaxKind.PredefinedType);
        }

        private void AnalyseReturnType(SyntaxNodeAnalysisContext context)
        {
            var enclosingMetod = context.Node.Parent as MethodDeclarationSyntax;

            if (enclosingMetod == null || !enclosingMetod.IsPublicOrProtected())
            {
                return;
            }

            var ensuresRefactoring = AddNotNullEnsuresRefactoring.Create(context.Node, context.SemanticModel,
                context.CancellationToken).Result;

            if (ensuresRefactoring.IsAvailableAsync(context.CancellationToken).Result)
            {
                var methodDeclaration = (MethodDeclarationSyntax) context.Node.Parent;
                var diagnostic = Diagnostic.Create(AddEnsuresRule, context.Node.GetLocation(), methodDeclaration.ReturnType);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}