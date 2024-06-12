using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    // Define a list to keep track of connected players
    private List<PlayerInput> connectedPlayers = new List<PlayerInput>();

    // Define the maximum number of players
    public int maxPlayers = 4;

    // Define the player prefabs or game objects to instantiate
    public GameObject[] playerPrefabs;

    // Array to store spawn points
    public Transform[] spawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to player join and leave events
        PlayerInputManager.instance.onPlayerJoined += AssignPlayer;
        PlayerInputManager.instance.onPlayerLeft += RemovePlayer;
    }

    // Method to assign player to the next available slot
    void AssignPlayer(PlayerInput playerInput)
    {
        if (connectedPlayers.Count < maxPlayers)
        {
            // Add the player to the list of connected players
            connectedPlayers.Add(playerInput);

            // Determine the player index
            int playerIndex = connectedPlayers.IndexOf(playerInput);

            // Instantiate the corresponding player prefab based on the player's index
            GameObject playerPrefab = playerPrefabs[playerIndex];

            // Get a random spawn point from the list
            Transform spawnPoint = GetRandomSpawnPoint();

            // Instantiate the player prefab at the spawn point position
            Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Maximum players reached!");
            // Optionally handle the case when maximum players are reached
        }
    }

    // Method to remove player from the list when they leave
    void RemovePlayer(PlayerInput playerInput)
    {
        connectedPlayers.Remove(playerInput);
    }

    // Method to get a random spawn point from the list of spawn points
    Transform GetRandomSpawnPoint()
    {
        // Choose a random index within the range of spawnPoints array
        int randomIndex = Random.Range(0, spawnPoints.Length);

        // Return the random spawn point
        return spawnPoints[randomIndex];
    }
}
