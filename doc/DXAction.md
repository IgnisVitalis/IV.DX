# DX Action in IV.DX

## Purpose

DX Action is a command-based execution framework built into IV.DX. It provides infrastructure to define, register, resolve, and execute named actions across any .NET host: console apps, background services, Blazor, MAUI, WPF, or workflow engines.

Key design goals:

- actions are identified by a `Module + Key` pair (e.g., `IV.DX` / `Ping`);
- action definitions are stored in the DX database as `DXActionDefinitionUnit` units with typed parameter metadata;
- action implementations are C# classes with DI-injected dependencies and attributed In/Out properties;
- a single `IDXActionExecutor` service runs any action by name — callers never reference concrete action classes;
- presentation units (`DXPButtonActionUnit`, etc.) are separate from the action definition — they reference `DXActionDefinitionUnit` via FK and add layer-specific UI metadata;
- the same action definition can be presented as a button, context menu item, workflow step, or any other trigger type without duplicating logic.

## Data model

Action definitions are DX units migrated via `Migration/DXAction.json`.

### DXActionDefinitionUnit

Top-level action definition unit.

| Column        | Type        | Description                                                  |
|---------------|-------------|--------------------------------------------------------------|
| `Name`        | string(200) | Human-readable action name                                   |
| `Description` | text        | Optional description                                         |
| `Module`      | string(100) | Owning module (e.g., `IV.DX`, `IV.DX.Presentation`)          |
| `Key`         | string(100) | Action identifier within the module (e.g., `Ping`, `Export`) |
| `IsEnabled`   | bool        | Whether the action is active (default: true)                 |

Unique constraint: `Module + Key`.

### DXActionParameterElement

Child element of `DXActionDefinitionUnit` — defines one parameter.

| Column         | Type                           | Description                                   |
|----------------|--------------------------------|-----------------------------------------------|
| `Key`          | string(100)                    | Parameter name                                |
| `Type`         | DXActionParameterTypeEnum      | Data type: GUID (1), String (2), Int (3)      |
| `Direction`    | DXActionParameterDirectionEnum | In (1), Out (2), InOut (3)                    |
| `DefaultValue` | string(500)                    | Optional default value                        |
| `Required`     | bool                           | Whether the parameter is required             |
| `IsMulti`      | bool                           | Reserved for future array/multi-value support |

### Enums

**DXActionParameterTypeEnum**: GUID (1), String (2), Int (3). Extensible in future IV.DX versions.

**DXActionParameterDirectionEnum**: In (1), Out (2), InOut (3).

## C# infrastructure

### Layer placement

| Component                      | Project                     | Purpose                                                   |
|--------------------------------|-----------------------------|-----------------------------------------------------------|
| `DXActionAttribute`            | IV.DX.Kernel                | Marks a class as an action with Module + Key              |
| `DXActionParameterAttribute`   | IV.DX.Kernel                | Marks a property as In/Out/InOut parameter                |
| `DXActionBase`                 | IV.DX.Application.Contracts | Abstract base class for all actions                       |
| `DXActionResult`               | IV.DX.Application.Contracts | Execution result (success/fail, message, output params)   |
| `DXActionParameters`           | IV.DX.Application.Contracts | Named parameter dictionary for passing values             |
| `IDXActionExecutor`            | IV.DX.Application.Contracts | Main executor interface                                   |
| `IDXActionRegistry`            | IV.DX.Application.Contracts | Registry interface (Module+Key → Type)                    |
| `DXActionExecutor`             | IV.DX.Application           | Executor implementation                                   |
| `DXActionRegistry`             | IV.DX.Application           | Registry implementation                                   |
| `DXActionScanner`              | IV.DX.Application           | Assembly scanner for action discovery                     |

### Defining an action

An action is a class that inherits from `DXActionBase`, is decorated with `[DXAction]`, and implements `ExecuteAsync`:

