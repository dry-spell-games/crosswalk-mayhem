using Godot;
using System.Text.Json;

namespace Crosswalk
{
    /// <summary>
    /// GameManager handles global game data, including scores, settings, language,
    /// volume control, and persistent save/load functionality.
    /// </summary>
    public partial class GameManager : Node
    {
        #region Public Properties

        // Singleton instance
        public static GameManager Instance { get; private set; }
        // Current in-game score
        public int _score { get; private set; }
        // Stored highscore
        public int _highscore { get; private set; }
        // Player lives
        public int _life { get; private set; } = 0;
        // Master volume in dB
        public float _masterVolume { get; set; }
        // Music volume in dB
        public float _musicVolume { get; set; }
        // SFX volume in dB
        public float _sfxVolume { get; set; }
        // Last saved master volume
        public float _savedMasterVolume { get; set; }
        // Last saved music volume
        public float _savedMusicVolume;
        // Last saved SFX volume
        public float _savedSfxVolume;
        // Currently selected language code ("en", "fi")
        public string _langCode { get; set; }
        // Game difficulty level
        public int _difficulty { get; set; } = 0;
        // Flag for checking if the game is over
        public bool _gameOver { get; set; } = false;
        // Flag for checking if life count should be checked
        public bool _disableLifeCheck = false;

        #endregion

        #region Private Fields

        // Reference to score label in the GUI
        private Label _scoreLabel;
        // Reference to the highscore label in the GUI
        private Label _highscoreLabel;
        // Reference to the life label in the GUI
        private Label _lifeLabel;

        #endregion

        #region Public Methods

        /// <summary>
        /// Links the GUI score label to this manager and updates it immediately.
        /// </summary>
        public void SetScoreLabel(Label label)
        {
            _scoreLabel = label;
            UpdateScoreLabel();
        }

        /// <summary>
        /// Adds to the current score and updates the GUI label.
        /// </summary>
        public void AddScore(int amount)
        {
            if (!_gameOver)
            {
                _score += amount;
                UpdateScoreLabel();
            }
        }

        /// <summary>
        /// Resets the score to zero and updates the GUI.
        /// </summary>
        public void ResetScore()
        {
            _score = 0;
            UpdateScoreLabel();
        }

        /// <summary>
        /// Updates the stored highscore value.
        /// </summary>
        public void SetHighscore(int highscore)
        {
            _highscore = highscore;
        }

        /// <summary>
        /// Updates the highscore to the current score.
        /// </summary>
        public void UpdateHighscore()
        {
            _highscore = _score;
            UpdateHighscoreLabel();
        }

        /// <summary>
        /// Links the GUI highscore label to this manager and updates it immediately.
        /// </summary>
        public void SetHighscoreLabel(Label label)
        {
            _highscoreLabel = label;
            UpdateHighscoreLabel();
        }

        /// <summary>
        /// Links the GUI life label to this manager and updates it immediately.
        /// </summary>
        public void SetLifeLabel(Label label)
        {
            _lifeLabel = label;
            UpdateLifeLabel();
        }

        /// <summary>
        /// Updates the player's lives by the specified value.
        /// </summary>
        public void UpdateLife(int value)
        {
            if ((_life + value) >= 0)
            {
                _life += value;
                UpdateLifeLabel();
            }
        }

        /// <summary>
        /// Resets the player's lives to 0.
        /// </summary>
        public void ResetLife()
        {
            _life = 0;
        }

        /// <summary>
        /// Saves highscore, volume levels, and language setting to a JSON file.
        /// </summary>
        public void SaveData()
        {
            var saveData = new
            {
                highscore = _highscore,
                masterVolume = _masterVolume,
                musicVolume = _musicVolume,
                sfxVolume = _sfxVolume,
                langCode = _langCode
            };

            string jsonString = JsonSerializer.Serialize(saveData);
            string path = "user://save.json";

            try
            {
                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
                file.StoreString(jsonString);
            }
            catch (System.Exception ex)
            {
                GD.PrintErr("Failed to save data: " + ex.Message);
            }

            _savedMasterVolume = _masterVolume;
            _savedMusicVolume = _musicVolume;
            _savedSfxVolume = _sfxVolume;
        }

