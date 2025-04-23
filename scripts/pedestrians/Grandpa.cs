using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// Grandpa is a subclass of abstract class Pedestrian. It uses mostly base methods,
    /// but it also randomly generates flight direction and does random stops.
    /// </summary>
    public partial class Grandpa : Pedestrian
    {
        #region Public Properties

        [Export] public override float Speed { get; set; } = 20.0f;
        [Export] public override float RotationSpeed { get; set; } = 30000;
        [Export] public override float StopDuration { get; set; } = 5.0f;
        [Export] public override float SpeedTimer { get; set; } = 2.0f;
        [Export] public override float FlightDirection { get; set; }

        #endregion

        #region Private Properties

        private Random random = new Random();
        private AnimatedSprite2D animatedSprite;

        #endregion

        #region Godot Built-In Methods

        /// <summary>
        /// Called when the node enters the scene tree.
        /// Initializes flight direction, sprite reference, and starts the random stop behavior.
        /// </summary>
        public override void _Ready()
        {
            base._Ready();
            FlightDirection = GD.RandRange(-300, 300);
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            StartRandomStop();
        }

        /// <summary>
        /// Called every frame. Controls movement or flying behavior depending on state.
        /// </summary>
        /// <param name="delta">Elapsed time since last frame.</param>
        public override void _Process(double delta)
        {
            if (_isFlying)
            {
                base.Fly(delta);
                animatedSprite.Offset = new Vector2(-3, 12);
            }
            else
            {
                base._Process(delta);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Starts and manages grandpa's random stop behavior.
        /// He occasionally stops walking if certain conditions are met.
        /// </summary>
        private async void StartRandomStop()
        {
            while (true)
            {
                // Wait a random amount of time before possibly stopping
                await ToSignal(GetTree().CreateTimer(random.Next(3, 8), false, true), "timeout");

                // Skip stopping if paused or not in a proper state
                if (GetTree().Paused || _isStopped || _isSpeeding || _isHit)
                    continue;

                // Don't stop if too close to the edges
                if (Position.X < 30 || Position.X > 390)
                    continue;

                _isStopped = true;
                _randomStop = true;
                base.PlayAnimation("idle2");

                // Wait before moving again
                await ToSignal(GetTree().CreateTimer(random.Next(2, 5), false, true), "timeout");

                _isStopped = false;
                _randomStop = false;
                _canBeStopped = true;
            }
        }

        /// <summary>
        /// Executes behavior when grandpa is hit by a car.
        /// Enables flying mode and resets flying timer.
        /// </summary>
        /// <param name="car">The car that collided with grandpa.</param>
        protected override void HandleCarCollision(Car car)
        {
            _isFlying = true;
            FlyTime = 0;
        }

        #endregion
    }
}
