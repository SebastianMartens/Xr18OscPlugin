# This is a (inofficial) Loupedeck Plugin for Behringer XAir XR18

The code can be used to build a Plugin for Loupedeck Controllers (nowadays Logitech or Razor branded).
The controller then can be used to remote control the Behringer XR12/16/18 products with hardware controls like buttons and knobs.

## Status

This plugin is an early project in "experimental" stage (as of Jan 2026).
I used .Net 9 to send and receive OSC (Open Sound Control) messages via UDP to/from the Behringer digital mixer.

The most useful feature is a dial adjustment that can be used to control the fader level and mute of the 18 input channels, the main channel and FX return channels.

More features like using the code to adjust bus mixes (e.g. for separate in-ear-monitoring mixes), to control input gain or to toggle compressor etc. may follow.

## HowTo use

- Download file "Xr18Osc.lplug4" from this repository's "GitHub Release" page.
- Open it (doubleclick in windows) or choose "install from file" from Loupedeck Add-on Manager. You can also do it via command line: "logiplugintool install ./Xr18Osc.lplug4"
- There is no default profile provided (yet). So use the Loupedeck Software to configure the dial actions via drag & drop.

## Contribution
Contributions welcome.
