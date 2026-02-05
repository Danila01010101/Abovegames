using UnityEngine;

public class DeviceLayoutCalculator
{
    [SerializeField, Range(1, 6)] private int columns = 3;
    [SerializeField, Range(0, 50)] private float spacing = 20f;
    [SerializeField, Range(0, 100)] private float padding = 20f;
    
    public Vector2 GetSquareSize(float containerWidth)
    {
        float availableWidth = containerWidth - (padding * 2) - (spacing * (columns - 1));
        float size = Mathf.Max(availableWidth / columns, 10f);
        return new Vector2(size, size);
    }
    
    public int GetColumns() => columns;
    public float GetSpacing() => spacing;
    public float GetPadding() => padding;
}