using System;
using System.Collections.Generic;
using UnityEngine;

public class CardPool : IDisposable
{
    private Transform cardsParent;
    private ImageCard cardPrefab;
    private int initialPoolSize = 20;
    private bool expandPool = true;
    
    private Queue<ImageCard> pool = new Queue<ImageCard>();
    private List<ImageCard> activeCards = new List<ImageCard>();
    
    public Action<ImageCard> OnCardImageRequested;

    public void SetData(Transform cardsParent, ImageCard cardPrefab, int initialPoolSize, bool expandPool)
    {
        this.cardsParent = cardsParent;
        this.cardPrefab = cardPrefab;
        this.initialPoolSize = initialPoolSize;
        this.expandPool = expandPool;
    }
    
    public void Initialize()
    {
        InitializePool();
    }
    
    private void InitializePool()
    {
        cardsParent.SetParent(cardsParent);
        cardsParent.gameObject.SetActive(false);
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledCard();
        }
    }
    
    private void CreatePooledCard()
    {
        ImageCard card = GameObject.Instantiate(cardPrefab, cardsParent);
        card.gameObject.SetActive(false);
        card.SetOnImageRequestedCallback(cardInstance => OnCardImageRequested?.Invoke(cardInstance));
        pool.Enqueue(card);
    }
    
    public ImageCard GetCard()
    {
        if (pool.Count == 0)
        {
            if (expandPool)
            {
                CreatePooledCard();
            }
            else
            {
                return null;
            }
        }
        
        ImageCard card = pool.Dequeue();
        card.gameObject.SetActive(true);
        activeCards.Add(card);
        return card;
    }
    
    public void ReturnCard(ImageCard card)
    {
        if (card == null) return;
        
        card.gameObject.SetActive(false);
        card.transform.SetParent(cardsParent);
        card.ClearImage();
        
        activeCards.Remove(card);
        pool.Enqueue(card);
    }
    
    public int GetActiveCardCount()
    {
        return activeCards.Count;
    }
    
    public int GetPooledCardCount()
    {
        return pool.Count;
    }

    public void Dispose()
    {
        foreach (ImageCard card in pool)
        {
            if (card != null)
            {
                GameObject.Destroy(card.gameObject);
            }
        }
        
        foreach (ImageCard card in activeCards)
        {
            if (card != null)
            {
                GameObject.Destroy(card.gameObject);
            }
        }
        
        pool.Clear();
        activeCards.Clear();
    }
}