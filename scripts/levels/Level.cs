using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// Manages level andgameplay: spawning pedestrians/vehicles,
    /// traffic lights, difficulty and game over.
    /// </summary>
    public partial class Level : Node2D
    {
        #region Public Properties

        // Flag for car green light state
        public static bool _carGreen { get; private set; } = true;
        // Flag for pedestrian green light state
        public static bool _pedestrianGreen { get; private set; } = false;
        // Flag to indicate blinking pedestrian light
        public static bool _blink { get; private set; } = false;
        // Pedestrian spawn counter
        public int _pedestriansToSpawn { get; private set; }

        // Duration of green light for pedestrians per difficulty level
        [Export] public float[] _pedestrianGreenTimer = { 10f, 7f, 6f, 5f, 4f, 0f };
        // Duration for blink warning before pedestrian light ends per difficulty level
        [Export] public float[] _blinkTimer = { 3f, 3f, 3f, 2f, 1f, 0f };

        #endregion

        #region Private Fields

        // Current difficulty level
        private int _difficulty = GameManager.Instance._difficulty;
        // GUI reference
        private GUI _gui;
        // Flag to prevent multiple game over checks
        private bool _checkLife = false;
        // Flag to prevent triggering difficulty up multiple times
        private bool _difficultyIncreasing = false;

        // Rate at which pedestrians spawn at each difficulty level
        [Export] private float[] _spawnRate = { 4f, 3f, 2f, 1f, 0.5f, 0.2f };
        // Rate at which cars spawn at each difficulty level
        [Export] private int[] _carSpawnRate = { 6, 5, 4, 3, 2, 1 };
        // Number of pedestrians per level
        [Export] private int[] _pedestrianCount = { 10, 20, 30, 50, 80, 666 };
        // Duration of green light for cars per difficulty level
        [Export] private float[] _carGreenTimer = { 3, 5, 7, 8, 9, 10 };
        // Transition delay between light changes per difficulty
        [Export] private float[] _lightTransitionTimer = { 4f, 3f, 2f, 2f, 2f, 0f };
        // Life bonus granted at each difficulty level
        [Export] private int[] _lifeBonus = { 5, 5, 5, 5, 5, 0 };
        // Background music player
        [Export] private AudioStreamPlayer _musicPlayer;
        // UI element to block player input
        [Export] private Control _inputBlocker;
        // Time duration to show messages (like Game Over, New Record, etc.)
        [Export] private float _messageTimer = 3f;

        // Packed scenes for pedestrians and vehicles
        private PackedScene GuiScene;
        private PackedScene GrandmaScene;
        private PackedScene GrandpaScene;
        private PackedScene GirlScene;
        private PackedScene BoyScene;
        private PackedScene ManScene;
        private PackedScene WomanScene;
        private PackedScene SportsCarScene;
        private PackedScene SedanScene;
        private PackedScene SedanScene1;
        private PackedScene SedanScene2;
        private PackedScene SedanScene3;
        private PackedScene SuvScene;
        private PackedScene SuvScene1;
        private PackedScene SuvScene2;
        private PackedScene SuvScene3;
        private PackedScene BeetleScene;
        private PackedScene BeetleScene1;
        private PackedScene BeetleScene2;
        private PackedScene BeetleScene3;
        private PackedScene BusScene;
        private PackedScene BusScene1;
        private PackedScene BusScene2;

        // Array for all bus scenes to instantiate them in order
        private PackedScene[] busScenes;
        private int currentBusIndex = 0;

        // Hitbox for traffic light interaction
        private CollisionShape2D vehicleTrafficLightHitbox;

        // Random number generator
        private Random random = new Random();

        #endregion

        #region Public Signal Methods

        // Signal declarations
        //[Signal] public delegate void CarLightEventHandler(bool _carGreen);
        [Signal] public delegate void PedestrianLightEventHandler(bool _pedestrianGreen);
        //[Signal] public delegate void BlinkEventHandler(bool _pedestrianGreen);

        #endregion

        #region Godot Built-in Methods

        /// <summary>
        /// Called when the node enters the scene tree.
        /// Loads scenes and starts the game.
        /// </summary>
        public override async void _Ready()
        {
            // Wait one frame to ensure the node is fully in the scene tree
            await ToSignal(GetTree(), "process_frame");

            // Load pedestrian scenes
            GrandmaScene = (PackedScene)GD.Load("res://scenes/pedestrians/grandma.tscn");
            GrandpaScene = (PackedScene)GD.Load("res://scenes/pedestrians/grandpa.tscn");
            GirlScene = (PackedScene)GD.Load("res://scenes/pedestrians/girl.tscn");
            BoyScene = (PackedScene)GD.Load("res://scenes/pedestrians/boy.tscn");
            ManScene = (PackedScene)GD.Load("res://scenes/pedestrians/man.tscn");
            WomanScene = (PackedScene)GD.Load("res://scenes/pedestrians/woman.tscn");

            // Load vehicle scenes
            SportsCarScene = (PackedScene)GD.Load("res://scenes/vehicles/sportscar.tscn");
            SedanScene = (PackedScene)GD.Load("res://scenes/vehicles/sedan.tscn");
            SedanScene1 = (PackedScene)GD.Load("res://scenes/vehicles/sedan1.tscn");
            SedanScene2 = (PackedScene)GD.Load("res://scenes/vehicles/sedan2.tscn");
            SedanScene3 = (PackedScene)GD.Load("res://scenes/vehicles/sedan3.tscn");
            SuvScene = (PackedScene)GD.Load("res://scenes/vehicles/suv.tscn");
            SuvScene1 = (PackedScene)GD.Load("res://scenes/vehicles/suv1.tscn");
            SuvScene2 = (PackedScene)GD.Load("res://scenes/vehicles/suv2.tscn");
            SuvScene3 = (PackedScene)GD.Load("res://scenes/vehicles/suv3.tscn");
            BeetleScene = (PackedScene)GD.Load("res://scenes/vehicles/beetle.tscn");
            BeetleScene1 = (PackedScene)GD.Load("res://scenes/vehicles/beetle1.tscn");
            BeetleScene2 = (PackedScene)GD.Load("res://scenes/vehicles/beetle2.tscn");
            BeetleScene3 = (PackedScene)GD.Load("res://scenes/vehicles/beetle3.tscn");
            BusScene = (PackedScene)GD.Load("res://scenes/vehicles/bus.tscn");
            BusScene1 = (PackedScene)GD.Load("res://scenes/vehicles/bus1.tscn");
            BusScene2 = (PackedScene)GD.Load("res://scenes/vehicles/bus2.tscn");
            busScenes = new PackedScene[] { BusScene, BusScene1, BusScene2 };

            // Load GUI
            _gui = GetNode<GUI>("GUI");

            // Get vehicle traffic light hitbox
            vehicleTrafficLightHitbox = GetNode<CollisionShape2D>("TrafficLightsVehicles/Hitbox");

            // Check if scenes loaded properly
            if (GrandmaScene == null) GD.PrintErr("Failed to load Grandma scene!");
            else if (GrandpaScene == null) GD.PrintErr("Failed to load Grandpa scene");
            else if (GirlScene == null) GD.PrintErr("Failed to load Girl scene!");
            else if (BoyScene == null) GD.PrintErr("Failed to load Boy scene!");
            else if (ManScene == null) GD.PrintErr("Failed to load ManScene");
            else if (WomanScene == null) GD.PrintErr("Failed to load WomanScene");

            StartGame();
        }

        /// <summary>
        /// Runs every frame; handles hitbox toggle and game checks.
        /// </summary>
        public override void _Process(Double delta)
        {
            vehicleTrafficLightHitbox.Disabled = _carGreen;

            if (!GameManager.Instance._disableLifeCheck)
            {
                CheckLife();
            }

            CheckIfPedestriansLeft();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Spawns pedestrians at intervals until count reaches zero.
        /// </summary>
        private async void StartSpawningPedestrians()
        {
            while (_pedestriansToSpawn > 0)
            {
                // Waits while game is on a pause
                while (GetTree().Paused)
                {
                    await ToSignal(GetTree(), "process_frame"); // waits for next frame
                }
                SpawnPedestrian();

                // Timer which doesn't do anything during pause
                await ToSignal(GetTree().CreateTimer(_spawnRate[_difficulty], true), "timeout");
                _pedestriansToSpawn--;
            }
        }

        /// <summary>
        /// Continuously spawns cars at fixed intervals.
        /// </summary>
        private async void StartSpawningCars()
        {
            while (true)
            {
                // Waits while game is on a pause
                while (GetTree().Paused)
                {
                    await ToSignal(GetTree(), "process_frame"); // waits for next frame
                }

                SpawnVehicle();

                // Timer that also works during pause
                await ToSignal(GetTree().CreateTimer(_carSpawnRate[_difficulty], true), "timeout");
            }
        }

        /// <summary>
        /// Handles the traffic light cycle between cars and pedestrians.
        /// </summary>
        private async void TrafficLights()
        {
            while (true)
            {
                await ToSignal(GetTree().CreateTimer(_carGreenTimer[_difficulty], false, true), "timeout");
                _carGreen = false;
                await ToSignal(GetTree().CreateTimer(_lightTransitionTimer[_difficulty], false, true), "timeout");
                _pedestrianGreen = true;
                EmitSignal(SignalName.PedestrianLight, _pedestrianGreen);
                await ToSignal(GetTree().CreateTimer(_pedestrianGreenTimer[_difficulty], false, true), "timeout");
                _pedestrianGreen = false;
                EmitSignal(SignalName.PedestrianLight, _pedestrianGreen);
                await ToSignal(GetTree().CreateTimer(_lightTransitionTimer[_difficulty], false, true), "timeout");
                _carGreen = true;
            }
        }

        /// <summary>
        /// Instantiates a random pedestrian based on difficulty.
        /// </summary>
        private void SpawnPedestrian()
        {
            int rand;
            Pedestrian pedestrian = null;

            if (_difficulty < 2)
                rand = random.Next(0, 4);
            else
                rand = random.Next(0, 6);

            // Selects and instantiates the corresponding pedestrian scene
            if (rand == 0 && WomanScene != null)
                pedestrian = (Pedestrian)WomanScene.Instantiate();
            else if (rand == 1 && GirlScene != null)
                pedestrian = (Pedestrian)GirlScene.Instantiate();
            else if (rand == 2 && BoyScene != null)
                pedestrian = (Pedestrian)BoyScene.Instantiate();
            else if (rand == 3 && ManScene != null)
                pedestrian = (Pedestrian)ManScene.Instantiate();
            else if (rand == 4 && GrandmaScene != null)
                pedestrian = (Pedestrian)GrandmaScene.Instantiate();
            else if (rand == 5 && GrandpaScene != null)
                pedestrian = (Pedestrian)GrandpaScene.Instantiate();

            if (pedestrian == null)
            {
                GD.PrintErr("Failed to instantiate pedestrian!");
                return;
            }

            Vector2 spawnPosition = pedestrian.StartPositions[GD.RandRange(0, pedestrian.StartPositions.Count - 1)];

            pedestrian.Initialize(spawnPosition);
            AddChild(pedestrian);
        }

        /// <summary>
        /// Instantiates a vehicle and spawns it if space is available.
        /// </summary>
        private void SpawnVehicle()
        {
            Car car = null;
            int rand;

            if (_difficulty < 2)
                rand = random.Next(0, 13);
            else if (_difficulty > 1 && _difficulty < 5)
                rand = random.Next(0, 14);
            else
                rand = 13;

            // Instantiate a random vehicle scene
            if (rand == 0 && BeetleScene != null)
                car = (Car)BeetleScene.Instantiate();
            else if (rand == 1 && SedanScene != null)
                car = (Car)SedanScene.Instantiate();
            else if (rand == 2 && SuvScene != null)
                car = (Car)SuvScene.Instantiate();
            else if (rand == 3 && SedanScene1 != null)
                car = (Car)SedanScene1.Instantiate();
            else if (rand == 4 && SedanScene2 != null)
                car = (Car)SedanScene2.Instantiate();
            else if (rand == 5 && SedanScene3 != null)
                car = (Car)SedanScene3.Instantiate();
            else if (rand == 6 && SuvScene1 != null)
                car = (Car)SuvScene1.Instantiate();
            else if (rand == 7 && SuvScene2 != null)
                car = (Car)SuvScene2.Instantiate();
            else if (rand == 8 && SuvScene3 != null)
                car = (Car)SuvScene3.Instantiate();
            else if (rand == 9 && BeetleScene1 != null)
                car = (Car)BeetleScene1.Instantiate();
            else if (rand == 10 && BeetleScene2 != null)
                car = (Car)BeetleScene2.Instantiate();
            else if (rand == 11 && BeetleScene3 != null)
                car = (Car)BeetleScene3.Instantiate();
            else if (rand == 12 && busScenes.Length > 0)
            {
                PackedScene selectedScene = busScenes[currentBusIndex];
                if (selectedScene != null)
                {
                    car = (Car)selectedScene.Instantiate();

                    // Rotates through all the buses
                    currentBusIndex = (currentBusIndex + 1) % busScenes.Length;
                }
            }
            else if (rand == 13 && SportsCarScene != null)
                car = (Car)SportsCarScene.Instantiate();

            Vector2 spawnPosition = car.StartPositions[GD.RandRange(0, car.StartPositions.Count - 1)];

            if (IsSpawnPointCarOccupied(spawnPosition))
            {
                return;
            }

            car.Initialize(spawnPosition);
            AddChild(car);
        }

        /// <summary>
        /// Checks if a car is already occupying the spawn position.
        /// </summary>
        private bool IsSpawnPointCarOccupied(Vector2 spawnPosition)
        {
            float minDistance = 50.0f;

            foreach (Node child in GetChildren())
            {
                if (child is Car existingCar)
                {
                    if (existingCar.Position.DistanceTo(spawnPosition) < minDistance)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Handles end-of-game logic, displays messages and saves data.
        /// </summary>
        private async void GameOver()
        {
            GameManager.Instance._gameOver = true;
            _inputBlocker.Visible = true;
            _musicPlayer.Stop();

            if (GameManager.Instance._score > GameManager.Instance._highscore)
            {
                GameManager.Instance.UpdateHighscore();
                GameManager.Instance.SaveData();
                await _gui.ShowMessage(_messageTimer, "NEW_RECORD");
            }

            _gui.ShowGameOver();
        }

        /// <summary>
        /// Checks if player's life is zero and triggers game over if true.
        /// </summary>
        private void CheckLife()
        {
            if (GameManager.Instance._life <= 0 && !_checkLife)
            {
                _checkLife = true;
                GameOver();
            }
        }

        /// <summary>
        /// Initializes and starts the game logic for the current level.
        /// </summary>
        private async void StartGame()
        {
            _difficulty = GameManager.Instance._difficulty;
            _pedestriansToSpawn = _pedestrianCount[_difficulty];

            GameManager.Instance._disableLifeCheck = false;
            GameManager.Instance._gameOver = false;
            GameManager.Instance.UpdateLife(_lifeBonus[_difficulty]);
            _inputBlocker.Visible = true;

            _carGreen = true;
            _pedestrianGreen = false;
            _blink = false;

            TrafficLights();
            await _gui.ShowMessage(_messageTimer, "GET_READY", "", true);

            _musicPlayer.Play();
            StartSpawningPedestrians();
            StartSpawningCars();
            EmitSignal(SignalName.PedestrianLight, _pedestrianGreen);
            _inputBlocker.Visible = false;
        }

        /// <summary>
        /// Checks if all pedestrians are gone and triggers difficulty increase.
        /// </summary>
        private void CheckIfPedestriansLeft()
        {
            if (_difficultyIncreasing || GetTree() == null || GameManager.Instance._gameOver)
            {
                return;
            }

            if (_pedestriansToSpawn <= 0 &&
                GetTree().GetNodesInGroup("pedestrians").Count == 0)
            {
                _difficultyIncreasing = true;
                if (GameManager.Instance._difficulty < 5)
                {
                    DifficultyUp();
                }
                else
                {
                    GameOver();
                }
            }
        }

        /// <summary>
        /// Increases difficulty and reloads the level scene.
        /// </summary>
        private async void DifficultyUp()
        {
            _inputBlocker.Visible = true;
            _musicPlayer.Stop();
            await _gui.ShowMessage(_messageTimer, "DIFFICULTY_UP");
            GameManager.Instance._difficulty += 1;
            GetTree().ChangeSceneToFile("res://scenes/levels/level.tscn");
        }

        #endregion
    }
}
