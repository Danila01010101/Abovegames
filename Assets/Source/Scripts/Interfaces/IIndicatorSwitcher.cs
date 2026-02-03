public interface IIndicatorSwitcher
{
    void Next();
    void Previous();
    void GoToBanner(int index);
    int CurrentBannerIndex { get; }
    int BannerCount { get; }
    event System.Action<int, int> OnBannerSwitching;
    event System.Action<int> OnBannerSwitchComplete;
}