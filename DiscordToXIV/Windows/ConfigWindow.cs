using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
 
namespace DiscordToXIV.Windows;


public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var configValue = Configuration.SomePropertyToBeSavedAndWithADefault;
        if (ImGui.Checkbox("Random Config Bool", ref configValue))
        {
            Configuration.SomePropertyToBeSavedAndWithADefault = configValue;
            Configuration.Save();
        }

        var movable = Configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            Configuration.IsConfigWindowMovable = movable;
            Configuration.Save();
        }
    }
}
