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
        private PackedScene GrandmaScene;
        private PackedScene GirlScene;
        private PackedScene BoyScene;
        private PackedScene FamilyCarScene;
        private PackedScene SportsCarScene;
        private CollisionShape2D collisionShape;


        private Random random = new Random();

        public override void _Ready()
        {
            GD.Print("Level 1 started");

            // Loads pedestrian scenes
            GrandmaScene = (PackedScene)ResourceLoader.Load("res://scenes/pedestrians/grandma.tscn");
            GirlScene = (PackedScene)ResourceLoader.Load("res://scenes/pedestrians/girl.tscn");
            BoyScene = (PackedScene)ResourceLoader.Load("res://scenes/pedestrians/boy.tscn");

            // Loads vehicle scenes
            FamilyCarScene = (PackedScene)ResourceLoader.Load("res://scenes/vehicles/familycar.tscn");
            SportsCarScene = (PackedScene)ResourceLoader.Load("res://scenes/vehicles/sportscar.tscn");

            // Gets trafficlight's hitbox
            collisionShape = GetNode<CollisionShape2D>("TrafficLightsVehicles/Hitbox");

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
            StartSpawningPedestrians();
            StartSpawningCars();
            RedLights();
        }
        public override void _Process(Double delta)
        {
            if (!RedLightForCars)
            {
                collisionShape.Disabled = true;
            }
            else
            {
                collisionShape.Disabled = false;
            }
        }

        // Asynchronous method which calls SpawnPedestrian method for set amount of Pedestrians
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
                SpawnCar();
                await ToSignal(GetTree().CreateTimer(CarSpawnRate), "timeout");
            }
        }

        // Asynchronous method that spawns pedestrians at set intervals
        // Spawns pedestrians until PedestrianCount reaches zero.
        private async void RedLights()
        {
            while(true)
            {
                await ToSignal(GetTree().CreateTimer(TrafficLightsTimer), "timeout");
                RedLightForCars = !RedLightForCars;
                GD.Print($"Autoille punainen {RedLightForCars}");
            }
        }

        private void SpawnPedestrian()
        {
            Pedestrian pedestrian = null;

            // Randomly generated pedestrian type
            int rand = random.Next(0, 3);  // 0 = Grandma, 1 = Girl, 2 Boy
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

        private void SpawnCar()
        {
            int rand = random.Next(0, 2); // 0 FamilyCar 1 SportsCar
            Car car = null;

            // Valitse auto
            if (rand == 0 && FamilyCarScene != null)
            {
                car = (Car)FamilyCarScene.Instantiate();
            }
            else if (rand == 1 && SportsCarScene != null)
            {
                car = (Car)SportsCarScene.Instantiate();
                GD.Print("Sporttiauto instantoitu");
            }

            // Valitse spawn-sijainti
            Vector2 spawnPosition = car.StartPositions[GD.RandRange(0, car.StartPositions.Count - 1)];

            // Tarkista, onko spawnpointissa jo auto
            if (IsSpawnPointCarOccupied(spawnPosition))
            {
                GD.Print("Spawnpoint on jo varattu, ei spawnata autoa.");
                return; // Älä spawnaa autoa, jos paikka on varattu
            }

            car.Initialize(spawnPosition);
            AddChild(car);
        }

        // Tarkistusmetodi, joka varmistaa ettei auto ole liian lähellä spawnpointtia
        private bool IsSpawnPointCarOccupied(Vector2 spawnPosition)
        {
            // Määritä sopiva etäisyys, kuinka lähellä spawnpointtia on tarkistettava
            float minDistance = 50.0f;

            foreach (Node child in GetChildren())
            {
                // Tarkista, onko lapsi tyyppiä Car
                if (child is Car existingCar)
                {
                    // Tarkista etäisyys nykyisen auton ja spawnpointin välillä
                    if (existingCar.Position.DistanceTo(spawnPosition) < minDistance)
                    {
                        return true; // Jos etäisyys on liian pieni, paikka on varattu
                    }
                }
            }
            return false; // Jos ei löytynyt autoa läheltä, paikka on vapaa
        }


    }
}
