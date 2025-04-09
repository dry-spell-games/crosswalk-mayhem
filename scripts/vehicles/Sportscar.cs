using Godot;
using System;
using System.ComponentModel;

namespace Crosswalk {
    public partial class Sportscar : Car
    {
        [Export] public override float Speed { get; set; } = 600.0f;
        [Export] public override float BrakingForce { get; set; } = 2000.0f;
        [Export] public override float AccelerationForce { get; set;} = 500.0f;
        [Export] private AudioStreamPlayer2D _sfxPlayer;
        private AnimatedSprite2D animatedSprite;
        private AnimatedSprite2D windShield;
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
            PlayLoopingSfx("res://assets/audio/sfx/vehicles/sports-engine.wav");
        }

        public override void _Process(double delta)
        {
            base._Process(delta); // Calls base _Process at Car class

            // Scales motor's SFX tempo. Compares current speed to original speed
            // Last 2 values are min and max tempo
            // Sportscar has individual multiplier before min, max values to react more to speed change
            float pitch = Mathf.Clamp((Speed / _initialSpeed) * 3, 1.0f, 5.0f);
            _sfxPlayer.PitchScale = pitch;
        }
    }
}