```csharp
using IV.DX.Application.Contracts.Actions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

[DXAction("MyModule", "SendEmail")]
public class SendEmailAction : DXActionBase
{
    private readonly IEmailService _emailService;

    // Constructor dependencies are resolved via DI
    public SendEmailAction(IEmailService emailService)
    {
        _emailService = emailService;
    }

    // Input parameters
    [DXActionParameter("To", DXActionParameterDirectionEnum.In)]
    public string To { get; set; } = string.Empty;

    [DXActionParameter("Subject", DXActionParameterDirectionEnum.In)]
    public string Subject { get; set; } = string.Empty;

    // Output parameters
    [DXActionParameter("MessageId", DXActionParameterDirectionEnum.Out)]
    public string MessageId { get; set; } = string.Empty;

    public override async Task<DXActionResult> ExecuteAsync(CancellationToken ct)
    {
        var id = await _emailService.SendAsync(To, Subject, ct);
        MessageId = id;
        return DXActionResult.Ok($"Email sent to {To}.");
    }
}
```

### DXActionBase

```csharp
public abstract class DXActionBase
{
    public abstract Task<DXActionResult> ExecuteAsync(CancellationToken ct);
}
```

Actions are single-use: created, configured with input parameters, executed once, then discarded.

### DXActionResult

```csharp
DXActionResult.Ok("Optional message");    // success
DXActionResult.Fail("Error description"); // failure
```

Properties:

| Property    | Type               | Description                                      |
|-------------|--------------------|--------------------------------------------------|
| `IsSuccess` | bool               | Whether the action succeeded                     |
| `Message`   | string?            | Success message (for UI display)                 |
| `Error`     | string?            | Error description on failure                     |
| `Output`    | DXActionParameters | Output parameter values (populated by executor)  |

Output parameters are read from `Out` and `InOut` properties automatically by the executor after successful execution. On failure, output parameters are not collected.

### DXActionParameters

A case-insensitive named parameter dictionary with type conversion:

```csharp
// Building parameters (fluent)
var parameters = new DXActionParameters()
    .Set("To", "user@example.com")
    .Set("Subject", "Hello")
    .Set("UnitId", someGuid);

// Reading values
var to = parameters.Get<string>("To");
var id = parameters.Get<Guid>("UnitId");      // also parses from string
var missing = parameters.Get<string>("Nope");  // returns null
```

### DXActionAttribute

```csharp
[DXAction("Module", "Key")]
```

- `Module` — the owning DX module name.
- `Key` — the action name within the module.
- `Inherited = false` — overriding actions must declare their own attribute.

### DXActionParameterAttribute

```csharp
[DXActionParameter("ParamKey", DXActionParameterDirectionEnum.In)]
```

- `Key` — parameter name (matches the parameter key in `DXActionParameters`).
- `Direction` — `In` (default), `Out`, or `InOut`.

## DI registration

Actions are registered through `DXBuilder`:

```csharp
builder.Services
    .AddDX(builder.Configuration)
    .UsePostgreSQL()
    .AddSecurity()
    .AddActions(typeof(Program).Assembly)   // scan app assembly for actions
    .AddHandlers(typeof(Program).Assembly)
    .RegisterHostedService();
```

Core actions (e.g., `DXPingAction`) are always registered automatically from `IV.DX.Application`.

The `AddActions` call scans the provided assemblies for classes that:
1. Inherit from `DXActionBase`.
2. Have the `[DXAction]` attribute.
3. Are not abstract.

## Execution flow

```
Caller                          IDXActionExecutor                 Action class
  │                                    │                                │
  │  ExecuteAsync(module, key, params) │                                │
  │───────────────────────────────────>│                                │
  │                                    │  1. Resolve type from registry │
  │                                    │  2. Create instance via DI     │
  │                                    │  3. Set In/InOut properties    │
  │                                    │───────────────────────────────>│
  │                                    │  4. ExecuteAsync(ct)           │
  │                                    │<───────────────────────────────│
  │                                    │  5. Read Out/InOut properties  │
  │           DXActionResult           │                                │
  │<───────────────────────────────────│                                │
```

