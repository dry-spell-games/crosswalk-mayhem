using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// MainMenu handles the game's main menu UI, including language selection,
    /// scene transition, and animated intro sign movement.
    /// </summary>
    public partial class MainMenu : Control
    {
        // UI Elements and animation control
        [Export] private TextureRect _signs = null; // Sign graphic to be animated into view
        [Export] private int _signsTargetYPos = 352; // Final Y position of the sign
        private float _signsMoveSpeed = 750f; // Speed at which the sign moves
        private bool _signsMoved = false; // Whether the sign has finished moving
        [Export] private AnimatedSprite2D _english = null; // English language flag animation
        [Export] private AnimatedSprite2D _finnish = null; // Finnish language flag animation

        /// <summary>
        /// Called when the English button is pressed.
        /// Sets the game language to English and plays the flag animation.
        /// </summary>
        public void _on_english_pressed()
        {
            GameManager.Instance._language = "english";
            _english.Play("wave");
            _finnish.Play("still");
        }

        /// <summary>
        /// Called when the Finnish button is pressed.
        /// Sets the game language to Finnish and plays the flag animation.
        /// </summary>
        public void _on_finnish_pressed()
        {
            GameManager.Instance._language = "finnish";
            _english.Play("still");
            _finnish.Play("wave");
        }

        /// <summary>
        /// Called when the Play button is pressed.
        /// Loads the main level scene.
        /// </summary>
        public void _on_play_pressed()
        {
            GetTree().ChangeSceneToFile("res://scenes/levels/level.tscn");
        }

        /// <summary>
        /// Called when the Quit button is pressed.
        /// Exits the game.
        /// </summary>
        public void _on_quit_pressed()
        {
            GetTree().Quit();
        }

        /// <summary>
        /// Smoothly moves the sign upward toward its target position.
        /// Snaps to the final position once reached.
        /// </summary>
        private void MoveSigns(double delta)
        {
            if (_signs.Position.Y > _signsTargetYPos)
            {
                _signs.Position += Vector2.Up * (float)(_signsMoveSpeed * delta);

                if (_signs.Position.Y <= _signsTargetYPos)
                {
                    _signs.Position = new Vector2(_signs.Position.X, _signsTargetYPos);
                    _signsMoved = true;
                }
            }
            else
            {
                _signsMoved = true;
            }
        }

        /// <summary>
        /// Called when the scene is ready.
        /// Initializes the flag animation based on the saved language setting.
        /// </summary>
        public override void _Ready()
        {
            if (GameManager.Instance._language == "english")
            {
                _english.Play("wave");
            }
            else
            {
                _finnish.Play("wave");
            }
        }

        /// <summary>
        /// Called every frame.
        /// Handles sign movement if it hasn’t yet reached its target position.
        /// </summary>
        public override void _Process(Double delta)
        {
            if (_signs != null && !_signsMoved)
            {
                MoveSigns(delta);
            }
        }
    }
}
