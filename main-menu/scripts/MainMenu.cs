using Godot;
using System;

namespace Crosswalk
{
    public partial class MainMenu : Control
    {
        public void _on_play_pressed()
        {
            GetTree().ChangeSceneToFile("res://scenes/levels/level.tscn");
        }
    }
}