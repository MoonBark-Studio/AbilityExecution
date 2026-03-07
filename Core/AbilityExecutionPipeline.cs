namespace AbilityExecution;

using AbilitySystem;
using EntityTargetingSystem;
using Friflo.Engine.ECS;

/// <summary>
/// Orchestrates the full ability execution pipeline.
/// </summary>
/// <remarks>
/// This class provides a complete ability execution flow:
/// 1. Validate command
/// 2. Check resources (mana)
/// 3. Check cooldown
/// 4. Validate targeting
/// 5. Execute ability
/// 6. Apply effects
/// 7. Start cooldown
/// </remarks>
public sealed class AbilityExecutionPipeline
{
    private readonly AbilityRegistry _abilityRegistry;
    private readonly ITargetingValidator _targetingValidator;

    /// <summary>
    /// Creates a new ability execution pipeline.
    /// </summary>
    /// <param name="abilityRegistry">The ability registry.</param>
    /// <param name="targetingValidator">The targeting validator.</param>
    public AbilityExecutionPipeline(AbilityRegistry abilityRegistry, ITargetingValidator targetingValidator)
    {
        _abilityRegistry = abilityRegistry;
        _targetingValidator = targetingValidator;
    }

    /// <summary>
    /// Executes an ability command through the full pipeline.
    /// </summary>
    /// <param name="command">The ability command to execute.</param>
    /// <param name="world">The ECS world.</param>
    /// <param name="caster">The entity casting the ability.</param>
    /// <returns>The result of the ability execution.</returns>
    public AbilityExecutionResult Execute(AbilityCommand command, EntityStore world, Entity caster)
    {
        // Step 1: Validate command
        var validationResult = ValidateCommand(command, caster);
        if (!validationResult.IsValid)
        {
            return AbilityExecutionResult.Failed(validationResult.FailureReason, AbilityExecutionStage.CommandValidation);
        }

        // Step 2: Check resources (mana)
        var manaResult = CheckMana(caster, command.AbilityId);
        if (!manaResult.IsValid)
        {
            return AbilityExecutionResult.Failed(manaResult.FailureReason, AbilityExecutionStage.ManaCheck);
        }

        // Step 3: Check cooldown
        var cooldownResult = CheckCooldown(caster, command.AbilityId);
        if (!cooldownResult.IsValid)
        {
            return AbilityExecutionResult.Failed(cooldownResult.FailureReason, AbilityExecutionStage.CooldownCheck);
        }

        // Step 4: Validate targeting
        var targetingResult = ValidateTargeting(command, caster);
        if (!targetingResult.CanTarget)
        {
            return AbilityExecutionResult.Failed(
                targetingResult.FailureReason ?? "Invalid target",
                AbilityExecutionStage.TargetingValidation,
                targetingResult.FailureKind);
        }

        // Step 5: Execute ability
        var executeResult = ExecuteAbility(command, caster);
        if (!executeResult.Succeeded)
        {
            return AbilityExecutionResult.Failed(
                executeResult.FailureReason ?? "Ability execution failed",
                AbilityExecutionStage.AbilityExecution);
        }

        // Step 6: Apply effects
        var applyResult = ApplyEffects(command, caster);
        if (!applyResult.Succeeded)
        {
            return AbilityExecutionResult.Failed(
                applyResult.FailureReason ?? "Failed to apply effects",
                AbilityExecutionStage.EffectApplication);
        }

        // Step 7: Start cooldown
        StartCooldown(caster, command.AbilityId);

        return AbilityExecutionResult.Success($"Ability '{command.AbilityId}' executed successfully");
    }

    /// <summary>
    /// Validates the ability command.
    /// </summary>
    private CommandValidationResult ValidateCommand(AbilityCommand command, Entity caster)
    {
        // Check if caster can cast abilities
        if (!caster.HasComponent<CanCastAbilitiesTag>())
        {
            return CommandValidationResult.Failure("Caster cannot cast abilities");
        }

        // Check if caster knows the ability
        if (caster.HasComponent<AbilityBookComponent>())
        {
            ref var book = ref caster.GetComponent<AbilityBookComponent>();
            if (!book.KnowsAbility(command.AbilityId))
            {
                return CommandValidationResult.Failure("Caster does not know this ability");
            }
        }

        // Check if caster is learning
        if (caster.HasComponent<LearningAbilityTag>())
        {
            return CommandValidationResult.Failure("Caster is currently learning");
        }

        // Check if ability exists
        if (!_abilityRegistry.Exists(command.AbilityId))
        {
            return CommandValidationResult.Failure("Ability does not exist");
        }

        return CommandValidationResult.Success();
    }

