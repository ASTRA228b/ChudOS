# ChudOS Menu Notes

Update history, detailed controls, behavior notes, and the full categorized button list.

[Back to the ChudOS overview](README.md)

Historical update entries describe their respective versions; newer entries take precedence.

## New in V0.1.5 Final

- **Custom JSON themes:** Settings > Custom Theme Editor supports JSON import/copy, RGB and hex controls, throttled live preview, Apply, reset, and saved files in ChudOS/Themes. Save Menu State preserves the active theme document. Invalid JSON leaves the current theme intact. Custom theme gradient controls now synchronize with the menu settings.
- **Pressed colors and GUI appearance:** custom themes have an independent pressed color. Supported GUI styles follow the selected palette and font settings. All window callbacks use unrestricted GUI.DragWindow().
- **Discord Rich Presence:** Main > Discord Rich Presence uses application 1564819192656437389, currently named CHUD Studio, with ChudOS activity details and the ChudGPT logo. Missing/closed Discord, connection failure, or startup timeout produces a notification and switches the feature off. Private room codes and account IDs are omitted. See Docs/DiscordPresence.md for runtime requirements.
- **Branding:** the rear menu label includes the embedded ChudGPT logo beside v0.1.5. The README also displays the same logo.
- **Main utilities:** Open Game Folder and Restart Game. A hidden restart helper waits for the existing game process to exit before asking Steam to relaunch it.
- **Soundboard:** Sound Bindings uses F9 or a selected controller input to play/stop the selected sound. Controller polling failures leave F9 usable. Desktop media previous/play-pause/next actions control the Windows media session.
- **Player:** Voice Level Bars show received speaker amplitude, refresh visuals at 30 Hz, and refresh player entries across room changes. Silent players have hidden indicators. The maximum is 32 indicators.
- **Install package:** the release ZIP includes ChudOS.dll and the four required Discord runtime DLLs. Keep them together. Build and isolated checks passed; live VR placement, microphone behavior, Discord rendering, and actual restart execution still need in-game verification.

## New in V0.1.5 Beta

Historical beta notes follow. The final release includes the additions listed above.

