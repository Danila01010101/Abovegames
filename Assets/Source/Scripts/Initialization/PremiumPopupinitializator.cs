using UnityEngine;

public class PremiumPopupinitializator : MonoBehaviour, IInitializable
{
    [SerializeField] private PremiumPopupShower premiumPopupShower;
    
    public bool isUIElement() => true;

    public void Initialize()
    {
        premiumPopupShower.Initialize();
    }
}
