using Godot;
using System;

namespace Crosswalk {
    public partial class Bus : Car
    {
        [Export] public override float Speed { get; set; } = 300.0f;
        [Export] public override float BrakingForce {get; set; } = 700.0f;
        [Export] public override float AccelerationForce { get; set; } = 150.0f;
        private AnimatedSprite2D animatedSprite;
        private AnimatedSprite2D windShield;
        private static int BusCounter = 0;


        public override void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            windShield = GetNode<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
            base._Ready();

            ZAsRelative = false;
            ZIndex = 500 - BusCounter;
            BusCounter++;
        }


        public override void _Process(double delta)
        {
            base._Process(delta); // Kutsutaan Car luokan processia
        }
    }
}
