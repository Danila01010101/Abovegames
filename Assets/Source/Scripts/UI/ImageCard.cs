using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageCard : MonoBehaviour
{
    [SerializeField] private Image imageDisplay;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private Button cardButton;
    
    private string cardName;
    private bool hasImage = false;
    private Action<ImageCard> onImageRequested;
    
    public string CardName => cardName;
    public bool HasImage => hasImage;
    
    public void Initialize(string name)
    {
        cardName = name;
        hasImage = false;
        
        ShowLoadingState(true);
        
        if (imageDisplay != null)
        {
            imageDisplay.sprite = null;
            imageDisplay.enabled = false;
        }
        
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(RequestImage);
        }
    }
    
    public void SetOnImageRequestedCallback(Action<ImageCard> callback)
    {
        onImageRequested = callback;
    }
    
    private void RequestImage()
    {
        if (!hasImage && onImageRequested != null)
        {
            onImageRequested?.Invoke(this);
        }
    }
    
    public void SetImage(Sprite sprite)
    {
        if (imageDisplay != null)
        {
            imageDisplay.sprite = sprite;
            imageDisplay.enabled = sprite != null;
        }
        
        hasImage = sprite != null;
        ShowLoadingState(!hasImage);
    }
    
    public void ClearImage()
    {
        if (imageDisplay != null)
        {
            imageDisplay.sprite = null;
            imageDisplay.enabled = false;
        }
        
        hasImage = false;
        ShowLoadingState(true);
    }
    
    private void ShowLoadingState(bool isLoading)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(isLoading);
        }
    }
    
    public void SetSize(Vector2 size)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = size;
        }
    }
    
    public void SetPosition(Vector2 position)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = position;
        }
    }
    
    private void OnBecameVisible()
    {
        if (!hasImage)
        {
            RequestImage();
        }
    }
    
    private void OnBecameInvisible()
    {
        // Опционально: можно очищать изображение при скрытии
        // ClearImage();
    }
    
    private void OnDisable()
    {
        ClearImage();
    }
}