namespace VStartNext.Core.Agent;

public sealed class AgentResponseLanguagePolicy
{
    public AgentLanguage Resolve(string input, string uiLanguage, bool followUiLanguage)
    {
        if (followUiLanguage)
        {
            return ResolveUiLanguage(uiLanguage);
        }

        var hasChinese = false;
        var hasEnglish = false;

        foreach (var ch in input)
        {
            if (ch >= '\u4e00' && ch <= '\u9fff')
            {
                hasChinese = true;
                continue;
            }

            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
            {
                hasEnglish = true;
            }
        }

        if (hasChinese && hasEnglish)
        {
            return AgentLanguage.Mixed;
        }

        if (hasChinese)
        {
            return AgentLanguage.Chinese;
        }

        if (hasEnglish)
        {
            return AgentLanguage.English;
        }

        return ResolveUiLanguage(uiLanguage);
    }

    private static AgentLanguage ResolveUiLanguage(string uiLanguage)
    {
        if (uiLanguage.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return AgentLanguage.Chinese;
        }

        if (uiLanguage.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            return AgentLanguage.English;
        }

        return AgentLanguage.Mixed;
    }
}
