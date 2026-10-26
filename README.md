# ChudOS

**Made by Astra**

ChudOS is a mod menu for **Gorilla Tag on Windows PCVR**, made by **Astra**. The **0.1.2 test build** has **302 total menu buttons**, including mods, settings, and actions.

**0.1.2 is in local testing and has not been released. The latest published release is 0.1.1.**

[Download the latest release](https://github.com/ASTRA228b/ChudOS/releases/latest)

## 0.1.2 test build

- **Long Arms:** default length 1.1 with a 0.5–2.5 range and finer steps near the default. Existing saved lengths migrate to the new choices; height settings stay separate.
- **Board text:** refresh outdated built-in text from older saves, preserve custom fields, and keep a backup at `ChudOS/boards.json.pre-012.bak` when migrating an existing file.
- **No Finger Gestures:** keep fingers open at the native finger mapper while preserving controller inputs for the menu and other mods.
- **Room:** Report All, Key-Leave with its input selector directly underneath, Disable Quit Box, Disable Map Triggers, Reconnect, Cancel Reconnect, Join Last Room, JoinRoom: ASTRA, and JoinRoom: CHUDOS. Random private-room selection now has 30 code choices.
- **Movement:** Drone Fly, Left And Right, Forward And Backwards, Velocity Movement, Throw, Double Jump, Fly Towards Gun, and Punch. Custom Platforms now has its own activation input setting.
- **Rig:** Rig Gun, Spazz Rig, Spazz Hands, Fixed LongArms, Grab Rig, and Fake Lag.
- **Settings:** Drone Speed/Response; Velocity Speed/Acceleration/Input/Direction/Release; Throw Power; Double Jump Power/Input; Punch Power; Custom Platform Input; and five additional themes.
- **Fun:** Unlock FPS; 30, 60, 72, 90, 120, 144, 165, 240, and 360 FPS presets; and Reset FPS.

**Controls and behavior**

- Drone Fly: left stick moves, right stick controls height and turning. Configure speed and response in Settings.
- Left And Right: left/right trigger. Forward And Backwards: left trigger forward, right trigger backwards. Both follow your horizontal head direction.
- Velocity Movement: selected input, direction, speed, acceleration, and release behavior from Settings. Hand direction uses the right controller.
- Throw: a quick hand swing away from surfaces adds a burst in that direction; slow the hand before another burst. Body translation is excluded from the hand sample.
- Double Jump: press A by default for one additional airborne jump. Landing rearms it; input and power are configurable.
- Punch: other players' nearby hand movements knock you back. Punch Power changes the impulse applied to you.
- Fly Towards Gun and Rig Gun: use the existing gun controls. Rig Gun places your rig slightly above the surface; Fly Towards Gun moves your player toward the hit point.
- Spazz Rig and Spazz Hands: hold A. Grab Rig: hold right grip. Fixed LongArms uses uniform 1.5 scale. Fake Lag updates the local rig pose every half second.
- Key-Leave: enable the toggle, then press its selected input. Enabling it while holding the input does not immediately leave.
- Reconnect and Join Last Room remember the last joined room during this game session. Cancel Reconnect stops a queued join before it is submitted; it cannot retract a request already sent to the game.
- Report All submits a native Cheating report for every other player in the room.
- Unlock FPS disables Unity VSync and its application frame cap. The VR runtime can still control headset frame rate. Reset FPS restores the previous frame limit and VSync settings.

These features are available in the source and local test DLL. In-game VR behavior still needs testing before publication.

## Features

- Movement tools, flight, platforms, and gravity options.
- Rig poses, animations, and rig guns.
- Player visuals and name tags with FPS and speaking indicators.
- Free camera with keyboard movement and mouse look.
- Soundboard and microphone controls.
- Custom MOTD and rules boards with settings to edit and reload text.
- PC and VR menus, search, an active mod list, and saved menu settings.
- Thirteen menu themes: OG, OG Blue, OG Red, Midnight, Forest, Sunset, Ice, Text Only, Arctic, Ember, Sakura, Mint, and Midnight Gold.
- A text-only menu with clickable buttons and a setting to reset the theme to OG.
- Safety options for ChudOS RPC request limits, mod errors, room changes, and rapid master changes.
- Player-targeting guns that include player trigger hitboxes and skip your local body.
- SB Control on the tablet: music-note icon, track title/artist, progress, elapsed/remaining time, previous, play/pause, next, and Back to Main.
- Independent local/microphone volumes, position-aware microphone resume, consistent track navigation, and audio-cache reloads.
- Click sound, click volume, and click vibration settings.
- English, Spanish, Portuguese, French, German, and Italian menu and standard-board translations. Optional automatic custom-board translation sends board text to Google only when enabled.
- Hug Gun, Follow Player Gun, Orbit Player Gun, and PC mouse targeting for rig guns.
- PC Button Click for the gorilla computer keyboard and other in-game buttons.
- Body Tracers start at the right hand.
- Wall Walk V2 with strength presets and configurable controller inputs.
- Pull Mod with all 12 APMH methods, activation settings, and three preset slots.
- A themed overlay for FPS, velocity, ping, room, and player count.

## Installation

1. Set up BepInEx for the Windows PC version of Gorilla Tag.
2. Download `ChudOS.dll` from the release page.
3. Put it in `Gorilla Tag/BepInEx/plugins`, replacing any previous ChudOS DLL.
4. Launch the game.

## Controls

- **PC menu:** press `H` to open or close it, then click buttons with your mouse.
- **VR menu:** press **Y on your left controller** (the left secondary button) to open or close it. Use your right-hand pointer to press menu buttons.
- **Free camera:** `WASD` to move, `Q/E` to move down/up, and hold the right mouse button to look around.
- **Custom boards:** use **Open Board Text** and **Reload Board Text** in Settings.
- **Saved settings:** use **Save Menu State** in Settings.
- **Themes:** open **Settings → Menu Theme**. Use **Save Menu State** to keep your selection for next time.
- **SB Control:** open **Main → SB Control**. Missing MP3 title/artist tags fall back to the filename and Unknown Artist.
- **Rig guns on PC:** hold the right mouse button to aim and the left mouse button to select. In VR, use right grip and trigger.
- **PC Button Click:** enable it in Main, then left-click in-game buttons. Disable it to restore the previous cursor state.
- **Language and click feedback:** change them in Settings, then use Save Menu State.
- **Tag tools:** Modded Tag Guard is on by default and restricts them to private MODDED rooms. Tag Gun, Tag All, and Tag Aura use bounded normal hand reach; they no longer teleport your rig or set unlimited reach. Tag Self requires the tag room master.

Safety options reduce specific errors and limit ChudOS requests. They do not guarantee protection from bans. Head pose mods remain usable online; using mods can still lead to a ban.

## Additional 0.0.8 features

- Favorites: grip + click in VR or Shift + click on PC adds/removes a favorite without activating it. [F] marks favorites.
- Optional Button Help and Remember Last Category; shared left/right Gun Hand setting.
- The first menu opening after restarting the game always starts at Home, page 0. Remember Last Category applies only to later openings during the same game session.
- Detected category for higher-risk tag and head mods, with warnings on opening or activation. This is a risk grouping, not a claim of confirmed server detection. Detected toggles are not automatically restored from saved state.
- Astra NoLeaves V2 with cached/downloaded object names, bounded detection, cancellation and restoration.
- Car, Reverse Velocity (X), Air Brake (both grips), and Ender Pearl (hold/release either grip).
- Invisible, Sticky, Plank and Custom Platforms. Settings control custom width, height, length, color, visibility and rotation.
- Platform Gun places at most 20 temporary platforms; placements expire after 30 seconds and clear when disabled or changing rooms.
- Speed Boost presets from Astra's PullMod V2: Normal 6.5/1.1, Boost 8.5/1.5, Strong 10/1.8; custom jump speed/multiplier options. Mosa Speed Boost uses 999/2.2. Only one boost mode runs at a time.
- Reset Temporary Mod State, movement-controller conflict handling and grouped repeated errors.

## New in 0.0.9

- Movement macros: record body movement and hand poses, save recordings, and select, rename, delete or preview them in the Macros GUI. Recordings are stored in `Gorilla Tag/ChudOS/Macros`.
- Enable Macro Controls for VR: hold left trigger to record and release to stop. Hold right trigger within one metre of the selected recording's starting point to play; releasing stops playback. Desktop users can use Record, Play and Stop in the GUI. Controller input remains live during playback.
- Recordings are limited to two minutes. Conflicting movement and rig mods must be disabled before recording or playback; changing rooms stops active macros.
- MultiMP3: open Soundboard > Download YouTube Audio, paste a supported HTTPS YouTube link, select MP3 quality, and download into the existing Sounds folder. Download Clipboard Audio also works from the VR menu using the Windows clipboard.
- Downloads support cancellation, validated responses and unique filenames. Successful downloads refresh the sound list without stopping the current song. The external processing service must be available.
- Sticky Platforms keep the OG platform objects and player color, with additional local collision faces and per-collider surface settings. Adhesion still needs in-game VR testing.


## New in 0.1.0

- Visuals: Fire Fists, Forest Lightning, Nebula Storm, Black Hole, White Hole, and Random Domain. These effects are local visuals; the holes do not apply forces.
- Movement: Long Arms, with separate Long Arm Length and Long Arm Height options in Settings. It scales the player transform horizontally and vertically, and restores the original scale when disabled.
- Long Arms and Size Changer turn each other off to prevent conflicting scale changes.
- Particle objects and materials are reused, with bounded lightning and Domain pools. Effects clear on disable, room changes, and shutdown. Draw and FireBall are not included in these additions.

## New in 0.1.1

- Soundboard control buttons fit translated labels within their existing dimensions.
- Long song titles and artist names scroll inside clipped viewports, with pauses at the start and end. Short text remains centered.
- Settings > Soundboard Text Scrolling turns the animation on or off. OFF fits the complete title and artist as stationary text. Soundboard Scroll Speed offers Normal, Slow, Fast, and Very Fast. Use Save Menu State to retain these settings.
- Play and pause icons replace the center control's text symbols while retaining its playback action.
- Metadata decoding respects UTF-8 and UTF-16 tags and repairs recoverable encoding mistakes without rewriting audio files or guessing missing characters.
- Long desktop notifications wrap within their existing width and grow vertically; short notifications retain their size, colors, and timing.
- Movement > Predictions applies configurable hand-motion prediction, with Default/Light/Medium/Strong presets, strength, movement threshold, smoothing and scale-aware reach in Settings. This is separate from Jump Predictions in Visuals. Rig pose mods and macros do not run against it.
- Movement > PitGeo configures Forest pit upper/lower walls and top/bottom ground. Select surfaces, velocity/max multipliers, input and Hold Input/Always On in Settings. Defaults select no surfaces. Original surface values restore on release, deselection, disable and room changes.
- Movement > NoClip uses right trigger by default, with right grip or Always On available in NoClip Input. PC: hold N. NoClip Fly uses X in VR or F on PC, with the existing fly speed. Player collision and gravity state restore when the modes end.
- Ender Pearl releases from the hand holding it even when the opposite grip stays held. Throw velocity uses recent controller motion and a short tracker history, with a small forward fallback when stationary.

## Full feature list

## Main — 4

- Join Discord
- SB Control
- Menu Source
- PC Button Click

## Movement — 51

- Speed Boost
- Low Gravity
- Mosa Speed Boost
- High Gravity
- Zero Gravity
- Dash (X)
- Fly (X)
- Slingshot Fly (A)
- Up And Down
- Iron Man
- Trigger Fly
- Joystick Fly (LJ)
- Zero Grav Fly (A)
- BarkFly (JR)(JL)
- Velocity BarkFly (JR)(JL)
- Hand Fly (B)
- NoGrav SlingshotFly (B)
- WASD Fly
- Forzone
- Platforms
- Platform Spam
- No Tag Freeze
- Tag Freeze
- Pull Mod
- Wall Walk
- Grappling Hooks
- Rewind
- Teleport Gun
- Checkpoint
- Size Changer
- Bouncy
- Slide Control
- Swim Momentum
- Rope Grab Reach
- Hoverboard Boost
- Glider Tuning
- Car
- Reverse Velocity
- Air Brake
- Ender Pearl
- Invisible Platforms
- Sticky Platforms
- Plank Platforms
- Custom Platforms
- Platform Gun
- Macros GUI
- Long Arms
- NoClip
- NoClip Fly
- Predictions
- PitGeo

## Macros — 7

- Macro Controls
- Record Macro
- Stop Macro
- Play Selected Macro
- Next Macro
- Reload Macros
- Open Macros Folder

## Player — 6

- Body Tracers
- Bone ESP
- 2D Box ESP
- 3D Box ESP
- 4D Box ESP
- Trails ESP

## Rig — 21

- Copy Movement Gun
- Dance
- Ghost Animations
- Ghost Monke
- Griddy
- Invis Monke
- Jerk Off
- Look At Gun
- PC Rig
- Rec Room Rig
- Rig Tweaks
- Rig Tweaks V2
- Shadow Clone Gun
- Smooth Rig
- Spin Bot
- PC Rig Mode
- Max Quest Score
- Evolving Cosmetic
- Hug Gun
- Follow Player Gun
- Orbit Player Gun

## Fun — 5

- Fix Head
- Fix All
- Free Camera
- Camera FOV
- Silent Hand Taps

## Visuals — 16

- Hide Fog
- Ambient Tint
- Visual Info
- Name Tags
- Overlay
- Jump Predictions
- Show Room Triggers
- Draw Gun
- Hide Head Cosmetics
- No Leaves
- Fire Fists
- Forest Lightning
- Nebula Storm
- Black Hole
- White Hole
- Random Domain

## Detected — 9

- UpsideDown Head
- Broken Neck
- Backwards Head
- Sideways Head
- HeadBang
- Tag Gun
- Tag Self
- Tag Aura
- Tag All

## Safety — 29

- Disable Movement
- Disable All Mods
- Stop All Sounds
- Close All Windows
- Return Home
- Leave Room
- No Finger Gestures
- Fake Valve Tracking
- Hide Mode Selector
- Room Status
- Check My Nickname
- Random Nickname
- Custom Nickname
- Name On Room Leave
- Nickname Length Alert
- Report Proximity Alert
- Report Alert Range
- Visualize Report Zone
- Auto GC (60s)
- Fake Power Off
- Fake Report Overlay
- Anti Lurker (Local)
- Clear My Buffered RPCs
- Anti Report Disconnect
- RPC Protection
- Mod Error Protection
- Disable Mods On Leave
- Rapid Master Protection
- Reset Temporary Mod State

## Room — 8

- Leave
- JoinRandom
- JoinRandom Private
- EU
- US
- USW
- Quit Game
- Disable Network Triggers

## SoundBoard — 15

- Play / Stop
- Next Sound
- Volume
- Sound Output
- Mic Volume
- Autoplay
- Repeat
- Shuffle
- Open Soundboard GUI
- Reload Sounds
- Download YouTube Audio
- Download Clipboard Audio
- Mute Other Players
- Microphone Mute
- Microphone Pitch

## Weather — 2

- Time
- No Rain

## Settings — 82

- PC View
- Array List
- GUI Input
- Close GUI Input
- Close All GUIs
- Save Menu State
- Canvas Notifications
- Custom Boards
- Custom MOTD Board
- Custom COC Board
- Board Uppercase
- Board Color
- Open Board Text
- Reload Board Text
- Menu Theme
- Reset Menu Theme
- Modded Tag Guard
- Click Sound
- Click Volume
- Click Vibrations
- Menu Language
- Auto Translate Boards
- Wall Walk Strength
- Wall Walk Input
- Pull Method
- Pull Hand
- Pull Input
- Pull Activation
- Pull Power
- Pull Uphill Power
- Pull Momentum
- Pull Maximum
- Pull Velocity Limit
- Pull Extra Power
- Pull Clamp Velocity
- Pull Release Window
- Pull Touch Linger
- Pull Preset Slot
- Save Pull Preset
- Load Pull Preset
- Reset Pull Settings
- Overlay Size
- Overlay Position
- Overlay FPS
- Overlay Velocity
- Overlay Ping
- Overlay Room
- Overlay Players
- Remember Last Category
- Button Help
- Gun Hand
- Car Speed
- Air Brake Strength
- Platform Width
- Platform Height
- Platform Length
- Platform Color
- Platform Visibility
- Platform Rotation
- Speed Boost Preset
- Speed Boost Jump Speed
- Speed Boost Multiplier
- Long Arm Length
- Long Arm Height
- Soundboard Text Scrolling
- Soundboard Scroll Speed
- NoClip Input
- Prediction Preset
- Prediction Strength
- Prediction Threshold
- Prediction Smoothness
- Prediction Reach
- Reset Predictions
- PitGeo Walls
- PitGeo Ground
- PitGeo Wall Multiplier
- PitGeo Wall Max Multiplier
- PitGeo Ground Multiplier
- PitGeo Ground Max Multiplier
- PitGeo Input
- PitGeo Activation
- Reset PitGeo Settings
