using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContentCell : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton;
    
    private RectTransform rectTransform;
    private Sprite currentSprite;
    private System.Action<ContentCell> onClickCallback;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    public void Initialize(Sprite sprite)
    {
        currentSprite = sprite;
        
        if (cardImage != null)
        {
            cardImage.sprite = sprite;
            cardImage.preserveAspect = true;
        }
    }

    public void SetSize(Vector2 size)
    {
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = size;
        }
    }

    public void SetPosition(Vector2 position)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = position;
        }
    }

    public void SetOnClickCallback(System.Action<ContentCell> callback)
    {
        onClickCallback = callback;
    }

    private void OnCardClicked()
    {
        onClickCallback?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardButton == null)
        {
            OnCardClicked();
        }
    }

    public Sprite GetSprite() => currentSprite;
    public Image GetImage() => cardImage;
}