using Crosswalk;
using Godot;
using System;

namespace Crosswalk
{
    public partial class PedestrianLight : AnimatedSprite2D
    {
        private int _difficulty = GameManager.Instance._difLvl;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            // Etsi Level1 instanssi
            Level1 level = GetNodeOrNull<Level1>("/root/Level1");
            // realLevel is supposed to be actual playable level
            Level realLevel = GetNodeOrNull<Level>("/root/Level");

            if (level != null)
            {
                level.PedestrianLight += OnPedestrianLightChanged; // Liitä signaali
            }
            else if (realLevel != null)
            {
                realLevel.PedestrianLight += OnPedestrianLightChanged;
            }
        }

        private async void OnPedestrianLightChanged(bool _pedestrianGreen)
        {
            Level1 level = GetNodeOrNull<Level1>("/root/Level1");
            Level realLevel = GetNodeOrNull<Level>("root/Level");

            if (_pedestrianGreen)
            {
                GD.Print("Playing GREEN");
                Play("green");
                if (realLevel != null)
                {
                    await ToSignal(GetTree().CreateTimer(realLevel._pedestrianGreenTimer[_difficulty] - realLevel._blinkTimer[_difficulty]), "timeout");
                }
                else if (level != null)
                {
                    await ToSignal(GetTree().CreateTimer(level._pedestrianGreenTimer - level._blinkTimer), "timeout");
                }

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
