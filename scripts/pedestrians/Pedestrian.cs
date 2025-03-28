using Godot;
using System;
using System.Collections.Generic; // Required for arraylist
using System.Linq;

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
        private bool RedLightsForPedestrians = false;


        // Attributes for mobile control
        private const float DoubleTapThreshold = 0.3f; // Max time for double tap
        private float lastTapTime = 0f;
        private bool isWaitingForDoubleTap = false;

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

            // Signal for area_entered
            Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));

            // Gets InteractionArea node and connects signal to it
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
                if (pedestrian.Position.X > 410 || pedestrian.Position.X < -40) // Tarkista sijainti
                {
                    if (!pedestrian.IsQueuedForDeletion()) // Tarkista, onko se jo poistettu
                    {
                        GD.Print("Poistetaan jalankulkija: ", pedestrian.Name);

                        switch (pedestrian.Name)
                        {
                            case "Grandma":
                            case "Grandpa":
                                GameManager.Instance.AddScore(50);
                                break;
                            case "Girl":
                            case "Boy":
                                GameManager.Instance.AddScore(30);
                                break;
                            case "Woman":
                            case "Man":
                                GameManager.Instance.AddScore(20);
                                break;
                        }

                        pedestrian.QueueFree(); // Merkitse poistettavaksi
                    }
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

        public void OnAreaEntered(Area2D area)
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
        private void OnInputEvent(InputEvent @event)
        {
            // Input for mobile devices works on mouseclick from godot project settings
            if (@event is InputEventScreenTouch touch && touch.Pressed)
            {
                HandleTouchInput(touch);
            }
        }

        // Handles mobile input asynchronously
        private async void HandleTouchInput(InputEventScreenTouch touch)
        {
            float currentTime = Time.GetTicksMsec() / 1000f;

            // If double tap happens in 0.3 seconds
            if (isWaitingForDoubleTap && (currentTime - lastTapTime) < DoubleTapThreshold)
            {
                StartSpeedBoost(); // Double tap speeds up
                isWaitingForDoubleTap = false; // Resets
            }
            else
            {
                isWaitingForDoubleTap = true;
                lastTapTime = currentTime;

                // Waits for possible second tap to activate double tap
                await ToSignal(GetTree().CreateTimer(DoubleTapThreshold), "timeout");

                // In case of only one tap in given time, stops pedestrian
                if (isWaitingForDoubleTap)
                {
                    HandleTapOrStop();
                    isWaitingForDoubleTap = false;
                }
            }
        }

        // Stops pedestrian if it's possible
        private async void HandleTapOrStop()
        {
            if (!canBeStopped)
            {
                GD.Print("Pedestrian cannot be stopped yet! Cooldown active.");
                return;
            }

            isStopped = true;
            canBeStopped = false;
            PlayAnimation("idle");
            GD.Print("Pedestrian stopped for " + StopDuration + " seconds.");

            await ToSignal(GetTree().CreateTimer(StopDuration), "timeout");

            isStopped = false;
            PlayAnimation("walk");
            GD.Print("Pedestrian resumed!");

            GD.Print($"Cooldown started for {StopCooldown} seconds.");
            await ToSignal(GetTree().CreateTimer(StopCooldown), "timeout");

            canBeStopped = true;
            GD.Print("Cooldown finished, pedestrian can be stopped again.");
        }

        // Sets IsSpeeding to true while timer is running.
        // IsSpeeding is checked at Move method
        private async void StartSpeedBoost()
        {
            GD.Print("Pedestrian speeding for " + SpeedTimer + " seconds.");
            IsSpeeding = true;
            await ToSignal(GetTree().CreateTimer(SpeedTimer), "timeout");
            IsSpeeding = false;
            GD.Print("Pedestrian back to normal speed.");
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
