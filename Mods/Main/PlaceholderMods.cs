using UnityEngine;
using ChudOS.Stuff;
using System.Diagnostics;


namespace ChudOS.Mods.Main;

public static class PlaceholderMods
{
    [MenuButton("Join Discord", ButtonType.Press, Category = "Main", Order = 0)]
    public static void JoinDiscord() => OpenLink("https://discord.gg/F6Et6BMQKw");

    [MenuButton("Menu Source", ButtonType.Press, Category = "Main", Order = 2)]
    public static void Source() => OpenLink("http://www.google.com/404");


    private static void OpenLink(string link)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = link,
            UseShellExecute = true
        });
    }
}
