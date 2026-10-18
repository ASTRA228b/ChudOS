using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications.CanvasNotify;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class MenuSettings
{
    [MenuButton("PC View", ButtonType.Press, Category = "Settings", Order = 0, Notify = false)]
    public static void PCView() => ChudOS.Core.Main.OpenPC();

    [MenuButton("Array List", ButtonType.Toggle, Category = "Settings", Order = 1, DefaultEnabled = true, runOption = RunOption.Once, Notify = false)]
    public static void ArrayList() => GUIManager.ArrayListEnabled = true;
    [MenuDisable("Array List")]
    public static void ArrayListOff() => GUIManager.ArrayListEnabled = false;

    [MenuButton("GUI Input", ButtonType.Option, Category = "Settings", Order = 2, Options = new[] { "CTRL", "Alt", "X", "Down Arrow" }, Notify = false)]
    public static void GUIInput() => GUIManager.SetOpenKey(ButtonManager.GetButton("GUI Input")!.OptionIndex);

    [MenuButton("Close GUI Input", ButtonType.Option, Category = "Settings", Order = 3, Options = new[] { "CTRL", "Alt", "X", "Down Arrow" }, Notify = false)]
    public static void CloseInput() => GUIManager.SetCloseKey(ButtonManager.GetButton("Close GUI Input")!.OptionIndex);

    [MenuButton("Close All GUIs", ButtonType.Press, Category = "Settings", Order = 4)]
    public static void CloseAll() => GUIManager.CloseAll();


    [MenuButton("Save Menu State", ButtonType.Press, Category = "Settings", Order = 5, Notify = false)]
    public static void SaveMenuState() => FileManager.SaveMenuState();


    [MenuButton("Canvas Notifications", ButtonType.Toggle, Category = "Settings", Order = 6, DefaultEnabled = true, runOption = RunOption.Once, Notify = false)]
    public static void Notifications() => CanvasNotifyLib.Enabled = true;
    [MenuDisable("Canvas Notifications")]
    public static void NotificationsOff() => CanvasNotifyLib.Enabled = false;
}
