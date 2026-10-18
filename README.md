# ChudOS

**Made by Astra**

ChudOS is a customizable Gorilla Tag PCVR mod menu. Version **0.0.5** includes **143 menu buttons**, covering mods, settings, and actions.

[Download the latest release](https://github.com/ASTRA228b/ChudOS/releases/latest)

## Features

- Movement tools, flight, platforms, and gravity options.
- Rig poses, animations, and rig guns.
- Player visuals and name tags with FPS and speaking indicators.
- Free camera with keyboard movement and mouse look.
- Soundboard and microphone controls.
- Custom MOTD and rules boards with settings to edit and reload text.
- PC and VR menus, search, an active mod list, and saved menu settings.
- Reduced idle menu work and cached active button dispatch.

## Installation

1. Set up BepInEx for the Windows PC version of Gorilla Tag.
2. Download `ChudOS.dll` from the release page.
3. Put it in `Gorilla Tag/BepInEx/plugins`, replacing any previous ChudOS DLL.
4. Launch the game.

## Controls

- **PC menu:** press `H`.
- **VR menu:** use the left controller secondary button.
- **Free camera:** `WASD` to move, `Q/E` to move down/up, and hold the right mouse button to look around.
- **Custom boards:** use **Open Board Text** and **Reload Board Text** in Settings.
- **Saved settings:** use **Save Menu State** in Settings.

## Building

Requires the .NET SDK and a local Gorilla Tag installation with BepInEx and the game assemblies referenced by the project.

```powershell
dotnet build ChudOS.csproj -c Release -p:Version=0.0.5 -p:GorillaTagDir="C:\path\to\Gorilla Tag" -p:BaseOutputPath=artifacts/ -p:OutputPath=artifacts/Release/
```

The release build completed with zero warnings and zero errors. In-game performance has not been measured for this release.
