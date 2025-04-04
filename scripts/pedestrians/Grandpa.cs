using Godot;
using System;

namespace Crosswalk
{
    public partial class Grandpa : Pedestrian
    {
        [Export] public override float Speed { get; set; } = 20.0f;
        [Export] public override float RotationSpeed { get; set; } = 30000;
        [Export] public override float StopDuration { get; set; } = 5.0f;
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
            while (!isStopped)
            {
                await ToSignal(GetTree().CreateTimer(random.Next(3, 8)), "timeout"); // Odottaa 3-8 sekuntia ennen pysähtymistä
                randomStop = true;
                isStopped = true;
                GD.Print("Grandpa randomly stopped!");
                base.PlayAnimation("idle2");

                await ToSignal(GetTree().CreateTimer(random.Next(4, 6)), "timeout"); // Odottaa 4-6 sekuntia pysähdyksissä
                isStopped = false;
                randomStop = false;
                GD.Print("Grandpa started moving again!");
            }
        }

        protected override void HandleCarCollision(Car car)
        {
            GD.Print("Grandpa was hit by a car!");
            isFlying = true;
            FlyTime = 0;
        }
    }
}
