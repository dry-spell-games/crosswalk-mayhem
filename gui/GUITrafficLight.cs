using Godot;

namespace Crosswalk
{
    /// <summary>
    /// GUITrafficLight is a GUI representation of the pedestrian traffic light.
    /// It listens for the pedestrian light signal from the Level node and changes animation accordingly.
    /// </summary>
    public partial class GUITrafficLight : AnimatedSprite2D
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
            Level level = GetNodeOrNull<Level>("/root/Level");

            if (level != null)
            {
                level.PedestrianLight += OnPedestrianLightChanged;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called when the pedestrian light state changes.
        /// Animates the UI traffic light accordingly.
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
