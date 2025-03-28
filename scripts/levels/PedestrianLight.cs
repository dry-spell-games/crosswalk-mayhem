using Crosswalk;
using Godot;
using System;

namespace Crosswalk
{
    public partial class PedestrianLight : AnimatedSprite2D
    {

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            // Etsi Level1 instanssi
            Level1 level = GetNode<Level1>("/root/Level1");

            if (level != null)
            {
                level.PedestrianLight += OnPedestrianLightChanged; // Liitä signaali
            }
        }

        private async void OnPedestrianLightChanged(bool _pedestrianGreen)
        {
            Level1 level = GetNode<Level1>("/root/Level1");

            if (_pedestrianGreen)
            {
                GD.Print("Playing GREEN");
                Play("green");
                await ToSignal(GetTree().CreateTimer(level._pedestrianGreenTimer - level._blinkTimer), "timeout");
                GD.Print("Playing BLINK");
                Play("blink");
            }
            else
            {
                GD.Print("Playing RED");
                Play("red");
            }
        }
    }
}
