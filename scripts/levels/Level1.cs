using Godot;
using System;
using System.Threading;

namespace Crosswalk
{
    public partial class Level1 : Node2D
    {
        [Export] private float SpawnRate { get; set; } = 2.0f;
        [Export] private float CarSpawnRate { get; set; } = 10.0f;
        [Export] private float TrafficLightsTimer { get; set; } = 1.0f;
        [Export] private int PedestrianCount { get; set; } = 20; // How many pedestrians level has
        private bool RedLightForCars = false;
        private PackedScene GuiScene;
        private PackedScene GrandmaScene;
        private PackedScene GirlScene;
        private PackedScene BoyScene;
        private PackedScene ManScene;
        private PackedScene WomanScene;
        private PackedScene FamilyCarScene;
        private PackedScene SportsCarScene;
        private CollisionShape2D vehicleTrafficLigthHitbox;
        private CollisionShape2D pedestrianTrafficLightLeft;
        private CollisionShape2D pedestrianTrafficLightRight;

        private Random random = new Random();

        public override void _Ready()
        {
            GD.Print("Level 1 started");

            // Loads pedestrian scenes
            GrandmaScene = (PackedScene)GD.Load("res://scenes/pedestrians/grandma.tscn");
            GirlScene = (PackedScene)GD.Load("res://scenes/pedestrians/girl.tscn");
            BoyScene = (PackedScene)GD.Load("res://scenes/pedestrians/boy.tscn");
            ManScene = (PackedScene)GD.Load("res://scenes/pedestrians/man.tscn");
            WomanScene = (PackedScene)GD.Load("res://scenes/pedestrians/woman.tscn");

            // Loads vehicle scenes
            FamilyCarScene = (PackedScene)GD.Load("res://scenes/vehicles/familycar.tscn");
            SportsCarScene = (PackedScene)GD.Load("res://scenes/vehicles/sportscar.tscn");
            // Loads GUI scene
            GuiScene = (PackedScene)GD.Load("res://gui/gui.tscn");

            // Gets trafficlight's hitboxes
            vehicleTrafficLigthHitbox = GetNode<CollisionShape2D>("TrafficLightsVehicles/Hitbox");
            pedestrianTrafficLightLeft = GetNode<CollisionShape2D>("TrafficLightsPedestriansLeft/Hitbox");
            pedestrianTrafficLightRight = GetNode<CollisionShape2D>("TrafficLightsPedestriansRight/Hitbox");

            // Instantiates GUI for level
            Node guiInstance = GuiScene.Instantiate();
            AddChild(guiInstance);

            if (GrandmaScene == null)
            {
                GD.PrintErr("Failed to load Grandma scene!");
                return;
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
            RedLights();
        }
        public override void _Process(Double delta)
        {
            if (!RedLightForCars)
            {
                vehicleTrafficLigthHitbox.Disabled = true;
                pedestrianTrafficLightLeft.Disabled = false;
                pedestrianTrafficLightRight.Disabled = false;
            }
            else
            {
                vehicleTrafficLigthHitbox.Disabled = false;
                pedestrianTrafficLightLeft.Disabled = true;
                pedestrianTrafficLightRight.Disabled = true;
            }
        }

        // Asynchronous method that spawns pedestrians at set intervals
        // Spawns pedestrians until PedestrianCount reaches zero.
        private async void StartSpawningPedestrians()
        {
            while (PedestrianCount > 0)
            {
                SpawnPedestrian();
                await ToSignal(GetTree().CreateTimer(SpawnRate), "timeout");
                PedestrianCount--;
                GD.Print($"Pedestrians left: {PedestrianCount}");
            }
        }

        private async void StartSpawningCars()
        {
            while (true)
            {
                SpawnVehicle();
                await ToSignal(GetTree().CreateTimer(CarSpawnRate), "timeout");
            }
        }

        private async void RedLights()
        {
            while(true)
            {
                await ToSignal(GetTree().CreateTimer(TrafficLightsTimer), "timeout");
                RedLightForCars = !RedLightForCars;
                GD.Print($"Red ligths for vehicle: {RedLightForCars}");
                GD.Print($"Red ligths for pedestrians: {!RedLightForCars}");
            }
        }

        private void SpawnPedestrian()
        {
            Pedestrian pedestrian = null;

            // Randomly generated pedestrian type
            int rand = random.Next(0, 5);  // 0 = Grandma, 1 = Girl, 2 Boy, 3 Man, 4 Woman
            if (rand == 0 && GrandmaScene != null)
            {
                pedestrian = (Pedestrian)GrandmaScene.Instantiate();
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
            else if (rand == 4 && WomanScene!= null)
            {
                pedestrian = (Pedestrian)WomanScene.Instantiate();
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
            int rand = random.Next(0, 2); // 0 FamilyCar 1 SportsCar
            Car car = null;

            // Randomizes vehicle
            if (rand == 0 && FamilyCarScene != null)
            {
                car = (Car)FamilyCarScene.Instantiate();
            }
            else if (rand == 1 && SportsCarScene != null)
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
