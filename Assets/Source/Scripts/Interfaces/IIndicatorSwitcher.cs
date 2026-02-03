public interface IIndicatorSwitcher
{
    int CurrentBannerIndex { get; }
    event System.Action<int, int> OnBannerSwitching;
    event System.Action<int> OnBannerSwitchComplete;
}