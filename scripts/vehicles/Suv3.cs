using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// Suv is a heavier vehicle with moderate speed and torque.
    /// It inherits movement and animation behavior from the Car class
    /// and includes its own sound effect scaling. Different SUV's have different
    /// attribute values.
    /// Uses mainly methods from abstract class Car.
    /// </summary>
    public partial class Suv3 : Car
    {
        #region Public Properties

        [Export] public override float _speed { get; set; } = 380.0f;
        [Export] public override float _brakingForce { get; set; } = 1100.0f;
        [Export] public override float _accelerationForce { get; set; } = 350.0f;

        #endregion

        #region Private Properties

        [Export] private AudioStreamPlayer2D _sfxPlayer;

        private AnimatedSprite2D _animatedSprite;
        private AnimatedSprite2D _windshield;
        private float _initialSpeed;

        #endregion

        #region Godot Built-In Methods

        /// <summary>
        /// Called when the node enters the scene tree. Initializes sprites and plays engine sound.
        /// </summary>
        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _windshield = GetNode<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
            base._Ready();

            _initialSpeed = _speed;
            PlayLoopingSfx("res://assets/audio/sfx/vehicles/suv-engine.wav");
        }

        /// <summary>
        /// Called every frame. Updates the pitch of the engine sound according to speed.
        /// </summary>
        /// <param name="delta">Time since the last frame in seconds.</param>
        public override void _Process(double delta)
        {
            base._Process(delta);

            float pitch = Mathf.Clamp(_speed / _initialSpeed, 0.4f, 0.8f);
            _sfxPlayer.PitchScale = pitch;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and plays a looping engine sound effect from a given file path.
        /// </summary>
        /// <param name="pathToSfx">Path to the engine SFX resource.</param>
        public void PlayLoopingSfx(string pathToSfx)
        {
            _sfxPlayer.Stream = GD.Load<AudioStream>(pathToSfx);
            _sfxPlayer.PitchScale = 1.0f;
            _sfxPlayer.Play();
        }

        #endregion

    }
}
