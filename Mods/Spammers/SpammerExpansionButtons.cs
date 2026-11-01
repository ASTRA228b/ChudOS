// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;

namespace ChudOS.Mods.Spammers;

public static class SpammerExpansionButtons
{
    [MenuButton("Projectile Gun", ButtonType.Toggle, Category = "Spammers", Order = 700, runOption = RunOption.Once, Controls = "(RG)(RT)", Description = "Local-only emitter; native snowball and water-balloon releases remain separate.")]
    public static void EnableProjectileGun() => BetaFeature.Enable<ExpansionEmitters>("Projectile Gun");
    [MenuDisable("Projectile Gun")]
    public static void DisableProjectileGun() => BetaFeature.Disable("Projectile Gun");

    [MenuButton("Projectile Pattern Emitter", ButtonType.Toggle, Category = "Spammers", Order = 701, runOption = RunOption.Once, Controls = "(RT)/(LMB)", Description = "Local-only emitter; native snowball and water-balloon releases remain separate.")]
    public static void EnableProjectilePatternEmitter() => BetaFeature.Enable<ExpansionEmitters>("Projectile Pattern Emitter");
    [MenuDisable("Projectile Pattern Emitter")]
    public static void DisableProjectilePatternEmitter() => BetaFeature.Disable("Projectile Pattern Emitter");

    [MenuButton("Impact Painter", ButtonType.Toggle, Category = "Spammers", Order = 702, runOption = RunOption.Once, Controls = "(RG)(RT)", Description = "Local-only emitter; native snowball and water-balloon releases remain separate.")]
    public static void EnableImpactPainter() => BetaFeature.Enable<ExpansionEmitters>("Impact Painter");
    [MenuDisable("Impact Painter")]
    public static void DisableImpactPainter() => BetaFeature.Disable("Impact Painter");

    [MenuButton("Bubble Wand", ButtonType.Toggle, Category = "Spammers", Order = 703, runOption = RunOption.Once, Controls = "(RT)/(LMB)", Description = "Local-only emitter; native snowball and water-balloon releases remain separate.")]
    public static void EnableBubbleWand() => BetaFeature.Enable<ExpansionEmitters>("Bubble Wand");
    [MenuDisable("Bubble Wand")]
    public static void DisableBubbleWand() => BetaFeature.Disable("Bubble Wand");

    [MenuButton("Confetti Cannon", ButtonType.Toggle, Category = "Spammers", Order = 704, runOption = RunOption.Once, Controls = "(RT)/(LMB)", Description = "Local-only emitter; native snowball and water-balloon releases remain separate.")]
    public static void EnableConfettiCannon() => BetaFeature.Enable<ExpansionEmitters>("Confetti Cannon");
    [MenuDisable("Confetti Cannon")]
    public static void DisableConfettiCannon() => BetaFeature.Disable("Confetti Cannon");

    [MenuButton("Firework Sequencer", ButtonType.Toggle, Category = "Spammers", Order = 705, runOption = RunOption.Once, Controls = "(RT)/(LMB)", Description = "Local-only emitter; native snowball and water-balloon releases remain separate.")]
    public static void EnableFireworkSequencer() => BetaFeature.Enable<ExpansionEmitters>("Firework Sequencer");
    [MenuDisable("Firework Sequencer")]
    public static void DisableFireworkSequencer() => BetaFeature.Disable("Firework Sequencer");

}
