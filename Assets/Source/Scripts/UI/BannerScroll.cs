using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class BannerScroll : MonoBehaviour, IIndicatorSwitcher, IBeginDragHandler, IDragHandler, IEndDragHandler, IDisposable
{
    private List<RectTransform> banners;
    private RectTransform rectTransform;
    private float switchDuration = 0.5f;
    private Ease easeType = Ease.OutCubic;
    private float autoScrollInterval = 5f;
    private float swipeThreshold = 100f;
    private int currentBannerIndex = 0;
    private float bannerWidth;
    private Vector2 dragStartPosition;
    private bool isDragging = false;
    private Tween currentTween;
    private float timeSinceLastScroll = 0f;
    private Vector3[] originalPositions;
    private Vector3 dragOffset;
    private bool isSwitching = false;

    public event System.Action<int, int> OnBannerSwitching;
    public event System.Action<int> OnBannerSwitchComplete;
    public int CurrentBannerIndex => currentBannerIndex;
    public int BannerCount => banners?.Count ?? 0;

    public void SetData(List<RectTransform> banners, RectTransform rectTransform, float switchDuration, Ease easeType, float autoScrollInterval, float swipeThreshold)
    {
        this.banners = banners;
        this.switchDuration = switchDuration;
        this.easeType = easeType;
        this.autoScrollInterval = autoScrollInterval;
        this.swipeThreshold = swipeThreshold;
        this.rectTransform = rectTransform;
    }

    public void Initialize()
    {
        if (banners == null || banners.Count == 0)
        {
            throw new NullReferenceException("No banners set in TabBarInitializator.");
        }

        if (banners.Count == 0) return;

        bannerWidth = banners[0].rect.width;
        originalPositions = new Vector3[banners.Count];
        
        for (int i = 0; i < banners.Count; i++)
        {
            originalPositions[i] = new Vector3(i * bannerWidth, 0, 0);
            banners[i].anchoredPosition = originalPositions[i];
        }

        SetupInput();
        StartAutoScroll();
    }

    private void SetupInput()
    {
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }

    public void Update()
    {
        if (banners != null && (isDragging || isSwitching || banners.Count <= 1)) return;

        timeSinceLastScroll += Time.deltaTime;
        if (timeSinceLastScroll >= autoScrollInterval)
        {
            Next();
            timeSinceLastScroll = 0f;
        }
    }

    public void Next()
    {
        if (banners.Count <= 1 || isSwitching) return;
        int nextIndex = (currentBannerIndex + 1) % banners.Count;
        SwitchToBanner(nextIndex);
    }

    public void Previous()
    {
        if (banners.Count <= 1 || isSwitching) return;
        int prevIndex = currentBannerIndex - 1;
        if (prevIndex < 0) prevIndex = banners.Count - 1;
        SwitchToBanner(prevIndex);
    }

    public void GoToBanner(int index)
    {
        if (index >= 0 && index < banners.Count && index != currentBannerIndex && !isSwitching)
        {
            SwitchToBanner(index);
        }
    }

    private void SwitchToBanner(int targetIndex)
    {
        if (targetIndex == currentBannerIndex || targetIndex < 0 || targetIndex >= banners.Count) return;
        
        isSwitching = true;
        OnBannerSwitching?.Invoke(currentBannerIndex, targetIndex);
        
        currentTween?.Kill();

        float offset = (currentBannerIndex - targetIndex) * bannerWidth;

        currentTween = DOTween.To(
            () => 0f,
            progress =>
            {
                dragOffset.x = Mathf.Lerp(0, offset, progress);
                UpdateBannerPositions();
            },
            1f,
            switchDuration
        ).SetEase(easeType).OnComplete(() => 
        {
            currentBannerIndex = targetIndex;
            ResetBannerPositions();
            isSwitching = false;
            OnBannerSwitchComplete?.Invoke(currentBannerIndex);
        });
    }

    private void UpdateBannerPositions()
    {
        for (int i = 0; i < banners.Count; i++)
        {
            banners[i].anchoredPosition = originalPositions[i] + dragOffset;
        }
    }

    private void ResetBannerPositions()
    {
        for (int i = 0; i < banners.Count; i++)
        {
            originalPositions[i] = new Vector3((i - currentBannerIndex) * bannerWidth, 0, 0);
            banners[i].anchoredPosition = originalPositions[i];
        }
        dragOffset = Vector3.zero;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isSwitching) return;
        
        isDragging = true;
        dragStartPosition = eventData.position;
        currentTween?.Kill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isSwitching) return;
        
        float dragDelta = eventData.position.x - dragStartPosition.x;
        dragOffset.x = dragDelta * 0.01f;
        UpdateBannerPositions();
        dragStartPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || isSwitching) return;
        
        isDragging = false;
        float dragDelta = eventData.position.x - dragStartPosition.x;
        
        if (Mathf.Abs(dragDelta) > swipeThreshold)
        {
            if (dragDelta > 0)
            {
                Previous();
            }
            else
            {
                Next();
            }
        }
        else
        {
            SwitchToBanner(currentBannerIndex);
        }
    }

    public void Dispose()
    {
        currentTween?.Kill();
    }

    private void StartAutoScroll()
    {
        timeSinceLastScroll = 0f;
    }
}