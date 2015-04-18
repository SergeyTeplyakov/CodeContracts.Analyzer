using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeContractor.Analyzers.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PublicMethodContractAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CC001";
        internal const string Title = "Method contract analyzer";
        public const string MessageFormatForRequires = "Lack of argument validation for nullable parameter '{0}'.";
        public const string MessageFormatForEnsures = "Lack of postcondition for nullable return type '{0}'.";
        internal const string Category = "CodeSmell";

        private static readonly DiagnosticDescriptor AddRequiresRule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormatForRequires, Category, DiagnosticSeverity.Info, isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor AddEnsuresRule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormatForRequires, Category, DiagnosticSeverity.Info, isEnabledByDefault: true);

        public override void Initialize(AnalysisContext context)
        {
            throw new System.NotImplementedException();
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AddRequiresRule, AddEnsuresRule);
    }
}