using Godot;
using System;
using System.Collections.Generic;

namespace Crosswalk
{
	public partial class Background : Node
	{
		[Export] private string veryEasyLevelsPath = "res://scenes/level-background/maps/very-easy-maps";
		[Export] private string easyLevelsPath = "res://scenes/level-background/maps/easy-maps";
		[Export] private string normalLevelsPath = "res://scenes/level-background/maps/normal-maps";
		[Export] private string hardLevelsPath = "res://scenes/level-background/maps/hard-maps";
		[Export] private string veryHardLevelsPath = "res://scenes/level-background/maps/very-hard-maps";
		[Export] private string mayhemLevelsPath = "res://scenes/level-background/maps/mayhem-maps";

		string _difficulty = "normal";
		private PackedScene _mapScene;
		private List<PackedScene> _mapScenes = new List<PackedScene>();

		private void LoadScenesFromFolder(string folderPath)
        {
            var dirAccess = DirAccess.Open(folderPath);
            if (dirAccess != null)
            {
                dirAccess.ListDirBegin();
                string fileName = dirAccess.GetNext();
                while (fileName != "")
                {
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

		private void LoadScenes()
		{
			switch(_difficulty)
			{
				case "very-easy":
					break;
				case "easy":
					break;
				case "normal":
					LoadScenesFromFolder(normalLevelsPath);
					break;
				case "hard":
					break;
				case "very-hard":
					break;
				case "mayhem":
					break;
			}
		}

		private void InstantiateMap()
        {
			LoadScenes();

            if (_mapScene == null && _mapScenes.Count > 0)
            {
                // Select a random map from the list
                _mapScene = _mapScenes[RandomMap()];

                if (_mapScene == null)
                {
                    GD.PrintErr("Failed to load map scene!");
                }
                else
                {
                    Node mapInstance = _mapScene.Instantiate();  // Correct syntax for Instantiation
                    AddChild(mapInstance);  // Add the instantiated map scene to the current scene
                }
            }
        }

		private int RandomMap()
        {
            Random random = new Random();
            return random.Next(0, _mapScenes.Count);  // Access _mapScenes for random index
        }

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			InstantiateMap();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
	}
}
