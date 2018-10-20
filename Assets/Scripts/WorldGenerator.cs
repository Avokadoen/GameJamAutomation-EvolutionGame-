using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class WorldGenerator : MonoBehaviour {

    [SerializeField] public string testSeed = "ItJustWorks";
    [Tooltip("Biome grid in biomes x biomes size")]
    [SerializeField] private int biomesGrid;
    [Tooltip("Size of each biome in tiles x tiles size")]
    [SerializeField] private int biomeSize;
    [SerializeField] private int biomeOddsFavour;
    [SerializeField] private int spawnObjectsOdds;

    public GameObject[] groundTiles;
    public GameObject[] defaultSpawnList;
    public GameObject[] animalsToSpawn;

    public Random rand;


    // Use this for initialization
    void Start () {
        rand = new Random(testSeed.GetHashCode());
        GenerateGround(rand);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void GenerateGround(Random rand)
    {
        int amountOfGroundTiles = groundTiles.Length;
        for (int biomeY = 0; biomeY < biomesGrid; biomeY++)
        {
            for(int biomeX = 0; biomeX < biomesGrid; biomeX++)
            {
                // Pick biome-type
                int biomeType = rand.Next(amountOfGroundTiles);
                // Set odds
                // Generate random biome


                for (int y = 0; y < biomeSize; y++)
                {
                    for (int x = 0; x < biomeSize; x++)
                    {
                        int spawnRoll = rand.Next(biomeOddsFavour);
                        Vector3 spawnPosition = new Vector3(biomeX * biomeSize + x, 0, biomeY * biomeSize + y);
                        if(spawnRoll % biomeOddsFavour != 0)
                        {
                            GameObject newObject = Instantiate(groundTiles[biomeType], spawnPosition, Quaternion.identity);
                            newObject.GetComponent<SpawnableList>().SetRandom(rand.Next());
                            if (rand.Next(spawnObjectsOdds) % spawnObjectsOdds == 0)
                            {
                                SpawnObjectsOnTile(rand, groundTiles[biomeType], spawnPosition);
                            }
                        }
                        else // This could also spawn the biome type ground tile, could make this different
                        {
                            int objectToSpawn = rand.Next(amountOfGroundTiles);
                            GameObject newObject = Instantiate(groundTiles[objectToSpawn], spawnPosition, Quaternion.identity);
                            newObject.GetComponent<SpawnableList>().SetRandom(rand.Next());
                            if (rand.Next(spawnObjectsOdds) % spawnObjectsOdds == 0)
                            {
                                SpawnObjectsOnTile(rand, groundTiles[objectToSpawn], spawnPosition);
                            }
                        }
                    }

                }
            }
        }
    }

    private void SpawnObjectsOnTile(Random rand, GameObject tileToSpawnOn, Vector3 spawnPos)
    {
        GameObject[] spawnList = tileToSpawnOn.GetComponent<SpawnableList>().spawnables;
        int spawnListSize = spawnList.Length;
        Vector3 newSpawnPos = new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.04f, spawnPos.z + 0.5f);
        if (spawnListSize > 0)
        {
            Instantiate<GameObject>(spawnList[rand.Next(spawnListSize)], newSpawnPos, Quaternion.identity);
        }
        else
        {
            Instantiate<GameObject>(defaultSpawnList[rand.Next(defaultSpawnList.Length)], newSpawnPos, Quaternion.identity);
        }
    }
}
