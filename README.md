# AbilityExecution

A C# ECS plugin for orchestrating ability execution pipelines in Godot games using Friflo.Engine.ECS.

## Purpose

This plugin provides a complete ability execution pipeline that integrates two core systems:
- **AbilitySystem** - Ability definitions and components
- **EntityTargetingSystem** - Targeting validation

## Core Functionality

### 1. Ability Execution Pipeline

The `AbilityExecutionPipeline` class orchestrates the complete ability execution flow:

1. **Command Validation** - Validates ability command structure
2. **Resource Check** - Verifies mana/sufficient resources
3. **Cooldown Check** - Ensures ability is not on cooldown
4. **Targeting Validation** - Validates target selection using targeting system
5. **Ability Execution** - Executes the ability logic
6. **Effect Application** - Applies ability effects (damage, healing, etc.)
7. **Cooldown Management** - Starts ability cooldown

### 2. Command Handling

The `AbilityCommandHandler` provides a simplified command handling interface:
- Processes `AbilityCommand` from CommandSystem
- Returns detailed `AbilityCocanslt`isuccess/failure information
- Provides clear failure reasons at each validation stage

### 3. Targeting Bridge

The `AbilityTargetingBridge` integrates ability and targeting systems:
- Checks if an ability can target a specific entity
- Checks if an ability can target a specific position
- Provides detailed targeting information including failure reasons

## Architecture

```
AbilityExecution
├── AbilityExecutionPipeline    # Full pipeline orchestration
├── AbilityCommandHandler       # Command processing
└── AbilityTargetingBridge      # Targeting integration
```

### Dependencies

- **AbilitySystem** - Ability definitions, components, and registry
- **EntityTargetingSystem** - Targeting validation logic
- **Friflo.Engine.ECS** - ECS framework

## Usage Example

### Setting Up the Pipeline

```csharp
// Create the pipeline with required dependencies
var abilityRegistry = new AbilityRegistry();
var targetingValidator = new FactionTargetingValidator(); // Your implementation
var pipeline = new AbilityExecutionPipeline(abilityRegistry, targetingValidator);
```

### Executing an Ability

```csharp
// Create an ability command
var command = new AbilityCommand
{
    AbilityId = "fireball",
    TargetEntityId = "enemy_123",
    TargetPosition = null
};

// Execute the ability
var result = pipeline.Execute(command, world, casterEntity);

if (result.Succeeded)
{
    Console.WriteLine(result.Summary);
}
else
{
    Console.WriteLine($"Failed at stage: {result.FailedStage}");
    Console.WriteLine($"Reason: {result.Summary}");
}
```

### Using the Command Handler

```csharp
var handler = new AbilityCommandHandler(abilityRegistry, targetingValidator);
var result = handler.Handle(command, world, casterEntity);
```

### Using the Targeting Bridge

```csharp
var bridge = new AbilityTargetingBridge(abilityRegistry, targetingValidator);

// Check if can target entity
bool canTarget = bridge.CanTargetAbility(caster, target, "fireball");

// Get detailed targeting info
var targetingInfo = bridge.GetTargetingInfo(caster, target, "fireball");
if (!targetingInfo.CanTarget)
{
    Console.WriteLine($"Cannot target: {targetingInfo.FailureReason}");
    Console.WriteLine($"Failure kind: {targetingInfo.FailureKind}");
}
```

## Execution Stages

The pipeline can fail at any of these stages:

| Stage | Description | Common Failures |
|-------|-------------|-----------------|
| `CommandValidation` | Command structure validation | Invalid ability ID, caster cannot cast |
| `ManaCheck` | Resource availability | Insufficient mana, missing mana component |
| `CooldownCheck` | Cooldown state | Ability on cooldown |
| `TargetingValidation` | Target validation | Invalid target, out of range |
| `AbilityExecution` | Ability logic execution | Ability execution failed |
| `EffectApplication` | Effect application | Failed to apply effects |

## Implementation Notes

### Customizing Ability Execution

The current implementation provides a framework. To add actual ability effects:

1. Implement ability-specific logic in `ExecuteAbility()`
2. Implement effect application in `ApplyEffects()`
3. Integrate with your game's combat, damage, or effect systems

### Targeting Validation

The plugin uses the `ITargetingValidator` interface from EntityTargetingSystem. Implement this interface to provide game-specific targeting logic:
- Faction/team validation
- Range checks
- Line-of-sight checks
- Custom targeting rules

### Command Integration

The plugin uses `AbilityCommand` for ability execution. Your game should:
1. Create ability commands based on user input
2. Pass commands to the handler/pipeline
3. Process results and provide feedback

## Project Structure

```
AbilityExecution/
├── Core/
│   ├── AbilityExecutionPipeline.cs    # Full pipeline orchestration
│   ├── AbilityCommandHandler.cs       # Command processing
│   ├── AbilityTargetingBridge.cs      # Targeting integration
│   └── AbilityCommandTypes.cs         # Command and result types
└── AbilityExecution.csproj            # Project file
```

## Design Principles

- **Clear Separation of Concerns** - Each class has a single responsibility
- **Comprehensive Validation** - Fail fast with clear error messages
- **Extensible** - Easy to customize for game-specific needs
- **Integration-Focused** - Bridges multiple systems cleanly

## Comparison with Related Plugins

| Plugin | Purpose |
|--------|---------|
| **AbilitySystem** | Ability definitions, components, registry |
| **EntityTargetingSystem** | Targeting validation logic |
| **AbilityExecution** (this plugin) | Ability execution pipeline orchestration |
| **CommandSystem** | Command handling infrastructure |


Potential additions:
- Ability chaining and combo system
- Ability interruption and cancellation
- Ability preview and targeting indicators
- Ability cooldown modifiers
- Resource cost scaling
- Ability effect templates

## License

[Your License Here]
