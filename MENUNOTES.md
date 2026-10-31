# ChudOS Menu Notes

Update history, detailed controls, behavior notes, and the full categorized button list.

[Back to the ChudOS overview](README.md)

Historical update entries describe their respective versions; newer entries take precedence.

## New in 0.1.4

- **Movement ports, Part 1:** Portal Gun (grip/trigger aims and places; A switches portals; desktop RMB/LMB/Q), personal C4 (RG places, A detonates; desktop G/Space), Solid Water, Solid Players, PC Walking, Hoverboards Everywhere, Slippery Hands, and Disable Planet Zones. Adds nine associated settings. Portal Gun uses surface-fit and exit-clearance checks, preserves transformed velocity, and shares GunLib ownership. Hoverboard-area and slip hooks are scoped to the local player while enabled and removed on disable. Original water trigger layers stay intact; Solid Water uses a local support collider at supported water surfaces. PC Walking uses WASD, Space jump, Shift sprint, Ctrl slow movement, and RMB look; it does not implement physical crouching. These additions still require live game testing.
- **Wall Walk:** keeps the original forward-force behavior, original strength/input settings, and saved button name.
- **Wall Walk V2:** a separate Movement toggle that attaches toward the contacted surface normal while holding the selected inputs. Has its own strength and input settings. Compensation for gravity only runs during valid contact; releasing leaves the original gravity setting intact. The two Wall Walk toggles turn each other off.
- **Reauthenticate:** Room button that retries native Mothership login once per request. Duplicate requests are blocked while pending, success/failure callbacks are detached afterward, and a long wait is reported without automatically retrying. No authentication payloads are shown.
- **Keyboard:** English QWERTY, Portuguese with accents/Ç, Spanish with accents/Ñ, French AZERTY, German QWERTZ, and Italian layouts. Classic, Side Panel, Compact, or Wide style; Theme/Accent/Text Only keys; adjustable spacing, number row, placement, and size. Keyboard language is independent of menu language. Desktop text events accept accents and punctuation, and the combined menu/keyboard fits the camera view.
- **Custom movement:** Momentum Bank (X stores speed, A releases toward the right hand), Surface Skate (RG), Air Paddle (LG+RG), Gravity Compass (X), and automatic Landing Brake, with individual settings.
- **Traversal:** Advanced Checkpoints (X places, A returns, B deletes, RG moves a nearby point), Climby Hands (either grip), Auto Climb (both grips), and Bunny Hop (hold A). Movement controllers disable conflicting controllers rather than fighting over velocity.
- **Practice:** Echo Race uses the selected saved macro as a local visual opponent (A starts/cancels). Gate Runner supports up to 32 local rings (X places, A starts/cancels, B clears). Beat Targets uses soundboard amplitude onsets for a local hand-target minigame. These tools do not send target or course objects to other players.
- **Visuals:** personal Momentum Ribbon, Contact Sparks, Landing Pulse, and desktop-camera smoothing. Camera smoothing skips stereo cameras and restores the native camera pose after rendering. Effects use cached objects and size/intensity/quality/color settings.
- **Rig:** Pose Sequencer (X captures, A plays/stops, B clears) stores up to 12 session poses and uses the existing rig ownership/restoration system.
- **Soundboard:** Beat Halo adds a music-reactive ring around the music icon, with an Audio Orbs style. Analysis is shared between enabled audio features; playback and microphone mixing are unchanged.
- **Profiles:** four named slots under ChudOS/Profiles, with save, preview, load, and reset controls. Mod restoration starts off; room actions and Detected mods are excluded. Invalid settings are rejected before applying a profile.
- **Controller labels:** new input-driven buttons display A/B/X/Y, LT/RT, and LG/RG hints without changing their saved button names. Air Brake now displays LG+RG and still slows momentum while both grips are held.
- **Ghost Hand Guides:** default-on Settings toggle. Two local player-colored controller markers appear while Ghost Monke or Invis Monke disables the rig. Markers have no active colliders, reuse their objects, and hide when normal tracking resumes.
- **Tag tools:** use native infection/It state and eligibility checks. Tag All queues nearby eligible players with a timeout and skips invalid targets; it keeps the existing reach, room, per-target cooldown, and shared request limits. Tag Gun explains rejected selections with throttled notices. Submitted requests are not treated as confirmed server-side tags.
- **Snowball Spam / Water Balloon Spam:** resolve native cosmetic/throwable IDs, request missing assets, validate material mappings, and allow the native growing-snowball release path. Hand velocity, ownership, existing held equipment, and configured intervals are respected. Availability still depends on the game's catalog and loading state.
- **Preserved:** 0.1.1 notification behavior, Align To Palm, shared controller input helpers, EveryPatch.cs, and original platform construction.
- Build validation uses isolated game stubs. Actual authentication responses, VR placement, wall contact and online projectile/tag behavior still need in-game testing.

