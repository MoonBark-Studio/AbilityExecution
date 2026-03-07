namespace AbilityExecution;

using AbilitySystem;
using EntityTargetingSystem;
using Friflo.Engine.ECS;
using System.Numerics;

/// <summary>
/// Bridges the ability system with the targeting system.
/// </summary>
/// <remarks>
/// This class provides a convenient API for checking if an ability can target
/// a specific entity or position, combining the ability registry with the targeting validator.
/// </remarks>
public sealed class AbilityTargetingBridge
{
    private readonly AbilityRegistry _abilityRegistry;
    private readonly ITargetingValidator _targetingValidator;

    /// <summary>
    /// Creates a new ability targeting bridge.
    /// </summary>
    /// <param name="abilityRegistry">The ability registry.</param>
    /// <param name="targetingValidator">The targeting validator.</param>
    public AbilityTargetingBridge(AbilityRegistry abilityRegistry, ITargetingValidator targetingValidator)
    {
        _abilityRegistry = abilityRegistry;
        _targetingValidator = targetingValidator;
    }

    /// <summary>
    /// Checks if a caster can target an entity with a specific ability.
    /// </summary>
    /// <param name="caster">The entity attempting to cast the ability.</param>
    /// <param name="target">The target entity.</param>
    /// <param name="abilityId">The ID of the ability.</param>
    /// <returns>True if the ability can target the entity, false otherwise.</returns>
    public bool CanTargetAbility(Entity caster, Entity target, string abilityId)
    {
        // Check if ability exists
        if (!_abilityRegistry.Exists(abilityId))
        {
            return false;
        }

        // Use targeting validator
        var result = _targetingValidator.CanTarget(caster, target, abilityId);
        return result.CanTarget;
    }

    /// <summary>
    /// Checks if a caster can target a position with a specific ability.
    /// </summary>
    /// <param name="caster">The entity attempting to cast the ability.</param>
    /// <param name="position">The target position.</param>
    /// <param name="abilityId">The ID of the ability.</param>
    /// <returns>True if the ability can target the position, false otherwise.</returns>
    public bool CanTargetAbilityAtPosition(Entity caster, Vector2 position, string abilityId)
    {
        // Check if ability exists
        if (!_abilityRegistry.Exists(abilityId))
        {
            return false;
        }

        // Use targeting validator
        var result = _targetingValidator.CanTargetPosition(caster, position, abilityId);
        return result.CanTarget;
    }

    /// <summary>
    /// Gets detailed targeting information for an ability.
    /// </summary>
    /// <param name="caster">The entity attempting to cast the ability.</param>
    /// <param name="target">The target entity.</param>
    /// <param name="abilityId">The ID of the ability.</param>
    /// <returns>The targeting result with detailed information.</returns>
    public TargetingResult GetTargetingInfo(Entity caster, Entity target, string abilityId)
    {
        // Check if ability exists
        if (!_abilityRegistry.Exists(abilityId))
        {
            return TargetingResult.Failure($"Ability '{abilityId}' does not exist", TargetingFailureKind.Other);
        }

        // Use targeting validator
        return _targetingValidator.CanTarget(caster, target, abilityId);
    }

    /// <summary>
    /// Gets detailed targeting information for a position.
    /// </summary>
    /// <param name="caster">The entity attempting to cast the ability.</param>
    /// <param name="position">The target position.</param>
    /// <param name="abilityId">The ID of the ability.</param>
    /// <returns>The targeting result with detailed information.</returns>
    public TargetingResult GetPositionTargetingInfo(Entity caster, Vector2 position, string abilityId)
    {
        // Check if ability exists
        if (!_abilityRegistry.Exists(abilityId))
        {
            return TargetingResult.Failure($"Ability '{abilityId}' does not exist", TargetingFailureKind.Other);
        }

        // Use targeting validator
        return _targetingValidator.CanTargetPosition(caster, position, abilityId);
    }
}

