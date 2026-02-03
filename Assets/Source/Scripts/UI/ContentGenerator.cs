using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentGenerator : MonoBehaviour
{
    [SerializeField] private CardData cardData;
    [SerializeField] private ContentCell cardPrefab;
    [SerializeField] private RectTransform container;
    [SerializeField] private Button allButton;
    [SerializeField] private Button oddButton;
    [SerializeField] private Button evenButton;
    [SerializeField] private float edgePadding = 20f;

    private List<ContentCell> spawnedCards = new List<ContentCell>();
    private RectTransform containerRect;
    
    private AllFilter<Sprite> allFilter = new AllFilter<Sprite>();
    private OddFilter<Sprite> oddFilter = new OddFilter<Sprite>();
    private EvenFilter<Sprite> evenFilter = new EvenFilter<Sprite>();
    private CardData filteredData;

    private void Awake()
    {
        containerRect = container;
        
        if (cardData != null && container != null)
        {
            GenerateContent();
        }
        
        allButton.onClick.AddListener(delegate { GenerateContent(); });
        oddButton.onClick.AddListener(delegate { GenerateContent(FilterType.Odd); });
        evenButton.onClick.AddListener(delegate { GenerateContent(FilterType.Even); });
    }

    public void GenerateContent(FilterType mode = FilterType.All)
    {
        ClearContent();
        FilterData(cardData, mode);
        
        if (!ValidateComponents()) return;
        if (filteredData.cardSprites.Count == 0) return;

        SetupGridLayout();
        CreateCards();
    }

    private void FilterData(CardData cardData, FilterType mode = FilterType.All)
    {
        filteredData = new CardData();
        filteredData.currentCardSize = cardData.currentCardSize;
        filteredData.cardsPerRow = cardData.cardsPerRow;
        filteredData.spacing = cardData.spacing;
        filteredData.cardSize = cardData.cardSize;
            
        switch (mode)
        {
            case FilterType.All:
                filteredData.cardSprites = allFilter.Filter(cardData.cardSprites);
                break;
            case FilterType.Even:
                filteredData.cardSprites = evenFilter.Filter(cardData.cardSprites);
                break;
            case FilterType.Odd:
                filteredData.cardSprites = oddFilter.Filter(cardData.cardSprites);
                break;
            default:
                throw new NullReferenceException("No such filter exists");
        }
    }

    private bool ValidateComponents()
    {
        if (filteredData == null)
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
        int cardsPerRow = Mathf.Max(1, filteredData.cardsPerRow);
            Vector2 availableSize = containerRect.rect.size - new Vector2(edgePadding * 2, edgePadding * 2);
        
        float availableWidth = availableSize.x - (filteredData.spacing * (cardsPerRow - 1));
        float cardWidth = Mathf.Min(availableWidth / cardsPerRow, filteredData.cardSize.x);
        float cardHeight = cardWidth * (filteredData.cardSize.y / filteredData.cardSize.x);
        
        filteredData.currentCardSize = new Vector2(cardWidth, cardHeight);
    }

    private void CreateCards()
    {
        int cardsPerRow = Mathf.Max(1, filteredData.cardsPerRow);
        int rowCount = Mathf.CeilToInt((float)filteredData.cardSprites.Count / cardsPerRow);
        Vector2 availableSize = containerRect.rect.size - new Vector2(edgePadding * 2, edgePadding * 2);

        for (int i = 0; i < filteredData.cardSprites.Count; i++)
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

        cardInstance.Initialize(filteredData.cardSprites[index]);
        cardInstance.SetSize(filteredData.currentCardSize);
        
        return cardInstance;
    }

    private void SetupCardPosition(ContentCell card, int index, int cardsPerRow, int rowCount, Vector2 availableSize)
    {
        int row = index / cardsPerRow;
        int col = index % cardsPerRow;
        
        float startX = -availableSize.x / 2 + edgePadding + filteredData.currentCardSize.x / 2;
        float startY = availableSize.y / 2 - edgePadding - filteredData.currentCardSize.y / 2;
        
        float posX = startX + col * (filteredData.currentCardSize.x + filteredData.spacing);
        float posY = startY - row * (filteredData.currentCardSize.y + filteredData.spacing);
        
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