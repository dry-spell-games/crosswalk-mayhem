using Godot;
using System;

namespace Crosswalk {
    public partial class Bus : Car
    {
        [Export] public override float Speed { get; set; } = 300.0f;
        [Export] public override float BrakingForce { get; set; } = 700.0f;
        [Export] public override float AccelerationForce { get; set; } = 150.0f;
        [Export] private AudioStreamPlayer2D _sfxPlayer;

        private AnimatedSprite2D animatedSprite;
        private AnimatedSprite2D windShield;
        private static int BusCounter = 0;

        private float _initialSpeed;

        public void PlayLoopingSfx(string pathToSfx)
        {
            _sfxPlayer.Stream = GD.Load<AudioStream>(pathToSfx);
            _sfxPlayer.PitchScale = 1.0f;
            _sfxPlayer.Play();
        }

        public override void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            windShield = GetNode<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
            base._Ready();

            _initialSpeed = Speed; // Saves car's original speed
            PlayLoopingSfx("res://assets/audio/sfx/vehicles/bus-engine.wav");

            ZAsRelative = false;
            ZIndex = 500 - BusCounter;
            BusCounter++;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // Scales motor's SFX tempo. Compares current speed to original speed
            // Last 2 values are min and max tempo
            float pitch = Mathf.Clamp(Speed / _initialSpeed, 0.5f, 2.0f);
            _sfxPlayer.PitchScale = pitch;
        }
    }
}
