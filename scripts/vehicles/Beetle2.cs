using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// Beetle is a specific implementation of Car with unique sound effects and properties.
    /// Uses mainly methods from abstract class Car.
    /// Different Beetles have different values for speed, braking force and acceleration force.
    /// </summary>
    public partial class Beetle2 : Car
    {
        #region Public Properties

        [Export] public override float _speed { get; set; } = 320.0f;
        [Export] public override float _brakingForce { get; set; } = 970.0f;
        [Export] public override float _accelerationForce { get; set; } = 240.0f;

        #endregion

        #region Private Properties

        [Export] private AudioStreamPlayer2D _sfxPlayer;
        private AnimatedSprite2D _animatedSprite;
        private AnimatedSprite2D _windshield;
        private float _initialSpeed;

        #endregion

        #region Godot Built-In Methods

        /// <summary>
        /// Called when the node is added to the scene tree. Initializes sprites, sound, and calls base logic.
        /// </summary>
        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _windshield = GetNode<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
            base._Ready();

            _initialSpeed = _speed; // Saves car's original speed
            PlayLoopingSfx("res://assets/audio/sfx/vehicles/beetle-engine.wav");
        }

        /// <summary>
        /// Called every frame. Updates pitch of engine SFX based on speed and calls base logic.
        /// </summary>
        public override void _Process(double delta)
        {
            base._Process(delta); // Calls base _Process at Car class

            // Scales motor's SFX tempo. Compares current speed to original speed
            // Last 2 values are min and max tempo
            float pitch = Mathf.Clamp(_speed / _initialSpeed, 0.4f, 10.0f);
            _sfxPlayer.PitchScale = pitch;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and plays a looping SFX from the given path.
        /// </summary>
        /// <param name="pathToSfx">Path to the sound effect resource.</param>
        public void PlayLoopingSfx(string pathToSfx)
        {
            _sfxPlayer.Stream = GD.Load<AudioStream>(pathToSfx);
            _sfxPlayer.PitchScale = 1.0f;
            _sfxPlayer.Play();
        }

        #endregion
    }
}
