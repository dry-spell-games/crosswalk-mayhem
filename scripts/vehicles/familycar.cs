using Godot;
using System;

namespace Crosswalk {
    public partial class FamilyCar : Car
    {
        [Export] public override float Speed { get; set; } = 300.0f;
        [Export] public override float BrakingForce {get; set; } = 1000.0f;
        [Export] public override float AccelerationForce { get; set; } = 200.0f;
        private AnimatedSprite2D animatedSprite;


        public override void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            base._Ready();
        }

        public override void _Process(double delta)
        {
            base._Process(delta); // Kutsutaan Car luokan processia
        }
    }
}