## New in 0.1.3

- **Menu alignment:** Auto faces the menu toward your head while following your hand. Align To Palm uses the original, unchanged one-time palm alignment; Fixed keeps a constant wrist-relative rotation. Settings includes distance, height, Recenter Menu, and Reset Menu Position. Desktop placement stays unchanged.
- **Fixed position update:** follows the controller world position and rotation directly, using uniform player scaling instead of inheriting a stretched hand transform. Default placement starts at the hand origin; distance and height offsets remain adjustable. Auto and Align To Palm are unchanged.
- **Defaults:** Auto alignment, Menu Open Sound, and Join Random: Current Map start on. Disable Mods On Leave starts off; explicit saved choices still restore. The existing room cleanup and mod reset behavior is retained.
- **Join Random:** captures a public join for the active map, waits for the network to finish leaving, and submits one native join request. Cancel Reconnect also cancels a pending random join. Unavailable maps and timeouts show a notification. Turning Current Map off explicitly selects Forest.
- **Hide Fog:** removes both scene fog and zone ground fog, including the Forest Halloween effect. Disabling it restores the captured fog values and override modes without replacing unrelated zone settings.
- **Neon Outline theme:** dark green panels, bright green rounded button outlines, and green highlights. All existing themes remain available.
- **Menu sounds:** separate Menu Open Sound toggle, Open Sound selector, and Open Volume. Opening works in VR and desktop mode and uses the existing cached local feedback source. Click Sound, Click Volume, and Click Vibrations remain separate.
- **Fun:** Snowball Spam and Water Balloon Spam repeatedly throw available native throwables while holding right trigger, using normal hand velocity. Throwable Interval offers 0.25, 0.5, or 1 second. These require the corresponding throwable to be available; they leave equipment you are already holding alone.
- **Rig:** Spazz Rig uses bounded, time-based offsets; Spazz Hands keeps its motion near the controllers instead of extending three metres. Head Spin Yaw, Head Spin Pitch, and Head Spin Roll have 30/60/90/120-degree speed choices and restore the original head pose when disabled.
- **Macro imports:** put ChudOS `.json`, Poison `.json`, or hamburbur GZip `.macro` files in `Gorilla Tag/ChudOS/Macros`, then press Reload Macros. Imported recordings use their native timing; hamburbur recordings retain head, rig, and hand poses. Renaming preserves each imported format. Corrupt, oversized, or invalid recordings are skipped without hiding valid files. Recorded-position macros use the selected start range (1m by default); Relative placement starts the translated route where you are. Playback stops on room changes and never replays menu button presses or enables itself from imported settings.
- **Corrected macro playback:** matches imported timing with a short smooth lead-in, keeps the physical head/body offset, leaves tracked head position alone, and keeps the final rig position when playback ends. This addresses the old underground/offset and snap-back paths; placement still needs to match the recording's map and game geometry.
- **Macro settings:** Legacy Macro Playback restores the earlier behavior when enabled; corrected mode is the default. Macro Playback Range offers 0.25/0.5/1/2/3m. Loop Macros repeats playback until stopped or the held play input is released. Playback Speed offers 0.5/0.75/1/1.25/1.5/2x. Macro Position chooses the recorded world route or a route translated to your current position. Macro Height Offset provides small vertical adjustments. The existing LT recording and RT playback controls are unchanged.
- **Notifications:** restored the notification code and rendering lifecycle from 0.1.1, including its canvas updates and desktop toast/array-list callbacks.
- Use **Save Menu State** to keep your theme, alignment, sounds, macro settings, and other menu preferences.

## New in 0.1.2

