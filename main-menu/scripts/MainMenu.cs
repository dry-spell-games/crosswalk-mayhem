using Godot;

namespace Crosswalk
{
    /// <summary>
    /// MainMenu handles the game's main menu UI, including language selection,
    /// scene transition, and animated sign movement.
    /// </summary>
    public partial class MainMenu : Control
    {
        #region UI Elements and Animation Control

        // Main menu signs (e.g. Play/Quit) that animate into view
        [Export] private TextureRect _signs = null;
        // Target Y position for the signs when fully visible
        [Export] private int _signsTargetYPos = 352;
        // Starting Y position for the signs (off-screen position)
        [Export] private int _signsStartYPos = 640;
        // Speed at which the signs move up/down
        private float _signsMoveSpeed = 750f;
        // Whether the signs are currently in the "up" (visible) position
        private bool _signsUp = true;

        // Settings panel that slides into view
        [Export] private TextureRect _settings = null;
        // Target Y position for the settings panel
        [Export] private int _settingsTargetYPos = 320;
        // Starting Y position for the settings panel (off-screen)
        [Export] private int _settingsStartYPos = 640;
        // Whether the settings panel is currently visible
        private bool _settingsUp = false;

        // English language flag with animation
        [Export] private AnimatedSprite2D _english = null;
        // Finnish language flag with animation
        [Export] private AnimatedSprite2D _finnish = null;

        #endregion

        #region Audio Controls and Players

        // Slider control for master volume
        [Export] private AudioControl _masterAudioControl = null;
        // Slider control for music volume
        [Export] private AudioControl _musicAudioControl = null;
        // Slider control for sound effects volume
        [Export] private AudioControl _sfxAudioControl = null;

        // Audio player node for background music
        [Export] private AudioStreamPlayer2D _musicPlayer;
        // Audio player node for UI sound effects
        [Export] private AudioStreamPlayer2D _sfxPlayer;

        // Delay before background music starts playing
        [Export] private float _musicDelayTimer = 1.0f;
        // Delay before input process after sound effects play
        [Export] private float _sfxDelayTimer = 0.1f;

        #endregion

        #region Tutorial and Settings

        // Panel displaying tutorial instructions
        [Export] private TextureRect _tutorialRect1;
        // Panel displaying info about the game
        [Export] private TextureRect _tutorialRect2;
        // Flag indicating whether to reset highscore on OK
        private bool _resetHighscore = false;

        #endregion

        #region Public Methods (UI Signal Handlers)

        /// <summary>
        /// Called when the English button is pressed.
        /// Sets the game language to English and plays the flag animation.
        /// </summary>
        public void _on_english_pressed()
        {
            GameManager.Instance.SetLanguage("en");
            _english.Play("wave");
            _finnish.Play("still");
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
        }

        /// <summary>
        /// Called when the Finnish button is pressed.
        /// Sets the game language to Finnish and plays the flag animation.
        /// </summary>
        public void _on_finnish_pressed()
        {
            GameManager.Instance.SetLanguage("fi");
            _english.Play("still");
            _finnish.Play("wave");
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
        }

        /// <summary>
        /// Called when the Play button is pressed.
        /// Loads the main level scene.
        /// </summary>
        public async void _on_play_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GameManager.Instance.SaveData();
            GetTree().ChangeSceneToFile("res://scenes/levels/level.tscn");
        }

        /// <summary>
        /// Called when the Settings button is pressed.
        /// Hides the main menu and shows the settings panel.
        /// </summary>
        public void _on_settings_pressed()
        {
            _signsUp = false;
            _settingsUp = true;
            PlaySfx("res://assets/audio/sfx/menu/slide-up-long.wav");
        }

