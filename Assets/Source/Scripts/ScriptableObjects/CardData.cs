using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Content Settings")]
    public List<Sprite> cardSprites = new List<Sprite>();
    
    [Header("Layout Settings")]
    public int cardsPerRow = 3;
    public float spacing = 20f;
    public Vector2 cardSize = new Vector2(200f, 300f);
    
    [HideInInspector] public Vector2 currentCardSize;
    
    public int TotalCards => cardSprites.Count;
    public int TotalRows => Mathf.CeilToInt((float)cardSprites.Count / cardsPerRow);
}