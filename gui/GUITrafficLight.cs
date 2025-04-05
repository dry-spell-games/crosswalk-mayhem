using Godot;
using System;

namespace Crosswalk
{
public partial class GUITrafficLight : AnimatedSprite2D
	{
		private int _difficulty = GameManager.Instance._difLvl;

		public override void _Ready()
        {
            Level level = GetNodeOrNull<Level>("/root/Level");

            if (level != null)
            {
                level.PedestrianLight += OnPedestrianLightChanged;
            }
        }

        private async void OnPedestrianLightChanged(bool _pedestrianGreen)
        {
            Level level = GetNodeOrNull<Level>("root/Level");

            if (_pedestrianGreen)
            {
                Play("green");

                if (level != null)
                {
                    await ToSignal(GetTree().CreateTimer(level._pedestrianGreenTimer[_difficulty] - level._blinkTimer[_difficulty]), "timeout");
                }

                Play("blink");
            }
            else
            {
                Play("red");
            }
        }
	}
}

