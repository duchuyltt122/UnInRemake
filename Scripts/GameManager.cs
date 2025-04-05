using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    
    // Events
    public event Action OnGameStateChanged;
    public event Action<int> OnDayChanged;
    public event Action<float> OnTimeChanged;
    
    // Thời gian trong game
    [SerializeField] private float dayDuration = 300f; // 5 phút thực = 1 ngày trong game
    private float currentTime = 0f;
    private int currentDay = 1;
    
    // Trạng thái game
    public enum GameState { Playing, Paused, Shopping, MiniGame }
    private GameState _currentState;
    public GameState CurrentState 
    { 
        get => _currentState;
        set
        {
            _currentState = value;
            OnGameStateChanged?.Invoke();
        }
    }
    
    // Các manager khác
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private PigManager pigManager;
    [SerializeField] private UIManager uiManager;
    
    // Danh sách thành tựu của người chơi
    private List<Achievement> achievements = new List<Achievement>();
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Khởi tạo trạng thái ban đầu
        CurrentState = GameState.Playing;
    }
    
    private void Start()
    {
        // Khởi tạo thời gian và các manager
        if (resourceManager == null)
            resourceManager = FindObjectOfType<ResourceManager>();
            
        if (pigManager == null)
            pigManager = FindObjectOfType<PigManager>();
            
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
            
        // Load game data nếu có
        LoadGame();
    }
    
    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            UpdateGameTime();
        }
    }
    
    private void UpdateGameTime()
    {
        // Cập nhật thời gian
        currentTime += Time.deltaTime;
        OnTimeChanged?.Invoke(currentTime / dayDuration);
        
        // Kiểm tra xem đã hết ngày chưa
        if (currentTime >= dayDuration)
        {
            currentTime = 0;
            currentDay++;
            OnDayChanged?.Invoke(currentDay);
            
            // Xử lý các sự kiện cuối ngày
            EndOfDay();
        }
    }
    
    private void EndOfDay()
    {
        // Cập nhật trạng thái của các con heo (đói, lớn lên, sinh sản)
        pigManager.UpdateAllPigs();
        
        // Cập nhật tài nguyên
        resourceManager.EndOfDayUpdate();
        
        // Kiểm tra thành tựu mới
        CheckAchievements();
        
        // Lưu game
        SaveGame();
    }
    
    public void PauseGame()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0;
    }
    
    public void ResumeGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1;
    }
    
    public void EnterShopMode()
    {
        CurrentState = GameState.Shopping;
    }
    
    public void ExitShopMode()
    {
        CurrentState = GameState.Playing;
    }
    
    public void EnterMiniGame(string miniGameName)
    {
        CurrentState = GameState.MiniGame;
        // TODO: Load mini game scene
    }
    
    private void CheckAchievements()
    {
        // TODO: Kiểm tra và cập nhật thành tựu
    }
    
    public void SaveGame()
    {
        // TODO: Lưu trạng thái game
        Debug.Log("Game saved");
    }
    
    public void LoadGame()
    {
        // TODO: Tải trạng thái game
        Debug.Log("Game loaded");
    }
}
