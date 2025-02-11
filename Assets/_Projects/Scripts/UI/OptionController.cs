using PurrNet;
using PurrNet.Modules;
using System.Collections.Generic;
using UnityEngine;

public class OptionController : NetworkBehaviour
{
    public static GameObject currentHightLightFrame_Character, currentHightLightFrame_Map;
    public  GameObject HighlightFrame;
    public bool isCharacter, isMap;
    public bool isInitiallySelected;

    private void Start()
    {
        if (isInitiallySelected)
        {
            if (isCharacter)
            {
                currentHightLightFrame_Character = HighlightFrame;
            }

            if (isMap)
            {
                currentHightLightFrame_Map = HighlightFrame;
            }
        }
    }

    public void SelectCharacter()
    {
        if (currentHightLightFrame_Character)
        {
            currentHightLightFrame_Character.SetActive(false);
            currentHightLightFrame_Character.transform.parent.GetComponent<CanvasGroup>().alpha = 0.2f;
        }
        HighlightFrame.SetActive(true);
        currentHightLightFrame_Character = HighlightFrame;
        currentHightLightFrame_Character.transform.parent.GetComponent<CanvasGroup>().alpha = 1f;
        //InstanceHandler.GetInstance<DataCarrier>().selectedCharacter = transform.name.Contains("Male") ? "Drax" : "Xynos";
        var dataCarrier = InstanceHandler.GetInstance<DataCarrier>();

        if (dataCarrier == null)
        {
            Debug.LogError("DataCarrier instance is null.");
            return;
        }

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager instance is null.");
            return;
        }

        var localPlayer = networkManager.localPlayer;
        if (localPlayer == null)
        {
            Debug.LogError("Local player ID is null.");
            return;
        }

        dataCarrier.CheckForDictionaryEntry(localPlayer);
        UpdateSyncDictToServer(networkManager.localPlayer);
    }

    [ServerRpc(requireOwnership: false)]
    public void UpdateSyncDictToServer(PlayerID playerID)
    {
        InstanceHandler.GetInstance<DataCarrier>().selectedCharacter[playerID] = transform.name.Contains("Male") ? "Drax" : "Xynos";
    }


    [ObserversRpc(runLocally:true)]
    public void SelectMap()
    {
        if (currentHightLightFrame_Map)
        {
            currentHightLightFrame_Map.SetActive(false);
            currentHightLightFrame_Map.transform.parent.GetComponent<CanvasGroup>().alpha = 0.2f;
        }
        HighlightFrame.SetActive(true);
        currentHightLightFrame_Map = HighlightFrame;
        currentHightLightFrame_Map.transform.parent.GetComponent<CanvasGroup>().alpha = 1f;
        InstanceHandler.GetInstance<DataCarrier>().selectedMap = transform.name.Contains("Playground") ? "Playground" : "Industrial";
    }
}
