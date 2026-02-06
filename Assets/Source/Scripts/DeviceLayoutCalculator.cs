using UnityEngine;

public class DeviceLayoutCalculator
{
    private int phoneColumns = 2;
    private int tabletColumns = 3;
    private float spacing = 20f;
    private float padding = 20f;
    
    private int cachedColumns = -1;
    private bool cachedIsTablet = false;
    
    public DeviceLayoutCalculator(int phoneCols = 2, int tabletCols = 3, float spacing = 20f, float padding = 20f)
    {
        this.phoneColumns = phoneCols;
        this.tabletColumns = tabletCols;
        this.spacing = spacing;
        this.padding = padding;
        
        UpdateCache();
    }
    
    public Vector2 GetSquareSize(float containerWidth)
    {
        int columns = GetColumns();
        float availableWidth = containerWidth - (padding * 2) - (spacing * (columns - 1));
        float size = Mathf.Max(availableWidth / columns, 10f);
        return new Vector2(size, size);
    }
    
    public int GetColumns()
    {
        if (cachedColumns == -1 || ScreenChanged())
        {
            UpdateCache();
        }
        return cachedColumns;
    }
    
    public bool IsTablet()
    {
        if (ScreenChanged())
        {
            UpdateCache();
        }
        return cachedIsTablet;
    }
    
    public float GetSpacing() => spacing;
    public float GetPadding() => padding;
    
    private bool ScreenChanged()
    {
        return true;
    }
    
    private void UpdateCache()
    {
        cachedIsTablet = IsTabletByDeviceModel();
        cachedColumns = cachedIsTablet ? tabletColumns : phoneColumns;
    }
    
    public bool IsTabletByDeviceModel()
    {
        string model = SystemInfo.deviceModel.ToLower();
        
        if (model.Contains("ipad"))
        {
            return true; 
        }
        if (model.Contains("iphone") || model.Contains("ipod"))
        {
            return false;
        }
        
        string[] tabletKeywords = {
            "tab",          // Samsung Galaxy Tab, Lenovo Tab
            "pad",          // Huawei MatePad, Xiaomi Mi Pad
            "tablet",       // Общее название
            "sm-t",         // Samsung Tab серия
            "sm-p",         // Samsung Tab Pro серия
            "pixel c",      // Google Pixel C
            "nexus 7",      // Nexus 7
            "nexus 9",      // Nexus 9
            "nexus 10",     // Nexus 10
            "mediapad",     // Huawei MediaPad
            "matepad",      // Huawei MatePad
            "mipad",        // Xiaomi Mi Pad
            "lenovo tab",   // Lenovo Tab
            "galaxy tab",   // Samsung Galaxy Tab
            "galaxy view",  // Samsung Galaxy View
            "surface",      // Microsoft Surface (может быть на Android)
            "kindle",       // Amazon Kindle Fire
            "kf",           // Kindle Fire (сокращение)
        };
        
        string[] phoneKeywords = {
            "phone",
            "galaxy s",    
            "galaxy a",    
            "galaxy note",  
            "pixel",       
            "oneplus",
            "redmi",
            "xiaomi",
            "huawei p",   
            "huawei mate", 
            "oppo",
            "vivo",
            "realme",
            "motorola",
            "nokia",
            "sony xperia",
            "lg g",
            "lg v"
        };
        
        foreach (string keyword in tabletKeywords)
        {
            if (model.Contains(keyword))
            {
                return true;
            }
        }
        
        foreach (string keyword in phoneKeywords)
        {
            if (model.Contains(keyword))
            {
                return false;
            }
        }
        
        return IsTabletByScreen();
    }

    private bool IsTabletByScreen()
    {
        float diagonal = GetScreenDiagonalInches();
        float aspectRatio = (float)Screen.width / Screen.height;
        
        return diagonal > 6.5f || aspectRatio < 1.7f;
    }

    private float GetScreenDiagonalInches()
    {
        if (Screen.dpi <= 0) return 0;
        float widthIn = Screen.width / Screen.dpi;
        float heightIn = Screen.height / Screen.dpi;
        return Mathf.Sqrt(widthIn * widthIn + heightIn * heightIn);
    }
    
    private float GetDiagonalInches()
    {
        if (Screen.dpi <= 0) return 0;
        float widthIn = Screen.width / Screen.dpi;
        float heightIn = Screen.height / Screen.dpi;
        return Mathf.Sqrt(widthIn * widthIn + heightIn * heightIn);
    }
}