- **Jarvis:** enable Main > Jarvis, then open Jarvis GUI or use Ask Jarvis. F8 always starts voice capture. Jarvis starts disabled and can be saved through Save Menu State.
- **Voice Assistant:** local commands work without an AI: enable/disable/toggle a mod, open a category, press an action, set an existing option value, increase/decrease an option, save settings, request a room, and control soundboard playback. Examples: "enable platforms", "set menu theme to OG Blue", "set drone speed to six", "set speed boost amount to ten", "open movement", and "join room ASTRA". Ambiguous names or nonexistent presets do not execute. Commands use the existing button callbacks and guards.
- **GPT Jarvis:** uses the compatible GPT chat service and menu-command response format. The service chooses its backend model; ChudOS does not identify it as a verified direct OpenAI connection.
- **ChudGPT Jarvis:** all 14 choices: Public, Music, Buggy, 700, 1300, 1500, 1600, Ultimate, Plus, Pro, Code, Mega, Plato, and Plato Mini. Sends only the user's message and conversation history, without a system prompt, Jarvis instructions, or menu snapshot. Public/Main retain separate per-model sessions; Plato uses bounded local message history. ChudGPT replies are conversation text and never execute menu commands; explicit local voice commands still work before contacting the model. If a request fails, Jarvis speaks and displays a suggestion to switch to GPT Jarvis. Providers never switch automatically.
- **Spoken replies:** all three modes use a separate local audio source, leaving the soundboard and microphone mixer intact. Windows Default uses an installed Windows voice through a hidden synthesis process. Brian, Amy, Russell, Nicole, Matthew, Joanna, and Raveena use the web TTS service, with Windows synthesis as a fallback when the web request fails. Cached clips and cache files are bounded. Replies are spoken up to 550 characters; the GUI retains the longer reply.
- **Voice controls:** Jarvis keeps listening for the selected wake word whenever enabled and ready, including when Push To Talk is selected. Enabling shows and speaks the active wake word even if ordinary Jarvis notifications are off; the GUI shows the active word and available choices. Choose Jarvis, Assistant, Friday, or System in Settings. Say the selected word, wait for the ready tone and **Listening â€” speak now**, then speak your command. Recognition pauses while a request is processed or a reply is spoken, then resumes automatically. Push To Talk additionally records while the selected input/F8 is held, within the configured listening limit. Ask Jarvis uses a timed listening window. Disabling Jarvis stops recognition.
- **Wake-word handoff:** a short ready tone plays only after dictation reports Running; the earlier spoken acknowledgement no longer claims listening has already started. Say the wake word, wait for the tone/Listening indicator, then speak. A wake word followed by silence returns to standby without speaking a transcription error; real recognition errors and unfinished transcriptions remain visible. The ready tone follows Spoken Replies and Jarvis Volume.
- **Recognition fixes/settings:** a fresh, disposable dictation session isolates callbacks from previous requests. Final results arriving after completion/stop are accepted during a bounded grace period. Partial transcripts are shown in the GUI but never execute commands. Low-confidence final text is retained for diagnosis instead of silently discarded. Balanced accepts Medium/High, High Confidence Only accepts High, and Allow Low Confidence also accepts Low (never Rejected). The default listening window is 12 seconds and silence timeout is 3 seconds; Settings and the Jarvis GUI expose alternatives. Windows Microphone Settings and Windows Speech Settings buttons open the relevant OS pages without changing permissions or device selection. Dictation uses Windows' default input device. The GUI shows Heard, speech status/error codes, and reply source, so recognition failures can be distinguished from AI replies. Raw speech text is not written to the console log.
- **Jarvis settings:** provider, ChudGPT model, activation, input, wake word, voice, volume, spoken replies, text notifications, menu controls, optional notification dictation, clear conversation, and restart. Typed input remains available when Windows recognition is unavailable. Recognition uses the Windows speech language and requires microphone access and Windows speech support.
- **GUI:** a themed window built with MakeWindow, provider/model selectors, typed requests, Listen, Stop, Clear Conversation, and the latest request/reply. No external AI request is sent in plain Voice Assistant mode.
- Windows recognition, selected web voices, and AI providers have their own service availability. Windows may use online dictation; cloud voice selection sends reply text to its TTS provider. Speech plays locally and is not deliberately injected into game voice chat.
- **Validation:** isolated command and provider-contract checks, a Windows-generated speech WAV, and live basic replies from the GPT service and all three ChudGPT API families. Full microphone recognition, VR input, and in-game coexistence still need testing. This beta is published for testing; in-game issues may remain.

### Expansion pack

