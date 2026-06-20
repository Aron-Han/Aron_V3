# Aron_V3 Project Rules

These rules apply when Codex modifies code in the Aron_V3 project.

## Architecture Principle

- Task is the top-level business requirement.
- Communication protocols are shared runtime resources, not owners of tasks.
- Communication endpoints should be modeled as `CommunicationInstance` objects.
- A task may be triggered by communication input and may output through different communication instances from any step.
- Task execution is asynchronous and must have an explicit concurrency policy.
- Keep legacy XML/configuration compatible during refactors; migrate gradually.

## Required Reference

- Read `docs/CommunicationTaskArchitecture.md` before communication, task scheduling, trigger, or runtime-output refactors.

## UI And Dialog Style

- Program dialogs should follow the Aron_V3 dark visual style instead of the default Windows message-box style.
- Dialogs should use a dark panel, subtle cyan border, clear title bar, high-contrast text, and compact spacing that matches the main application.
- Dialog icons should communicate the action meaning directly, such as delete, warning, information, or success; avoid generic system question icons.
- Destructive actions should use a clearly marked red primary button, with a neutral outlined cancel button.
- Dialog text should explain the consequence of the action in plain language, especially when deleting or overwriting configuration.
- Before replacing a family of existing dialogs, make or confirm a UI preview when the visual direction is uncertain.

## Localization

- Persist the selected UI language through `LanguagePreferenceStore`; startup screens and all new forms/controls must read or apply the persisted language before showing.
- Any user-facing text introduced in UI, dialogs, buttons, grid headers, placeholder pages, startup status, or log-facing UI must provide both Chinese and English variants or route through an existing localization helper.
- New reusable pages or dialogs should implement `ILocalizable` or accept/apply `isEnglish`, and parent pages must apply the current language immediately when creating them, not only after the user manually toggles language.

## Configuration Reference Integrity

- When a named signal, communication variable, channel, or task/program reference is renamed, update all dependent runtime and persisted references to the new name.
- When a referenced signal, communication variable, or global variable is deleted, clear dependent settings instead of leaving stale names in XML or UI.
- Prefer surfacing missing references clearly in the UI and blocking saves that would preserve invalid references.

## Conflict Handling

- If a requested change conflicts with these rules, tell the user clearly before changing code.
- If the user updates the rule or explicitly approves an exception, follow the updated instruction.
