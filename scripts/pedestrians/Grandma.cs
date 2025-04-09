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
        private Random random = new Random();


        private AnimatedSprite2D animatedSprite;

        public override void _Ready()
        {
            base._Ready();
            FlightDirection = GD.RandRange(-300, 300);

            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            StartRandomStop();
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
        private async void StartRandomStop()
        {
            while (true)
            {
                // Wait before maybe stopping
                await ToSignal(GetTree().CreateTimer(random.Next(3, 8), false, true), "timeout");

                // Check again BEFORE stopping
                if (GetTree().Paused || isStopped || IsSpeeding || _isHit)
                    continue;

                // Don't stop if near the edges
                if (Position.X < 30 || Position.X > 390)
                    continue;

                isStopped = true;
                randomStop = true;
                GD.Print("Grandma stopped!");
                base.PlayAnimation("idle2");

                await ToSignal(GetTree().CreateTimer(random.Next(2, 5), false, true), "timeout");

                isStopped = false;
                randomStop = false;
                GD.Print("Grandma started moving again!");
            }
        }

        protected override void HandleCarCollision(Car car)
        {
            isFlying = true;
            GD.Print("Grandma was hit by a car!");
            FlyTime = 0;
        }
    }
}
