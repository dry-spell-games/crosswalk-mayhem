using Godot;
using System;
using System.Dynamic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Crosswalk
{
    public partial class Level : Node2D
    {
        [Export] private float[] _spawnRate  = {4, 3.5f, 3, 2.5f, 2, 1.5f};
        [Export] private int[] _carSpawnRate = {7, 6, 5, 4, 3, 2};
         // How many pedestrians level has
        [Export] private int[] _pedestrianCount = {20, 30, 50, 75, 100, 2000};
        // Traffic lights
        [Export] private float[] _carGreenTimer = {5, 6, 7, 8, 9, 10};
        public static bool _carGreen { get; private set; } = true;
        [Export] public float[] _pedestrianGreenTimer = {8, 7, 6, 5, 4, 3};
        public static bool _pedestrianGreen { get; private set; } = false;
        [Export] public float[] _blinkTimer = {2, 2, 2, 2, 1, 1};
        public static bool _blink { get; private set; } = false;
        [Export] private float[] _lightTransitionTimer = {4, 3, 2, 2, 2, 1};
        private PackedScene GuiScene;
        private PackedScene GrandmaScene;
        private PackedScene GrandpaScene;
        private PackedScene GirlScene;
        private PackedScene BoyScene;
        private PackedScene ManScene;
        private PackedScene WomanScene;
        private PackedScene FamilyCarScene;
        private PackedScene SportsCarScene;
        private PackedScene SedanScene;
        private PackedScene SedanScene1;
        private PackedScene SedanScene2;
        private PackedScene SedanScene3;
        private PackedScene SuvScene;
        private PackedScene SuvScene1;
        private PackedScene SuvScene2;
        private PackedScene SuvScene3;
        private CollisionShape2D vehicleTrafficLightHitbox;
        private Random random = new Random();
        // Signals for traffic lights
        //[Signal] public delegate void CarLightEventHandler(bool _carGreen);
        [Signal] public delegate void PedestrianLightEventHandler(bool _pedestrianGreen);
        //[Signal] public delegate void BlinkEventHandler(bool _pedestrianGreen);
        private int _difficulty = GameManager.Instance._difLvl;



        public override void _Ready()
        {
                    // @@@@@ TESTI VAIKEUDEN ASETUS@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            _difficulty = 3;
            GD.Print($"Level {_difficulty} started");

            // Loads pedestrian scenes
            GrandmaScene = (PackedScene)GD.Load("res://scenes/pedestrians/grandma.tscn");
            GrandpaScene = (PackedScene)GD.Load("res://scenes/pedestrians/grandpa.tscn");
            GirlScene = (PackedScene)GD.Load("res://scenes/pedestrians/girl.tscn");
            BoyScene = (PackedScene)GD.Load("res://scenes/pedestrians/boy.tscn");
            ManScene = (PackedScene)GD.Load("res://scenes/pedestrians/man.tscn");
            WomanScene = (PackedScene)GD.Load("res://scenes/pedestrians/woman.tscn");

            // Loads vehicle scenes
            FamilyCarScene = (PackedScene)GD.Load("res://scenes/vehicles/familycar.tscn");
            SportsCarScene = (PackedScene)GD.Load("res://scenes/vehicles/sportscar.tscn");
            SedanScene = (PackedScene)GD.Load("res://scenes/vehicles/sedan.tscn");
            SedanScene1 = (PackedScene)GD.Load("res://scenes/vehicles/sedan1.tscn");
            SedanScene2 = (PackedScene)GD.Load("res://scenes/vehicles/sedan2.tscn");
            SedanScene3 = (PackedScene)GD.Load("res://scenes/vehicles/sedan3.tscn");
            SuvScene = (PackedScene)GD.Load("res://scenes/vehicles/suv.tscn");
            SuvScene1 = (PackedScene)GD.Load("res://scenes/vehicles/suv1.tscn");
            SuvScene2 = (PackedScene)GD.Load("res://scenes/vehicles/suv2.tscn");
            SuvScene3 = (PackedScene)GD.Load("res://scenes/vehicles/suv3.tscn");

            // Loads GUI scene
            GuiScene = (PackedScene)GD.Load("res://gui/gui.tscn");

            // Gets trafficlight's hitboxes
            vehicleTrafficLightHitbox = GetNode<CollisionShape2D>("TrafficLightsVehicles/Hitbox");

            // Instantiates GUI for level
            Node guiInstance = GuiScene.Instantiate();
            AddChild(guiInstance);

            if (GrandmaScene == null)
            {
                GD.PrintErr("Failed to load Grandma scene!");
            }
            if (GrandpaScene == null)
            {
                GD.PrintErr("Failed to load Grandpa scene");
            }
            else if (GirlScene == null) {
                GD.Print("Failed to load Girl scene!");
            }
            else if (BoyScene == null) {
                GD.Print("Failed to load Boy scene!");
            }
            else if (ManScene == null) {
                GD.Print("Failed to load ManScene");
            }
            else if (WomanScene == null) {
                GD.Print("Failed to load WomanScene");
            }
            StartSpawningPedestrians();
            StartSpawningCars();
            TrafficLights();
            EmitSignal(SignalName.PedestrianLight, _pedestrianGreen);
        }
        public override void _Process(Double delta)
        {
            if (_carGreen)
            {
                vehicleTrafficLightHitbox.Disabled = true;
            }
            else
            {
                vehicleTrafficLightHitbox.Disabled = false;
            }
        }

        // Asynchronous method that spawns pedestrians at set intervals
        // Spawns pedestrians until PedestrianCount reaches zero.
        private async void StartSpawningPedestrians()
        {
            while (_pedestrianCount[_difficulty] > 0)
            {
                SpawnPedestrian();
                await ToSignal(GetTree().CreateTimer(_spawnRate[_difficulty]), "timeout");
                _pedestrianCount[_difficulty]--;
                GD.Print($"Pedestrians left: {_pedestrianCount[_difficulty]}");
            }
        }

        private async void StartSpawningCars()
        {
            while (true)
            {
                SpawnVehicle();
                await ToSignal(GetTree().CreateTimer(_carSpawnRate[_difficulty]), "timeout");
            }
        }

        private async void TrafficLights()
        {
            while(true)
            {
                GD.Print("Green light for cars");
                //EmitSignal(SignalName.CarLight, _carGreen);
                await ToSignal(GetTree().CreateTimer(_carGreenTimer[_difficulty]), "timeout");
                _carGreen = false;
                //EmitSignal(SignalName.CarLight, _carGreen);
                GD.Print("Traffic light transition");
                await ToSignal(GetTree().CreateTimer(_lightTransitionTimer[_difficulty]), "timeout");
                _pedestrianGreen = true;
                GD.Print("Green ligth for pedestrians");
                EmitSignal(SignalName.PedestrianLight, _pedestrianGreen);
                await ToSignal(GetTree().CreateTimer(_pedestrianGreenTimer[_difficulty]), "timeout");
                _pedestrianGreen = false;
                EmitSignal(SignalName.PedestrianLight, _pedestrianGreen);
                GD.Print("Traffic light transition");
                await ToSignal(GetTree().CreateTimer(_lightTransitionTimer[_difficulty]), "timeout");
                _carGreen = true;
            }
        }

        private void SpawnPedestrian()
        {
            // Randomly generated pedestrian type
            int rand;
            Pedestrian pedestrian = null;
            // Difficulties 0-1 don't include "The Elderly"
            if (_difficulty < 2)
            {
                rand = random.Next(0,4);
            }
            else {
            rand = random.Next(0, 6);  // 0 = Grandma, 1 = Girl, 2 Boy, 3 Man, 4 Woman, 5 Grandpa
            }

            if (rand == 0 && WomanScene != null)
            {
                pedestrian = (Pedestrian)WomanScene.Instantiate();
            }
            else if (rand == 1 && GirlScene != null)
            {
                pedestrian = (Pedestrian)GirlScene.Instantiate();
            }
            else if (rand == 2 && BoyScene != null)
            {
                pedestrian = (Pedestrian)BoyScene.Instantiate();
            }
            else if (rand == 3 && ManScene != null)
            {
                pedestrian = (Pedestrian)ManScene.Instantiate();
            }
            else if (rand == 4 && GrandmaScene!= null)
            {
                pedestrian = (Pedestrian)GrandmaScene.Instantiate();
            }
            else if (rand == 5 && GrandpaScene != null)
            {
                pedestrian = (Pedestrian)GrandpaScene.Instantiate();
            }
            if (pedestrian == null)
            {
                GD.PrintErr("Failed to instantiate pedestrian!");
                return;
            }

            // Spawnposition from abstract class Pedestrian
            Vector2 spawnPosition = pedestrian.StartPositions[GD.RandRange(
                0, pedestrian.StartPositions.Count - 1)];

            pedestrian.Initialize(spawnPosition);
            AddChild(pedestrian);
        }

        private void SpawnVehicle()
        {
             // 0 FamilyCar, 1 SportsCar, 2 Blue Sedan, 3 Blue SUV
            // 4 Green Sedan, 5 Red Sedan, 6 Yellow Sedan, 7 Yellow SUV, 8 Green SUV, 9 Red SUV
            Car car = null;
            int rand; // Randomizes vehicle
            if (_difficulty < 2)
            {
                rand = random.Next(0, 9); // Levels 0-1 do NOT include sportscar
            }
            else
            {
                rand = random.Next(0, 10);
            }

            // Instantiates randomized vehicle
            if (rand == 0 && FamilyCarScene != null)
            {
                car = (Car)FamilyCarScene.Instantiate();
            }
            else if (rand == 1 && SedanScene != null)
            {
                car = (Car)SedanScene.Instantiate();
            }
            else if (rand == 2 && SuvScene != null)
            {
                car = (Car)SuvScene.Instantiate();
            }
            else if (rand == 3 && SedanScene1 != null)
            {
                car = (Car)SedanScene1.Instantiate();
            }
            else if (rand == 4 && SedanScene2 != null)
            {
                car = (Car)SedanScene2.Instantiate();
            }
            else if (rand == 5 && SedanScene3 != null)
            {
                car = (Car)SedanScene3.Instantiate();
            }
            else if (rand == 6 && SuvScene1 != null)
            {
                car = (Car)SuvScene1.Instantiate();
            }
            else if (rand == 7 && SuvScene2 != null)
            {
                car = (Car)SuvScene2.Instantiate();
            }
            else if (rand == 8 && SuvScene3 != null)
            {
                car = (Car)SuvScene3.Instantiate();
            }
            else if (rand == 9 && SportsCarScene != null)
            {
                car = (Car)SportsCarScene.Instantiate();
            }

            // Randomizes spawn point for vehicle
            Vector2 spawnPosition = car.StartPositions[GD.RandRange(0, car.StartPositions.Count - 1)];

            // Checks if the spawnpoint already has a vehicle
            if (IsSpawnPointCarOccupied(spawnPosition))
            {
                GD.Print("Spawnpoint occupied, can not spawn a vehicle");
                return; // Doesn't spawn a car if car is already near spawn point
            }
            car.Initialize(spawnPosition);
            AddChild(car);
        }

        // Method checks there isn't any vehicle too close to spawn point already
        private bool IsSpawnPointCarOccupied(Vector2 spawnPosition)
        {
            // Minimum distance before vehicle can be instantiated
            float minDistance = 50.0f;

            foreach (Node child in GetChildren())
            {
                // Checks if child is car
                if (child is Car existingCar)
                {
                    // Compares distance from child in vehicles
                    if (existingCar.Position.DistanceTo(spawnPosition) < minDistance)
                    {
                        return true; // If distance is too short, returns true
                    }
                }
            }
            return false; // No vehicles found too close, returns false to spawn a vehicle
        }
    }
}
