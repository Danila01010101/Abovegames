using System.Collections.Generic;
using UnityEngine;

public class IndicatorInitializer : MonoBehaviour
{
    [SerializeField] private IndicatorScroll indicatorScroll;
    [SerializeField] private Indicator indicator;
    [SerializeField] private RectTransform activeIndicator;
    [SerializeField] private List<RectTransform> dotPositions;
    
    private void Start()
    {
        indicator.Initialize(indicatorScroll, activeIndicator, dotPositions);
    }
}