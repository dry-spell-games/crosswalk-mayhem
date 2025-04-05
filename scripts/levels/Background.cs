using Godot;
using System;
using System.Collections.Generic;

namespace Crosswalk
{
    /// <summary>
    /// Background node responsible for loading and instantiating a random background map
    /// based on the current game difficulty level.
    /// Different difficulty levels pull maps from different folders, allowing for visual variation.
    /// </summary>
    public partial class Background : Node
	{
		// Exported folder paths for each difficulty level
		[Export] private string veryEasyLevelsPath = "res://scenes/level-background/maps/very-easy-maps";
		[Export] private string easyLevelsPath = "res://scenes/level-background/maps/easy-maps";
		[Export] private string normalLevelsPath = "res://scenes/level-background/maps/normal-maps";
		[Export] private string hardLevelsPath = "res://scenes/level-background/maps/hard-maps";
		[Export] private string veryHardLevelsPath = "res://scenes/level-background/maps/very-hard-maps";
		[Export] private string mayhemLevelsPath = "res://scenes/level-background/maps/mayhem-maps";

        // Current difficulty setting (can be set dynamically elsewhere in the project)
        private int _difficulty = GameManager.Instance._difficulty;

        // Reference to the selected map scene
        private PackedScene _mapScene;

        // List of all available map scenes loaded from the folder
        private List<PackedScene> _mapScenes = new List<PackedScene>();

        /// <summary>
        /// Loads all .tscn scenes from a given folder and adds them to the _mapScenes list.
        /// </summary>
        /// <param name="folderPath">Folder path to load .tscn scenes from.</param>
        private void LoadScenesFromFolder(string folderPath)
        {
            var dirAccess = DirAccess.Open(folderPath);
            if (dirAccess != null)
            {
                dirAccess.ListDirBegin();

                string fileName = dirAccess.GetNext();
                while (fileName != "")
                {
                    string[] fileNameParts = fileName.Split(".");
                    if (fileNameParts.Length > 1 && fileNameParts[1] == "tscn");
                    {
                        fileName = fileNameParts[0] + "." + fileNameParts[1];
                    }
                    // Check if file is a scene file
                    if (fileName.EndsWith(".tscn"))
                    {
                        string scenePath = folderPath + "/" + fileName;
                        PackedScene scene = (PackedScene)ResourceLoader.Load(scenePath);

                        if (scene != null)
                        {
                            _mapScenes.Add(scene);
                        }
                        else
                        {
                            GD.PrintErr("Failed to load scene: " + scenePath);
                        }
                    }

                    fileName = dirAccess.GetNext();
                }

                dirAccess.ListDirEnd();
            }
            else
            {
                GD.PrintErr("Failed to open directory: " + folderPath);
            }
        }

        /// <summary>
        /// Chooses which folder to load map scenes from based on the current difficulty setting.
        /// </summary>
        private void LoadScenes()
        {
            switch (_difficulty)
            {
                case 0:
                    LoadScenesFromFolder(veryEasyLevelsPath);
                    break;
                case 1:
                    LoadScenesFromFolder(easyLevelsPath);
                    break;
                case 2:
                    LoadScenesFromFolder(normalLevelsPath);
                    break;
                case 3:
                    LoadScenesFromFolder(hardLevelsPath);
                    break;
                case 4:
                    LoadScenesFromFolder(veryHardLevelsPath);
                    break;
                case 5:
                    LoadScenesFromFolder(mayhemLevelsPath);
                    break;
                default:
                    GD.PrintErr("Unknown difficulty: " + _difficulty);
                    break;
            }
        }

        /// <summary>
        /// Loads background map scenes and instantiates one at random into the scene tree.
        /// </summary>
        private void InstantiateMap()
        {
            LoadScenes();

            // Only instantiate a map if none is currently loaded and we have options
            if (_mapScene == null && _mapScenes.Count > 0)
            {
                _mapScene = _mapScenes[RandomMap()];

                if (_mapScene == null)
                {
                    GD.PrintErr("Failed to load map scene!");
                }
                else
                {
                    Node mapInstance = _mapScene.Instantiate();
                    AddChild(mapInstance);
                }
            }
        }

        /// <summary>
        /// Returns a random index within the bounds of _mapScenes list.
        /// </summary>
        private int RandomMap()
        {
            Random random = new Random();
            return random.Next(0, _mapScenes.Count);
        }

        /// <summary>
        /// Called automatically by Godot when the node enters the scene tree.
        /// Triggers the background map instantiation process.
        /// </summary>
        public override void _Ready()
        {
            InstantiateMap();
        }
    }
}
