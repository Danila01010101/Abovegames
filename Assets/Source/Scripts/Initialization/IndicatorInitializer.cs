using System.Collections.Generic;
using UnityEngine;

public class IndicatorInitializer : MonoBehaviour
{
    [SerializeField] private Indicator indicator;
    [SerializeField] private RectTransform activeIndicator;
    [SerializeField] private List<RectTransform> dotPositions;
    
    private BannerScroll bannerScroll;

    public void SetBannerScroll(BannerScroll bannerScroll)
    {
        this.bannerScroll = bannerScroll;
    }
    
    public void Initialize()
    {
        indicator.Initialize(bannerScroll, activeIndicator, dotPositions);
    }
}