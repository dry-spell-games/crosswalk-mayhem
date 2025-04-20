using Godot;

namespace Crosswalk
{
    /// <summary>
    /// PedestrianLight is a visual representation of the pedestrian traffic light in the level map.
    /// It listens for the pedestrian light signal from the Level and changes animation accordingly.
    /// </summary>
    public partial class PedestrianLight : AnimatedSprite2D
    {
        #region Private Fields

        // Current difficulty level pulled from GameManager
        private int _difficulty = GameManager.Instance._difficulty;

        #endregion

        #region Godot Built-in Methods

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Called when the pedestrian light state changes.
        /// Animates the traffic light accordingly.
        /// </summary>
        /// <param name="_pedestrianGreen">True if pedestrian light is green, false otherwise.</param>
        private async void OnPedestrianLightChanged(bool _pedestrianGreen)
        {
            if (GetTree().Paused)
                return;

            Level level = GetNodeOrNull<Level>("/root/Level");
            _difficulty = GameManager.Instance._difficulty;

            if (_pedestrianGreen)
            {
                Play("green");

                if (level != null)
                {
                    await ToSignal(GetTree().CreateTimer(
                        level._pedestrianGreenTimer[_difficulty] - level._blinkTimer[_difficulty],
                        false,
                        true
                    ), "timeout");
                }

                Play("blink");
            }
            else
            {
                Play("red");
            }
        }

        #endregion
    }
}
