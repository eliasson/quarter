using System.Threading.Tasks;

namespace Quarter.StartupTasks;

public interface IStartupTask
{
    Task ExecuteAsync();
}