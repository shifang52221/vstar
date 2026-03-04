using System.Diagnostics;

namespace VStartNext.Infrastructure.Launch;

public interface IProcessStarter
{
    void Start(ProcessStartInfo startInfo);
}

public sealed class SystemProcessStarter : IProcessStarter
{
    public void Start(ProcessStartInfo startInfo)
    {
        Process.Start(startInfo);
    }
}
