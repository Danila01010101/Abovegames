using UnityEngine;

[CreateAssetMenu(fileName = "Tab Bar Data", menuName = "Card System/new Tab Bar Data")]
public class TabBarData : ScriptableObject
{
    [Header ("Card Pool")]
    [SerializeField] private ImageCard cardPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private bool expandPool = true;
}
