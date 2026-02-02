using System;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [Header("Префабы для инициализации")]
    [SerializeField] private GameObject[] prefabsToInitialize;
    
    [Header("Порядок инициализации")]
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
        GameObject instance = Instantiate(prefab, transform);
        instance.name = prefab.name;
        
        var initializable = instance.GetComponent<IInitializable>();
        if (initializable != null)
        {
            initializable.Initialize();
            Debug.Log($"Инициализирован: {prefab.name}");
        }
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
    void Initialize();
}