using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace CodeContractor.UnitTests.Contracts
{
    /// <summary>
    /// Represents template for the testing class that is useful for testing purposes.
    /// </summary>
    /// <remarks>
    /// TODO: name is terrible!
    /// </remarks>
    public sealed class ClassTemplate
    {
        const string ClassDeclarationTemplate =
            @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class SampleClass
{
  {method}
}
";
        const string ClassDeclarationTemplateWithContractUsings =
            @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

internal class SampleClass
{
  {method}
}
";
        private Document _document;
        private int _position;
        private SyntaxNode _selectedNode;
        private SyntaxNode _root;
        private bool _diagnosticEnabled;
        private string _source;

        public static Task<ClassTemplate> FromFullSource(string source)
        {
            Contract.Requires(source != null);

            var specials = 
                new[] {"{caret}", "{on}", "{off}"}
                    .Select(t => new {Text = t, Position = source.IndexOf(t, StringComparison.Ordinal)}).ToList();

            if (specials.Count(x => x.Position != -1) > 1)
            {
                throw new InvalidOperationException("Source should have only {caret}, {on} or {off}. But not all of them.");
            }

            foreach (var text in specials.Where(x => x.Position != -1))
            {
                source = source.Replace(text.Text, "");
            }

            var special = specials.SingleOrDefault(x => x.Position != -1);

            int position = special?.Position ?? -1;
            bool diagnosticEnabled = special?.Text == "{on}" || special?.Text == "{caret}";

            return FromFullSource(source, position, diagnosticEnabled);
        }

        private static async Task<ClassTemplate> FromFullSource(string source, int position, bool diagnosticEnabled)
        {
            var document = RoslynTestsUtils.CreateDocument(source);
            var root = await document.GetSyntaxRootAsync();
            var selectedNode = root;

            if (position != -1)
            {
                selectedNode = root.FindNode(TextSpan.FromBounds(position, position + 1));
            }

            return new ClassTemplate
            {
                _source = source,
                _position = position == -1 ? 0 : position,
                _document = document,
                _selectedNode = selectedNode,
                _root = root,
                _diagnosticEnabled = diagnosticEnabled,
            };
        }

        public static Task<ClassTemplate> FromMethodAsync(string method, bool withContractUsings = false)
        {
            string template = withContractUsings ? ClassDeclarationTemplateWithContractUsings : ClassDeclarationTemplate;
            var source = template.Replace("{method}", method);

            var specials =
                            new[] { "{caret}", "{on}", "{off}" }
                                .Select(t => new { Text = t, Position = source.IndexOf(t, StringComparison.Ordinal) }).ToList();

            if (specials.Count(x => x.Position != -1) > 1)
            {
                throw new InvalidOperationException("Source should have only {caret}, {on} or {off}. But not all of them.");
            }

            foreach (var text in specials.Where(x => x.Position != -1))
            {
                source = source.Replace(text.Text, "");
            }

            var special = specials.SingleOrDefault(x => x.Position != -1);

            int position = special?.Position ?? -1;
            bool diagnosticEnabled = special?.Text == "{on}" || special?.Text == "{caret}";

            return FromFullSource(source, position, diagnosticEnabled);
        }

        public string Source => _source;

        public SyntaxNode Root => _root;

        public SyntaxNode SelectedNode => _selectedNode;

        public bool DiagnosticEnabled => _diagnosticEnabled;

        public IEnumerable<int> DiagnosticPositions => _diagnosticEnabled ? new int[] {Position} : new int[0];

        public BaseMethodDeclarationSyntax SelectedMethod()
        {
            return SelectedNode.AncestorsAndSelf().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        }

        public int Position => _position;

        public Document Document => _document;

        public SemanticModel SemanticModel => _document.GetSemanticModelAsync().Result;
    }
}