using System;
using UnityEngine;
using UnityEngine.UI;

public class ImageCard : MonoBehaviour
{
    [SerializeField] private Image imageDisplay;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private Button cardButton;
    [SerializeField] private GameObject premiumPopup;
    [SerializeField] private Image premiumButton;
    
    private ICardStrategy currentStrategy;
    private string cardName;
    private bool hasImage = false;
    private Action<ImageCard> onImageRequested;
    
    public string CardName => cardName;
    public Sprite CurrentImage => imageDisplay.sprite;
    public bool HasImage => hasImage;
    
    public void Initialize(string name)
    {
        cardName = name;
        hasImage = false;
        premiumButton.gameObject.SetActive(false);
        cardButton.onClick.RemoveAllListeners();
        currentStrategy = new BasicButton();
        currentStrategy.Initialize(this);
        cardButton.onClick.AddListener(currentStrategy.Open);
        
        ShowLoadingState(true);
        
        if (imageDisplay != null)
        {
            imageDisplay.sprite = null;
            imageDisplay.enabled = false;
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
    
    private void OnDisable()
    {
        ClearImage();
    }

    public void Activate()
    {
        cardButton.onClick.RemoveAllListeners();
        currentStrategy = new PremiumButton();
        currentStrategy.Initialize(this);
        cardButton.onClick.AddListener(currentStrategy.Open);
        premiumButton.gameObject.SetActive(true);
    }
}