using Godot;
using System;

namespace Crosswalk
{
    public partial class MainMenu : Control
    {
        public void _on_play_pressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/Levels/Level1.tscn");
        }
    }
}