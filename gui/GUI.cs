using Godot;
using System;
namespace Crosswalk
{

    public partial class GUI : Control
    {
        public override void _Ready()
        {
            Label scoreLabel = GetNode<Label>("ScoreLabel"); // Hae ScoreLabel
            GameManager.Instance.SetScoreLabel(scoreLabel);  // Aseta GameManageriin
        }
    }
}
