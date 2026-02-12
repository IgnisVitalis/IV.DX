# AGENTS.md

## Purpose
Instructions for the coding agent while working in this repo.

## Tool usage rules

- Do NOT run commands in parallel;
- Run at most ONE shell command at a time;
- Never scan the entire repository;
- Never auto-run formatting or analysis tools;
- 

## Must Follow
- If request contains question before modyfing code discuss the question;
- Using best practices of desing patterns;
- Use existing patterns and conventions in the repo;
- Avoid breaking changes without explicit approval;
- Prefer small, focused changes;
- Do not commit unless asked;

## Code Style
- C#: follow existing formatting and naming conventions.
- Keep changes minimal and consistent with nearby code.

## Tests
- `dotnet test src/IV.DX/Tests/IntTests/IV.DX.Persistence.IntTests/IV.DX.Persistence.IntTests.csproj`
- `dotnet test src/IV.DX/Tests/IntTests/IV.DX.Application.IntTests/IV.DX.Application.IntTests.csproj`

## Workflow
- Summarize changes and list affected files.
