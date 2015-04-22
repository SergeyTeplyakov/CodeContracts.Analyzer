using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using CodeContractor.Contracts;
using Microsoft.CodeAnalysis;

namespace CodeContractor.Utils
{
    internal static class MethodDeclarationExtensions
    {
        /// <summary>
        /// Helper method that unwraps underlying type from complex types such as <see cref="Task{TResult}"/>
        /// </summary>
        /// <param name="methodDeclaration"></param>
        /// <returns></returns>
        public static TypeSyntax UnwrapReturnTypeIfNeeded(this MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            Contract.Ensures(Contract.Result<TypeSyntax>() != null);

            var returnType = methodDeclaration.ReturnType;

            // Task<T> is a special type
            if (returnType.TypeEquals(typeof (Task<>), semanticModel))
            {
                returnType = ((GenericNameSyntax) returnType).TypeArgumentList.Arguments.First();
            }

            return returnType;
        }

        public static bool IsPublicOrProtected(this BaseMethodDeclarationSyntax methodDeclaration)
        {
            Contract.Requires(methodDeclaration != null);

            return methodDeclaration.Modifiers.Any(x => x.Text == "public" || x.Text == "protected");
        }
    }
}