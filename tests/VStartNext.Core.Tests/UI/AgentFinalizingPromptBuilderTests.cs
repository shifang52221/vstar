using FluentAssertions;
using VStartNext.App.Agent;
using VStartNext.Core.Agent;

namespace VStartNext.Core.Tests.UI;

public class AgentFinalizingPromptBuilderTests
{
    [Fact]
    public void Build_WithChineseInput_UsesChineseTemplate()
    {
        var preview = new AgentExecutionPreview(
            "打开 浏览器",
            [new AgentPlanStep("launch_app", "浏览器", AgentRiskLevel.Low)]);
        var result = new AgentRunResult(
            true,
            "Completed",
            [new AgentStepExecution("launch_app", "浏览器", true, "ok:browser")]);

        var prompt = AgentFinalizingPromptBuilder.Build(preview, result);

        prompt.Should().Contain("你是桌面启动器执行总结助手");
        prompt.Should().Contain("仅输出中文");
        prompt.Should().Contain("结果:");
        prompt.Should().Contain("步骤:");
        prompt.Should().Contain("launch_app(浏览器) => ok:browser");
    }

    [Fact]
    public void Build_WithEnglishInput_UsesEnglishTemplate()
    {
        var preview = new AgentExecutionPreview(
            "open chrome",
            [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]);
        var result = new AgentRunResult(
            false,
            "Launch failed",
            [new AgentStepExecution("launch_app", "chrome", false, "process not found")]);

        var prompt = AgentFinalizingPromptBuilder.Build(preview, result);

        prompt.Should().Contain("You are a desktop launcher execution summarizer");
        prompt.Should().Contain("Output only English");
        prompt.Should().Contain("Result:");
        prompt.Should().Contain("Steps:");
        prompt.Should().Contain("launch_app(chrome) => process not found");
    }

    [Fact]
    public void Build_WithMixedInput_UsesBilingualTemplateAndFallbackStep()
    {
        var preview = new AgentExecutionPreview(
            "打开 chrome and edge",
            [new AgentPlanStep("launch_app", "chrome", AgentRiskLevel.Low)]);
        var result = new AgentRunResult(
            true,
            "Completed",
            []);

        var prompt = AgentFinalizingPromptBuilder.Build(preview, result);

        prompt.Should().Contain("Output bilingual Chinese and English");
        prompt.Should().Contain("结果/Result:");
        prompt.Should().Contain("步骤/Steps:");
        prompt.Should().Contain("No executed steps.");
    }
}
