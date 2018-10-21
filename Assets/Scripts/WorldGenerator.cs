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
    [SerializeField] private int spawnAgentOdds;

    public GameObject[] groundTiles;
    public GameObject[] defaultSpawnList;
    public GameObject[] agentsToSpawn;

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
                            else if (rand.Next(spawnAgentOdds) % spawnAgentOdds == 0)
                            {
                                SpawnAgentOnTile(rand, spawnPosition);
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
                            else if (rand.Next(spawnAgentOdds) % spawnAgentOdds == 0)
                            {
                                SpawnAgentOnTile(rand, spawnPosition);
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
        Vector3 newSpawnPos = new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.15f, spawnPos.z + 0.5f);
        if (spawnListSize > 0)
        {
            Instantiate(spawnList[rand.Next(spawnListSize)], newSpawnPos, Quaternion.identity);
        }
        else
        {
            Instantiate(defaultSpawnList[rand.Next(defaultSpawnList.Length)], newSpawnPos, Quaternion.identity);
        }
    }

    private void SpawnAgentOnTile(Random rand, Vector3 spawnPos)
    {
        GameObject agentToSpawn = Instantiate(agentsToSpawn[rand.Next(agentsToSpawn.Length)], spawnPos, Quaternion.identity);
        Agent agent = agentToSpawn.GetComponent<Agent>();
        //agent.state.fed = 1.0f;
        agent.state.metabolism    = (float)(rand.Next(1, 10) / 10.0f);
        agent.state.wakeFullness  = (float)(rand.Next(1, 10) / 10.0f);
        agent.state.maxMoveSpeed  = (float)(rand.Next(20, 100) / 10.0f);
        agent.state.maxStamina    = (float)(rand.Next(20, 100) / 10.0f);
        agent.state.stamina       = (float)(agent.state.maxStamina/2.0f);
        agent.state.perception    = (float)(rand.Next(50, 200) / 10.0f);
        
    }
}
