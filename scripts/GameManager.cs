using Godot;
using System;

namespace Crosswalk{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        public int _score { get; private set; }
        public int _highScore { get; private set; }
        public int _life { get; private set; }
        public int _musicVolume { get; private set; }
        public int _effectVolume { get; private set; }
        public String _language { get; set; } = "english";
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
            _score += amount;
            GD.Print("Score: " + _score);
            UpdateScoreLabel();
        }

        private void UpdateScoreLabel()
        {
            if (scoreLabel != null)
            {
                scoreLabel.Text = $"Score: {_score}";
            }
        }

        public void ResetScore()
        {
            _score = 0;
            UpdateScoreLabel();
        }
    }
}
