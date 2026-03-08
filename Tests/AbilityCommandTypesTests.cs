namespace AbilityExecution.Tests;

using Xunit;

public sealed class AbilityCommandTypesTests
{
    [Fact]
    public void CommandValidationResult_Failure_PreservesReason()
    {
        CommandValidationResult result = CommandValidationResult.Failure("invalid");

        Assert.False(result.IsValid);
        Assert.Equal("invalid", result.FailureReason);
    }

    [Fact]
    public void AbilityExecuteResult_Success_SetsSucceeded()
    {
        AbilityExecuteResult result = AbilityExecuteResult.Success();

        Assert.True(result.Succeeded);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public void AbilityCommandResult_Failure_PreservesSummary()
    {
        AbilityCommandResult result = AbilityCommandResult.Failure("denied");

        Assert.False(result.Succeeded);
        Assert.Equal("denied", result.Summary);
    }
}
