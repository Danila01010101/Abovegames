public class PremiumButton : ICardStrategy
{
    public void Initialize(ImageCard card) { }

    public void Open()
    {
        PremiumPopupShower.Instance.Show();
    }
}