- **Movement:** Airplane, Bird Fly, Bubble Locomotion, Zipline Launcher, Rideable Rockets, Nailgun Climbing, Auto Walk, Auto Branch, Air Swim, Controller Flick, Wall Kick, Ledge Hang, Parkour Vault, Wind Tunnel Builder, Moving Walkway, and Bungee Anchor. Air Swim requires loaded native water data. Auto Branch uses nearby loaded branch names; it is a proximity helper, not a map pathfinder. Zipline releases at its endpoint.
- **Rig:** Weeping Angel, Minecraft Animations, Chicken Rig, Amputate Rig, Floating Rig, and Eye Contact. These use the existing rig ownership and pose restoration system. Avatar bone compatibility affects Amputate Rig.
- **Player:** Cosmetic Inspector shows matching locally loaded cosmetic renderers; Player Audio Panel adjusts individual received voice sources and restores their volumes. Outline ESP samples up to four nearby player silhouettes. Voice Radar, Nearby Player Alert, and FOV Cone Viewer add local indicators.
- **Visuals:** Minimap, World Mirror, Fullbright, Underwater Vision, Skybox Editor, Surface Inspector, Wind Barrier Viewer, Projectile Trajectory Preview, Photo Mode, Camera Dolly, NPC Tracker, and Resource Tracker. Tracker matches depend on objects loaded by the current map. World Mirror is a live camera screen. Trajectory Preview is a local ballistic estimate. Photos save under `ChudOS/Photos`.
- **Optimize Game:** Visuals toggle lowering mipmapped textures to quarter resolution or lower, disabling shadows, anti-aliasing, anisotropic filtering, soft particles and realtime reflection updates, reducing pixel lights and LOD detail. It captures and restores the previous values; settings change only on enable/disable. Some game shaders, textures and rendering paths may ignore global quality settings. It does not change FPS caps or collision geometry.
- **Weather:** Snow Control hides matching loaded looping snowfall particles and restores their prior emission state when disabled.
- **Fun:** Physics Sandbox, Domino Builder, Marble Run, Force Lab, Rope Bridge Builder, RC Drone, Boomerang, Paper Planes, Yo-Yo, Juggling Practice, Mini Bowling, Ring Toss, Virtual Darts, Cup Stacking, Air Hockey, Pocket Basketball, Rhythm Drums, Metronome, Reaction Trainer, Memory Pads, Precision Landing Challenge, Balance Beam Challenge, Laser Maze, Throwable Target Range, and Theremin. These are local practice props/effects. Cup Stacking uses solid cylinder props. Sandbox supports grabbing, scale sliders and joining the last two props. Rope Bridge uses two gun-selected anchors.
- **Spammers:** Snowball Spam and Water Balloon Spam move here from Fun without changing their saved names. Their release path now creates the native held throwable before checking ownership, respects occupied hands and the shared network budget, supports left/right hand and configurable inputs, and throttles missing asset requests. A loaded native catalog and throwable are still required; submission is not confirmation of server acceptance.
- **Local emitters:** Projectile Gun, Projectile Pattern Emitter (Fountain/Rain/Orbit), Impact Painter, Bubble Wand, Confetti Cannon, and Firework Sequencer live in Spammers. All six are local previews/effects; they do not broadcast spawned objects to the room. Native projectile previews require a loaded throwable prefab.
- **Room:** Favorite Rooms and Room History share a small notebook GUI; Join Clipboard Room validates a room code before requesting a native join, and Copy Room Code copies the current code. Up to 30 favorites/history entries save locally in `ChudOS/room-notebook.json`.
- **Macros:** Macro Editor saves trimmed copies without overwriting the original recording. Macro Compare draws two normalized frame-sampled routes with optional start alignment. Route Deviation Coach measures distance from the selected route. Scrubbing displays a marker and does not teleport the player.
- **Safety:** Mod Health Monitor shows readiness and recent ChudOS errors while enabled. It does not suppress errors, alter authentication or guarantee protection from bans.
- **Shared settings:** Expansion Movement Power/Range, Expansion Object Limit, Sandbox Prop Mass, Toy Launch Power, Local Emitter Interval, Expansion Visual Range, and Practice Audio Volume. Geometry/mass settings apply when new props or scenes are created; disable/re-enable an active tool to rebuild it with a new limit.
- **Controls:** new buttons include short input hints and descriptions. Common practice controls are A/Space to reset, B/R to change shape, RG/RMB to grab/release, and X/F for the tool's alternate action. RC Drone uses left stick/arrows, A/Space up and LG/Shift down. Gun tools retain GunLib aiming/selection. Close the menu to fire emitters.
- **Performance:** feature components are created on enable and destroyed on disable; local prop/effect counts are bounded. Scene discovery is throttled, effect objects are reused, outline baking is staggered, and quality changes run only on setup/cleanup. New GUIs use MenuHelper.MakeWindow. No measured idle-time or FPS guarantee is made without profiling in the game.
- **Expansion validation:** compiled callback/cleanup checks, numerical geometry/route tests, quality restoration tests, and native throwable creation/release regression tests passed. The full Release build succeeds with eight existing missing-reference warnings. These checks do not simulate the game, VR tracking, or remote server behavior. This is a beta prerelease, not a guarantee that every feature works in-game.

### Gradients, fonts, OP and holdables

