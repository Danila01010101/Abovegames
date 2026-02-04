using System;
using System.Collections.Generic;
using UnityEngine;

public class ContentGenerator : MonoBehaviour
{
    [SerializeField] private CardData cardData;
    [SerializeField] private CardPool cardPool;
    [SerializeField] private RectTransform container;
    [SerializeField] private ScrollViewMonitor scrollMonitor;
    [SerializeField] private float edgePadding = 20f;
    [SerializeField] private DeviceLayoutCalculator layoutCalculator;
    [SerializeField] private OnlineImageLoader imageLoader;
    
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
    
    // Ссылка на imageLoader для удобства доступа
    
    private void Awake()
    {
        containerRect = container;
        
        // Получаем ссылки на компоненты
        if (layoutCalculator == null)
            layoutCalculator = new DeviceLayoutCalculator();
        
        if (cardPool != null)
        {
            cardPool.OnCardImageRequested += HandleCardImageRequest;
        }
        
        // Ищем или получаем OnlineImageLoader
        if (imageLoader == null && Camera.main != null)
        {
            imageLoader = Camera.main.gameObject.AddComponent<OnlineImageLoader>();
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
        LoadInitialImages();
    }
    
    public void ApplyFilter(FilterType filterType)
    {
        currentFilter = filterType;
        
        // Используем фильтры для создания отфильтрованного списка имен
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
        // Используем DeviceLayoutCalculator для определения параметров
        columns = layoutCalculator.GetCurrentColumns();
        cardSize = layoutCalculator.GetCurrentCardSize(container, edgePadding);
        
        // Получаем spacing (если он есть в layoutCalculator, иначе используем значение по умолчанию)
        spacing = 20f; // Можно вынести в поле или получать из layoutCalculator
        
        // Рассчитываем высоту контейнера на основе количества карточек
        int rows = Mathf.CeilToInt((float)filteredCardNames.Count / columns);
        float containerHeight = rows * (cardSize.y + spacing) + edgePadding * 2;
        
        // Устанавливаем размер контейнера
        containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, containerHeight);
        
        Debug.Log($"Layout calculated: {columns} columns, {rows} rows, card size: {cardSize}");
    }
    
    private void CreateCardStructure()
    {
        int requiredCards = filteredCardNames.Count;
        EnsurePoolCapacity(requiredCards);
        
        for (int i = 0; i < requiredCards; i++)
        {
            CreateCard(i);
        }
        
        Debug.Log($"Created {spawnedCards.Count} cards");
    }
    
    private void CreateCard(int index)
    {
        ImageCard card = cardPool.GetCard();
        
        if (card != null)
        {
            card.transform.SetParent(container);
            card.transform.localScale = Vector3.one;
            
            // Получаем имя карточки из отфильтрованного списка
            string cardName = filteredCardNames[index];
            
            // Инициализируем карточку с именем
            // Предполагаем, что ImageCard может работать с именами
            // Если нет, нужно изменить ImageCard.Initialize
            card.Initialize(cardName);
            card.SetSize(cardSize);
            card.SetPosition(CalculateCardPosition(index));
            
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
            
            Debug.Log($"Added {cardsToAdd} cards to pool. Total: {currentTotal + cardsToAdd}");
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
        
        // Сначала выгружаем далекие карточки
        UnloadDistantCards(visibleRange[0], visibleRange[1]);
        
        // Затем загружаем видимые
        for (int i = visibleRange[0]; i <= visibleRange[1]; i++)
        {
            if (i >= 0 && i < spawnedCards.Count)
            {
                ImageCard card = spawnedCards[i];
                
                if (!card.HasImage && currentLoadCount < MaxSimultaneousLoads)
                {
                    LoadCardImage(card);
                }
            }
        }
    }
    
    private async void LoadCardImage(ImageCard card)
    {
        if (card == null || card.HasImage) return;
        
        currentLoadCount++;
        
        try
        {
            // Здесь card должен иметь имя карточки, которое используется для загрузки
            // Предполагаем, что ImageCard имеет свойство CardName
            string cardName = GetCardNameForImageCard(card);
            
            if (string.IsNullOrEmpty(cardName))
            {
                Debug.LogWarning($"Card has no name: {card}");
                return;
            }
            
            Sprite sprite = await imageLoader.LoadImageAsync(cardName);
            
            if (sprite != null && card != null)
            {
                card.SetImage(sprite);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading image for card {card}: {e.Message}");
        }
        finally
        {
            currentLoadCount--;
        }
    }
    
    private void HandleCardImageRequest(ImageCard card)
    {
        if (!card.HasImage && currentLoadCount < MaxSimultaneousLoads)
        {
            LoadCardImage(card);
        }
    }
    
    private string GetCardNameForImageCard(ImageCard card)
    {
        int index = spawnedCards.IndexOf(card);
        if (index >= 0 && index < filteredCardNames.Count)
        {
            return filteredCardNames[index];
        }
        return string.Empty;
    }
    
    private int[] GetVisibleCardRange()
    {
        if (spawnedCards.Count == 0) return new int[] { 0, 0 };
        
        float containerHeight = containerRect.rect.height;
        float scrollPosition = scrollMonitor.GetVisibleStartPosition();
        float viewportHeight = scrollMonitor.GetViewportHeight();
        
        float visibleStart = scrollPosition * (containerHeight - viewportHeight);
        float visibleEnd = visibleStart + viewportHeight;
        
        int startRow = Mathf.FloorToInt((visibleStart - edgePadding) / (cardSize.y + spacing));
        int endRow = Mathf.CeilToInt((visibleEnd - edgePadding) / (cardSize.y + spacing));
        
        int startIndex = Mathf.Max(0, (startRow - PreloadMargin) * columns);
        int endIndex = Mathf.Min(spawnedCards.Count - 1, (endRow + PreloadMargin) * columns);
        
        return new int[] { startIndex, endIndex };
    }
    
    private void LoadInitialImages()
    {
        if (spawnedCards.Count == 0) return;
        
        int initialLoadCount = Mathf.Min(12, spawnedCards.Count);
        
        for (int i = 0; i < initialLoadCount; i++)
        {
            if (i < spawnedCards.Count && currentLoadCount < MaxSimultaneousLoads)
            {
                LoadCardImage(spawnedCards[i]);
            }
        }
    }
    
    private void UnloadDistantCards(int visibleStart, int visibleEnd)
    {
        int unloadMargin = PreloadMargin * 2;
        
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
    
    public void UpdateCardData(CardData newData)
    {
        if (newData != null)
        {
            cardData = newData;
            GenerateContent(currentFilter);
        }
    }
    
    public void RefreshContent()
    {
        GenerateContent(currentFilter);
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
            imageLoader.ClearMemoryCache();
        }
    }
    
    // Вспомогательные методы для отладки
    public int GetTotalCardsCount() => cardData?.cardNames?.Count ?? 0;
    public int GetFilteredCardsCount() => filteredCardNames.Count;
    public int GetActiveCardsCount() => spawnedCards.Count;
    public Vector2 GetCurrentCardSize() => cardSize;
    public int GetCurrentColumns() => columns;
}