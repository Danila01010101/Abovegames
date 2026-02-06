using System;
using System.Collections.Generic;
using UnityEngine;

public class ContentGenerator : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    
    private ScrollViewMonitor scrollMonitor;
    private OnlineImageLoader imageLoader;
    private CardPool cardPool;
    private CardData cardData;
    private float edgePadding = 20f;
    
    private DeviceLayoutCalculator layoutCalculator;
    private List<string> filteredCardNames = new List<string>();
    private List<ImageCard> spawnedCards = new List<ImageCard>();
    private RectTransform containerRect;
    private Vector2 cardSize;
    private int columns;
    private float spacing;
    private bool isProcessing = false;
    private FilterType currentFilter = FilterType.All;
    
    private const int PreloadMargin = 5;
    private const int MaxSimultaneousLoads = 3;
    private int currentLoadCount = 0;

    public void SetData(OnlineImageLoader imageLoader, CardPool cardPool, ScrollViewMonitor scrollViewMonitor,
        CardData cardData, float edgePadding)
    {
        this.imageLoader = imageLoader;
        this.cardPool = cardPool;
        scrollMonitor = scrollViewMonitor;
        this.cardData = cardData;
        this.edgePadding = edgePadding;
    }
    
    public void Initialize()
    {
        containerRect = container;
        
        if (layoutCalculator == null)
            layoutCalculator = new DeviceLayoutCalculator();
        
        if (cardPool != null)
        {
            cardPool.OnCardImageRequested += HandleCardImageRequest;
        }
        
        if (imageLoader == null)
        {
            throw new NullReferenceException("No Image Loader assigned");
        }
        
        InitializeContent();
    }
    
    private void InitializeContent()
    {
        if (ValidateComponents())
        {
            SetupScrollListener();
            GenerateContent(currentFilter);
        }
    }
    
    private bool ValidateComponents()
    {
        bool isValid = cardData != null && 
                      cardPool != null && 
                      container != null && 
                      scrollMonitor != null &&
                      layoutCalculator != null;
        
        if (!isValid)
        {
            Debug.LogError("ContentGenerator: Missing required components!");
            return false;
        }
        
        return true;
    }
    
    private void SetupScrollListener()
    {
        scrollMonitor.AddScrollListener(OnScrollPositionChanged);
    }
    
    public void GenerateContent(FilterType filterType = FilterType.All)
    {
        ReturnAllCardsToPool();
        
        if (!ValidateComponents() || cardData.cardNames.Count == 0) 
        {
            Debug.LogWarning("No card names available to generate content.");
            return;
        }
        
        currentFilter = filterType;
        ApplyFilter(filterType);
        
        
        if (filteredCardNames.Count == 0) 
        {
            Debug.LogWarning($"No cards after applying {filterType} filter.");
            return;
        }
        
        CalculateLayout();
        CreateCardStructure();
        
        ScrollToTop();
        
        ProcessVisibleCards();
        LoadInitialImages();
    }
    
    public void ApplyFilter(FilterType filterType)
    {
        currentFilter = filterType;
        
        IContentFilter<string> filter = CreateFilter(filterType);
        filteredCardNames = filter.Filter(cardData.cardNames);
        
        Debug.Log($"Applied {filterType} filter: {filteredCardNames.Count} cards");
    }
    
    private IContentFilter<string> CreateFilter(FilterType filterType)
    {
        return filterType switch
        {
            FilterType.All => new AllFilter<string>(),
            FilterType.Even => new EvenFilter<string>(),
            FilterType.Odd => new OddFilter<string>(),
            _ => new AllFilter<string>()
        };
    }
    
    private void CalculateLayout()
    {
        columns = layoutCalculator.GetColumns();
        cardSize = layoutCalculator.GetSquareSize(container.rect.size.x);
        spacing = 20f;
        
        int rows = Mathf.CeilToInt((float)filteredCardNames.Count / columns);
        float containerHeight = rows * (cardSize.y + spacing) + edgePadding * 2;
        
        containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, containerHeight);
    }
    
    private void CreateCardStructure()
    {
        int requiredCards = filteredCardNames.Count;
        EnsurePoolCapacity(requiredCards);
        
        for (int i = 0; i < requiredCards; i++)
        {
            CreateCard(i);
        }
    }
    
    private void CreateCard(int index)
    {
        ImageCard card = cardPool.GetCard();
        
        if (card != null)
        {
            card.transform.SetParent(container);
            card.transform.localScale = Vector3.one;
            
            string cardName = filteredCardNames[index];
            card.Initialize(cardName);
            card.SetSize(cardSize);
            card.SetPosition(CalculateCardPosition(index));

            if ((index + 1) % 4 == 0)
            {
                card.Activate();
            }
            
            spawnedCards.Add(card);
        }
    }
    
    private Vector2 CalculateCardPosition(int index)
    {
        int row = index / columns;
        int column = index % columns;
        
        Vector2 availableSize = containerRect.rect.size - new Vector2(edgePadding * 2, edgePadding * 2);
        
        float startX = -availableSize.x / 2 + edgePadding + cardSize.x / 2;
        float startY = availableSize.y / 2 - edgePadding - cardSize.y / 2;
        
        float positionX = startX + column * (cardSize.x + spacing);
        float positionY = startY - row * (cardSize.y + spacing);
        
        return new Vector2(positionX, positionY);
    }
    
    private void EnsurePoolCapacity(int requiredCount)
    {
        int currentTotal = cardPool.GetActiveCardCount() + cardPool.GetPooledCardCount();
        
        if (currentTotal < requiredCount)
        {
            int cardsToAdd = requiredCount - currentTotal;
            
            for (int i = 0; i < cardsToAdd; i++)
            {
                ImageCard tempCard = cardPool.GetCard();
                cardPool.ReturnCard(tempCard);
            }
        }
    }
    
    private void OnScrollPositionChanged(float normalizedPosition)
    {
        if (isProcessing || spawnedCards.Count == 0) return;
        
        isProcessing = true;
        ProcessVisibleCards();
        isProcessing = false;
    }
    
    private void ProcessVisibleCards()
    {
        int[] visibleRange = GetVisibleCardRange();
    
        UnloadDistantCards(visibleRange[0], visibleRange[1]);
    
        for (int i = visibleRange[0]; i <= visibleRange[1]; i++)
        {
            if (i >= 0 && i < spawnedCards.Count)
            {
                ImageCard card = spawnedCards[i];
            
                if (!card.HasImage)
                {
                    StartCoroutine(QueueCardImageLoad(card));
                }
            }
        }
    }

    private System.Collections.IEnumerator QueueCardImageLoad(ImageCard card)
    {
        while (currentLoadCount >= MaxSimultaneousLoads)
        {
            yield return null;
        }
    
        LoadCardImage(card);
    }
    
    private void LoadCardsInBatches(List<int> cardIndices)
    {
        int loadedCount = 0;
        
        foreach (int index in cardIndices)
        {
            if (loadedCount >= MaxSimultaneousLoads) break;
            
            ImageCard card = spawnedCards[index];
            if (card != null && !card.HasImage)
            {
                LoadCardImage(card);
                loadedCount++;
            }
        }
    }
    
    private void LoadCardImage(ImageCard card)
    {
        if (card == null || card.HasImage) return;
        
        int cardIndex = spawnedCards.IndexOf(card);
        if (cardIndex < 0 || cardIndex >= filteredCardNames.Count) return;
        
        string cardName = filteredCardNames[cardIndex];
        if (string.IsNullOrEmpty(cardName)) return;
        
        currentLoadCount++;
        
        imageLoader.LoadImage(cardName, (sprite) =>
        {
            if (card != null)
            {
                card.SetImage(sprite);
            }
            currentLoadCount--;
        });
    }
    
    private void HandleCardImageRequest(ImageCard card)
    {
        LoadCardImage(card);
    }
    
    private int[] GetVisibleCardRange()
    {
        if (spawnedCards.Count == 0 || columns == 0) 
            return new int[] { 0, 0 };
        
        float visibleStart = scrollMonitor.GetVisibleStartPosition();
        float visibleEnd = scrollMonitor.GetVisibleEndPosition();
        
        visibleStart += edgePadding;
        visibleEnd += edgePadding;
        
        float rowHeight = cardSize.y + spacing;
        int startRow = Mathf.FloorToInt(visibleStart / rowHeight);
        int endRow = Mathf.CeilToInt(visibleEnd / rowHeight);
        
        startRow = Mathf.Max(0, startRow - PreloadMargin);
        endRow = Mathf.Min(
            Mathf.CeilToInt((float)filteredCardNames.Count / columns) - 1, 
            endRow + PreloadMargin
        );
        
        int startIndex = Mathf.Max(0, startRow * columns);
        int endIndex = Mathf.Min(
            spawnedCards.Count - 1, 
            Mathf.Min((endRow + 1) * columns - 1, spawnedCards.Count - 1)
        );
        
        return new int[] { startIndex, endIndex };
    }
    
    private void LoadInitialImages()
    {
        if (spawnedCards.Count == 0) return;
        
        ProcessVisibleCards();
    }
    
    private void UnloadDistantCards(int visibleStart, int visibleEnd)
    {
        int unloadMargin = PreloadMargin * 3;
        
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            ImageCard card = spawnedCards[i];
            
            if (card != null && card.HasImage && 
                (i < visibleStart - unloadMargin || i > visibleEnd + unloadMargin))
            {
                card.ClearImage();
            }
        }
    }
    
    private void ReturnAllCardsToPool()
    {
        foreach (ImageCard card in spawnedCards)
        {
            if (card != null)
            {
                card.ClearImage();
                cardPool.ReturnCard(card);
            }
        }
        
        spawnedCards.Clear();
        filteredCardNames.Clear();
    }
    
    private void ScrollToTop()
    {
        if (scrollMonitor != null)
        {
            scrollMonitor.ScrollToTop();
        }
    }
    
    public void RefreshContent()
    {
        GenerateContent(currentFilter);
    }
    
    public void SetFilter(FilterType filterType)
    {
        GenerateContent(filterType);
    }
    
    private void OnDestroy()
    {
        if (scrollMonitor != null)
        {
            scrollMonitor.RemoveScrollListener(OnScrollPositionChanged);
        }
        
        if (cardPool != null)
        {
            cardPool.OnCardImageRequested -= HandleCardImageRequest;
        }
        
        ReturnAllCardsToPool();
        
        if (imageLoader != null)
        {
            imageLoader.ClearCache();
        }
    }
}