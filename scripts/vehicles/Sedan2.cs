using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// Sedan is a specific implementation of the Car class, simulating a faster passenger vehicle
    /// with unique acceleration, braking, and engine sound behavior.
    /// Different Sedans have different attribute values
    /// </summary>
    public partial class Sedan2 : Car
    {
        #region Public Properties

        [Export] public override float Speed { get; set; } = 390.0f;
        [Export] public override float BrakingForce { get; set; } = 1200.0f;
        [Export] public override float AccelerationForce { get; set; } = 270.0f;

        #endregion

        #region Private Properties

        [Export] private AudioStreamPlayer2D _sfxPlayer;

        private AnimatedSprite2D animatedSprite;
        private AnimatedSprite2D windShield;
        private float _initialSpeed;

        #endregion

        #region Godot Built-In Methods

        /// <summary>
        /// Called when the node enters the scene tree. Initializes sprite nodes and engine sound.
        /// </summary>
        public override void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            windShield = GetNode<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
            base._Ready();

            _initialSpeed = Speed;
            PlayLoopingSfx("res://assets/audio/sfx/vehicles/sedan-engine.wav");
        }

        /// <summary>
        /// Called every frame. Adjusts engine sound pitch based on current speed.
        /// </summary>
        /// <param name="delta">Time since the last frame in seconds.</param>
        public override void _Process(double delta)
        {
            base._Process(delta);

            // Scales motor's SFX tempo based on current speed
            float pitch = Mathf.Clamp(Speed / _initialSpeed, 0.4f, 10.0f);
            _sfxPlayer.PitchScale = pitch;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and plays a looping engine SFX from the given path.
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
