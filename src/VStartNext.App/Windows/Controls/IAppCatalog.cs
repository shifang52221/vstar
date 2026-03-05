namespace VStartNext.App.Windows.Controls;

public interface IAppCatalog
{
    IReadOnlyList<LauncherAppEntry> Load(int maxCount = 24);
}
