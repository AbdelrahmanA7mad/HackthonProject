# AI Architecture - Phase 1 Baseline

## Goals completed
- Unified AI API surface between full AI page and floating widget.
- Added missing endpoints used by the widget (`GET /Ai/GetHistory`, `POST /Ai/Clear`).
- Standardized assistant message role to `assistant` for new stored replies.
- Removed hardcoded OpenRouter API key from `appsettings.json`.
- Added environment-variable fallback via `OPENROUTER_API_KEY` in DI registration.

## Current AI components
- Controller: `Controllers/AiController.cs`
- Orchestration: `Services/AiServices/AiOrchestratorService.cs`
- Tools execution: `Services/AiServices/AiToolExecutor.cs`
- Tool definitions: `Services/AiServices/AiToolDefinitions.cs`
- Conversation persistence: `Services/AiServices/AiConversationService.cs`

## Next phases
- Phase 2: Prompt builder + context assembler (versioned prompts, token budget, summarized memory).
- Phase 3: Tool contract normalization (schema validation, unified output envelope, retries/timeouts).
- Phase 4: Agent loop refactor (step-based orchestration with execution policy).
- Phase 5: Test coverage + telemetry + quality dashboards.
