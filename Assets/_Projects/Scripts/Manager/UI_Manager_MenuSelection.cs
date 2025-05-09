using PurrNet;
using System.Collections;
using UnityEngine;

public class UI_Manager_MenuSelection : NetworkBehaviour
{
    [SerializeField] private GameObject clientView;

    [Header("PANELS")]
    [Tooltip("The UI Panel that holds the character window tab")]
    public GameObject PanelCharacter;
    [Tooltip("The UI Panel that holds the map window tab")]
    public GameObject PanelMap;
    [Tooltip("The UI Panel that holds the weapon window tab")]
    public GameObject PanelWeapon;
   

    // highlights in settings screen
    [Header("SETTINGS SCREEN")]
    [Tooltip("Highlight Image for when character Tab is selected in Settings")]
    public GameObject lineCharacter;
    [Tooltip("Highlight Image for when map Tab is selected in Settings")]
    public GameObject lineMap;
    [Tooltip("Highlight Image for when weaponTab is selected in Settings")]
    public GameObject lineWeapon;


    void DisablePanels()
    {
        PanelMap.SetActive(false);
        PanelWeapon.SetActive(false);
        PanelCharacter.SetActive(false);

        lineCharacter.SetActive(false);
        lineMap.SetActive(false);
        lineWeapon.SetActive(false);
    }

    private void Start()
    {
        CharacterPanel();
        StartCoroutine(ForceResetDataCarrier());
    }

    
    IEnumerator ForceResetDataCarrier()
    {
        DataCarrier dataCarrier;
        while (!InstanceHandler.GetInstance<DataCarrier>())
        {
            yield return null;
        }

        dataCarrier = InstanceHandler.GetInstance<DataCarrier>();
        dataCarrier.selectedMap = "Playground";
        dataCarrier.maxRound = 3;

        while (!networkManager)
        {
            yield return null;
        }

        if (dataCarrier.selectedCharacter.ContainsKey(networkManager.localPlayer))
        {
            UpdateSyncDictToServer(networkManager.localPlayer);
        }
    }

    [ServerRpc(requireOwnership: false)]
    public void UpdateSyncDictToServer(PlayerID playerID)
    {
        InstanceHandler.GetInstance<DataCarrier>().selectedCharacter[playerID] = "Drax";
    }


    public void CharacterPanel()
    {
        DisablePanels();
        PanelCharacter.SetActive(true);
        lineCharacter.SetActive(true);
    }

    public void MapPanel()
    {
        DisablePanels();
        PanelMap.SetActive(true);
        lineMap.SetActive(true);

        if (!isServer)
        {
            clientView.SetActive(true);
        }
    }

    public void WeaponPanel()
    {
        DisablePanels();
        PanelWeapon.SetActive(true);
        lineWeapon.SetActive(true);
    }


}
