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
    
    private void OnDestroy()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }
        scrollListeners.Clear();
    }
}