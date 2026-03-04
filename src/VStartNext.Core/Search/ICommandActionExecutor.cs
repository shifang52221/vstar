namespace VStartNext.Core.Search;

public interface ICommandActionExecutor
{
    Task ExecuteOpenTargetAsync(string target);
}
