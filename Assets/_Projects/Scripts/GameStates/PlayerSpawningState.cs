using DG.Tweening;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawningState : StateNode
{
    [SerializeField] private PlayerHealth playerPrefab;
    [SerializeField] private List<Transform> spawnPoints = new ();

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);    

        if (!asServer)
        {
           
            return;
        }

        DespawnPlayers();

        InstanceHandler.GetInstance<ObjectPoolManager>().ResetFracture();
        InstanceHandler.GetInstance<ObjectPoolManager>().TurnAllBulletHolesOff();
        Debug.Log("SpawnPlayers");     

        var spawnedPlayers = SpawnPlayers();

        machine.Next(spawnedPlayers);
    }

   

    private List<PlayerHealth> SpawnPlayers()
    {
        var spawnedPlayers = new List<PlayerHealth>();

        // Shuffle the spawn points to randomize the spawning sequence
        Shuffle(spawnPoints);

        int currentSpawnIndex = 0;
        foreach (var player in networkManager.players)
        {
            if (currentSpawnIndex >= spawnPoints.Count)
            {
                Debug.LogWarning("Not enough spawn points for all players!");
                break;
            }

            var spawnPoint = spawnPoints[currentSpawnIndex];
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            newPlayer.GiveOwnership(player);
            spawnedPlayers.Add(newPlayer);
            currentSpawnIndex++;
        }

        return spawnedPlayers;
    }

    // Fisher-Yates shuffle algorithm to randomize the spawn points
    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int r = Random.Range(i, n);
            T value = list[r];
            list[r] = list[i];
            list[i] = value;
        }
    }

    private void DespawnPlayers()
    {
        var allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            Destroy(player.gameObject);
        }
    }
}


