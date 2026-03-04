namespace VStartNext.Core.Search;

public enum CommandPrefixType
{
    None,
    Ws,
    Url,
    Calc
}

public sealed record ParsedPrefix(CommandPrefixType Type, string Payload);