- **Gradients:** Off preserves the selected original theme. Aurora, Sunset, Midnight, Ice, Cotton Candy and Custom use cached opaque textures, adjustable direction, separate enabled colors, and optional throttled UV animation. Existing pressed material references and highlighting are retained.
- **Fonts:** Original, Segoe UI, Verdana and Consolas choices for menu, boards and GUIs, with menu/GUI size presets and font reset. Missing OS fonts fall back through available fonts. Menu labels use glyph advances to fit; soundboard title/artist marquees remeasure when fonts change. Original board fonts restore with the board manager. Options save through menu state/profiles; notifications are unchanged.
- **OP:** Guardian Controls and Team Controls open registered GUIs. Guardian mode/valid zones are required; team assignment requires the active Monke Ball arena. Both require current room master authority, validate current players and use the existing request budget. Summon/Despawn, Grab, Move Gun, Freeze, Pet and Physics Lucy tools require a loaded, locally owned ghost. Continuous Lucy controllers are exclusive and restore captured state if still owned.
- **Prop Control Gun:** local repositioning of supported dropped transferable objects owned by you; restores captured rigidbody flags on release. Sticky/physical holdable controllers do not compete with it.
- **GR Trail / Draw Output:** current Ghost Reactor handprint API is local. This build sends no guessed BroadcastHandprint RPC. Both trail and optional Draw Gun output require the active Reactor zone and throttle printing.
- **Premade holdables:** choose Toy Sword, Toy Hammer, Round Shield, Magic Wand, Banana, Flower, Toy Microphone, ChudOS Flag, Wooden Cross, Bisexual Flag, Gay Pride Flag (rainbow), or Trans Flag in Premade Holdable, then enable Equip Premade Holdable. No bundle, asset download or desktop GUI is required. These are twelve model choices, not twelve separate gameplay mods.
- **Holdable Browser:** includes all twelve premade models and optional visual bundle imports, with previews, favorites and independent left/right model scale. Up to four bundles stay cached while open; asynchronous loads release assets even if the browser closes before completion. Script-bearing prefabs are rejected. See [Holdables](Docs/Holdables.md) for the manifest format.
- **Other Holdables:** Sticky Holdables retains your grabbed native items until B/R releases them. Physical Holdables uses approximate local sphere proxies for compatible owned dropped items. Handheld Camera has a 10 Hz preview and saves PNGs to ChudOS/Photos. Squeeze Toy, Custom Flashlight, Measuring Tape, Dual Hand Orbs and Laser Eyes are local. Repeated orbs use a bounded pool. New tool inputs ignore active GUI areas and Jarvis text editing.
- **Settings:** hand, original/theme/player/preset color, size, offsets, rotation, throw strength, haptics, placement reset, flashlight range/brightness and Lucy physics bounce. Hand-dependent hints update with the hand selector.
- **Player/Soundboard/Safety:** Palm Player List and Palm Stats add hand-mounted panels. Cosmetic Inspector reads equipped cosmetic data plus room-local nickname, actor and color. Optional tracer filtering uses equipped cosmetics, not staff detection. Nearby alerts can filter for infected players. Microphone Echo restores its prior recorder state; Refresh Microphone restarts only a recording game recorder. It does not restart Windows dictation. Copy Pose Data copies local transform arrays to the clipboard.
- **Movement/Rig:** Upside Down Body uses native gravity/rotation overrides; Mirrored Rig reflects tracked poses through RigRuntime. The existing Rig Tweaks modes were checked and rotate rather than reflect poses.
- **Preserved:** NoClip, all platform constructors/variants, EveryPatch and shared input helpers remain unchanged. The reported NoClip/platform interaction is unconfirmed and remains plan-only.
- **Validation:** Release build succeeds with no C# warnings/errors and eight existing unresolved-reference warnings. Isolated checks cover compiled registrations/lifecycles, settings and gradient restoration, holdable loading/cancellation and per-hand scale, and built-in model caching/collider/resource ownership. In-game VR placement, physics, fonts/gradients and online mode behavior still require testing. These checks were performed during beta development; live behavior still needs testing.

## New in 0.1.4

- **Movement ports, Part 1:** Portal Gun (grip/trigger aims and places; A switches portals; desktop RMB/LMB/Q), personal C4 (RG places, A detonates; desktop G/Space), Solid Water, Solid Players, PC Walking, Hoverboards Everywhere, Slippery Hands, and Disable Planet Zones. Adds nine associated settings. Portal Gun uses surface-fit and exit-clearance checks, preserves transformed velocity, and shares GunLib ownership. Hoverboard-area and slip hooks are scoped to the local player while enabled and removed on disable. Original water trigger layers stay intact; Solid Water uses a local support collider at supported water surfaces. PC Walking uses WASD, Space jump, Shift sprint, Ctrl slow movement, and RMB look; it does not implement physical crouching. These additions still require live game testing.
- **Wall Walk:** keeps the original forward-force behavior, original strength/input settings, and saved button name.
- **Wall Walk V2:** a separate Movement toggle that attaches toward the contacted surface normal while holding the selected inputs. Has its own strength and input settings. Compensation for gravity only runs during valid contact; releasing leaves the original gravity setting intact. The two Wall Walk toggles turn each other off.
- **Reauthenticate:** Room button that retries native Mothership login once per request. Duplicate requests are blocked while pending, success/failure callbacks are detached afterward, and a long wait is reported without automatically retrying. No authentication payloads are shown.
- **Keyboard:** English QWERTY, Portuguese with accents/Ã‡, Spanish with accents/Ã‘, French AZERTY, German QWERTZ, and Italian layouts. Classic, Side Panel, Compact, or Wide style; Theme/Accent/Text Only keys; adjustable spacing, number row, placement, and size. Keyboard language is independent of menu language. Desktop text events accept accents and punctuation, and the combined menu/keyboard fits the camera view.
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

