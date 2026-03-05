using System.Collections.ObjectModel;

namespace VStartNext.App.Windows.Controls;

public sealed class ShortcutAppCatalog : IAppCatalog
{
    private static readonly string[] SupportedExtensions = [".lnk", ".url", ".appref-ms"];
    private readonly IReadOnlyList<string> _roots;

    public ShortcutAppCatalog(IEnumerable<string>? roots = null)
    {
        _roots = roots?.ToArray() ?? ResolveDefaultRoots();
    }

    public IReadOnlyList<LauncherAppEntry> Load(int maxCount = 24)
    {
        if (maxCount <= 0)
        {
            return [];
        }

        var map = new Dictionary<string, LauncherAppEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in _roots)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                continue;
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(file);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (!map.ContainsKey(name))
                {
                    map[name] = new LauncherAppEntry(name, file);
                }
            }
        }

        return new ReadOnlyCollection<LauncherAppEntry>(
            map.Values
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Take(maxCount)
                .ToList());
    }

    private static IReadOnlyList<string> ResolveDefaultRoots()
    {
        return
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"),
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
        ];
    }
}
