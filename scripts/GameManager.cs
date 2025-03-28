using Godot;
using System;

namespace Crosswalk{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        public int Score { get; private set; }
        private Label scoreLabel; // Reference to score label

        public int _difLvl = 3; // Game difficulty level

        public int DifLvl
        {
            get { return _difLvl; }
            set { _difLvl = value; }
        }

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
        public void SetScoreLabel(Label label)
        {
            scoreLabel = label;
            UpdateScoreLabel(); // Päivitä heti alussa
        }

        public void AddScore(int amount)
        {
            Score += amount;
            GD.Print("Score: " + Score);
            UpdateScoreLabel();
        }

        private void UpdateScoreLabel()
        {
            if (scoreLabel != null)
            {
                scoreLabel.Text = $"Score: {Score}";
            }
        }

        public void ResetScore()
        {
            Score = 0;
            UpdateScoreLabel();
        }
    }
}
