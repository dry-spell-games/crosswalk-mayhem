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

            // Ladataan Jalankulkija scenet
            GrandmaScene = (PackedScene)ResourceLoader.Load("res://Scenes/Pedestrian/Grandma.tscn");
            GirlScene = (PackedScene)ResourceLoader.Load("res://Scenes/Pedestrian/Girl.tscn");
            BoyScene = (PackedScene)ResourceLoader.Load("res://Scenes/Pedestrian/Boy.tscn");

            // Ladataan autojen scenet
            FamilyCarScene = (PackedScene)ResourceLoader.Load("res://Scenes/Car/FamilyCar.tscn");
            SportsCarScene = (PackedScene)ResourceLoader.Load("res://Scenes/Car/SportsCar.tscn");

            // Haetaan CollisionShape2d, jotta tarkastus voidaan laittaa päälle/pois
            collisionShape = GetNode<CollisionShape2D>("TrafficLights/Hitbox");

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

        private async void StartSpawningPedestrians()
        {
            while (true)
            {
                SpawnPedestrian();
                await ToSignal(GetTree().CreateTimer(SpawnRate), "timeout");
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
        // Asynkronoitu metodi, vaihtaa autojen liikennevalot.
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

            // Valitaan satunnaisesti, minkä tyyppinen olio luodaan
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

            // Spawn positio Pedestrian luokasta.
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
            if (IsSpawnPointOccupied(spawnPosition))
            {
                GD.Print("Spawnpoint on jo varattu, ei spawnata autoa.");
                return; // Älä spawnaa autoa, jos paikka on varattu
            }

            car.Initialize(spawnPosition);
            AddChild(car);
        }

        // Tarkistusmetodi, joka varmistaa ettei auto ole liian lähellä spawnpointtia
        private bool IsSpawnPointOccupied(Vector2 spawnPosition)
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
