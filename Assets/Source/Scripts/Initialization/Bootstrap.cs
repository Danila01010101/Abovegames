using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bootstrap : MonoBehaviour
{
    [Header("Initialization parents")]
    [SerializeField] private Canvas uIParent;
    
    [Header("Initializable prefabs")]
    [SerializeField] private GameObject[] prefabsToInitialize;
    
    [Header("Initialization order")]
    [SerializeField] private bool initializeOnAwake = true;
    [SerializeField] private float delayBetweenInitializations = 0.1f;
    
    public event Action OnInitializationComplete;
    public event Action<string> OnInitializationError;
    
    private int currentIndex = 0;
    private bool isInitializing = false;
    
    private void Awake()
    {
        if (initializeOnAwake)
        {
            StartInitialization();
        }
    }
    
    public void StartInitialization()
    {
        if (isInitializing)
        {
            Debug.LogWarning("Инициализация уже запущена");
            return;
        }
        
        isInitializing = true;
        currentIndex = 0;
        
        if (prefabsToInitialize == null || prefabsToInitialize.Length == 0)
        {
            CompleteInitialization();
            return;
        }
        
        InitializeNextPrefab();
    }
    
    private void InitializeNextPrefab()
    {
        if (currentIndex >= prefabsToInitialize.Length)
        {
            CompleteInitialization();
            return;
        }
        
        GameObject prefab = prefabsToInitialize[currentIndex];
        
        if (prefab == null)
        {
            HandleError($"Префаб {currentIndex} не назначен");
            currentIndex++;
            ContinueWithDelay();
            return;
        }
        
        try
        {
            InstantiateAndInitialize(prefab);
        }
        catch (Exception ex)
        {
            HandleError($"Ошибка инициализации {prefab.name}: {ex.Message}");
        }
        
        currentIndex++;
        ContinueWithDelay();
    }
    
    private void InstantiateAndInitialize(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab);
        instance.name = prefab.name;
    
        var initializable = instance.GetComponent<IInitializable>();
        if (initializable != null)
        {
            if (initializable.isUIElement())
            {
                RectTransform rectTransform = instance.GetComponent<RectTransform>();
                Vector2 savedSize = rectTransform != null ? rectTransform.sizeDelta : Vector2.zero;
            
                instance.transform.SetParent(uIParent.transform, false);
            
                if (rectTransform != null)
                {
                    StartCoroutine(DelayedSizeRestore(rectTransform, savedSize));
                }
            }
            else
            {
                instance.transform.SetParent(transform);
            }
        
            StartCoroutine(DelayedInitialize(initializable));
            Debug.Log($"Инициализирован: {prefab.name}");
        }
    }

    private IEnumerator DelayedSizeRestore(RectTransform rectTransform, Vector2 savedSize)
    {
        yield return null;
    
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    
        if (savedSize != Vector2.zero)
        {
            rectTransform.sizeDelta = savedSize;
        }
    
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private IEnumerator DelayedInitialize(IInitializable initializable)
    {
        yield return null;
        initializable.Initialize();
    }
    
    private void ContinueWithDelay()
    {
        if (delayBetweenInitializations > 0)
        {
            Invoke(nameof(InitializeNextPrefab), delayBetweenInitializations);
        }
        else
        {
            InitializeNextPrefab();
        }
    }
    
    private void HandleError(string errorMessage)
    {
        Debug.LogError($"[Инициализатор] {errorMessage}");
        OnInitializationError?.Invoke(errorMessage);
    }
    
    private void CompleteInitialization()
    {
        isInitializing = false;
        Debug.Log("Инициализация завершена");
        OnInitializationComplete?.Invoke();
    }
    
    public void AddPrefab(GameObject prefab)
    {
        if (prefab == null) return;
        
        Array.Resize(ref prefabsToInitialize, (prefabsToInitialize?.Length ?? 0) + 1);
        prefabsToInitialize[^1] = prefab;
    }
    
    public void ClearPrefabs()
    {
        prefabsToInitialize = Array.Empty<GameObject>();
    }
}

public interface IInitializable
{
    bool isUIElement();
    void Initialize();
}