namespace VStartNext.Core.Search;

public enum CommandPrefixType
{
    None,
    Run,
    Ws,
    Url,
    Calc
}

public sealed record ParsedPrefix(CommandPrefixType Type, string Payload);
