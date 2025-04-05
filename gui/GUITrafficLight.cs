using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// GUITrafficLight is a UI representation of the pedestrian traffic light.
    /// It listens for the pedestrian light signal from the Level and changes animation accordingly.
    /// </summary>
    public partial class GUITrafficLight : AnimatedSprite2D
    {
        // Current difficulty level pulled from GameManager
        private int _difficulty = GameManager.Instance._difficulty;

        /// <summary>
        /// Called when the node is added to the scene tree.
        /// Connects to the PedestrianLight signal from the Level node.
        /// </summary>
        public override void _Ready()
        {
            // Attempt to get the Level node from the scene tree
            Level level = GetNodeOrNull<Level>("/root/Level");

            // Connect to the signal if Level is found
            if (level != null)
            {
                level.PedestrianLight += OnPedestrianLightChanged;
            }
        }

        /// <summary>
        /// Called when the pedestrian light state changes.
        /// Animates the UI traffic light accordingly.
        /// </summary>
        /// <param name="_pedestrianGreen">True if pedestrian light is green, false otherwise.</param>
        private async void OnPedestrianLightChanged(bool _pedestrianGreen)
        {
            // Double-check we can access Level in case the scene changed
            Level level = GetNodeOrNull<Level>("/root/Level");

            // Refresh difficulty every time the light changes
            _difficulty = GameManager.Instance._difficulty;

            if (_pedestrianGreen)
            {
                // Start with green light
                Play("green");

                // Wait before starting blink animation (based on timing arrays)
                if (level != null)
                {
                    await ToSignal(GetTree().CreateTimer(
                        level._pedestrianGreenTimer[_difficulty] - level._blinkTimer[_difficulty]),
                        "timeout");
                }

                // Switch to blinking green animation
                Play("blink");
            }
            else
            {
                // Show red light
                Play("red");
            }
        }
    }
}
