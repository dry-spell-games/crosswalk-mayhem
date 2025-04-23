using Godot;
using System;
using System.ComponentModel;

namespace Crosswalk
{
    /// <summary>
    /// Sportscar is a fast and highly responsive implementation of the Car class.
    /// Special feature is that it doesn't stop to red lights unlike other cars.
    /// Uses mainly methods from abstract class Car.
    /// It features high acceleration and braking values and a distinct engine sound.
    /// </summary>
    public partial class Sportscar : Car
    {
        #region Public Properties

        [Export] public override float Speed { get; set; } = 600.0f;
        [Export] public override float BrakingForce { get; set; } = 2000.0f;
        [Export] public override float AccelerationForce { get; set; } = 500.0f;

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

            _initialSpeed = Speed;
            PlayLoopingSfx("res://assets/audio/sfx/vehicles/sports-engine.wav");
        }

        /// <summary>
        /// Called every frame. Updates the pitch of the engine sound according to speed.
        /// </summary>
        /// <param name="delta">Time since the last frame in seconds.</param>
        public override void _Process(double delta)
        {
            base._Process(delta);

            // Sportscar uses a more sensitive pitch scaling factor
            float pitch = Mathf.Clamp((Speed / _initialSpeed) * 3, 1.0f, 5.0f);
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
