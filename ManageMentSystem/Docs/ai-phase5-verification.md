# AI Verification Plan - Phase 5

## Stream Protocol Checks
1. Send chat request to `/Ai/Stream` and verify status order:
- `[STATUS] analyzing`
- `[STATUS] generating`
- `[STATUS] finalizing`
- `[DONE]`

2. Ensure control events are not persisted in conversation messages.

## Tool Envelope Checks
For each tool call result payload verify fields:
- `success`
- `function`
- `data`
- `error`
- `meta`

## Telemetry Checks
Call `GET /Ai/TelemetrySnapshot` and verify counters move after each chat:
- `statuses.analyzing`
- `statuses.generating`
- `statuses.finalizing`
- `streams.success_count`
- `tools.tool:<name>:calls`

## Error Path Checks
1. Empty prompt should return `[ERROR] Empty message.`
2. Invalid date range for tool arguments should increment validation error counters.
3. Force timeout by lowering `AI:Tools:TimeoutMs` and verify timeout counters.

## Notes
- Build execution in this environment is blocked by external NuGet permission restrictions.
- Run local verification in your normal dev environment with full NuGet access.
