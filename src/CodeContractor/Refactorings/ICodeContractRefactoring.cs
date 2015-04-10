using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CodeContractor.Refactorings
{
    public interface ICodeContractRefactoring
    {
        Task<bool> IsAvailableAsync(CancellationToken token);
        Task<Document> ApplyRefactoringAsync(CancellationToken token);
    }
}