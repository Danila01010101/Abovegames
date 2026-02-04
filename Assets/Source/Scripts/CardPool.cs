using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPool : MonoBehaviour
{
    [SerializeField] private ImageCard cardPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private bool expandPool = true;
    
    private Queue<ImageCard> pool = new Queue<ImageCard>();
    private List<ImageCard> activeCards = new List<ImageCard>();
    private Transform poolParent;
    
    public Action<ImageCard> OnCardImageRequested;
    
    private void Awake()
    {
        InitializePool();
    }
    
    private void InitializePool()
    {
        poolParent = new GameObject("CardPool").transform;
        poolParent.SetParent(transform);
        poolParent.gameObject.SetActive(false);
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledCard();
        }
    }
    
    private void CreatePooledCard()
    {
        ImageCard card = Instantiate(cardPrefab, poolParent);
        card.gameObject.SetActive(false);
        card.SetOnImageRequestedCallback(cardInstance => OnCardImageRequested?.Invoke(cardInstance));
        pool.Enqueue(card);
    }
    
    private void HandleImageRequest(ImageCard card)
    {
        OnCardImageRequested?.Invoke(card);
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
        card.transform.SetParent(poolParent);
        card.ClearImage();
        
        activeCards.Remove(card);
        pool.Enqueue(card);
    }
    
    public void ReturnAllCards()
    {
        foreach (ImageCard card in activeCards.ToArray())
        {
            ReturnCard(card);
        }
    }
    
    public int GetActiveCardCount()
    {
        return activeCards.Count;
    }
    
    public int GetPooledCardCount()
    {
        return pool.Count;
    }
    
    private void OnDestroy()
    {
        foreach (ImageCard card in pool)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        
        foreach (ImageCard card in activeCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        
        pool.Clear();
        activeCards.Clear();
        
        if (poolParent != null)
        {
            Destroy(poolParent.gameObject);
        }
    }
}