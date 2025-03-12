using Godot;
using System;
using System.Collections.Generic;
using System.Linq; // Required for arraylist

namespace Crosswalk
{
    public abstract partial class Pedestrian : Area2D
    {
        [Export] public virtual float Speed { get; set; }
        [Export] public virtual float RotationSpeed { get; set; }
        // Flight time, default value
        [Export] public virtual float FlyTime { get; set; }
        // How long pedestrian stays stopped, default value
        [Export] public virtual float StopDuration { get; set; } = 2.0f;
        // Cooldown for stop action, default value
        [Export] public virtual float StopCooldown { get; set; } = 3.0f;
        [Export] public virtual float SpeedTimer { get; set; } // Timer for faster speed
        [Export] public float SpeedMultiplierFast { get; set; } = 3.0f; // Multiplier for sprinting
        [Export] public float SpeedMultiplierNormal { get; set; } = 1.0f;
        [Export] public virtual float FlightDirection { get; set; } // Direction of flight

        // ArrayList can not be exported
        public List<Vector2> StartPositions { get; set; } = new List<Vector2>
        {
            new Vector2(-30, 480),
            new Vector2(-30, 520),
            new Vector2(-30, 560),
            new Vector2(400, 500),
            new Vector2(400, 540),
            new Vector2(400, 580)
        };

        protected bool isFlying = false;
        protected bool isStopped = false; // if true pedestrian is moving
        protected bool canBeStopped = true; // False if pedestrian was stopped recently
        private AnimatedSprite2D animatedSprite; // Refers to AnimatedSprite2D component
        protected float InitialSpeed; // Saves original speed
        protected bool IsSpeeding = false;

        public override void _Ready()
        {
            InitialSpeed = Speed; // Saves original speed
            AddToGroup("pedestrians"); // Adds pedestrians to group so they can be removed later

            // Signals for area_entered and input_event
            Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));

            // Gets InteractionArea node and cinnects signal to it
            var interactionArea = GetNode<Area2D>("InteractionArea");
            if (interactionArea != null)
            {
                interactionArea.Connect("input_event", Callable.From((Node viewport,
                InputEvent @event, int shapeIdx) => OnInputEvent(@event)));
            }
            // Gets AnimatedSprite2d for pedestrians
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        public void Initialize(Vector2 position)
        {
            Position = position;

            // Sets ZIndex for rendering Pedestrians, higher value of Y is rendered on top
            ZIndex = Mathf.RoundToInt(GlobalPosition.Y);

            // If SpawnPoints X coordinate is over 399, Speed is converted to negative
            if (Position.X > 399)
            {
                Speed = -MathF.Abs(Speed); // Converts to negative speed
            }
            else
            {
                Speed = MathF.Abs(Speed); // Else speed is positive
            }

            GD.Print($"{this} spawned at {Position} with speed {Speed}");
        }

        public override void _Process(double delta)
        {

            if (isStopped)
            {
                PlayAnimation("idle");
            }
            else if (!isFlying) // If pedestrian is not flying, move pedestrian
            {
                Move(delta, IsSpeeding);
            }

            // Goes through every Pedestrian class' child and removes those which are out of bounds
            foreach (Area2D pedestrian in GetTree().GetNodesInGroup("pedestrians").Cast<Area2D>())
            {
                if (pedestrian.Position.X > 410 || pedestrian.Position.X < -50) // Tarkista sijainti
                {
                    GD.Print("Poistetaan jalankulkija: ", pedestrian.Name);
                    pedestrian.QueueFree(); // Poista jalankulkija
                }
            }
        }


        protected virtual void Move(double delta, bool IsSpeeding)
        {
            // Flip the animation based on direction
            animatedSprite.FlipH = Speed < 0; // Flips the character if moving left (negative speed)

            // Adjust movement speed based on whether speeding is enabled
            float SpeedMultiplier = IsSpeeding ? SpeedMultiplierFast : SpeedMultiplierNormal;
            Position += new Vector2(Speed * SpeedMultiplier * (float)delta, 0); // Apply movement

            // Play the appropriate animation based on movement speed
            if (!IsSpeeding)
            {
                PlayAnimation("walk");
            }
            else
            {
                PlayAnimation("run");
            }
        }

        private void OnAreaEntered(Area2D area)
        {
            if (area is Car)
            {
                GD.Print("Pedestrian detected a Car!");
                HandleCarCollision(area as Car);
            }
        }

        protected void Fly(double delta)
        {
            FlyTime += (float)delta;
            Position += new Vector2(FlightDirection, -250) * (float)delta;
            RotationDegrees += RotationSpeed * (float)delta;
        }

        protected virtual void HandleCarCollision(Car car)
        {
            GD.Print("Handling collision with a Car...");
        }

        /// <summary>
        /// Handles pedestrian click events, including a cooldown mechanism.
        /// This function is triggered when the user clicks on the pedestrian with the mouse.
        /// It stops the character for a specified duration and prevents reactivation during cooldown.
        /// Uses an async method to handle timing asynchronously in the background.
        /// </summary>
        /// <param name="viewport">The viewport where the event occurred (not used in this method,
        /// but required by Godot for the method signature).</param>
        /// <param name="event">The event containing information about the user's input.
        /// @event is used because "event" is a reserved keyword in C#.</param>
        /// <param name="shapeIdx">The index of the clicked CollisionShape2D element
        /// (useful if the pedestrian has multiple hitboxes).</param>
        private async void OnInputEvent(InputEvent @event)
        {
            // Check if the event is a mouse button press
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                // Left mouse button click: Attempt to stop the pedestrian
                if (mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    // If the pedestrian cannot be stopped due to cooldown, exit early
                    if (!canBeStopped)
                    {
                        GD.Print("Pedestrian cannot be stopped yet! Cooldown active.");
                        return;
                    }

                    // Stop the pedestrian and play idle animation
                    isStopped = true;
                    canBeStopped = false;
                    PlayAnimation("idle");
                    GD.Print("Pedestrian stopped for " + StopDuration + " seconds.");

                    // Wait asynchronously for the stop duration
                    await ToSignal(GetTree().CreateTimer(StopDuration), "timeout");

                    // Resume walking after the stop duration ends
                    isStopped = false;
                    PlayAnimation("walk");
                    GD.Print("Pedestrian resumed!");

                    // Start cooldown period
                    GD.Print($"Cooldown started for {StopCooldown} seconds.");
                    await ToSignal(GetTree().CreateTimer(StopCooldown), "timeout");

                    // Cooldown finished, pedestrian can be stopped again
                    canBeStopped = true;
                    GD.Print("Cooldown finished, pedestrian can be stopped again.");
                }

                // Right mouse button click: Speed up the pedestrian temporarily
                else if (mouseEvent.ButtonIndex == MouseButton.Right)
                {
                    GD.Print("Pedestrian speeding for " + SpeedTimer + " seconds.");
                    IsSpeeding = true;
                    PlayAnimation("run");

                    // Wait asynchronously for the speed duration
                    await ToSignal(GetTree().CreateTimer(SpeedTimer), "timeout");

                    // Return pedestrian to normal walking speed
                    IsSpeeding = false;
                    GD.Print("Pedestrian back to normal speed.");
                    PlayAnimation("walk");
                }
            }
        }

        // Method gets animation name string as parameter and plays given animation
        protected void PlayAnimation(string animationName)
        {
            if (animatedSprite != null && animatedSprite.Animation != animationName)
            {
                GD.Print($"Starting animation: {animationName}");
                animatedSprite.Play(animationName);
            }
        }
    }
}