    /// <summary>
    /// Checks if the caster has enough mana.
    /// </summary>
    private ManaCheckResult CheckMana(Entity caster, string abilityId)
    {
        if (!caster.HasComponent<ManaComponent>())
        {
            return ManaCheckResult.Failure("Caster has no mana component");
        }

        var abilityDefinition = _abilityRegistry.Get(abilityId);
        ref var mana = ref caster.GetComponent<ManaComponent>();
        
        if (mana.CurrentMana < abilityDefinition.ManaCost)
        {
            return ManaCheckResult.Failure($"Insufficient mana (need {abilityDefinition.ManaCost}, have {mana.CurrentMana})");
        }

        return ManaCheckResult.Success();
    }

    /// <summary>
    /// Checks if the ability is on cooldown.
    /// </summary>
    private CooldownCheckResult CheckCooldown(Entity caster, string abilityId)
    {
        if (!caster.HasComponent<AbilityCooldownComponent>())
        {
            return CooldownCheckResult.Failure("Caster has no cooldown component");
        }

        ref var cooldown = ref caster.GetComponent<AbilityCooldownComponent>();
        if (cooldown.AbilityId != abilityId)
        {
            return CooldownCheckResult.Failure("Cooldown component does not match ability");
        }

        if (cooldown.IsOnCooldown)
        {
            return CooldownCheckResult.Failure($"Ability is on cooldown ({cooldown.RemainingCooldownSeconds:F1}s remaining)");
        }

        return CooldownCheckResult.Success();
    }

    /// <summary>
    /// Validates the targeting for the ability.
    /// </summary>
    private TargetingResult ValidateTargeting(AbilityCommand command, Entity caster)
    {
        // If there's a target entity, validate entity targeting
        if (!string.IsNullOrWhiteSpace(command.TargetEntityId))
        {
            // NOTE: This is a simplified implementation.
            // In a real implementation, you would resolve the target entity from the ID
            // and call _targetingValidator.CanTarget(caster, target, command.AbilityId)
            
            // For now, assume targeting is valid
            return TargetingResult.Success();
        }

        // If there's a target position, validate position targeting
        if (!string.IsNullOrWhiteSpace(command.TargetPosition))
        {
            // NOTE: This is a simplified implementation.
            // In a real implementation, you would parse the position
            // and call _targetingValidator.CanTargetPosition(caster, position, command.AbilityId)
            
            // For now, assume targeting is valid
            return TargetingResult.Success();
        }

        // No target specified - assume self-targeting or area effect
        return TargetingResult.Success();
    }

    /// <summary>
    /// Executes the ability.
    /// </summary>
    private AbilityExecuteResult ExecuteAbility(AbilityCommand command, Entity caster)
    {
        // NOTE: In a real implementation, you would execute the ability's effects here
        // (damage, healing, buffs, debuffs, etc.)
        
        // For now, assume execution succeeds
        return AbilityExecuteResult.Success();
    }

    /// <summary>
    /// Applies the ability's effects.
    /// </summary>
    private EffectApplyResult ApplyEffects(AbilityCommand command, Entity caster)
    {
        // NOTE: In a real implementation, you would apply the ability's effects here
        // (damage to target, healing to caster, buffs, debuffs, etc.)
        
        // For now, assume effects are applied successfully
        return EffectApplyResult.Success();
    }

    /// <summary>
    /// Starts the cooldown for the ability.
    /// </summary>
    private void StartCooldown(Entity caster, string abilityId)
    {
        if (!caster.HasComponent<AbilityCooldownComponent>())
        {
            return;
        }

        ref var cooldown = ref caster.GetComponent<AbilityCooldownComponent>();
        if (cooldown.AbilityId == abilityId)
        {
            cooldown.StartCooldown();
            
            // Add OnCooldownTag if not present
            if (!caster.HasComponent<OnCooldownTag>())
            {
                caster.AddComponent(new OnCooldownTag());
            }
        }
    }
}

/// <summary>
/// Result of an ability execution.
/// </summary>
public readonly record struct AbilityExecutionResult(
    bool Succeeded,
    string? Summary,
    AbilityExecutionStage? FailedStage = null,
    TargetingFailureKind? TargetingFailureKind = null)
{
    /// <summary>
    /// Creates a successful ability execution result.
    /// </summary>
    public static AbilityExecutionResult Success(string summary) => new(true, summary);

    /// <summary>
    /// Creates a failed ability execution result.
    /// </summary>
    public static AbilityExecutionResult Failed(string reason, AbilityExecutionStage stage, TargetingFailureKind? targetingFailureKind = null) =>
        new(false, reason, stage, targetingFailureKind);
}

/// <summary>
/// The stage at which ability execution failed.
/// </summary>
public enum AbilityExecutionStage
{
    /// <summary>
    /// Command validation failed.
    /// </summary>
    CommandValidation,

    /// <summary>
    /// Mana check failed.
    /// </summary>
    ManaCheck,

    /// <summary>
    /// Cooldown check failed.
    /// </summary>
    CooldownCheck,

    /// <summary>
    /// Targeting validation failed.
    /// </summary>
    TargetingValidation,

    /// <summary>
    /// Ability execution failed.
    /// </summary>
    AbilityExecution,

    /// <summary>
    /// Effect application failed.
    /// </summary>
    EffectApplication
}

