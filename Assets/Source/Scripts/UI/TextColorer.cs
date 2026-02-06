using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextColorer : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> textsToColor = new List<TextMeshProUGUI>();
    [SerializeField] private Indicator indicator;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color selectedColor;
    
    private TextMeshProUGUI currentColoredText;

    private void Color(int index)
    {
        if (currentColoredText != null)
            currentColoredText.color = defaultColor;
        
        currentColoredText = textsToColor[index];
        currentColoredText.color = selectedColor;
    }

    private void OnEnable()
    {
        indicator.OnIndicatorStartMoving += Color;
    }

    private void OnDisable()
    {
        indicator.OnIndicatorStartMoving -= Color;
    }
}
