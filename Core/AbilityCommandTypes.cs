namespace MoonBark.AbilityExecution;

/// <summary>
/// Command to execute an ability.
/// </summary>
public readonly record struct AbilityCommand
{
    /// <summary>
    /// The ID of the ability to execute.
    /// </summary>
    public string AbilityId { get; init; }

    /// <summary>
    /// The ID of the target entity (optional).
    /// </summary>
    public string? TargetEntityId { get; init; }

    /// <summary>
    /// The target position (optional).
    /// </summary>
    public string? TargetPosition { get; init; }
}

/// <summary>
/// Result of command validation.
/// </summary>
public readonly record struct CommandValidationResult(
    bool IsValid,
    string? FailureReason
)
{
    public static CommandValidationResult Success() => new(true, null);
    public static CommandValidationResult Failure(string reason) => new(false, reason);
}

/// <summary>
/// Result of mana check.
/// </summary>
public readonly record struct ManaCheckResult(
    bool IsValid,
    string? FailureReason
)
{
    public static ManaCheckResult Success() => new(true, null);
    public static ManaCheckResult Failure(string reason) => new(false, reason);
}

/// <summary>
/// Result of cooldown check.
/// </summary>
public readonly record struct CooldownCheckResult(
    bool IsValid,
    string? FailureReason
)
{
    public static CooldownCheckResult Success() => new(true, null);
    public static CooldownCheckResult Failure(string reason) => new(false, reason);
}

/// <summary>
/// Result of ability execution.
/// </summary>
public readonly record struct AbilityExecuteResult(
    bool Succeeded,
    string? FailureReason = null
)
{
    public static AbilityExecuteResult Success() => new(true);
    public static AbilityExecuteResult Failure(string reason) => new(false, reason);
}

/// <summary>
/// Result of effect application.
/// </summary>
public readonly record struct EffectApplyResult(
    bool Succeeded,
    string? FailureReason = null
)
{
    public static EffectApplyResult Success() => new(true);
    public static EffectApplyResult Failure(string reason) => new(false, reason);
}

/// <summary>
/// Result of an ability command.
/// </summary>
public readonly record struct AbilityCommandResult(
    bool Succeeded,
    string? Summary
)
{
    public static AbilityCommandResult Success(string summary) => new(true, summary);
    public static AbilityCommandResult Failure(string reason) => new(false, reason);
}
