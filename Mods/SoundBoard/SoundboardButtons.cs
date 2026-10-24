// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.SoundBoard;

public static class SoundboardButtons
{
    [MenuButton("SB Control", ButtonType.Press, Category = "Main", Order = 1, Notify = false)]
    public static void ControlPage() => ChudOS.Core.Main.OpenSoundboardControl();

    [MenuButton("Play / Stop", ButtonType.Press, Category = "Soundboard", Order = 0)]
    public static void PlayStop() => SoundboardManager.TogglePlay();

    [MenuButton("Next Sound", ButtonType.Press, Category = "Soundboard", Order = 1)]
    public static void NextSound() => SoundboardManager.Next();

    [MenuButton("Volume", ButtonType.Option, Category = "Soundboard", Order = 2, Options = new[] { "25%", "50%", "75%", "100%" })]
    public static void Volume() => SoundboardManager.SetVolume(ButtonManager.GetButton("Volume")!.OptionIndex);

    [MenuButton("Sound Output", ButtonType.Option, Category = "Soundboard", Order = 3, Options = new[] { "Local", "Microphone", "Both" })]
    public static void Output() => SoundboardManager.SetPlaybackMode((SoundManager.PlaybackMode)ButtonManager.GetButton("Sound Output")!.OptionIndex);

    [MenuButton("Mic Volume", ButtonType.Option, Category = "Soundboard", Order = 4, Options = new[] { "25%", "50%", "75%", "100%" })]
    public static void MicVolume() => SoundboardManager.SetMicVolume(ButtonManager.GetButton("Mic Volume")!.OptionIndex);

   
    [MenuButton("Autoplay", ButtonType.Toggle, Category = "Soundboard", Order = 5, runOption = RunOption.Once)]
    public static void Autoplay() => SoundboardManager.SetAutoPlay(true);
    [MenuDisable("Autoplay")]
    public static void DisableAutoplay() => SoundboardManager.SetAutoPlay(false);

    [MenuButton("Repeat", ButtonType.Toggle, Category = "Soundboard", Order = 6, runOption = RunOption.Once)]
    public static void Repeat() => SoundboardManager.SetRepeat(true);
    [MenuDisable("Repeat")]
    public static void DisableRepeat() => SoundboardManager.SetRepeat(false);

    [MenuButton("Shuffle", ButtonType.Toggle, Category = "Soundboard", Order = 7, runOption = RunOption.Once)]
    public static void Shuffle() => SoundboardManager.SetShuffle(true);
    [MenuDisable("Shuffle")]
    public static void DisableShuffle() => SoundboardManager.SetShuffle(false);

    [MenuButton("Open Soundboard GUI", ButtonType.Press, Category = "Soundboard", Order = 8)]
    public static void GUI() => SoundboardGUI.Visible = !SoundboardGUI.Visible;

    [MenuButton("Reload Sounds", ButtonType.Press, Category = "Soundboard", Order = 9)]
    public static void Reload() => SoundboardManager.Reload();
}
