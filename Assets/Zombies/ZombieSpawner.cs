using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ZombieSpawner : MonoBehaviour
{
    public GameObject[] zombiePrefabs;
    private float spawnRadius = 10.0f;
    private int maxZombies = 10;
    private int currentZombies = 0;
    private int zombiesToSpawn = 1;
    private float offset = 3.0f;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("num zom prefabs" + zombiePrefabs.Length);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SpawnZombies();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GameObject.Find("Character").transform.position, spawnRadius);
    }

    public void SpawnZombies()
    {
        Vector3 spawnPosition = GameObject.Find("Character").transform.position;
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        spawnPosition.x += randomCircle.x;
        spawnPosition.z += randomCircle.y;

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            if (currentZombies < maxZombies)
            {
                
                Vector3 spawnOffset = new Vector3(Random.Range(-offset, offset), 0, Random.Range(-offset, offset));
                spawnOffset += spawnPosition;

                spawnOffset.y = Terrain.activeTerrain.SampleHeight(spawnOffset);

                GameObject newZombie = Instantiate(zombiePrefabs[Random.Range(0, zombiePrefabs.Length)], spawnOffset, Quaternion.identity); 
                newZombie.transform.parent = transform;
                newZombie.SetActive(true);
                currentZombies++;

            }
        }


    }

}

