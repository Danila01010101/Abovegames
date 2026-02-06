using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class Indicator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    
    [Header("Position Settings")]
    [SerializeField] private bool useAnchoredPosition = true;
    [SerializeField] private Vector2 positionOffset = Vector2.zero;
    
    [Header("Size Settings")]
    [SerializeField] private bool matchTargetWidth = false;
    [SerializeField] private bool matchTargetHeight = false;
    
    [Header("Size Animation")]
    [SerializeField] private float sizeAnimationDuration = 0.3f;
    [SerializeField] private Ease sizeEase = Ease.OutBack;
    
    private Vector2[] dotAnchoredPositions;
    private Vector2[] dotLocalPositions;
    private Vector2[] dotSizes;
    private int lastBannerIndex = -1;
    private Tween currentTween;
    private Tween sizeTween;
    private bool isAnimating = false;
    private IIndicatorSwitcher indicatorSwitcher;
    private RectTransform activeIndicator;
    private List<RectTransform> dotPositions;
    private RectTransform indicatorRectTransform;
    private Vector2 initialIndicatorSize;

    public void Initialize(IIndicatorSwitcher indicatorSwitcher, RectTransform activeIndicator, List<RectTransform> dotPositions)
    {
        this.indicatorSwitcher = indicatorSwitcher; 
        this.activeIndicator = activeIndicator;
        this.dotPositions = dotPositions;
        indicatorRectTransform = activeIndicator;
        initialIndicatorSize = activeIndicator.sizeDelta;
        
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
        dotSizes = new Vector2[dotPositions.Count];
        
        for (int i = 0; i < dotPositions.Count; i++)
        {
            if (dotPositions[i] != null)
            {
                dotAnchoredPositions[i] = dotPositions[i].anchoredPosition + positionOffset;
                dotLocalPositions[i] = dotPositions[i].localPosition + (Vector3)positionOffset;
                dotSizes[i] = GetWorldRect(dotPositions[i]).size;
            }
        }
    }
    
    private static Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        return new Rect(
            corners[0].x,
            corners[0].y,
            corners[2].x - corners[0].x,
            corners[2].y - corners[0].y
        );
    }
    
    private Vector2 GetTargetPosition(int index)
    {
        if (index < 0 || index >= dotPositions.Count) return Vector2.zero;
        
        if (useAnchoredPosition)
        {
            return dotAnchoredPositions[index];
        }
        else
        {
            return dotLocalPositions[index];
        }
    }
    
    private Vector2 GetTargetSize(int index)
    {
        if (index < 0 || index >= dotSizes.Length) return initialIndicatorSize;
        
        Vector2 targetSize = initialIndicatorSize;
        
        if (matchTargetWidth)
        {
            targetSize.x = dotSizes[index].x;
        }
        
        if (matchTargetHeight)
        {
            targetSize.y = dotSizes[index].y;
        }
        
        return targetSize;
    }

    private void OnIndicatorSwitching(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex || activeIndicator == null || dotPositions.Count == 0) return;
        
        int fromDotIndex = Mathf.Clamp(fromIndex, 0, dotPositions.Count - 1);
        int toDotIndex = Mathf.Clamp(toIndex, 0, dotPositions.Count - 1);
        
        if (fromDotIndex != toDotIndex)
        {
            currentTween?.Kill();
            sizeTween?.Kill();
            isAnimating = true;
            
            Vector2 targetPosition = GetTargetPosition(toDotIndex);
            Vector2 targetSize = GetTargetSize(toDotIndex);
            
            if (useAnchoredPosition)
            {
                currentTween = indicatorRectTransform.DOAnchorPos(targetPosition, moveDuration)
                    .SetEase(moveEase)
                    .OnComplete(() => OnSizeAnimationComplete(toDotIndex));
            }
            else
            {
                currentTween = indicatorRectTransform.DOLocalMove(targetPosition, moveDuration)
                    .SetEase(moveEase)
                    .OnComplete(() => OnSizeAnimationComplete(toDotIndex));
            }
            
            if (matchTargetWidth || matchTargetHeight)
            {
                sizeTween = indicatorRectTransform.DOSizeDelta(targetSize, sizeAnimationDuration)
                    .SetEase(sizeEase)
                    .SetDelay(0.1f);
            }
            
            if (dotPositions[toDotIndex] != null)
            {
                activeIndicator.DORotateQuaternion(dotPositions[toDotIndex].rotation, moveDuration);
                activeIndicator.DOScale(dotPositions[toDotIndex].localScale, moveDuration);
            }
        }
    }
    
    private void OnSizeAnimationComplete(int dotIndex)
    {
        if (matchTargetWidth || matchTargetHeight)
        {
            indicatorRectTransform.sizeDelta = GetTargetSize(dotIndex);
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
                indicatorRectTransform.anchoredPosition = GetTargetPosition(dotIndex);
            }
            else
            {
                activeIndicator.localPosition = GetTargetPosition(dotIndex);
            }
            
            if (matchTargetWidth || matchTargetHeight)
            {
                indicatorRectTransform.sizeDelta = GetTargetSize(dotIndex);
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
                indicatorRectTransform.anchoredPosition = GetTargetPosition(dotIndex);
            }
            else
            {
                activeIndicator.localPosition = GetTargetPosition(dotIndex);
            }
            
            if (matchTargetWidth || matchTargetHeight)
            {
                indicatorRectTransform.sizeDelta = GetTargetSize(dotIndex);
            }
            
            activeIndicator.rotation = dotPositions[dotIndex].rotation;
            activeIndicator.localScale = dotPositions[dotIndex].localScale;
        }
        
        lastBannerIndex = bannerIndex;
    }

    private void Update()
    {
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
        sizeTween?.Kill();
    }
}