using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public interface IBannerIndicator
{
    void UpdateIndicator(int fromIndex, int toIndex);
}

public class BannerIndicator : MonoBehaviour, IBannerIndicator
{
    [SerializeField] private IBannerSwitcher bannerSwitcher;
    [SerializeField] private RectTransform activeIndicator;
    [SerializeField] private List<RectTransform> dotPositions = new List<RectTransform>();
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    
    private Vector3[] dotWorldPositions;
    private int lastBannerIndex = -1;
    private Tween currentTween;
    private bool isAnimating = false;

    private void Start()
    {
        ValidateComponents();
        CacheDotPositions();
        
        if (bannerSwitcher != null)
        {
            bannerSwitcher.OnBannerSwitching += OnBannerSwitching;
            bannerSwitcher.OnBannerSwitchComplete += OnBannerSwitchComplete;
            MoveIndicatorImmediately(bannerSwitcher.CurrentBannerIndex);
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
            if (rectTransform != null) dotPositions.Add(rectTransform);
        }
    }

    private void CacheDotPositions()
    {
        dotWorldPositions = new Vector3[dotPositions.Count];
        for (int i = 0; i < dotPositions.Count; i++)
        {
            if (dotPositions[i] != null)
            {
                dotWorldPositions[i] = dotPositions[i].position;
            }
        }
    }

    private void OnBannerSwitching(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex || activeIndicator == null || dotPositions.Count == 0) return;
        
        int fromDotIndex = Mathf.Clamp(fromIndex, 0, dotPositions.Count - 1);
        int toDotIndex = Mathf.Clamp(toIndex, 0, dotPositions.Count - 1);
        
        if (fromDotIndex != toDotIndex)
        {
            currentTween?.Kill();
            isAnimating = true;
            
            Vector3 startPos = dotWorldPositions[fromDotIndex];
            Vector3 endPos = dotWorldPositions[toDotIndex];
            
            currentTween = activeIndicator.DOMove(endPos, moveDuration)
                .SetEase(moveEase);
            
            if (dotPositions[toDotIndex] != null)
            {
                activeIndicator.DORotateQuaternion(dotPositions[toDotIndex].rotation, moveDuration);
                activeIndicator.DOScale(dotPositions[toDotIndex].localScale, moveDuration);
            }
        }
    }

    private void OnBannerSwitchComplete(int bannerIndex)
    {
        if (activeIndicator == null || dotPositions.Count == 0) return;
        
        isAnimating = false;
        lastBannerIndex = bannerIndex;
        
        int dotIndex = Mathf.Clamp(bannerIndex, 0, dotPositions.Count - 1);
        
        if (dotPositions[dotIndex] != null)
        {
            activeIndicator.position = dotWorldPositions[dotIndex];
            activeIndicator.rotation = dotPositions[dotIndex].rotation;
            activeIndicator.localScale = dotPositions[dotIndex].localScale;
        }
    }

    public void UpdateIndicator(int fromIndex, int toIndex)
    {
        OnBannerSwitching(fromIndex, toIndex);
    }

    private void MoveIndicatorImmediately(int bannerIndex)
    {
        if (bannerIndex == lastBannerIndex) return;
        if (activeIndicator == null || dotPositions.Count == 0) return;
        
        int dotIndex = Mathf.Clamp(bannerIndex, 0, dotPositions.Count - 1);
        
        if (dotPositions[dotIndex] != null)
        {
            activeIndicator.position = dotWorldPositions[dotIndex];
            activeIndicator.rotation = dotPositions[dotIndex].rotation;
            activeIndicator.localScale = dotPositions[dotIndex].localScale;
        }
        
        lastBannerIndex = bannerIndex;
    }

    private void OnDestroy()
    {
        if (bannerSwitcher != null)
        {
            bannerSwitcher.OnBannerSwitching -= OnBannerSwitching;
            bannerSwitcher.OnBannerSwitchComplete -= OnBannerSwitchComplete;
        }
        
        currentTween?.Kill();
    }

    private void OnValidate()
    {
        if (bannerSwitcher == null)
        {
            bannerSwitcher = GetComponentInParent<IBannerSwitcher>();
        }
        
        if (dotPositions.Count == 0)
        {
            CollectDotPositions();
        }
    }
}