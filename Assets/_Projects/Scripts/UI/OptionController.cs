using System.Collections.Generic;
using UnityEngine;

public class OptionController : MonoBehaviour
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
    }

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
    }
}
