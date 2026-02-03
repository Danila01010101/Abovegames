using System.Collections.Generic;
using UnityEngine;

public class TabBarIndicatorInitializer : MonoBehaviour
{
    [SerializeField] private FilterButtonController bannerScroll;
    [SerializeField] private Indicator indicator;
    [SerializeField] private RectTransform activeIndicator;
    [SerializeField] private List<RectTransform> dotPositions;
    
    private void Awake()
    {
        indicator.Initialize(bannerScroll, activeIndicator, dotPositions);
    }
}
