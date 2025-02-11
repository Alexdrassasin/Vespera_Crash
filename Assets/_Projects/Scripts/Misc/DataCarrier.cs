using PurrNet;
using System.Collections.Generic;
using UnityEngine;

public class DataCarrier : NetworkBehaviour
{
    public string selectedMap;
    [SerializeField] public SyncDictionary<PlayerID, string> selectedCharacter = new(true);

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        selectedMap = "Playground";
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<DataCarrier>();
    }

    [ServerRpc(requireOwnership:false)]
    public void CheckForDictionaryEntry(PlayerID playerID)
    {
        if (isOwner)
        {
            Debug.Log("isOwner");
        }
        else
        {
            Debug.Log("Not Owner");
        }

        if (!selectedCharacter.ContainsKey(playerID))
        {
            selectedCharacter.Add(playerID, "Drax");
        }
    }
}
