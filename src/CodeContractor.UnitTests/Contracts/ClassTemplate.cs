using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.Contracts;

namespace CodeContractor.UnitTests.Contracts
{
    class ClassTemplate
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

        public static Task<ClassTemplate> FromFullSource(string source)
        {
            Contract.Requires(source != null);
            var position = source.IndexOf("{caret}");
            source = source.Replace("{caret}", "");

            return FromFullSource(source, position);
        }

        public static async Task<ClassTemplate> FromFullSource(string source, int position)
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
                _position = position == -1 ? 0 : position,
                _document = document,
                _selectedNode = selectedNode,
                _root = root
            };
        }

        public static Task<ClassTemplate> FromMethodAsync(string method, bool withContractUsings = false)
        {
            string template = withContractUsings ? ClassDeclarationTemplateWithContractUsings : ClassDeclarationTemplate;
            var source = template.Replace("{method}", method);

            var position = source.IndexOf("{caret}");
            source = source.Replace("{caret}", "");

            return FromFullSource(source, position);
        }

        public SyntaxNode Root => _root;

        public SyntaxNode SelectedNode => _selectedNode;

        public BaseMethodDeclarationSyntax SelectedMethod()
        {
            return SelectedNode.AncestorsAndSelf().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        }

        public int Position => _position;

        public Document Document => _document;
        public SemanticModel SemanticModel => _document.GetSemanticModelAsync().Result;
    }
}