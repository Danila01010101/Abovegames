using System.Collections.Generic;
using UnityEngine;

public class ContentGenerator : MonoBehaviour
{
    [SerializeField] private CardData cardData;
    [SerializeField] private ContentCell cardPrefab;
    [SerializeField] private RectTransform container;
    [SerializeField] private float edgePadding = 20f;

    private List<ContentCell> spawnedCards = new List<ContentCell>();
    private RectTransform containerRect;

    private void Awake()
    {
        containerRect = container;if (cardData != null && container != null)
        {
            GenerateContent();
        }
    }

    public void GenerateContent()
    {
        ClearContent();
        
        if (!ValidateComponents()) return;
        if (cardData.cardSprites.Count == 0) return;

        SetupGridLayout();
        CreateCards();
    }

    private bool ValidateComponents()
    {
        if (cardData == null)
        {
            Debug.LogWarning("CardData не назначен");
            return false;
        }

        if (cardPrefab == null)
        {
            Debug.LogWarning("CardPrefab не назначен");
            return false;
        }

        if (container == null)
        {
            Debug.LogWarning("Container не назначен");
            return false;
        }

        return true;
    }

    private void SetupGridLayout()
    {
        int cardsPerRow = Mathf.Max(1, cardData.cardsPerRow);
            Vector2 availableSize = containerRect.rect.size - new Vector2(edgePadding * 2, edgePadding * 2);
        
        float availableWidth = availableSize.x - (cardData.spacing * (cardsPerRow - 1));
        float cardWidth = Mathf.Min(availableWidth / cardsPerRow, cardData.cardSize.x);
        float cardHeight = cardWidth * (cardData.cardSize.y / cardData.cardSize.x);
        
        cardData.currentCardSize = new Vector2(cardWidth, cardHeight);
    }

    private void CreateCards()
    {
        int cardsPerRow = Mathf.Max(1, cardData.cardsPerRow);
        int rowCount = Mathf.CeilToInt((float)cardData.cardSprites.Count / cardsPerRow);
        Vector2 availableSize = containerRect.rect.size - new Vector2(edgePadding * 2, edgePadding * 2);

        for (int i = 0; i < cardData.cardSprites.Count; i++)
        {
            ContentCell card = InstantiateCard(i);
            if (card == null) continue;

            SetupCardPosition(card, i, cardsPerRow, rowCount, availableSize);
            spawnedCards.Add(card);
        }
    }

    private ContentCell InstantiateCard(int index)
    {
        ContentCell cardInstance = Instantiate(cardPrefab, container);
        if (cardInstance == null) return null;

        cardInstance.Initialize(cardData.cardSprites[index]);
        cardInstance.SetSize(cardData.currentCardSize);
        
        return cardInstance;
    }

    private void SetupCardPosition(ContentCell card, int index, int cardsPerRow, int rowCount, Vector2 availableSize)
    {
        int row = index / cardsPerRow;
        int col = index % cardsPerRow;
        
        float startX = -availableSize.x / 2 + edgePadding + cardData.currentCardSize.x / 2;
        float startY = availableSize.y / 2 - edgePadding - cardData.currentCardSize.y / 2;
        
        float posX = startX + col * (cardData.currentCardSize.x + cardData.spacing);
        float posY = startY - row * (cardData.currentCardSize.y + cardData.spacing);
        
        card.SetPosition(new Vector2(posX, posY));
    }

    public void ClearContent()
    {
        foreach (ContentCell card in spawnedCards)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }
        spawnedCards.Clear();
    }

    public void SetCardData(CardData newData)
    {
        cardData = newData;
        GenerateContent();
    }

    public void UpdateCardData(CardData newData)
    {
        cardData = newData;
        GenerateContent();
    }

    private void OnDestroy()
    {
        ClearContent();
    }
}