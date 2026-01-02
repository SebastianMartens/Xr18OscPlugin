# Copilot Instructions for Xr18OscPlugin

## Project Overview
- This is a Loupedeck (Logitech/Razer) plugin for controlling Behringer XR18 (and similar) digital mixers via OSC (Open Sound Control) over UDP.
- Main features: adjust fader levels, mute/unmute channels, and basic mixer discovery/connection.
- The codebase is experimental and evolving, with a focus on channel and main mix control.

## Architecture
- **Plugin entry point:** `src/Xr18OscPlugin.cs` (class `Xr18OscPlugin`)
  - Holds a static `Mixer` instance (see `src/Domain/Mixer.cs`), which manages all mixer state and communication.
- **OSC communication:**
  - Uses a custom fork of `SharpOSC` (see `src/SharpOSC/`).
  - UDP send/receive logic is in `src/SharpOSC/UDPConnection.cs`.
  - Send an OSC message with parameters to the mixer in order to set volume or configure properties.
  - Send an empty OSC message to a specific OSC address to query the mixer for current state of that address.
- **Domain model:**
  - `Mixer` (src/Domain/Mixer.cs): Handles OSC connection, mixer discovery via UDP broadcast, and message routing.
  - `MixerChannels`/`MixerChannel` (src/Domain/MixerChannels.cs`, `MixerChannel.cs`): Represent and manage all mixer channels.
- **Plugin actions:**
  - Located in `src/Actions/` (e.g., `ChannelVolumeAdjustment.cs`, `ChannelMuteCommand.cs`).
  - Actions interact with the `Mixer` and its channels.

## Developer Workflows
- **Build:**
  - Use VS Code tasks: "Build (Debug)" or "Build (Release)" (see `.vscode/tasks.json`).
  - Or run: `dotnet build -c Debug` or `dotnet build -c Release` from the root.
  - The build is configured to write a .link file in the Loupedeck directory so that the Loupedeck Software can load the plugin directly from the build output without needing to install or package it.
- **Package:**
  - Use the "Package Plugin" task or run: `logiplugintool pack ./bin/Release ./Xr18Osc.lplug4`
- **Install/Uninstall:**
  - Use "Install Plugin"/"Uninstall Plugin" tasks or run: `logiplugintool install ./Xr18Osc.lplug4` / `logiplugintool uninstall Xr18Osc`
- **Debug:**
  - Attach to the `LogiPluginService` process (see `.vscode/launch.json`).
  - On Windows, the service is typically at `C:\Program Files\Logi\LogiPluginService\LogiPluginService.exe`.

## Project Conventions & Patterns
- **.NET 8, C# latest features, nullable enabled.**
- **All OSC communication is async and event-driven.** Domain model state is only updated in response to incoming OSC messages.
- **Mixer auto-discovery** is the default, but manual IP can be set via the Connect action.
- **Logging:** Use `PluginLog` for plugin-side logs.
- **Code style:** See `.editorconfig` for formatting and naming rules.
- **SharpOSC** is a modified fork; do not update from upstream without review.

## Key Files & Directories
- `src/Xr18OscPlugin.cs` — plugin entry point
- `src/Domain/` — mixer and channel domain logic
- `src/Actions/` — plugin actions/commands
- `src/SharpOSC/` — OSC protocol implementation (custom fork)
- `.vscode/tasks.json` — build/package/install tasks
- `.vscode/launch.json` — debug configuration
- `README.md` — project summary and contribution notes

## Integration Points
- **Loupedeck Plugin API:** Reference via `PluginApi.dll` (see `.csproj` for path logic).
- **External tool:** `logiplugintool` is required for packaging/installing plugins.
- **Mixer communication:** All OSC messages are sent/received via UDP; see `Mixer` and `UDPConnection`.

---

For questions about project structure or workflows, see the README or ask the project maintainer.
