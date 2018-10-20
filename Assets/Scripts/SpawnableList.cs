using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class SpawnableList : MonoBehaviour {
    public GameObject[] spawnables;
    public bool canSpawnObjects;
    public float timeSinceLastSpawn = 0.0f;
    public int maxSpawnTimer = 200;
    public int minSpawnTimer = 50;
    private int spawnTimer;

    private Random rand;

    private void FixedUpdate()
    {
        if(canSpawnObjects == true)
        {
            timeSinceLastSpawn += Time.fixedDeltaTime;
            if (timeSinceLastSpawn > spawnTimer)
            {
                timeSinceLastSpawn = 0.0f;
                TryToSpawnObject();
            }
        }
    }

    private void TryToSpawnObject()
    {
        Vector3 rayCastPos = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z + 0.5f);
        Vector3 spawnPos = new Vector3(transform.position.x + 0.5f, transform.position.y + 0.15f, transform.position.z + 0.5f);

        if (!Physics.Raycast(rayCastPos, Vector3.up, maxDistance: 0.5f))
        {
            Instantiate(spawnables[rand.Next(spawnables.Length)], spawnPos, Quaternion.identity);
            return;
        }
    }

    public void SetRandom(int seed)
    {
        rand = new Random(seed);

        if (spawnables.Length >= 1)
        {
            canSpawnObjects = true;
        }
        spawnTimer = rand.Next(minSpawnTimer, maxSpawnTimer);
    }

}
