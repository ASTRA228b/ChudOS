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

using System;

namespace ChudOS.Stuff;

public enum ButtonType { Toggle, Press, Option }
public enum RunOption { Update, FixedUpdate, LateUpdate, Once }

[AttributeUsage(AttributeTargets.Method)]
public sealed class MenuButtonAttribute : Attribute
{
    public string? Name { get; }
    public ButtonType Type { get; }
    public string Category { get; set; } = "Main";
    public int Order { get; set; }
    public string Description { get; set; } = "";
    public RunOption runOption { get; set; } = RunOption.Update;
    public bool DefaultEnabled { get; set; }
    public bool Notify { get; set; } = true;
    public string messageEnable { get; set; } = "";
    public string messageDisable { get; set; } = "";
    public string messagePress { get; set; } = "";
    public string[] Options { get; set; } = Array.Empty<string>();
    public bool LoopOptions { get; set; } = true;

    public MenuButtonAttribute(string name, ButtonType type) { Name = name; Type = type; }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class MenuEnableAttribute : Attribute
{
    public string Name { get; }
    public MenuEnableAttribute(string name) { Name = name; }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class MenuDisableAttribute : Attribute
{
    public string Name { get; }
    public MenuDisableAttribute(string name) { Name = name; }
}