        /// <summary>
        /// Loads saved game data from disk.
        /// If no file exists or loading fails, default values are used.
        /// </summary>
        public void LoadData()
        {
            string path = "user://save.json";

            if (!FileAccess.FileExists(path))
            {
                LoadDefaultData();
                return;
            }

            try
            {
                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                string jsonString = file.GetAsText();
                var saveData = JsonSerializer.Deserialize<JsonElement>(jsonString);

                _highscore = saveData.GetProperty("highscore").GetInt32();
                _masterVolume = (float)saveData.GetProperty("masterVolume").GetDouble();
                _musicVolume = (float)saveData.GetProperty("musicVolume").GetDouble();
                _sfxVolume = (float)saveData.GetProperty("sfxVolume").GetDouble();
                _langCode = saveData.GetProperty("langCode").GetString();

                _savedMasterVolume = _masterVolume;
                _savedMusicVolume = _musicVolume;
                _savedSfxVolume = _sfxVolume;
            }
            catch (System.Exception ex)
            {
                GD.PrintErr("Failed to load data: " + ex.Message);
                LoadDefaultData();
            }
        }

        /// <summary>
        /// Sets the current game language and updates the translation.
        /// </summary>
        public void SetLanguage(string langCode)
        {
            _langCode = langCode;
            TranslationServer.SetLocale(langCode);
        }

        /// <summary>
        /// Sets the volume in decibels for a given audio bus by name.
        /// </summary>
        /// <returns>True if the bus exists and volume was set, false otherwise.</returns>
        public bool SetVolume(string busName, float volumeDb)
        {
            int busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex < 0)
            {
                GD.PrintErr($"Bus '{busName}' not found!");
                return false;
            }

            AudioServer.SetBusVolumeDb(busIndex, volumeDb);
            return true;
        }

        /// <summary>
        /// Gets the current volume in decibels for a given audio bus by name.
        /// </summary>
        /// <param name="volumeDb">Outputs the current volume of the bus.</param>
        /// <returns>True if successful, false if the bus was not found.</returns>
        public bool GetVolume(string busName, out float volumeDb)
        {
            int busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex < 0)
            {
                GD.PrintErr($"Bus '{busName}' not found!");
                volumeDb = float.NaN;
                return false;
            }

            volumeDb = AudioServer.GetBusVolumeDb(busIndex);
            return true;
        }

        #endregion

        #region Godot Built-in Methods

        /// <summary>
        /// Called when the node enters the scene tree.
        /// Initializes the singleton instance.
        /// </summary>
        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree(); // Prevent duplicate GameManager instances
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the GUI score label with the current score.
        /// </summary>
        private void UpdateScoreLabel()
        {
            if (_scoreLabel != null)
            {
                _scoreLabel.Text = $"{_score}";
            }
        }

        /// <summary>
        /// Updates the GUI highscore label with the saved highscore.
        /// </summary>
        private void UpdateHighscoreLabel()
        {
            if (_highscoreLabel != null)
            {
                _highscoreLabel.Text = $"{_highscore}";
            }
        }

        /// <summary>
        /// Updates the GUI life label with the current lives.
        /// </summary>
        private void UpdateLifeLabel()
        {
            if (_lifeLabel != null)
            {
                _lifeLabel.Text = $"{_life}";
            }
        }

        /// <summary>
        /// Loads default values for volume, highscore, and language.
        /// Used when no save file is present or data fails to load.
        /// </summary>
        private void LoadDefaultData()
        {
            _highscore = 0;
            _masterVolume = -6.0f;
            _musicVolume = -6.0f;
            _sfxVolume = -6.0f;
            _langCode = "en";
        }

        #endregion
    }
}
