using UnityEngine;
using ChudOS.Stuff;

namespace ChudOS.Mods.Room;

public static class RoomMods
{
    private static readonly string[] Codes = { "1", "12", "1245", "1234", "LUCIO", "MODS", "MOD", "TTTPIG", "MODDING", "MENU", "GHGHFJSDJ", "HSDFHSD", "SDFSDDFS", "HHHHH", "GGG3IU9" };

    [MenuButton("Leave", ButtonType.Press, Category = "Room", Order = 0)]
    public static void Leave() => Room.Leave();

    [MenuButton("JoinRandom", ButtonType.Press, Category = "Room", Order = 1)]
    public static void JoinRando() => Room.JoinRandom();

    [MenuButton("JoinRandom Private", ButtonType.Press, Category = "Room", Order = 2, Description = "Joins a random private code that may or may not have people in it.")]
    public static void JRP()
    {
        string code = Codes[UnityEngine.Random.Range(0, Codes.Length)];
        Room.JoinRoom(code);
    }

    [MenuButton("EU", ButtonType.Press, Category = "Room", Order = 3)]
    public static void EU() => Room.ConnectToRegion("eu");

    [MenuButton("US", ButtonType.Press, Category = "Room", Order = 4)]
    public static void US() => Room.ConnectToRegion("us");

    [MenuButton("USW", ButtonType.Press, Category = "Room", Order = 5)]
    public static void USW() => Room.ConnectToRegion("usw");

    [MenuButton("Quit Game", ButtonType.Press, Category = "Room", Order = 6)]
    public static void KillGame() => Application.Quit();

    [MenuButton("Disable Network Triggers", ButtonType.Toggle, Category = "Room", Order = 7, runOption = RunOption.Once)]
    public static void DisableNetworkTriggers()
    {
        Room.IsEnabled = true;
    }

    [MenuDisable("Disable Network Triggers")]
    public static void EnableNetworkTriggers()
    {
        Room.IsEnabled = false;
    }
}
