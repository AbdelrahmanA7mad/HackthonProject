# AI Architecture - Phase 2 (Prompt + Context)

## Implemented
- Added `IAiPromptBuilder` + `AiPromptBuilder` to centralize system-prompt construction.
- Added `IAiContextAssembler` + `AiContextAssembler` to build request history from:
  - dynamic system prompt
  - bounded conversation history (`AI:Context:MaxHistoryMessages`)
- Updated `AiController` to use `IAiContextAssembler` instead of building prompt/history inline.
- Registered new services in DI (`Program.cs`).
- Cleaned orchestrator so prompt logic is no longer duplicated there.

## Current flow
1. `AiController.Stream` validates request.
2. `AiContextAssembler` builds history (system + recent messages).
3. `AiOrchestratorService` appends current user message and streams model output.
4. Assistant response persists to conversation storage.

## Config knobs
- `AI:Context:MaxHistoryMessages` controls how many previous messages are included.

## Next (Phase 3)
- Add tool contracts (input/output schema envelope).
- Add validation guardrail and unified error envelope for tools.
- Add retry/timeout policy per tool execution.