### Calling an action

```csharp
// Inject IDXActionExecutor via DI
public class MyService(IDXActionExecutor actionExecutor)
{
    public async Task DoWork(CancellationToken ct)
    {
        var result = await actionExecutor.ExecuteAsync(
            "MyModule", "SendEmail",
            new DXActionParameters()
                .Set("To", "user@example.com")
                .Set("Subject", "Hello"),
            ct);

        if (result.IsSuccess)
        {
            var messageId = result.Output.Get<string>("MessageId");
            // ...
        }
        else
        {
            // result.Error contains the error description
        }
    }
}
```

## Overriding actions

An app can replace or extend a core action by registering a new class with the same `Module + Key`. The last registration wins (app assemblies are scanned after core assemblies).

### Full replacement

```csharp
[DXAction("IV.DX", "Ping")]
public class CustomPingAction : DXActionBase
{
    public override Task<DXActionResult> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(DXActionResult.Ok("Custom ping."));
    }
}
```

### Extend with original logic

Inherit from the original action and call `base.ExecuteAsync()`:

```csharp
[DXAction("IV.DX", "Ping")]
public class ExtendedPingAction : DXPingAction
{
    private readonly ILogger _logger;

    public ExtendedPingAction(ILogger<ExtendedPingAction> logger)
    {
        _logger = logger;
    }

    public override async Task<DXActionResult> ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Before ping");
        var result = await base.ExecuteAsync(ct);
        _logger.LogInformation("After ping");
        return result;
    }
}
```

Core actions must not be `sealed` if they are designed to be overridable.

## UI integration

DX Actions are designed for easy UI integration across frameworks (Blazor FluentUI, MAUI, WPF).

### Presentation unit types (IV.DX.Presentation)

Presentation units describe how an action appears in the UI. They are **not** subtypes of `DXActionDefinitionUnit` — they are separate DX units with an FK relation (`ActionDefinition`) pointing to a `DXActionDefinitionUnit`.

**`DXBaseActionUnit`** — base for all presentation action units. Defines the `ActionDefinition` FK relation.

**`DXPButtonActionUnit`** — inherits `DXBaseActionUnit`, adds button-specific UI properties:

| Column                 | Type                    | Description                              |
|------------------------|-------------------------|------------------------------------------|
| `ActionDefinition`     | → DXActionDefinitionUnit | FK to the action contract               |
| `Icon`                 | DXPActionIconEnum       | Semantic icon (edit, delete, export …)   |
| `Emphasis`             | DXPActionEmphasisEnum   | accent, danger, info, warning, neutral, success |
| `RequiresConfirmation` | bool                    | Show confirmation dialog before executing |
| `ConfirmationMessage`  | string                  | Message shown in the confirmation dialog |

The `Name` and `Description` from the referenced `DXActionDefinitionUnit` are used as the button label and tooltip respectively.

This separation means the same `DXActionDefinitionUnit` can be referenced by multiple presentation units simultaneously — as a button in one component and as a context menu item in another — without duplicating the action logic or parameter contract.

### Rendering pattern

1. **Load presentation unit** (e.g., `DXPButtonActionUnit`) — provides icon, emphasis, confirmation settings.
2. **Navigate FK** to `DXActionDefinitionUnit` — provides `Module`, `Key`, `Name`, `Description`, parameters.
3. **Render UI control** using the presentation metadata.
4. **Execute** via `IDXActionExecutor` with `Module + Key` from the definition.
5. **Display result** — show `Message` on success or `Error` on failure.

