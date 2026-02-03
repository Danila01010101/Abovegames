using System;
using UnityEngine;
using UnityEngine.UI;

public class FilterButtonController : MonoBehaviour, IIndicatorSwitcher
{
    [SerializeField] private ContentGenerator contentGenerator;
    [SerializeField] private Button allButton;
    [SerializeField] private Button oddButton;
    [SerializeField] private Button evenButton;
    
    private IIndicatorSwitcher indicatorSwitcherImplementation;

    public int CurrentBannerIndex { get; private set; } = 0;
    public event Action<int, int> OnBannerSwitching;
    public event Action<int> OnBannerSwitchComplete;

    private void Start()
    {
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (allButton != null)
            allButton.onClick.AddListener(() => ChangeFilter(0 ,FilterType.All));
        
        if (oddButton != null)
            oddButton.onClick.AddListener(() => ChangeFilter(1, FilterType.Odd));
        
        if (evenButton != null)
            evenButton.onClick.AddListener(() => ChangeFilter(2, FilterType.Even));
    }

    private void ChangeFilter(int newButtonIndex, FilterType filter)
    {
        OnBannerSwitching?.Invoke(CurrentBannerIndex, newButtonIndex);
        contentGenerator.GenerateContent(filter);
        CurrentBannerIndex = newButtonIndex;
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