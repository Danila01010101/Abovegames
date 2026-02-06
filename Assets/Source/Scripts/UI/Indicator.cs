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
    [SerializeField] private Vector2 positionOffset = Vector2.zero; // Offset позиции
    
    [Header("Size Settings")]
    [SerializeField] private bool matchTargetWidth = false; // Будет ли индикатор принимать ширину точки
    [SerializeField] private bool matchTargetHeight = false; // Будет ли индикатор принимать высоту точки
    
    [Header("Size Animation")]
    [SerializeField] private float sizeAnimationDuration = 0.3f;
    [SerializeField] private Ease sizeEase = Ease.OutBack;
    
    private Vector2[] dotAnchoredPositions;
    private Vector2[] dotLocalPositions;
    private Vector2[] dotSizes; // Добавляем массив размеров точек
    private int lastBannerIndex = -1;
    private Tween currentTween;
    private Tween sizeTween;
    private bool isAnimating = false;
    private IIndicatorSwitcher indicatorSwitcher;
    private RectTransform activeIndicator;
    private List<RectTransform> dotPositions;
    private RectTransform indicatorRectTransform;
    private Vector2 initialIndicatorSize; // Изначальный размер индикатора

    public void Initialize(IIndicatorSwitcher indicatorSwitcher, RectTransform activeIndicator, List<RectTransform> dotPositions)
    {
        this.indicatorSwitcher = indicatorSwitcher; 
        this.activeIndicator = activeIndicator;
        this.dotPositions = dotPositions;
        this.indicatorRectTransform = activeIndicator;
        this.initialIndicatorSize = activeIndicator.sizeDelta; // Сохраняем изначальный размер
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
        dotSizes = new Vector2[dotPositions.Count]; // Инициализируем массив размеров
        
        for (int i = 0; i < dotPositions.Count; i++)
        {
            if (dotPositions[i] != null)
            {
                dotAnchoredPositions[i] = dotPositions[i].anchoredPosition + positionOffset;
                dotLocalPositions[i] = dotPositions[i].localPosition + (Vector3)positionOffset;
                dotSizes[i] = GetWorldRect(dotPositions[i]).size; // Сохраняем размер точки
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
            
            // Анимация размера, если нужно
            if (matchTargetWidth || matchTargetHeight)
            {
                sizeTween = indicatorRectTransform.DOSizeDelta(targetSize, sizeAnimationDuration)
                    .SetEase(sizeEase)
                    .SetDelay(0.1f); // Небольшая задержка перед анимацией размера
            }
            
            // Анимация поворота и масштаба
            if (dotPositions[toDotIndex] != null)
            {
                activeIndicator.DORotateQuaternion(dotPositions[toDotIndex].rotation, moveDuration);
                activeIndicator.DOScale(dotPositions[toDotIndex].localScale, moveDuration);
            }
        }
    }
    
    private void OnSizeAnimationComplete(int dotIndex)
    {
        // Гарантируем точный размер после анимации
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
            // Устанавливаем позицию с учетом offset
            if (useAnchoredPosition)
            {
                indicatorRectTransform.anchoredPosition = GetTargetPosition(dotIndex);
            }
            else
            {
                activeIndicator.localPosition = GetTargetPosition(dotIndex);
            }
            
            // Устанавливаем размер, если нужно
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
            // Позиция
            if (useAnchoredPosition)
            {
                indicatorRectTransform.anchoredPosition = GetTargetPosition(dotIndex);
            }
            else
            {
                activeIndicator.localPosition = GetTargetPosition(dotIndex);
            }
            
            // Размер
            if (matchTargetWidth || matchTargetHeight)
            {
                indicatorRectTransform.sizeDelta = GetTargetSize(dotIndex);
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
    
    // Метод для обновления offset в runtime
    public void SetPositionOffset(Vector2 newOffset)
    {
        positionOffset = newOffset;
        CacheDotPositions();
        
        if (lastBannerIndex >= 0)
        {
            MoveIndicatorImmediately(lastBannerIndex);
        }
    }
    
    // Метод для включения/выключения match size в runtime
    public void SetMatchSize(bool matchWidth, bool matchHeight)
    {
        matchTargetWidth = matchWidth;
        matchTargetHeight = matchHeight;
        
        if (lastBannerIndex >= 0 && lastBannerIndex < dotPositions.Count)
        {
            // Немедленно применяем изменения размера
            if (matchTargetWidth || matchTargetHeight)
            {
                indicatorRectTransform.sizeDelta = GetTargetSize(lastBannerIndex);
            }
            else
            {
                indicatorRectTransform.sizeDelta = initialIndicatorSize;
            }
        }
    }
    
    // Метод для получения текущих настроек размера
    public Vector2 GetCurrentSizeSettings()
    {
        return new Vector2(
            matchTargetWidth ? 1 : 0,
            matchTargetHeight ? 1 : 0
        );
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
    
    #if UNITY_EDITOR
    // Визуализация offset в Editor
    private void OnDrawGizmosSelected()
    {
        if (dotPositions == null || dotPositions.Count == 0) return;
        
        foreach (var dot in dotPositions)
        {
            if (dot != null)
            {
                // Показываем позицию с offset
                Vector3 originalPos = dot.position;
                Vector3 offsetPos = originalPos + (Vector3)positionOffset;
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(originalPos, 5f);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(offsetPos, 3f);
                
                Gizmos.color = Color.red;
                Gizmos.DrawLine(originalPos, offsetPos);
            }
        }
    }
    #endif
}