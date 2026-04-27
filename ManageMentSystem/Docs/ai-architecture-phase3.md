# AI Architecture - Phase 3 (Tools Framework)

## Implemented
- Refactored `AiToolExecutor` to a unified execution pipeline:
  - core dispatch (`ExecuteCoreAsync`)
  - validation errors surfaced as structured errors
  - retry policy (`AI:Tools:Retries`)
  - timeout guard (`AI:Tools:TimeoutMs`)
- Added standardized result envelope for all tools:
  - `success`
  - `function`
  - `data`
  - `error`
  - `meta` (attempt/timestamps)
- Added input guardrails:
  - date-range validation (`from_date <= to_date`)
  - bounded integers (`top_n`, `threshold`, `year`)
  - allowed enum values for `period`
  - required tenant context check

## Config knobs
- `AI:Tools:TimeoutMs`
- `AI:Tools:Retries`

## Notes
- Tool payloads are now wrapped in a stable envelope for better orchestration and debugging.
- This keeps compatibility with current serialization flow in `AiToolDefinitions`.

## Next (Phase 4)
- Step-based agent loop with explicit states:
  - analyze -> tool_calls -> synthesis -> final
- Unified streaming events and user-visible statuses.
