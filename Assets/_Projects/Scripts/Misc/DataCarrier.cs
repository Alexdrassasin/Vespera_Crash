using PurrNet;
using System.Collections.Generic;
using UnityEngine;

public class DataCarrier : MonoBehaviour
{
    public string selectedMap;
    [SerializeField] public SyncDictionary<PlayerID, string> selectedCharacter = new();

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        selectedMap = "Playground";
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<DataCarrier>();
    }

    public void CheckForDictionaryEntry(PlayerID playerID)
    {
        if (!selectedCharacter.ContainsKey(playerID))
        {
            selectedCharacter.Add(playerID, "Drax");
        }
    }
}
