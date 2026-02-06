using UnityEngine;
using UnityEngine.UI;

public abstract class PopupShower : MonoBehaviour
{
    [SerializeField] protected GameObject popupWindow;
    [SerializeField] protected Button closeButton;

    public virtual void Initialize()
    {
        closeButton.onClick.AddListener(Hide);
    }

    protected abstract void Hide();

    protected virtual void Unsubscribe()
    {
        closeButton.onClick.RemoveListener(Hide);
    }

    public abstract void Show();

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
