using UnityEngine;

public class BasicButton : ICardStrategy
{
    private ImageCard card;
    
    public void Initialize(ImageCard card)
    {
        this.card = card;
    }

    public virtual void Open()
    {
        if (card.HasImage)
        {
            ImagePopupShower.Instance.SetImage(card.CurrentImage);
            ImagePopupShower.Instance.Show();
        }
        else
        {
            Debug.Log("Card image isn`t loaded yet.");
        }
    }
}
