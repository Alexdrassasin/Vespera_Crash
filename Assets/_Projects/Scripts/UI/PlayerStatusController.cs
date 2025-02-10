using System.Data;
using TMPro;
using UnityEngine;

public class PlayerStatusController : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public GameObject ready, notReady;

    public void UpdateStatus(string name, bool status)
    {
        playerName.text = name;
        if (status)
        {
            ready.SetActive(true);
            notReady.SetActive(false);
        }
        else
        {
            ready.SetActive(false);
            notReady.SetActive(true);
        }
    }
}
