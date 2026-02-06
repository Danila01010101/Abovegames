using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OnlineImageLoader
{
    private string baseUrl = "http://data.ikppbb.com/test-task-unity-data/pics/";
    
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    
    public void SetUrl(string url) => baseUrl = url;
    
    public void LoadImage(string imageName, Action<Sprite> callback)
    {
        if (spriteCache.TryGetValue(imageName, out Sprite cachedSprite))
        {
            callback?.Invoke(cachedSprite);
            return;
        }
        
        CoroutineStarter.Instance.StartCoroutine(LoadImageCoroutine(imageName, callback));
    }
    
    private IEnumerator LoadImageCoroutine(string imageName, Action<Sprite> callback)
    {
        string url = $"{baseUrl}{imageName}.jpg";
        
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            
            spriteCache[imageName] = sprite;
            
            callback?.Invoke(sprite);
        }
        else
        {
            Debug.LogError($"Failed to load image: {imageName}, Error: {request.error}");
            callback?.Invoke(CreatePlaceholderSprite());
        }
    }
    
    private Sprite CreatePlaceholderSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.gray);
        texture.Apply();
        
        return Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );
    }
    
    public void ClearCache()
    {
        foreach (var sprite in spriteCache.Values)
        {
            if (sprite != null && sprite.texture != null)
            {
                GameObject.Destroy(sprite.texture);
            }
        }
        spriteCache.Clear();
    }
    
    private void OnDestroy()
    {
        ClearCache();
    }
}