using System.Collections.Generic;
using UnityEngine;

public enum FilterType
{
    All,
    Odd,
    Even
}

public interface IContentFilter<T>
{
    List<T> Filter(List<T> items);
    FilterType GetFilterType();
}

public class AllFilter<T> : IContentFilter<T>
{
    public List<T> Filter(List<T> items) => new List<T>(items);
    public FilterType GetFilterType() => FilterType.All;
}

public class OddFilter<T> : IContentFilter<T>
{
    public List<T> Filter(List<T> items)
    {
        var filtered = new List<T>();
        for (int i = 0; i < items.Count; i++)
            if (i % 2 == 0) filtered.Add(items[i]);
        return filtered;
    }
    
    public FilterType GetFilterType() => FilterType.Odd;
}

public class EvenFilter<T> : IContentFilter<T>
{
    public List<T> Filter(List<T> items)
    {
        var filtered = new List<T>();
        for (int i = 0; i < items.Count; i++)
            if (i % 2 == 1) filtered.Add(items[i]);
        return filtered;
    }
    
    public FilterType GetFilterType() => FilterType.Even;
}