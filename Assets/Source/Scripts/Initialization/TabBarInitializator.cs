using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabBarInitializator : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private TabBarData tabBarData;
    [Header ("BannerScroll")]
    [SerializeField] private BannerScroll bannerScroll;
    [SerializeField] private List<RectTransform> banners;
    [SerializeField] private RectTransform bannerTransform;
    [Header ("Indicators")]
    [SerializeField] private IndicatorInitializer bannerIndicator;
    [SerializeField] private TabBarIndicatorInitializer filtersIndicator;
    [Header("Card pool settings")]
    [SerializeField] private Transform cardsParent;
    [Header("ScrollView settings")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [Header("Components")]
    [SerializeField] private ContentGenerator contentGenerator;
    [SerializeField] private FilterButtonController filterButtonController;

    private List<IDisposable> disposables = new List<IDisposable>();
    
    public bool isUIElement() => true;

    public void Initialize()
    {
        bannerScroll.SetData(banners, bannerTransform, tabBarData.switchDuration, tabBarData.easeType,
            tabBarData.autoScrollInterval, tabBarData.swipeThreshold);
        bannerScroll.Initialize();
        bannerIndicator.SetBannerScroll(bannerScroll);
        bannerIndicator.Initialize();
        disposables.Add(bannerScroll);
        
        filtersIndicator.Initialize();
        
        ScrollViewMonitor scrollViewMonitor = new ScrollViewMonitor();
        scrollViewMonitor.SetData(scrollRect, content);
        
        OnlineImageLoader onlineImageLoader = new OnlineImageLoader();
        filterButtonController.Initialize();
        
        CardPool cardPool = new CardPool();
        cardPool.SetData(cardsParent, tabBarData.cardPrefab, tabBarData.initialPoolSize, tabBarData.expandPool);
        cardPool.Initialize();
        disposables.Add(cardPool);
        
        contentGenerator.SetData(onlineImageLoader, cardPool, scrollViewMonitor, tabBarData.cardData, tabBarData.edgePadding);
        contentGenerator.Initialize();
    }

    public void Dispose()
    {
        foreach (var item in disposables)
        {
            item.Dispose();
        }
    }
}