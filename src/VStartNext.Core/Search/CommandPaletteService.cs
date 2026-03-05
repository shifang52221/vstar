using System.Data;
using System.Globalization;

namespace VStartNext.Core.Search;

public sealed record CommandExecutionResult(bool Success, string DisplayText);

public sealed class CommandPaletteService
{
    private readonly CommandPrefixParser _parser = new();
    private readonly ICommandActionExecutor _executor;

    public CommandPaletteService(ICommandActionExecutor executor)
    {
        _executor = executor;
    }

    public async Task<CommandExecutionResult> ExecuteAsync(string input)
    {
        var parsed = _parser.Parse(input);

        if (parsed.Type == CommandPrefixType.Calc)
        {
            var value = Evaluate(parsed.Payload);
            return new CommandExecutionResult(true, value);
        }

        if (parsed.Type == CommandPrefixType.Run)
        {
            await _executor.ExecuteOpenTargetAsync(parsed.Payload);
            return new CommandExecutionResult(true, parsed.Payload);
        }

        if (parsed.Type == CommandPrefixType.Url)
        {
            await _executor.ExecuteOpenTargetAsync(parsed.Payload);
            return new CommandExecutionResult(true, parsed.Payload);
        }

        if (parsed.Type == CommandPrefixType.Ws)
        {
            var target = NormalizeWebsite(parsed.Payload);
            await _executor.ExecuteOpenTargetAsync(target);
            return new CommandExecutionResult(true, target);
        }

        return new CommandExecutionResult(false, "Unsupported command");
    }

    private static string Evaluate(string expression)
    {
        var dt = new DataTable();
        var raw = dt.Compute(expression, string.Empty);
        if (raw is null)
        {
            return "0";
        }

        var num = Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
        if (decimal.Truncate(num) == num)
        {
            return num.ToString("0", CultureInfo.InvariantCulture);
        }

        return num.ToString(CultureInfo.InvariantCulture);
    }

    private static string NormalizeWebsite(string payload)
    {
        if (payload.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            payload.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return payload;
        }

        return $"https://{payload}";
    }
}
