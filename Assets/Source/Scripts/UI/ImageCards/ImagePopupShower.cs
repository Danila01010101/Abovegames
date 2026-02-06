using UnityEngine;
using UnityEngine.UI;

public class ImagePopupShower : PopupShower
{
    [SerializeField] private Image image;

    public static ImagePopupShower Instance { get; private set; }

    public override void Initialize()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            closeButton.onClick.AddListener(Hide);
            Hide();
        }
    }

    protected override void Hide()
    {
        Instance.popupWindow.SetActive(false);
    }

    public override void Show()
    {
        Instance.popupWindow.SetActive(true);
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
    }
}
