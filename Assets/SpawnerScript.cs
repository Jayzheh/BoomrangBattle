using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int playerIndex = Random.Range(0, playerPrefabs.Length);
            Instantiate(playerPrefabs[playerIndex], transform.position, Quaternion.identity);
        }
    }
}
