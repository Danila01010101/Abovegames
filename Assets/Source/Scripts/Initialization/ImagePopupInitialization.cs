using UnityEngine;

public class ImagePopupInitializator : MonoBehaviour, IInitializable
{
    [SerializeField] private ImagePopupShower imagePopupShower;
    
    public bool isUIElement() => true;

    public void Initialize()
    {
        imagePopupShower.Initialize();
    }
}
