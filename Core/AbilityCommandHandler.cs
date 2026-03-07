namespace AbilityExecution;

using AbilitySystem;
using EntityTargetingSystem;
using Friflo.Engine.ECS;

/// <summary>
/// Handles ability commands from the CommandSystem.
/// </summary>
/// <remarks>
/// This handler integrates the CommandSystem with the AbilitySystem and EntityTargetingSystem,
/// providing a complete ability execution pipeline.
/// </remarks>
public sealed class AbilityCommandHandler
{
    private readonly AbilityRegistry _abilityRegistry;
    private readonly ITargetingValidator _targetingValidator;

    /// <summary>
    /// Creates a new ability command handler.
    /// </summary>
    /// <param name="abilityRegistry">The ability registry.</param>
    /// <param name="targetingValidator">The targeting validator.</param>
    public AbilityCommandHandler(AbilityRegistry abilityRegistry, ITargetingValidator targetingValidator)
    {
        _abilityRegistry = abilityRegistry;
        _targetingValidator = targetingValidator;
    }

    /// <summary>
    /// Handles an ability command.
    /// </summary>
    /// <param name="command">The ability command to handle.</param>
    /// <param name="world">The ECS world.</param>
    /// <param name="caster">The entity casting the ability.</param>
    /// <returns>The result of handling the command.</returns>
    public AbilityCommandResult Handle(AbilityCommand command, EntityStore world, Entity caster)
    {
        // Validate the command
        var validationResult = ValidateCommand(command, caster);
        if (!validationResult.IsValid)
        {
            return AbilityCommandResult.Failure(validationResult.FailureReason);
        }

        // Get the ability definition
        var abilityDefinition = _abilityRegistry.Get(command.AbilityId);

        // Check mana
        var manaResult = CheckMana(caster, abilityDefinition.ManaCost);
        if (!manaResult.IsValid)
        {
            return AbilityCommandResult.Failure(manaResult.FailureReason);
        }

        // Check cooldown
        var cooldownResult = CheckCooldown(caster, command.AbilityId);
        if (!cooldownResult.IsValid)
        {
            return AbilityCommandResult.Failure(cooldownResult.FailureReason);
        }

        // Validate targeting
        var targetingResult = ValidateTargeting(command, caster);
        if (!targetingResult.CanTarget)
        {
            return AbilityCommandResult.Failure(targetingResult.FailureReason ?? "Invalid target");
        }

        // Execute the ability
        var executionResult = ExecuteAbility(command, caster, abilityDefinition);

        return executionResult;
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
    private ManaCheckResult CheckMana(Entity caster, float manaCost)
    {
        if (!caster.HasComponent<ManaComponent>())
        {
            return ManaCheckResult.Failure("Caster has no mana component");
        }

        ref var mana = ref caster.GetComponent<ManaComponent>();
        if (mana.CurrentMana < manaCost)
        {
            return ManaCheckResult.Failure($"Insufficient mana (need {manaCost}, have {mana.CurrentMana})");
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
    private AbilityCommandResult ExecuteAbility(AbilityCommand command, Entity caster, AbilitySystem.AbilityDefinition abilityDefinition)
    {
        // Consume mana
        if (caster.HasComponent<ManaComponent>())
        {
            ref var mana = ref caster.GetComponent<ManaComponent>();
            mana.ConsumeMana(abilityDefinition.ManaCost);
        }

        // Start cooldown
        if (caster.HasComponent<AbilityCooldownComponent>())
        {
            ref var cooldown = ref caster.GetComponent<AbilityCooldownComponent>();
            cooldown.StartCooldown();
            
            // Add OnCooldownTag if not present
            if (!caster.HasComponent<OnCooldownTag>())
            {
                caster.AddComponent(new OnCooldownTag());
            }
        }

        // NOTE: In a real implementation, you would apply the ability's effects here
        // (damage, healing, buffs, debuffs, etc.)

        return AbilityCommandResult.Success($"Ability '{abilityDefinition.Name}' executed successfully");
    }
}
