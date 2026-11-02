# Discord Rich Presence

**Discord Rich Presence can cause FPS instability. Turn it off if you notice stuttering or FPS drops.**

Enable **Main > Discord Rich Presence** with the Discord desktop client running. If Discord is absent, closes, rejects the connection, or cannot finish connecting within ten seconds, ChudOS notifies you and turns the feature off. It does not launch Discord automatically.

The application ID is `1564819192656437389`. Its public application name is currently **CHUD Studio**. The activity details identify **ChudOS / Gorilla Tag / v0.1.5**, and include elapsed time, a lobby summary, and Download/Source buttons. Private room codes and account IDs are not published.

The large artwork uses `https://chudgpt-landing.vercel.app/chudgpt-logo.png`. This was verified to match the embedded PNG at implementation time. Discord must be able to fetch the image; activity visibility also depends on Discord settings. Renaming the application or uploading an alternative asset is separate from this build.

## Single-DLL installation

The corrected build merges its Discord transport into **ChudOS.dll**. Copy that DLL into the plugins folder and restart the game. The transport uses the game's existing Newtonsoft.Json assembly and the framework supplied by the game; separate DiscordRPC and Windows-package DLLs are not required by ChudOS.

The earlier build referenced a separate DiscordRPC assembly during Harmony and Unity input-system startup scans. A missing companion DLL could therefore prevent the entire menu from loading. The merged build removes that assembly reference. The artwork guard is installed programmatically only when Discord Rich Presence starts, and removed when it stops.

The transport license is in ThirdParty/DiscordRPC-LICENSE.txt. Source builds restore the pinned NuGet package and ILRepack build tool, then merge the transport automatically.

## Other additions in this batch

Main has Open Game Folder and Restart Game. Restart uses a hidden helper that waits up to thirty seconds for the current game to exit, then asks Steam to relaunch it. If the helper cannot start, ChudOS leaves the game open.

Soundboard has desktop media previous/play-pause/next controls and **Sound Bindings**. F9 or the selected controller input toggles ChudOS playback. A controller-read failure disables controller polling for that instance, leaving F9 usable. Changing the input or re-enabling retries it. Typing in Jarvis or the theme editor suppresses the shortcut.

Player has **Voice Level Bars**, which updates visuals at 30 Hz and scans players once per second, with a maximum of 32 indicators. Silent players have hidden bars. Room changes refresh entries without automatically disabling the feature.

The rear menu version label now includes the embedded ChudGPT icon. VR appearance, live microphone levels, Discord artwork rendering, and an actual game restart still require in-game testing.
