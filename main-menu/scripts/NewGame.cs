using Godot;
using System;

namespace Crosswalk
{
    public partial class NewGame : Node
    {
        public void _on_StartGameButton_pressed()
        {
            GD.Print("New Game painettu!");
            GetTree().ChangeSceneToFile("res://scenes/levels/level.tscn");
        }
    }
}