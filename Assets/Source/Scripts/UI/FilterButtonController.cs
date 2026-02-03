using UnityEngine;
using UnityEngine.UI;

public class FilterButtonController : MonoBehaviour
{
    [SerializeField] private ContentGenerator contentGenerator;
    [SerializeField] private Button allButton;
    [SerializeField] private Button oddButton;
    [SerializeField] private Button evenButton;

    private void Start()
    {
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (allButton != null)
            allButton.onClick.AddListener(() => contentGenerator.GenerateContent(FilterType.All));
        
        if (oddButton != null)
            oddButton.onClick.AddListener(() => contentGenerator.GenerateContent(FilterType.Odd));
        
        if (evenButton != null)
            evenButton.onClick.AddListener(() => contentGenerator.GenerateContent(FilterType.Even));
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }

    private void RemoveButtonListeners()
    {
        if (allButton != null)
            allButton.onClick.RemoveAllListeners();
        
        if (oddButton != null)
            oddButton.onClick.RemoveAllListeners();
        
        if (evenButton != null)
            evenButton.onClick.RemoveAllListeners();
    }
}