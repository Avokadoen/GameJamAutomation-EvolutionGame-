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

    public GameObject[] groundTiles;
    public GameObject[] objectsToSpawn;
    public GameObject[] animalsToSpawn;


    // Use this for initialization
    void Start () {
        Random rand = new Random(testSeed.GetHashCode());
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
                            GameObject.Instantiate<GameObject>(groundTiles[biomeType], spawnPosition, Quaternion.identity);
                        }
                        else // This could also spawn the biome type ground tile, could make this different
                        {
                            GameObject.Instantiate<GameObject>(groundTiles[rand.Next(amountOfGroundTiles)], spawnPosition, Quaternion.identity);
                        }
                    }

                }
            }
        }
    }
}
