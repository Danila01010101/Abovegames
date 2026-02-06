using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "Tab Bar Data", menuName = "Card System/new Tab Bar Data")]
public class TabBarData : ScriptableObject
{
    public CardData cardData;
    public float edgePadding;
    [Header ("BannerScroll")]
    public float switchDuration = 0.5f;
    public Ease easeType = Ease.OutCubic;
    public float autoScrollInterval = 5f;
    public float swipeThreshold = 100f;
    [Header ("Card Pool")]
    public ImageCard cardPrefab;
    public int initialPoolSize = 66;
    public bool expandPool = false;
    
}
