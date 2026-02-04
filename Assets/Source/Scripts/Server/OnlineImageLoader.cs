using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class OnlineImageLoader : MonoBehaviour
{
    private const string BaseUrl = "http://data.ikppbb.com/test-task-unity-data/pics/";
    private const int TotalImages = 66;
    
    private Dictionary<int, Texture2D> textureCache = new Dictionary<int, Texture2D>();
    private Dictionary<int, Sprite> spriteCache = new Dictionary<int, Sprite>();
    
    public int GetTotalImages() => TotalImages;
    
    public bool IsImageLoaded(int imageNumber)
    {
        return spriteCache.ContainsKey(imageNumber);
    }
    
    public Sprite GetCachedSprite(int imageNumber)
    {
        spriteCache.TryGetValue(imageNumber, out Sprite sprite);
        return sprite;
    }
    
    public async Task<Sprite> LoadImageAsync(int imageNumber)
    {
        if (spriteCache.TryGetValue(imageNumber, out Sprite cachedSprite))
        {
            return cachedSprite;
        }
        
        string url = $"{BaseUrl}{imageNumber}.jpg";
        
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        
        while (!operation.isDone)
        {
            await Task.Yield();
        }
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = CreateSpriteFromTexture(texture);
            
            CacheImage(imageNumber, texture, sprite);
            return sprite;
        }
        
        return CreatePlaceholderSprite();
    }
    
    private Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }
    
    private void CacheImage(int imageNumber, Texture2D texture, Sprite sprite)
    {
        textureCache[imageNumber] = texture;
        spriteCache[imageNumber] = sprite;
    }
    
    private Sprite CreatePlaceholderSprite()
    {
        Texture2D placeholder = new Texture2D(1, 1);
        placeholder.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f));
        placeholder.Apply();
        
        return Sprite.Create(
            placeholder,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );
    }
    
    public void ClearMemoryCache()
    {
        foreach (var texture in textureCache.Values)
        {
            Destroy(texture);
        }
        
        foreach (var sprite in spriteCache.Values)
        {
            Destroy(sprite);
        }
        
        textureCache.Clear();
        spriteCache.Clear();
    }
}