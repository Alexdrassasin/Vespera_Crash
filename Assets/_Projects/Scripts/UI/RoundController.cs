using PurrNet;
using UnityEngine;
using UnityEngine.UI;

public class RoundController : NetworkBehaviour
{
    public static RoundController currentSelectedButton;

    [SerializeField] private int Round;
    [SerializeField] private string activeHexCode, deactiveHexCode;
    public Image BackgroundImage;
    [SerializeField] private bool isInitialSelectedButton;

    private void Start()
    {
        if (isInitialSelectedButton)
        {
            currentSelectedButton = this;
            ChangeColor(BackgroundImage, activeHexCode);
        }
    }

    [ObserversRpc(runLocally: true)]
    public void SelectRound()
    {
        if (currentSelectedButton != this)
        {
            ChangeColor(currentSelectedButton.BackgroundImage,deactiveHexCode); 
        }

        currentSelectedButton = this;
        ChangeColor(BackgroundImage, activeHexCode);
        InstanceHandler.GetInstance<DataCarrier>().maxRound = Round;
    }

    public void ChangeColor(Image targetImage, string hexCode)
    {
        string hexColorCode = hexCode; // Update the hex code

        Color color; // Store the color to set

        // Attempt to convert the hex code to a Color
        if (ColorUtility.TryParseHtmlString(hexColorCode, out color))
        {
            // Set the image's color
            targetImage.color = color;
        }
        else
        {
            Debug.LogError("Invalid hex color code: " + hexColorCode);
        }
    }
}
