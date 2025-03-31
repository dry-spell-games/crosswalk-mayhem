using Godot;
using System;
using System.ComponentModel;

namespace Crosswalk {
    public partial class Sportscar : Car
    {
        [Export] public override float Speed { get; set; } = 600.0f;
        [Export] public override float BrakingForce { get; set; } = 2000.0f;
        [Export] public override float AccelerationForce { get; set;} = 500.0f;
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
