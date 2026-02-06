using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PremiumPopupShower : PopupShower
{
    [SerializeField] private Button continueButton;
    [SerializeField] private List<Button> exampleButtons;
    [SerializeField] private GameObject exampleWindow;

    public static PremiumPopupShower Instance { get; private set; }

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
            
            foreach (var button in exampleButtons)
            {
                button.onClick.AddListener(ShowExampleWindow);
            }
            
            Instance.popupWindow.SetActive(false);
        }
    }

    public override void Show()
    {
        Instance.popupWindow.SetActive(true);
    }

    private void ShowExampleWindow()
    {
        exampleWindow.gameObject.SetActive(true);
    }

    protected override void Hide()
    {
        Instance.popupWindow.SetActive(false);
    }

    protected override void Unsubscribe()
    {
        closeButton.onClick.RemoveListener(Hide);
            
        foreach (var button in exampleButtons)
        {
            button.onClick.RemoveListener(ShowExampleWindow);
        }
    }
}