        /// <summary>
        /// Called when the Quit button is pressed.
        /// Exits the game.
        /// </summary>
        public async void _on_quit_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GameManager.Instance.SaveData();
            GetTree().Quit();
        }

        /// <summary>
        /// Called when the Tutorial button is pressed.
        /// Displays the tutorial instructions panel.
        /// </summary>
        public void _on_tutorial_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            _tutorialRect2.Visible = true;
            _tutorialRect1.Visible = true;
        }

        /// <summary>
        /// Called when the Exit Tutorial button 1 is pressed.
        /// Hides the tutorial instructions panel.
        /// </summary>
        public void _on_tutorial1_exit_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            _tutorialRect1.Visible = false;
        }

        /// <summary>
        /// Called when the Exit Tutorial button 2 is pressed.
        /// Hides the about the game panel.
        /// </summary>
        public void _on_tutorial2_exit_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            _tutorialRect2.Visible = false;
        }

        /// <summary>
        /// Called when the Reset Highscore toggle is changed.
        /// Updates the internal flag and plays a sound effect.
        /// </summary>
        public void _on_reset_highscore_toggled(bool toggledOn)
        {
            _resetHighscore = toggledOn;
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
        }

        /// <summary>
        /// Called when the OK button is pressed in the settings menu.
        /// Applies changes (resets highscore if toggled) and hides the settings panel.
        /// </summary>
        public void _on_ok_pressed()
        {
            if (_resetHighscore)
            {
                GameManager.Instance.SetHighscore(0);
            }
            _resetHighscore = false;

            GameManager.Instance.SaveData();

            GetNode<Button>("Settings/ResetHighscore").ButtonPressed = false;
            _signsUp = true;
            _settingsUp = false;

            PlaySfx("res://assets/audio/sfx/menu/slide-up-long.wav");
        }

        /// <summary>
        /// Called when the Cancel button is pressed in the settings menu.
        /// Reverts audio settings and resets the highscore toggle.
        /// </summary>
        public void _on_cancel_pressed()
        {
            _masterAudioControl.SetVolumeSlider(GameManager.Instance._savedMasterVolume);
            _musicAudioControl.SetVolumeSlider(GameManager.Instance._savedMusicVolume);
            _sfxAudioControl.SetVolumeSlider(GameManager.Instance._savedSfxVolume);

            _masterAudioControl.ApplyCurrentSliderValue();
            _musicAudioControl.ApplyCurrentSliderValue();
            _sfxAudioControl.ApplyCurrentSliderValue();

            _resetHighscore = false;

            GetNode<Button>("Settings/ResetHighscore").ButtonPressed = false;
            _signsUp = true;
            _settingsUp = false;

            PlaySfx("res://assets/audio/sfx/menu/slide-up-long.wav");
        }

        #endregion

        #region Godot Built-in Methods

        /// <summary>
        /// Called when the scene is ready.
        /// Initializes the flag animation based on the saved language setting.
        /// </summary>
        public override void _Ready()
        {
            GameManager.Instance.LoadData();
            TranslationServer.SetLocale(GameManager.Instance._langCode);

            if (GameManager.Instance._langCode == "en")
            {
                _english.Play("wave");
            }
            else
            {
                _finnish.Play("wave");
            }

            if (_masterAudioControl != null)
            {
                _masterAudioControl.Initialize(GameManager.Instance._masterVolume);
            }
            if (_musicAudioControl != null)
            {
                _musicAudioControl.Initialize(GameManager.Instance._musicVolume);
            }
            if (_sfxAudioControl != null)
            {
                _sfxAudioControl.Initialize(GameManager.Instance._sfxVolume);
            }

            PlaySfx("res://assets/audio/sfx/menu/slide-up.wav");
            PlayMusic();
        }

        /// <summary>
        /// Called every frame.
        /// Handles sign movement if it hasn’t yet reached its target position.
        /// </summary>
        public override void _Process(double delta)
        {
            if (_signs != null && _settings != null && _signsUp && !_settingsUp)
            {
                MoveDown(delta, _settingsStartYPos, _settings);
                MoveUp(delta, _signsTargetYPos, _signs);
            }
            else if (_signs != null && _settings != null && !_signsUp && _settingsUp)
            {
                MoveDown(delta, _signsStartYPos, _signs);
                MoveUp(delta, _settingsTargetYPos, _settings);
            }
        }

        #endregion

        #region Private Methods (Helpers)

        /// <summary>
        /// Moves a TextureRect up toward a target Y position at a fixed speed.
        /// </summary>
        private void MoveUp(double delta, int target, TextureRect toBeMoved)
        {
            if (toBeMoved.Position.Y > target) // Move up if current Y position is above the target
            {
                toBeMoved.Position += Vector2.Up * (float)(_signsMoveSpeed * delta);

                // If it reaches or passes the target Y position, snap it to the target
                if (toBeMoved.Position.Y <= target)
                {
                    toBeMoved.Position = new Vector2(toBeMoved.Position.X, target);
                }
            }
        }

        /// <summary>
        /// Moves a TextureRect down toward a target Y position at a fixed speed.
        /// </summary>
        private void MoveDown(double delta, int target, TextureRect toBeMoved)
        {
            if (toBeMoved.Position.Y < target) // Move down if current Y position is below the target
            {
                toBeMoved.Position += Vector2.Down * (float)(_signsMoveSpeed * delta);

                // If it reaches or passes the target Y position, snap it to the target
                if (toBeMoved.Position.Y >= target)
                {
                    toBeMoved.Position = new Vector2(toBeMoved.Position.X, target);
                }
            }
        }

        /// <summary>
        /// Plays the background music after a short delay.
        /// </summary>
        public async void PlayMusic()
        {
            await ToSignal(GetTree().CreateTimer(_musicDelayTimer), "timeout");

            if (_musicPlayer != null)
            {
                _musicPlayer.Play();
            }
        }

        /// <summary>
        /// Plays a sound effect from a given file path.
        /// </summary>
        public void PlaySfx(string pathToSfx)
        {
            if (_sfxPlayer != null && pathToSfx != null)
            {
                _sfxPlayer.Stream = GD.Load<AudioStream>(pathToSfx);
                _sfxPlayer.Play();
            }
        }

        #endregion
    }
}
