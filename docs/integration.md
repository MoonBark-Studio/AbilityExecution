# AbilityExecution Integration

## Current Role

`AbilityExecution` owns shared ability command/result contracts and the plugin execution pipeline abstraction.

## Thistletide Integration

Thistletide currently keeps game-specific effect execution in its action handlers while consuming shared command/result types and plugin-owned ability data contracts.

This keeps behavior stable while enabling future migration of effect application into the plugin.
