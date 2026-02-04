using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewMonitor : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    
    private List<System.Action<float>> scrollListeners = new List<System.Action<float>>();
    
    private void Start()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
    }
    
    private void OnScrollValueChanged(Vector2 scrollPosition)
    {
        float normalizedPosition = 1f - scrollPosition.y;
        NotifyScrollListeners(normalizedPosition);
    }
    
    public void AddScrollListener(System.Action<float> listener)
    {
        if (!scrollListeners.Contains(listener))
        {
            scrollListeners.Add(listener);
        }
    }
    
    public void RemoveScrollListener(System.Action<float> listener)
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
    
    public float GetVisibleStartPosition()
    {
        if (scrollRect == null || content == null) return 0f;
        
        float scrollValue = 1f - scrollRect.verticalNormalizedPosition;
        float contentHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        
        return Mathf.Max(0f, scrollValue * (contentHeight - viewportHeight));
    }
    
    public float GetVisibleEndPosition()
    {
        if (scrollRect == null || content == null) return 0f;
        
        float startPosition = GetVisibleStartPosition();
        float viewportHeight = scrollRect.viewport.rect.height;
        
        return startPosition + viewportHeight;
    }
    
    public void ScrollToPosition(float normalizedPosition)
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(1f - normalizedPosition);
        }
    }
    
    public float GetContentHeight()
    {
        return content != null ? content.rect.height : 0f;
    }
    
    public float GetViewportHeight()
    {
        return scrollRect != null && scrollRect.viewport != null ? 
               scrollRect.viewport.rect.height : 0f;
    }
    
    private void OnDestroy()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }
        scrollListeners.Clear();
    }
}