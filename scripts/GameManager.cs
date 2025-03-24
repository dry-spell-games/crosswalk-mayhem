using Godot;
using System;

namespace Crosswalk{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        public int Score { get; private set; }

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                GD.Print("GameManager initialized");
            }
            else
            {
                QueueFree(); // Poistetaan ylimääräinen instanssi
            }
        }

        public void AddScore(int amount)
        {
            Score += amount;
            GD.Print("Score: " + Score);
        }

        public void ResetScore()
        {
            Score = 0;
        }
    }
}
