using PurrNet;
using System.Collections.Generic;
using UnityEngine;

public class DataCarrier : NetworkBehaviour
{
    public string selectedMap;
    [SerializeField] public SyncDictionary<PlayerID, string> selectedCharacter = new(true);

    private void Awake()
    {      
        selectedMap = "Playground";
        InstanceHandler.RegisterInstance(this);
        DontDestroyOnLoad(gameObject);     
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<DataCarrier>();
    }

    [ServerRpc(requireOwnership:false)]
    public void CheckForDictionaryEntry(PlayerID playerID)
    {
        if (!selectedCharacter.ContainsKey(playerID))
        {
            selectedCharacter.Add(playerID, "Drax");
        }
    }
}
