using Godot;
using System;

namespace Crosswalk {
    public partial class Suv1 : Car
    {
        [Export] public override float Speed { get; set; } = 340.0f;
        [Export] public override float BrakingForce {get; set; } = 1050.0f;
        [Export] public override float AccelerationForce { get; set; } = 250.0f;
        private AnimatedSprite2D animatedSprite;
        private AnimatedSprite2D windShield;


        public override void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            windShield = GetNode<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
            base._Ready();
        }

        public override void _Process(double delta)
        {
            base._Process(delta); // Kutsutaan Car luokan processia
        }
    }
}