- **Long Arms:** default length 1.1 with a 0.5–2.5 range and finer steps near the default. Existing saved lengths migrate to the new choices; height settings stay separate.
- **Board text:** refresh outdated built-in text from older saves, preserve custom fields, and keep a backup at `ChudOS/boards.json.pre-012.bak` when migrating an existing file.
- **No Finger Gestures:** keep fingers open at the native finger mapper while preserving controller inputs for the menu and other mods.
- **Room:** Report All, Key-Leave with its input selector directly underneath, Disable Quit Box, Disable Map Triggers, Reconnect, Cancel Reconnect, Join Last Room, JoinRoom: ASTRA, and JoinRoom: CHUDOS. Random private-room selection now has 30 code choices.
- **Movement:** Drone Fly, Left And Right, Forward And Backwards, Velocity Movement, Throw, Double Jump, Fly Towards Gun, and Punch. Custom Platforms now has its own activation input setting.
- **Rig:** Rig Gun, Spazz Rig, Spazz Hands, Fixed LongArms, Grab Rig, and Fake Lag.
- **Settings:** Drone Speed/Response; Velocity Speed/Acceleration/Input/Direction/Release; Throw Power; Double Jump Power/Input; Punch Power; Custom Platform Input; and five additional themes.
- **Spider Man:** hold either trigger to attach a web to a surface, swing under gravity, and release to retain momentum. Each hand has its own web.
- **Platform Gun:** hold the trigger to rapidly place platforms, with a 50 ms minimum interval and a maximum of 20 active gun platforms. The original platform construction and collisions are retained.
- **Fun:** Unlock FPS; 30, 60, 72, 90, 120, 144, 165, 240, and 360 FPS presets; and Reset FPS.

### Additional 0.1.2 features

Movement:
- Custom Arms — hold left trigger to increase length and right trigger to decrease it.
- Adjustable range: 0.5–2.5, starting at 1.1; height uses the existing Long Arm Height setting.
- Custom Arms Speed and Reset Custom Arms settings.

Player:
- Chams — solid-color player highlights visible through walls, with the original avatar materials preserved.
- Beacons — vertical markers above other players' heads.
- Player/theme colors, Chams opacity, and Beacon height/width settings.

Rig:
- Helicopter (Trolling)
- T-Pose (Trolling)
- Freeze Rig Body (Trolling)
- Freeze Rig Limbs (Trolling)
- Jumpscare Gun (Trolling)

(Trolling) identifies playful avatar-pose features. It is not a controller keybind or a safety rating. The freeze mods affect your own rig. Helicopter and T-Pose use A; Jumpscare Gun uses the existing VR/PC gun controls.

**Controls and behavior**

- Spider Man: aim a controller at a surface and hold its trigger. Release to detach. It uses surface anchors and rope tension rather than the existing Grappling Hooks pull.
- Drone Fly: left stick moves, right stick controls height and turning. Configure speed and response in Settings.
- Left And Right: left/right trigger. Forward And Backwards: left trigger forward, right trigger backwards. Both follow your horizontal head direction.
- Velocity Movement: selected input, direction, speed, acceleration, and release behavior from Settings. Hand direction uses the right controller.
- Throw: a quick hand swing away from surfaces adds a burst in that direction; slow the hand before another burst. Body translation is excluded from the hand sample.
- Double Jump: press A by default for one additional airborne jump. Landing rearms it; input and power are configurable.
- Punch: other players' nearby hand movements knock you back. Punch Power changes the impulse applied to you.
- Platform Gun: aim with grip and hold trigger to keep placing platforms; on PC, hold right mouse to aim and left mouse to place. Release either button to stop.
- Fly Towards Gun and Rig Gun: use the existing gun controls. Rig Gun places your rig slightly above the surface; Fly Towards Gun moves your player toward the hit point.
- Spazz Rig and Spazz Hands: hold A. Grab Rig: hold right grip. Fixed LongArms uses uniform 1.5 scale. Fake Lag updates the local rig pose every half second.
- Key-Leave: enable the toggle, then press its selected input. Enabling it while holding the input does not immediately leave.
- Reconnect and Join Last Room remember the last joined room during this game session. Cancel Reconnect stops a queued join before it is submitted; it cannot retract a request already sent to the game.
- Report All submits a native Cheating report for every other player in the room.
- Unlock FPS disables Unity VSync and its application frame cap. The VR runtime can still control headset frame rate. Reset FPS restores the previous frame limit and VSync settings.

Download ChudOS.dll from the release page to use these features.

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

409 unique buttons, including mods, settings, and actions.

## Main — 4

- Join Discord
- SB Control
- Menu Source
- PC Button Click

## Movement — 79

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
- Air Brake (RG)(LG)
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
- Drone Fly
- Left And Right
- Forward And Backwards
- Velocity Movement
- Throw
- Double Jump
- Fly Towards Gun
- Punch
- Custom Arms
- Spider Man
- Wall Walk V2
- Momentum Bank (X)(A)
- Surface Skate (RG)
- Air Paddle (RG)(LG)
- Gravity Compass (X)
- Landing Brake
- Advanced Checkpoints (X)(A)(B)(RG)
- Climby Hands (LG)/(RG)
- Auto Climb (RG)(LG)
- Bunny Hop (A)
- Portal Gun (RG)(RT)(A)
- C4 (RG)(A)
- Hoverboards Everywhere
- Disable Planet Zones
- Slippery Hands
- Solid Players
- Solid Water
- PC Walking (WASD)(Space)(Shift)(Ctrl)(RMB)

