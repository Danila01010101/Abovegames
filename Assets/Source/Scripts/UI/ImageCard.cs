using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageCard : MonoBehaviour
{
    [SerializeField] private Image imageDisplay;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private Button cardButton;
    
    private int imageNumber;
    private bool hasImage = false;
    
    public int ImageNumber => imageNumber;
    public bool HasImage => hasImage;
    
    private System.Action<ImageCard> onClickCallback;
    
    public void Initialize(int number)
    {
        imageNumber = number;
        hasImage = false;
        
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
        
        if (imageDisplay != null)
        {
            imageDisplay.sprite = null;
            imageDisplay.enabled = false;
        }
        
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }
    
    public void SetImage(Sprite sprite)
    {
        if (imageDisplay != null)
        {
            imageDisplay.sprite = sprite;
            imageDisplay.enabled = true;
        }
        
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
        
        hasImage = true;
    }
    
    public void ClearImage()
    {
        if (imageDisplay != null)
        {
            imageDisplay.sprite = null;
            imageDisplay.enabled = false;
        }
        
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
        
        hasImage = false;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardButton == null)
        {
            OnCardClicked();
        }
    }

    private void OnCardClicked()
    {
        onClickCallback?.Invoke(this);
    }
}