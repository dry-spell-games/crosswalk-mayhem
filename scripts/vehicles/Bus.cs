using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// Bus is a specific implementation of the Car class, simulating a larger vehicle
    /// with unique acceleration, braking, and sound behavior.
    /// It also staggers its rendering order with Z-indexing.
    /// Uses mainly methods from abstract class Car.
    /// </summary>
    public partial class Bus : Car
    {
        #region Public Properties

        [Export] public override float _speed { get; set; } = 300.0f;
        [Export] public override float _brakingForce { get; set; } = 700.0f;
        [Export] public override float _accelerationForce { get; set; } = 150.0f;

        #endregion

        #region Private Properties

        [Export] private AudioStreamPlayer2D _sfxPlayer;

        private AnimatedSprite2D _animatedSprite;
        private AnimatedSprite2D _windshield;
        private static int _busCounter = 0;

        private float _initialSpeed;

        #endregion

        #region Godot Built-In Methods

        /// <summary>
        /// Called when the node enters the scene tree. Initializes sprite nodes, sound, and Z-index.
        /// </summary>
        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _windshield = GetNode<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
            base._Ready();

            _initialSpeed = _speed;
            PlayLoopingSfx("res://assets/audio/sfx/vehicles/bus-engine.wav");

            ZAsRelative = false;
            ZIndex = 500 - _busCounter;
            _busCounter++;
        }

        /// <summary>
        /// Called every frame. Adjusts the engine sound's pitch based on the current speed.
        /// </summary>
        /// <param name="delta">Time since the last frame in seconds.</param>
        public override void _Process(double delta)
        {
            base._Process(delta);

            // Scales motor's SFX tempo. Compares current speed to original speed
            float pitch = Mathf.Clamp(_speed / _initialSpeed, 0.5f, 2.0f);
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

        #region Private Methods
        // No private methods currently
        #endregion
    }
}
