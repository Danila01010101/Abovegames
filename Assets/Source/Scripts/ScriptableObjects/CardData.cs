using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Content Settings")]
    public List<string> cardNames = new List<string>();
}