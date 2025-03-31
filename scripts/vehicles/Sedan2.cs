using Godot;
using System;

namespace Crosswalk {
    public partial class Sedan2 : Car
    {
        [Export] public override float Speed { get; set; } = 390.0f;
        [Export] public override float BrakingForce {get; set; } = 1200.0f;
        [Export] public override float AccelerationForce { get; set; } = 270.0f;
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
