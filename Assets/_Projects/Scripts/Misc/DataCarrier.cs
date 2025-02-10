using PurrNet;
using UnityEngine;

public class DataCarrier : MonoBehaviour
{
    public string selectedCharacter, selectedMap;


    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        selectedCharacter = "Drax";
        selectedMap = "Playground";
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<DataCarrier>();
    }
}