## Rig — 36

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
- Rig Gun
- Spazz Rig
- Spazz Hands
- Fixed LongArms
- Grab Rig
- Fake Lag
- Helicopter
- T-Pose
- Freeze Rig Body
- Freeze Rig Limbs
- Jumpscare Gun
- Head Spin Yaw
- Head Spin Pitch
- Head Spin Roll
- Pose Sequencer (X)(A)(B)

## Fun — 21

- Fix Head
- Fix All
- Free Camera
- Unlock FPS
- 30 FPS
- Camera FOV
- 60 FPS
- Silent Hand Taps
- 72 FPS
- 90 FPS
- 120 FPS
- 144 FPS
- 165 FPS
- 240 FPS
- 360 FPS
- Reset FPS
- Snowball Spam
- Water Balloon Spam
- Echo Race (A)
- Gate Runner (X)(A)(B)
- Beat Targets

## Player — 8

- Body Tracers
- Bone ESP
- 2D Box ESP
- 3D Box ESP
- 4D Box ESP
- Trails ESP
- Chams
- Beacons

## Room — 19

- Leave
- JoinRandom
- JoinRandom Private
- EU
- US
- USW
- Quit Game
- Disable Network Triggers
- Report All
- Key-Leave
- Key-Leave Input
- Disable Quit Box
- Disable Map Triggers
- Reconnect
- Cancel Reconnect
- Join Last Room
- JoinRoom: ASTRA
- JoinRoom: CHUDOS
- Reauthenticate

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

## Visuals — 20

- Hide Fog
- Ambient Tint
- Visual Info
- Name Tags
- Overlay
- Jump Predictions
- Show Room Triggers
- Draw Gun
- Hide Head Cosmetics
- Fire Fists
- Forest Lightning
- Nebula Storm
- Black Hole
- White Hole
- Random Domain
- No Leaves
- Momentum Ribbon
- Contact Sparks
- Landing Pulse
- Smooth Desktop Camera

## Weather — 2

- Time
- No Rain

## Soundboard — 16

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
- Beat Halo

## Macros — 7

- Macro Controls
- Record Macro
- Stop Macro
- Play Selected Macro
- Next Macro
- Reload Macros
- Open Macros Folder

## Settings — 159

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
- Menu Open Sound
- Auto Translate Boards
- Open Sound
- Open Volume
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
- Custom Platform Input
- Speed Boost Preset
- Long Arm Length
- Speed Boost Jump Speed
- Long Arm Height
- Speed Boost Multiplier
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
- Drone Speed
- Drone Response
- Velocity Speed
- Velocity Acceleration
- Velocity Input
- Velocity Direction
- Velocity Release
- Throw Power
- Double Jump Power
- Double Jump Input
- Punch Power
- Custom Arms Speed
- Reset Custom Arms
- Chams Color
- Chams Opacity
- Beacon Height
- Beacon Width
- Beacon Color
- Menu Alignment
- Menu Distance
- Menu Height
- Recenter Menu
- Reset Menu Position
- Join Random: Current Map
- Head Spin Speed
- Throwable Interval
- Legacy Macro Playback
- Macro Playback Range
- Loop Macros
- Macro Playback Speed
- Macro Position
- Macro Height Offset
- Keyboard Style
- Keyboard Placement
- Keyboard Size
- Ghost Hand Guides
- Wall Walk V2 Strength
- Wall Walk V2 Input
- Keyboard Language
- Keyboard Keys
- Keyboard Spacing
- Keyboard Numbers
- Momentum Bank Capacity
- Surface Skate Speed
- Air Paddle Power
- Compass Gravity
- Landing Brake Distance
- Auto Climb Power
- Auto Climb Mode
- Bunny Hop Power
- Gate Runner Size
- Pose Transition Time
- Desktop Camera Smoothing
- Beat Halo Style
- Effect Size
- Effect Intensity
- Effect Quality
- Effect Colors
- Profile Slot
- Load Profile Mods
- Save Profile
- Preview Profile
- Load Profile
- Reset Profile Settings
- Portal Size
- Portal Cooldown
- C4 Power
- C4 Radius
- PC Walking Speed
- PC Sprint Multiplier
- PC Look Sensitivity
- Solid Player Collision
- Slippery Hands Strength
