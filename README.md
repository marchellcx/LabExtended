# LabExtended
[![Build](https://github.com/marchellc/LabExtended/actions/workflows/dotnet.yml/badge.svg)](https://github.com/marchellc/LabExtended/actions/workflows/dotnet.yml)

These APIs include, but are not limited to:
- Custom Ammo, Items, Teams, Roles
- Voice Chat API with threaded voice message modifications.
- Hint Overlay API
- Settings API for easy server-specific settings management with fixes to prevent settings being overriden by other plugins.
- Remote Admin API that allows simple actions implementations
- More events and fixes to LabAPI and base-game.
- A fully-custom Commands API  
.. and much more!

# Documentation
Every method in the project *should* be documented, but extended descriptions can be found in the [Wiki](https://github.com/marchellc/LabExtended/wiki)!  
If there's something still unclear, create an issue and I'll try to answer.

# Installation
Grab the assembly from the [Releases](https://github.com/marchellc/LabExtended/releases) page and put it in the server's plugin directory!  
**Do not remove the zero before the plugin name, it's required to be loaded first!**

## Dependencies
- [NVorbis](https://github.com/NVorbis/NVorbis)
  - Required only if you intend to use the audio API.
- [Harmony](https://github.com/pardeike/Harmony)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
  - Should be a part of the server's dependencies.

# Contributing
I do not limit contributions at all, if you see something that may be wrong or needs an improvement, create an issue and explain it.  
Any request is welcome, no matter how stupid it may seem!