- **Long Arms:** default length 1.1 with a 0.5â€“2.5 range and finer steps near the default. Existing saved lengths migrate to the new choices; height settings stay separate.
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
- Custom Arms â€” hold left trigger to increase length and right trigger to decrease it.
- Adjustable range: 0.5â€“2.5, starting at 1.1; height uses the existing Long Arm Height setting.
- Custom Arms Speed and Reset Custom Arms settings.

Player:
- Chams â€” solid-color player highlights visible through walls, with the original avatar materials preserved.
- Beacons â€” vertical markers above other players' heads.
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
- **Themes:** open **Settings â†’ Menu Theme**. Use **Save Menu State** to keep your selection for next time.
- **SB Control:** open **Main â†’ SB Control**. Missing MP3 title/artist tags fall back to the filename and Unknown Artist.
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

597 unique buttons, including mods, settings, and actions.

284 toggle buttons outside Settings, plus 78 actions and 11 selectors. Settings has 224 entries. These counts are not all standalone gameplay mods.

Input hints: A/B/X/Y are controller face buttons; LT/RT are triggers; LG/RG are grips; LJ/RJ are joystick clicks; LMB/RMB are mouse buttons. A plus sign means hold together. F9 controls sound bindings and F8 starts Jarvis capture. Button descriptions provide additional controls.

## Main — 11

- Join Discord
- SB Control
- Menu Source
- PC Button Click
- Jarvis
- Jarvis GUI
- Ask Jarvis (F8)
- Stop Jarvis Response
- Discord Rich Presence
- Open Game Folder
- Restart Game

## Movement — 96

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
- Airplane (Arms Out)
- Bird Fly (Flap Arms)
- Bubble Locomotion (LG)(RG)
- Zipline Launcher (RG)(RT)
- Rideable Rockets (LG)(RG)
- Nailgun Climbing (RG)(RT)
- Auto Walk
- Auto Branch (RG)
- Air Swim
- Controller Flick (X)
- Wall Kick (A)
- Ledge Hang (RG)
- Parkour Vault (A)
- Wind Tunnel Builder (RG)(RT)
- Moving Walkway (RG)(RT)
- Bungee Anchor (RG)(RT)(B)
- Upside Down Body

## Rig — 43

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
- Weeping Angel
- Minecraft Animations
- Chicken Rig
- Amputate Rig
- Floating Rig
- Eye Contact
- Mirrored Rig

## Player — 17

- Body Tracers
- Bone ESP
- 2D Box ESP
- 3D Box ESP
- 4D Box ESP
- Trails ESP
- Chams
- Beacons
- Voice Level Bars
- Cosmetic Inspector (RG)(RT)
- Player Audio Panel
- Outline ESP
- Voice Radar
- Nearby Player Alert
- FOV Cone Viewer
- Palm Player List
- Palm Stats

## Visuals — 33

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
- Minimap
- World Mirror
- Fullbright
- Underwater Vision
- Skybox Editor
- Surface Inspector (RG)(RT)
- Wind Barrier Viewer
- Projectile Trajectory Preview
- Photo Mode (A)
- Camera Dolly (X)(A)
- NPC Tracker
- Resource Tracker
- Optimize Game

## Weather — 3

- Time
- No Rain
- Snow Control

