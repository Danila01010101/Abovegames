using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewMonitor : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    
    private List<Action<float>> scrollListeners = new List<Action<float>>();
    
    public ScrollRect ScrollRect => scrollRect;
    
    private void Start()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
        else
        {
            Debug.LogError("ScrollRect is not assigned!");
        }
    }
    
    private void OnScrollValueChanged(Vector2 scrollPosition)
    {
        // В вертикальном ScrollRect Y=1 - вверху, Y=0 - внизу
        float normalizedPosition = scrollRect.vertical 
            ? 1f - scrollPosition.y 
            : scrollPosition.x;
            
        NotifyScrollListeners(normalizedPosition);
    }
    
    public void AddScrollListener(Action<float> listener)
    {
        if (!scrollListeners.Contains(listener))
        {
            scrollListeners.Add(listener);
        }
    }
    
    public void RemoveScrollListener(Action<float> listener)
    {
        scrollListeners.Remove(listener);
    }
    
    private void NotifyScrollListeners(float normalizedPosition)
    {
        foreach (var listener in scrollListeners)
        {
            listener?.Invoke(normalizedPosition);
        }
    }
    
    public float GetScrollPosition()
    {
        if (scrollRect == null) return 0f;
        return scrollRect.vertical ? 1f - scrollRect.verticalNormalizedPosition : scrollRect.horizontalNormalizedPosition;
    }
    
    public float GetVisibleStartPosition()
    {
        if (scrollRect == null || content == null || scrollRect.viewport == null) 
            return 0f;
        
        float scrollValue = GetScrollPosition();
        float contentHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        
        // Если контент меньше viewport, всегда показываем с начала
        if (contentHeight <= viewportHeight) 
            return 0f;
        
        return Mathf.Clamp(scrollValue * (contentHeight - viewportHeight), 0f, contentHeight - viewportHeight);
    }
    
    public float GetVisibleEndPosition()
    {
        if (scrollRect == null || content == null || scrollRect.viewport == null) 
            return 0f;
        
        float startPosition = GetVisibleStartPosition();
        float viewportHeight = scrollRect.viewport.rect.height;
        float contentHeight = content.rect.height;
        
        return Mathf.Min(startPosition + viewportHeight, contentHeight);
    }
    
    public float GetViewportHeight()
    {
        if (scrollRect == null || scrollRect.viewport == null) 
            return 0f;
        return scrollRect.viewport.rect.height;
    }
    
    public float GetContentHeight()
    {
        return content != null ? content.rect.height : 0f;
    }
    
    private void OnDestroy()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }
        scrollListeners.Clear();
    }
    
    // Для отладки
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log($"Scroll: {GetScrollPosition():F2}, Start: {GetVisibleStartPosition():F1}, " +
                     $"End: {GetVisibleEndPosition():F1}, Viewport: {GetViewportHeight():F1}, " +
                     $"Content: {GetContentHeight():F1}");
        }
    }
}