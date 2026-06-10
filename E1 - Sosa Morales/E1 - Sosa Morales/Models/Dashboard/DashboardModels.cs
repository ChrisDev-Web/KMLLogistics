using System;
using System.Collections.Generic;

namespace E1___Sosa_Morales.Models.Dashboard;

public class DashboardCard
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Footer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string DefaultAction { get; set; } = string.Empty;
}

public class ModuleTab
{
    public string Label { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class ModuleViewModel
{
    public string Title { get; set; } = string.Empty;
    public string CurrentController { get; set; } = string.Empty;
    public string CurrentTab { get; set; } = string.Empty;
    public List<ModuleTab> Tabs { get; set; } = new List<ModuleTab>();
    public string SidebarActive { get; set; } = string.Empty;
}

public class SidebarItem
{
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}