## Fun — 45

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
- Echo Race (A)
- Gate Runner (X)(A)(B)
- Beat Targets
- Physics Sandbox (RG)(RT)(A)(B)(X)
- Domino Builder (RG)(RT)(A)(X)
- Marble Run (RG)(RT)(A)
- Force Lab (LG)(RG)(A)
- Rope Bridge Builder (RG)(RT)(A)
- RC Drone (Left Stick Axis)(A)(LG)
- Boomerang (RG)(A)
- Paper Planes (RG)(A)
- Yo-Yo (RG)(A)
- Juggling Practice (RG)(A)
- Mini Bowling (RG)(A)
- Ring Toss (RG)(A)
- Virtual Darts (RG)(A)
- Cup Stacking (RG)(A)
- Air Hockey (Hands)(RG)(A)
- Pocket Basketball (RG)(A)
- Rhythm Drums (Hands)(A)
- Metronome (GUI)
- Reaction Trainer (Hands)(A)
- Memory Pads (Hands)(A)
- Precision Landing Challenge (Body)(A)
- Balance Beam Challenge (Body)(A)
- Laser Maze (Body)(A)
- Throwable Target Range (Projectiles)(A)
- Theremin (Hands)
- Laser Eyes (RG)/(O)

## Room — 23

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
- Favorite Rooms
- Room History
- Join Clipboard Room
- Copy Room Code

## Soundboard — 22

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
- Desktop Media Play / Pause
- Desktop Media Next
- Desktop Media Previous
- Sound Bindings (F9)
- Mute Other Players
- Microphone Mute
- Microphone Pitch
- Beat Halo
- Microphone Echo
- Refresh Microphone

## Macros — 10

- Macro Controls
- Record Macro
- Stop Macro
- Play Selected Macro
- Next Macro
- Reload Macros
- Open Macros Folder
- Macro Editor
- Macro Compare
- Route Deviation Coach

## Holdables — 10

- Holdable Browser
- Sticky Holdables (B)/(R)
- Physical Holdables
- Equip Premade Holdable
- Premade Holdable
- Handheld Camera (RT)/(LMB)
- Squeeze Toy (RG)/(RMB)
- Custom Flashlight (A)/(Space)
- Measuring Tape (RT)/(LMB)(X)
- Dual Hand Orbs (LG)(RG)

## Spammers — 9

- Snowball Spam (RT)/(LMB)
- Water Balloon Spam (RT)/(LMB)
- Throwable Status
- Projectile Gun (RG)(RT)
- Projectile Pattern Emitter (RT)/(LMB)
- Impact Painter (RG)(RT)
- Bubble Wand (RT)/(LMB)
- Confetti Cannon (RT)/(LMB)
- Firework Sequencer (RT)/(LMB)

## OP — 11

- Guardian Controls
- Team Controls
- Summon Lucy
- Despawn Lucy
- Grab Lucy (RG)/(RMB)
- Move Lucy Gun (RG)(RT)
- Freeze Lucy
- Pet Lucy
- Lucy Physics (RG)/(RMB)
- Prop Control Gun (RG)(RT)
- GR Trail

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

## Safety — 31

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
- Mod Health Monitor
- Copy Pose Data

## Settings — 224

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
- Jarvis Provider
- ChudGPT Model
- Jarvis Activation
- Jarvis Input
- Jarvis Wake Word
- Jarvis Voice
- Jarvis Volume
- Jarvis Spoken Replies
- Jarvis Notifications
- Jarvis Menu Controls
- Jarvis Dictate Notifications
- Clear Jarvis Conversation
- Restart Jarvis
- Jarvis Listening Time
- Jarvis Silence Time
- Jarvis Recognition
- Windows Microphone Settings
- Windows Speech Settings
- Throwable Hand
- Throwable Input
- Jarvis Speech Input
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
- Expansion Movement Power
- Expansion Movement Range
- Expansion Object Limit
- Sandbox Prop Mass
- Toy Launch Power
- Local Emitter Interval
- Expansion Visual Range
- Practice Audio Volume
- Gradient Theme
- Gradient Start Color
- Gradient End Color
- Gradient Enabled Start
- Gradient Enabled End
- Gradient Direction
- Gradient Animation
- Gradient Speed
- Menu Font
- Board Font
- GUI Font
- Menu Text Size
- GUI Text Size
- Reset Fonts
- Holdable Hand
- Holdable Size
- Holdable Color
- Holdable Offset X
- Holdable Offset Y
- Holdable Offset Z
- Holdable Pitch
- Holdable Yaw
- Holdable Roll
- Holdable Throw Strength
- Holdable Vibrations
- Reset Holdable Placement
- Flashlight Range
- Flashlight Brightness
- Lucy Physics Bounce
- Nearby Alert Targets
- Tracer Filter
- Draw Output
- Custom Theme Editor
- Next Custom Theme
- Reload Custom Themes
- Soundboard Bind Input
