using System;
using System.Collections.Generic;
using UnityEngine;

public class ContentGenerator : MonoBehaviour
{
    [SerializeField] private CardData cardData;
    [SerializeField] private ImageCard cardPrefab;
    [SerializeField] private RectTransform container;
    [SerializeField] private float edgePadding = 20f;

    private List<ImageCard> spawnedCards = new List<ImageCard>();
    private RectTransform containerRect;
    private CardData filteredData;

    private void Awake()
    {
        containerRect = container;
        InitializeContent();
    }

    private void InitializeContent()
    {
        if (IsDataValid())
        {
            GenerateContent();
        }
    }

    public void GenerateContent(FilterType filterType = FilterType.All)
    {
        ClearContent();
        
        if (!IsDataValid()) return;

        filteredData = CreateFilteredData(filterType);
        
        if (filteredData.cardSprites.Count == 0) return;

        CalculateCardSize();
        CreateCards();
    }

    private bool IsDataValid()
    {
        return cardData != null && cardPrefab != null && container != null;
    }

    private CardData CreateFilteredData(FilterType filterType)
    {
        var filter = CreateFilter(filterType);
        var filteredSprites = filter.Filter(cardData.cardSprites);
        
        return cardData.CreateFilteredCopy(filteredSprites);
    }

    private IContentFilter<Sprite> CreateFilter(FilterType filterType)
    {
        return filterType switch
        {
            FilterType.All => new AllFilter<Sprite>(),
            FilterType.Even => new EvenFilter<Sprite>(),
            FilterType.Odd => new OddFilter<Sprite>(),
            _ => throw new ArgumentException($"Unknown filter type: {filterType}")
        };
    }

    private void CalculateCardSize()
    {
        int cardsPerRow = Mathf.Max(1, filteredData.cardsPerRow);
        Vector2 availableSize = containerRect.rect.size - new Vector2(edgePadding * 2, edgePadding * 2);
        
        float availableWidth = availableSize.x - (filteredData.spacing * (cardsPerRow - 1));
        float cardWidth = Mathf.Min(availableWidth / cardsPerRow, filteredData.cardSize.x);
        float cardHeight = cardWidth * (filteredData.cardSize.y / filteredData.cardSize.x);
        
        filteredData.currentCardSize = new Vector2(cardWidth, cardHeight);
    }

    private void CreateCards()
    {
        Vector2 availableSize = containerRect.rect.size - new Vector2(edgePadding * 2, edgePadding * 2);
        int cardsPerRow = Mathf.Max(1, filteredData.cardsPerRow);

        for (int i = 0; i < filteredData.cardSprites.Count; i++)
        {
            ImageCard card = CreateCard(i);
            if (card == null) continue;

            PositionCard(card, i, cardsPerRow, availableSize);
            spawnedCards.Add(card);
        }
    }

    private ImageCard CreateCard(int index)
    {
        ImageCard card = Instantiate(cardPrefab, container);
        if (card == null) return null;

        card.Initialize(index);
        card.SetSize(filteredData.currentCardSize);
        
        return card;
    }

    private void PositionCard(ImageCard card, int index, int cardsPerRow, Vector2 availableSize)
    {
        int row = index / cardsPerRow;
        int column = index % cardsPerRow;
        
        Vector2 startPosition = CalculateStartPosition(availableSize);
        Vector2 cardPosition = CalculateCardPosition(startPosition, row, column);
        
        card.SetPosition(cardPosition);
    }

    private Vector2 CalculateStartPosition(Vector2 availableSize)
    {
        float startX = -availableSize.x / 2 + edgePadding + filteredData.currentCardSize.x / 2;
        float startY = availableSize.y / 2 - edgePadding - filteredData.currentCardSize.y / 2;
        
        return new Vector2(startX, startY);
    }

    private Vector2 CalculateCardPosition(Vector2 startPosition, int row, int column)
    {
        float positionX = startPosition.x + column * (filteredData.currentCardSize.x + filteredData.spacing);
        float positionY = startPosition.y - row * (filteredData.currentCardSize.y + filteredData.spacing);
        
        return new Vector2(positionX, positionY);
    }

    private void ClearContent()
    {
        foreach (ImageCard card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        spawnedCards.Clear();
        
        if (filteredData != null && filteredData != cardData)
        {
            Destroy(filteredData);
        }
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