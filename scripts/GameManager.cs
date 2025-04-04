using Godot;
using System;
using System.Text.Json;

namespace Crosswalk
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }
        public int _score { get; private set; }
        public int _highscore { get; private set; }
        public int _life { get; private set; }
        public float _masterVolume { get; set; }
        public float _musicVolume { get; set; }
        public float _sfxVolume { get; set; }
        public float _savedMasterVolume;
        public float _savedMusicVolume;
        public float _savedSfxVolume;
        public String _langCode { get; set; }
        private Label scoreLabel; // Reference to score label
        public int _difLvl { get; set; } = 3; // Game difficulty level

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

        private void LoadDefaultData()
        {
            _highscore = 0;
            _masterVolume = -6.0f;
            _musicVolume = -6.0f;
            _sfxVolume = -6.0f;
            _langCode = "en";
        }

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
                GD.Print("Data saved!");
            }
            catch (System.Exception ex)
            {
                GD.PrintErr("Failed to save data: " + ex.Message);
            }

            _savedMasterVolume = _masterVolume;
            _savedMusicVolume = _musicVolume;
            _savedSfxVolume = _sfxVolume;
        }

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

                GD.Print("Data loaded!");
            }
            catch (System.Exception ex)
            {
                GD.PrintErr("Failed to load data: " + ex.Message);
                LoadDefaultData();
            }
        }

        public void SetLanguage(string langCode)
        {
            GameManager.Instance._langCode = langCode;
            TranslationServer.SetLocale(langCode);
        }

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

        public void SetHighscore(int highscore)
        {
            _highscore = highscore;
        }
    }
}
