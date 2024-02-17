using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Utils;

namespace Quarter.Core.Commands
{
    public interface ICommand
    {
    }

    public interface ICommandHandler
    {
        Task ExecuteAsync(ICommand command, OperationContext oc, CancellationToken ct);
    }
}
