# TextField

## Definition

A controlled single-line editor with native focus, selection, caret, and IME.

TextField: `Value`, `MaxLength`, `Validator`, `OnValueChange`, `Placeholder`, `Disabled`, `OnSubmit`, `OnCancel`, `Font`, `TextAlign`, `Color`, `Tooltip`, `DrawTextFieldBackground`

`TextField : Widget`; defaults: `Value = ""`, `MaxLength = 9999`, `Font = Small`, `TextAlign = MiddleLeft`, `DrawTextFieldBackground = true`.

## Examples

Tree slices; unshown values are the owner's State/Props. Input proposes a value through OnValueChange; the owner accepts or rejects it.

### ControlledInput

```csharp
new TextField
{
    Value = Query, Placeholder = "Search", Width = 280.Point(),
    OnValueChange = new Action<string>(value => Query.Target = value),
}
```

### ValidatedInput

```csharp
new TextField
{
    Value = Name, MaxLength = 32,
    Validator = new Func<string, bool>(value => value.All(c => char.IsLetterOrDigit(c) || c == '_')),
    OnValueChange = new Action<string>(value => Name.Target = value),
}
```

### CommitAndCancel

```csharp
new TextField
{
    Value = Draft, Disabled = Busy,
    OnValueChange = new Action<string>(value => Draft.Target = value),
    OnSubmit = new Action(() => OnCommit.Value?.Invoke(Draft.Value)),
    OnCancel = new Action(() => Draft.Target = Saved.Value),
}
```

Enter triggers OnSubmit and Escape triggers OnCancel; both stop event propagation.

### DecoratedNativeEditor

```csharp
new View
{
    PaddingAll = 6.Point(), BorderAll = 1.Point(), BorderColor = Accent, BorderRadius = 8f,
    Children = new WidgetList
    {
        new TextField
        {
            Value = Query, DrawTextFieldBackground = false,
            OnValueChange = new Action<string>(value => Query.Target = value),
        },
    },
}
```
