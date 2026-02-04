using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class OnlineImageLoader : MonoBehaviour
{
    private const string BaseUrl = "http://data.ikppbb.com/test-task-unity-data/pics/";
    
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    
    public async Task<Sprite> LoadImageAsync(string imageName)
    {
        if (string.IsNullOrEmpty(imageName))
        {
            Debug.LogError("Image name is null or empty");
            return CreatePlaceholderSprite();
        }
        
        if (spriteCache.TryGetValue(imageName, out Sprite cachedSprite))
        {
            return cachedSprite;
        }
        
        // Просто формируем URL на основе имени карточки
        // Предполагаем, что imageName содержит номер изображения (например: "1", "2", "33")
        string url = $"{BaseUrl}{imageName}.jpg";
        
        Debug.Log($"Loading image from: {url}");
        
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
            
            CacheImage(imageName, texture, sprite);
            return sprite;
        }
        else
        {
            Debug.LogError($"Failed to load image {imageName}: {request.error}");
            return CreatePlaceholderSprite();
        }
    }
    
    private Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }
    
    private void CacheImage(string imageName, Texture2D texture, Sprite sprite)
    {
        if (!string.IsNullOrEmpty(imageName))
        {
            textureCache[imageName] = texture;
            spriteCache[imageName] = sprite;
        }
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
    
    public Sprite GetCachedSprite(string imageName)
    {
        spriteCache.TryGetValue(imageName, out Sprite sprite);
        return sprite;
    }
    
    public bool IsImageLoaded(string imageName)
    {
        return spriteCache.ContainsKey(imageName);
    }
    
    public void ClearMemoryCache()
    {
        foreach (var texture in textureCache.Values)
        {
            if (texture != null)
            {
                Destroy(texture);
            }
        }
        
        foreach (var sprite in spriteCache.Values)
        {
            if (sprite != null)
            {
                Destroy(sprite);
            }
        }
        
        textureCache.Clear();
        spriteCache.Clear();
    }
}