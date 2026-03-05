namespace VStartNext.Core.Search;

public sealed class CommandPrefixParser
{
    public ParsedPrefix Parse(string input)
    {
        var value = input.Trim();

        if (TryReadPrefix(value, "run:", out var runPayload))
        {
            return new ParsedPrefix(CommandPrefixType.Run, runPayload);
        }

        if (TryReadPrefix(value, "ws:", out var wsPayload))
        {
            return new ParsedPrefix(CommandPrefixType.Ws, wsPayload);
        }

        if (TryReadPrefix(value, "url:", out var urlPayload))
        {
            return new ParsedPrefix(CommandPrefixType.Url, urlPayload);
        }

        if (TryReadPrefix(value, "calc:", out var calcPayload))
        {
            return new ParsedPrefix(CommandPrefixType.Calc, calcPayload);
        }

        return new ParsedPrefix(CommandPrefixType.None, value);
    }

    private static bool TryReadPrefix(string input, string prefix, out string payload)
    {
        if (!input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            payload = string.Empty;
            return false;
        }

        payload = input[prefix.Length..].Trim();
        return true;
    }
}
