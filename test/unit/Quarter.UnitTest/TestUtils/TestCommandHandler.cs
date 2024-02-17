using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Utils;

namespace Quarter.UnitTest.TestUtils;

public class TestCommandHandler : ICommandHandler
{
    public record ExecutedCommand(OperationContext Context, ICommand Command);

    public IList<ExecutedCommand> ExecutedCommands { get; set; } = new List<ExecutedCommand>();

    public Task ExecuteAsync(ICommand command, OperationContext oc, CancellationToken ct)
    {
        ExecutedCommands.Add(new ExecutedCommand(oc, command));
        return Task.CompletedTask;
    }

    public ExecutedCommand LastExecutedCommand()
        => ExecutedCommands.Last();

    public T LastExecutedCommandOrFail<T>() where T : ICommand
    {
        var cmd = ExecutedCommands.Last();
        if (cmd.Command is T res)
            return res;
        throw new Exception($"Expected {nameof(T)} to have been executed");
    }
}