```csharp
// Blazor example
async Task OnButtonActionClick(DXPButtonActionUnit buttonAction, Guid selectedUnitId)
{
    // Module + Key come from the linked DXActionDefinitionUnit
    var result = await ActionExecutor.ExecuteAsync(
        buttonAction.ActionDefinition.Module,
        buttonAction.ActionDefinition.Key,
        new DXActionParameters().Set("UnitId", selectedUnitId));

    if (result.IsSuccess)
        MessageService.ShowSuccess(result.Message);
    else
        MessageService.ShowError(result.Error);
}
```

The UI never references concrete action classes — only `Module + Key` and `DXActionParameters`.

## Extensibility

### Presentation unit hierarchy

`DXActionDefinitionUnit` defines the action contract and stays in IV.DX. `DXBaseActionUnit` also lives in IV.DX and serves as the base for all action trigger units. Presentation and other layers define their own concrete types that inherit from `DXBaseActionUnit` and reference `DXActionDefinitionUnit` via FK — they do not inherit from `DXActionDefinitionUnit`.

```
DXActionDefinitionUnit  (IV.DX)
    Module, Key, Name, Description, Parameters
    C# class: [DXAction(module, key)]
         ↑ ActionDefinition FK
DXBaseActionUnit  (IV.DX)
    base for all action trigger units
         ↑ inherits
DXPButtonActionUnit       (IV.DX.Presentation) — button with icon, emphasis, confirmation
DXPContextMenuActionUnit  (IV.DX.Presentation) — context menu item (future)
DXPShortcutActionUnit     (IV.DX.Presentation) — keyboard shortcut (future)

DXWorkflowActionUnit  (future IV.DX.Workflow)
    workflow step referencing the same FK
```

The executor is agnostic to the trigger type — it only uses `Module + Key` from the referenced `DXActionDefinitionUnit`. The same action definition can be triggered from a button, a context menu, a workflow step, or a console call — all using the same C# implementation.

### Parameter types

The current `DXActionParameterTypeEnum` values (GUID, String, Int) are basic types. Additional types (Bool, DateTime, Decimal, Long, etc.) will be added in future IV.DX versions.

### Workflow integration

DX Actions can be used as workflow activities without modification. A workflow engine would:

1. Define a step that references an action by `Module + Key` with parameter mappings.
2. Call `IDXActionExecutor.ExecuteAsync()` for each step.
3. Map output parameters of one step to input parameters of the next.

The action class itself has no awareness of workflow context — it is a standalone unit of work.

## Built-in actions

### IV.DX / Ping

A reference action for testing the infrastructure.

| Parameter   | Direction | Type     | Description                |
|-------------|-----------|----------|----------------------------|
| `Message`   | In        | string   | Input message              |
| `Response`  | Out       | string   | Returns `Pong: {Message}`  |
| `Timestamp` | Out       | DateTime | UTC timestamp of execution |

```csharp
var result = await executor.ExecuteAsync("IV.DX", "Ping",
    new DXActionParameters().Set("Message", "Hello"));

// result.Output.Get<string>("Response")   → "Pong: Hello"
// result.Output.Get<DateTime>("Timestamp") → 2026-03-25T18:00:00Z
```

## Tests

Unit tests are in `Tests/UnitTests/IV.DX.Contracts.UnitTests/Actions/`:

| Test class                 | What it covers                                                |
|----------------------------|---------------------------------------------------------------|
| `DXActionParametersTests`  | Set/Get, type conversion, Guid parsing, case-insensitive keys |
| `DXActionResultTests`      | Ok/Fail factory methods, output parameters                    |
| `DXActionRegistryTests`    | Register/resolve, override, validation                        |
| `DXActionScannerTests`     | Assembly scanning, filtering                                  |
| `DXActionExecutorTests`    | Full execution flow, parameter mapping, DI injection          |
| `DXPingActionTests`        | PingAction end-to-end                                         |

Run:

```
dotnet test src/IV.DX/Tests/UnitTests/IV.DX.Contracts.UnitTests/ --filter "FullyQualifiedName~Actions"
```
