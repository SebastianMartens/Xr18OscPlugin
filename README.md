This is a (inofficial) Loupedeck Plugin for Behringer XAir XR18
--------------------------------------------------

The code can be used to build a Plugin for Loupedeck Controllers (nowadays Logitech or Razor branded).
The controller then can be used to remote control the Behringer XR12/16/18 products with hardware controls like buttons and knobs.

Status
----------------------
This plugin is an early project in "experimental" stage (as of Jan 2026).
I used .Net 9 to send and receive OSC (Open Sound Control) messages via UDP to/from the Behringer digital mixer.

The most useful feature is a dial adjustment that can be used to control the fader level and mute of the 18 input channels, the main channel and FX return channels.
This was already battle-tested and turned out to be useful for Karaoke-Sessions with the mixer or band rehearsals.

More features like using the code to adjust bus mixes (e.g. for separate in-ear-monitoring mixes) may follow.

Contribution
------------------------
Contributions welcome. Feel free to open pull requests.
