using VStartNext.Core.Agent;

namespace VStartNext.App.Agent;

public static class AgentFinalizingPromptBuilder
{
    private static readonly AgentResponseLanguagePolicy LanguagePolicy = new();

    public static string Build(
        AgentExecutionPreview preview,
        AgentRunResult result,
        string uiLanguage = "zh-CN",
        bool followUiLanguage = false)
    {
        var language = LanguagePolicy.Resolve(preview.Input, uiLanguage, followUiLanguage);
        var executionLines = result.Executions.Count == 0
            ? ["No executed steps."]
            : result.Executions
                .Select((execution, index) =>
                    $"{index + 1}. {execution.ToolName}({execution.Arguments}) => {execution.Message}")
                .ToArray();

        return language switch
        {
            AgentLanguage.Chinese => BuildChineseTemplate(preview, result, executionLines),
            AgentLanguage.English => BuildEnglishTemplate(preview, result, executionLines),
            _ => BuildMixedTemplate(preview, result, executionLines)
        };
    }

    private static string BuildChineseTemplate(
        AgentExecutionPreview preview,
        AgentRunResult result,
        IReadOnlyList<string> executionLines)
    {
        return string.Join(
            '\n',
            [
                "你是桌面启动器执行总结助手。",
                "仅输出中文，使用简洁自然语言，不要输出 Markdown 代码块。",
                "请严格按以下结构输出：",
                "结果: 成功 或 失败",
                "摘要: 一句话概括执行结果",
                "步骤:",
                "1. ...",
                "风险提示: 若无风险请写无",
                "",
                "上下文：",
                $"用户输入: {preview.Input}",
                $"执行成功: {result.Success}",
                $"执行消息: {result.Message}",
                "执行轨迹:",
                ..executionLines
            ]);
    }

    private static string BuildEnglishTemplate(
        AgentExecutionPreview preview,
        AgentRunResult result,
        IReadOnlyList<string> executionLines)
    {
        return string.Join(
            '\n',
            [
                "You are a desktop launcher execution summarizer.",
                "Output only English plain text. Do not output markdown code fences.",
                "Use exactly this structure:",
                "Result: Success or Failed",
                "Summary: one concise sentence",
                "Steps:",
                "1. ...",
                "Risk: write None when there is no risk",
                "",
                "Context:",
                $"User input: {preview.Input}",
                $"Execution success: {result.Success}",
                $"Execution message: {result.Message}",
                "Execution trace:",
                ..executionLines
            ]);
    }

    private static string BuildMixedTemplate(
        AgentExecutionPreview preview,
        AgentRunResult result,
        IReadOnlyList<string> executionLines)
    {
        return string.Join(
            '\n',
            [
                "You are a desktop launcher execution summarizer.",
                "Output bilingual Chinese and English in plain text only.",
                "Use exactly this structure:",
                "结果/Result: 成功/Success 或 失败/Failed",
                "摘要/Summary: 一句话中文 + one concise English sentence",
                "步骤/Steps:",
                "1. ...",
                "风险/Risk: 无/None when no risk",
                "",
                "Context:",
                $"User input: {preview.Input}",
                $"Execution success: {result.Success}",
                $"Execution message: {result.Message}",
                "Execution trace:",
                ..executionLines
            ]);
    }
}
