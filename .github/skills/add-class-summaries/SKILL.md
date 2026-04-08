---
name: add-class-summaries
description: "Find C# classes missing XML summary comments and add meaningful summaries. Use when: ensuring documentation completeness, reviewing class documentation, adding missing XML doc comments to classes."
argument-hint: "Optional: path or glob to limit scope (e.g. src/Domain/)"
---

# Add Class Summaries

## When to Use
- Ensure all C# classes have `<summary>` XML doc comments
- Review documentation completeness after adding new classes
- Bulk-add missing summaries across the codebase

## Procedure

1. **Search for classes without summaries.** Look for C# class declarations in `*.cs` files that are NOT preceded by a `/// <summary>` block. Limit to the argument path if provided, otherwise search `src/`.

2. **For each class missing a summary:**
   - Read the class body to understand its purpose, fields, methods, and relationships.
   - Write a concise `<summary>` that describes:
     - What the class represents or does
     - Its main responsibility (single responsibility)
   - Add the `/// <summary>` block directly above the class declaration.

3. **Do NOT:**
   - Modify any existing summaries
   - Add summaries to method, property, or field level (only class level)
   - Add generic or vague descriptions like "This class handles things"

4. **Build** to verify no syntax errors were introduced: `dotnet build -c Debug`

## Example

Before:
```csharp
public class Channel : IChannelBase
{
```

After:
```csharp
/// <summary>
/// Represents a single input channel on the XR18 mixer, managing its fader level,
/// mute state, and OSC communication for real-time control.
/// </summary>
public class Channel : IChannelBase
{
```

## Output
Report which classes were updated and confirm the build succeeds.
