// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Stuff;

namespace ChudOS.Mods.Room;

public static class RoomNotebookButtons
{
    [MenuButton("Favorite Rooms", ButtonType.Press, Category = "Room", Order = 700, Notify = false)]
    public static void FavoriteRooms() => RoomNotebook.Open(false);

    [MenuButton("Room History", ButtonType.Press, Category = "Room", Order = 701, Notify = false)]
    public static void RoomHistory() => RoomNotebook.Open(true);

    [MenuButton("Join Clipboard Room", ButtonType.Press, Category = "Room", Order = 702, Notify = false)]
    public static void JoinClipboardRoom() => RoomNotebook.JoinClipboard();

    [MenuButton("Copy Room Code", ButtonType.Press, Category = "Room", Order = 703, Notify = false)]
    public static void CopyRoomCode() => RoomNotebook.CopyCode();

}
