using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class Indicator : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    [SerializeField] private bool useAnchoredPosition = true; // Новая опция
    
    private Vector2[] dotAnchoredPositions;
    private Vector2[] dotLocalPositions;
    private int lastBannerIndex = -1;
    private Tween currentTween;
    private bool isAnimating = false;
    private IIndicatorSwitcher indicatorSwitcher;
    private RectTransform activeIndicator;
    private List<RectTransform> dotPositions;
    private RectTransform indicatorRectTransform;

    public void Initialize(IIndicatorSwitcher indicatorSwitcher, RectTransform activeIndicator, List<RectTransform> dotPositions)
    {
        this.indicatorSwitcher = indicatorSwitcher; 
        this.activeIndicator = activeIndicator;
        this.dotPositions = dotPositions;
        this.indicatorRectTransform = activeIndicator;
    }

    private void Start()
    {
        ValidateComponents();
        CacheDotPositions();
        
        if (indicatorSwitcher != null)
        {
            indicatorSwitcher.OnBannerSwitching += OnIndicatorSwitching;
            indicatorSwitcher.OnBannerSwitchComplete += OnIndicatorSwitchComplete;
            MoveIndicatorImmediately(indicatorSwitcher.CurrentBannerIndex);
        }
    }

    private void ValidateComponents()
    {
        if (activeIndicator == null) return;

        if (dotPositions.Count == 0)
        {
            CollectDotPositions();
        }
    }

    private void CollectDotPositions()
    {
        dotPositions.Clear();
        foreach (Transform child in transform)
        {
            var rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null) 
            {
                dotPositions.Add(rectTransform);
            }
        }
    }

    private void CacheDotPositions()
    {
        if (dotPositions == null || dotPositions.Count == 0) return;
        
        dotAnchoredPositions = new Vector2[dotPositions.Count];
        dotLocalPositions = new Vector2[dotPositions.Count];
        
        for (int i = 0; i < dotPositions.Count; i++)
        {
            if (dotPositions[i] != null)
            {
                dotAnchoredPositions[i] = dotPositions[i].anchoredPosition;
                dotLocalPositions[i] = dotPositions[i].localPosition;
            }
        }
    }

    private void OnIndicatorSwitching(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex || activeIndicator == null || dotPositions.Count == 0) return;
        
        int fromDotIndex = Mathf.Clamp(fromIndex, 0, dotPositions.Count - 1);
        int toDotIndex = Mathf.Clamp(toIndex, 0, dotPositions.Count - 1);
        
        if (fromDotIndex != toDotIndex)
        {
            currentTween?.Kill();
            isAnimating = true;
            
            if (useAnchoredPosition)
            {
                // Используем anchoredPosition для квадратного экрана
                Vector2 startPos = dotAnchoredPositions[fromDotIndex];
                Vector2 endPos = dotAnchoredPositions[toDotIndex];
                
                currentTween = indicatorRectTransform.DOAnchorPos(endPos, moveDuration)
                    .SetEase(moveEase);
            }
            else
            {
                // Используем localPosition
                Vector2 startPos = dotLocalPositions[fromDotIndex];
                Vector2 endPos = dotLocalPositions[toDotIndex];
                
                currentTween = indicatorRectTransform.DOLocalMove(endPos, moveDuration)
                    .SetEase(moveEase);
            }
            
            if (dotPositions[toDotIndex] != null)
            {
                activeIndicator.DORotateQuaternion(dotPositions[toDotIndex].rotation, moveDuration);
                activeIndicator.DOScale(dotPositions[toDotIndex].localScale, moveDuration);
            }
        }
    }

    private void OnIndicatorSwitchComplete(int bannerIndex)
    {
        if (activeIndicator == null || dotPositions.Count == 0) return;
        
        isAnimating = false;
        lastBannerIndex = bannerIndex;
        
        int dotIndex = Mathf.Clamp(bannerIndex, 0, dotPositions.Count - 1);
        
        if (dotPositions[dotIndex] != null)
        {
            if (useAnchoredPosition)
            {
                indicatorRectTransform.anchoredPosition = dotAnchoredPositions[dotIndex];
            }
            else
            {
                activeIndicator.localPosition = dotLocalPositions[dotIndex];
            }
            
            activeIndicator.rotation = dotPositions[dotIndex].rotation;
            activeIndicator.localScale = dotPositions[dotIndex].localScale;
        }
    }

    public void UpdateIndicator(int fromIndex, int toIndex)
    {
        if (!isAnimating)
        {
            OnIndicatorSwitching(fromIndex, toIndex);
        }
    }

    private void MoveIndicatorImmediately(int bannerIndex)
    {
        if (bannerIndex == lastBannerIndex) return;
        if (activeIndicator == null || dotPositions.Count == 0) return;
        
        int dotIndex = Mathf.Clamp(bannerIndex, 0, dotPositions.Count - 1);
        
        if (dotPositions[dotIndex] != null)
        {
            if (useAnchoredPosition)
            {
                indicatorRectTransform.anchoredPosition = dotAnchoredPositions[dotIndex];
            }
            else
            {
                activeIndicator.localPosition = dotLocalPositions[dotIndex];
            }
            
            activeIndicator.rotation = dotPositions[dotIndex].rotation;
            activeIndicator.localScale = dotPositions[dotIndex].localScale;
        }
        
        lastBannerIndex = bannerIndex;
    }

    // Добавим метод для обновления позиций при изменении размера экрана
    private void Update()
    {
        // Проверяем изменение размера экрана
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            RefreshPositions();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }
    
    private int lastScreenWidth;
    private int lastScreenHeight;
    
    public void RefreshPositions()
    {
        CacheDotPositions();
        
        if (lastBannerIndex >= 0 && lastBannerIndex < dotPositions.Count)
        {
            MoveIndicatorImmediately(lastBannerIndex);
        }
    }

    private void OnDestroy()
    {
        if (indicatorSwitcher != null)
        {
            indicatorSwitcher.OnBannerSwitching -= OnIndicatorSwitching;
            indicatorSwitcher.OnBannerSwitchComplete -= OnIndicatorSwitchComplete;
        }
        
        currentTween?.Kill();
    }
}