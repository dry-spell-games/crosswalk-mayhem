using Godot;
using System;

namespace Crosswalk
{
    public partial class Grandma : Pedestrian
    {
        [Export] public override float Speed { get; set; } = 30.0f;
        [Export] public override float RotationSpeed { get; set; } = 30000;
        [Export] public override float StopDuration { get; set; } = 3.0f;
        [Export] public override float StopCooldown { get; set; } = 10.0f;
        [Export] public override float SpeedTimer { get; set; } = 2.0f;
        [Export] public override float FlightDirection { get; set; }


        private AnimatedSprite2D animatedSprite;

        public override void _Ready()
        {
            base._Ready();
            FlightDirection = GD.RandRange(-300, 300);

            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        public override void _Process(double delta)
        {
            if (isFlying)
            {
                base.Fly(delta);
            }
            else
            {
                base._Process(delta);
            }
        }

        protected override void HandleCarCollision(Car car)
        {
            GD.Print("Grandma was hit by a car!");
            isFlying = true;
            FlyTime = 0;
        }
    }
}
