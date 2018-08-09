using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellSpawner : MonoBehaviour {

    public float timeBetweenSpawns;

    public float spawnDistance;

    public Cell[] cellPrefabs;
    public int MaxNumberOfCells;


    private float timeSinceLastSpawn;
    private int numOfCells;

    private void Awake()
    {
        numOfCells = 0;
    }

    void FixedUpdate()
    {
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= timeBetweenSpawns && numOfCells< MaxNumberOfCells)
        {
            timeSinceLastSpawn -= timeBetweenSpawns;
            SpawnCell();
            numOfCells++;
        }
    }


    void SpawnCell()
    {
        Cell prefab = cellPrefabs[Random.Range(0, cellPrefabs.Length)];
        Cell spawn = Instantiate<Cell>(prefab);
        spawn.transform.localPosition = Random.onUnitSphere * spawnDistance;
    }
}
