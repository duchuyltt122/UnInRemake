using UnityEngine;
using System;

public class Pig : MonoBehaviour
{
    // Events
    public event Action OnPigStateChanged;
    public event Action OnPigLevelUp;
    public event Action OnPigDeath;
    
    // Thông tin cơ bản
    [SerializeField] private string pigId;
    [SerializeField] private string pigName;
    [SerializeField] private int level = 1;
    [SerializeField] private PigType pigType;
    [SerializeField] private Sprite pigSprite;
    
    // Thuộc tính
    [SerializeField] private float hunger = 100f; // 0-100
    [SerializeField] private float happiness = 100f; // 0-100
    [SerializeField] private float cleanliness = 100f; // 0-100
    [SerializeField] private float health = 100f; // 0-100
    
    // Tốc độ giảm các chỉ số mỗi ngày
    [SerializeField] private float hungerDecreaseRate = 20f;
    [SerializeField] private float happinessDecreaseRate = 15f;
    [SerializeField] private float cleanlinessDecreaseRate = 25f;
    
    // Thời gian phát triển
    [SerializeField] private int daysToGrow = 3;
    [SerializeField] private int currentGrowthDay = 0;
    
    // Giá trị
    [SerializeField] private int baseValue = 500;
    [SerializeField] private int feedConsumption = 2;
    
    // Trạng thái
    public enum PigState { Idle, Eating, Sleeping, Playing, Sick }
    private PigState _currentState = PigState.Idle;
    public PigState CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            OnPigStateChanged?.Invoke();
            UpdateAnimation();
        }
    }
    
    // Các thành phần
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    // Định nghĩa loại heo
    public enum PigType { Normal, Spotted, Black, Golden }
    
    private void Start()
    {
        // Khởi tạo các thành phần
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        if (animator == null)
            animator = GetComponent<Animator>();
            
        // Cập nhật hình ảnh theo loại heo
        UpdateVisual();
    }
    
    private void UpdateVisual()
    {
        // Cập nhật sprite và animation theo loại heo và level
        // TODO: Load sprite từ resource
    }
    
    private void UpdateAnimation()
    {
        // Cập nhật animation theo trạng thái hiện tại
        if (animator != null)
        {
            animator.SetInteger("PigState", (int)CurrentState);
        }
    }
    
    #region Actions
    
    public void Feed()
    {
        if (ResourceManager.Instance.RemoveFeed(feedConsumption))
        {
            hunger = Mathf.Min(hunger + 40f, 100f);
            happiness = Mathf.Min(happiness + 10f, 100f);
            cleanliness = Mathf.Max(cleanliness - 10f, 0f);
            
            // Chuyển sang trạng thái ăn
            CurrentState = PigState.Eating;
            
            // Trở lại trạng thái idle sau khi ăn xong
            Invoke("SetIdleState", 3f);
        }
    }
    
    public void Pet()
    {
        happiness = Mathf.Min(happiness + 15f, 100f);
        
        // Chuyển sang trạng thái chơi
        CurrentState = PigState.Playing;
        
        // Trở lại trạng thái idle sau khi chơi xong
        Invoke("SetIdleState", 2f);
    }
    
    public void Clean()
    {
        cleanliness = 100f;
        happiness = Mathf.Min(happiness + 5f, 100f);
        
        // Hiệu ứng làm sạch
        // TODO: Thêm hiệu ứng particle
    }
    
    public void GiveMedicine()
    {
        if (ResourceManager.Instance.RemoveItem("medicine"))
        {
            health = 100f;
            
            // Hồi phục từ bệnh
            if (CurrentState == PigState.Sick)
            {
                CurrentState = PigState.Idle;
            }
        }
    }
    
    private void SetIdleState()
    {
        // Trở về trạng thái nghỉ ngơi
        CurrentState = PigState.Idle;
    }
    
    #endregion
    
    public void DailyUpdate()
    {
        // Giảm các chỉ số hàng ngày
        hunger = Mathf.Max(hunger - hungerDecreaseRate, 0f);
        happiness = Mathf.Max(happiness - happinessDecreaseRate, 0f);
        cleanliness = Mathf.Max(cleanliness - cleanlinessDecreaseRate, 0f);
        
        // Kiểm tra sức khỏe
        UpdateHealth();
        
        // Kiểm tra tăng trưởng
        CheckGrowth();
    }
    
    private void UpdateHealth()
    {
        // Sức khỏe giảm nếu đói hoặc bẩn
        if (hunger < 20f || cleanliness < 20f)
        {
            health = Mathf.Max(health - 15f, 0f);
        }
        else if (hunger > 80f && cleanliness > 80f)
        {
            health = Mathf.Min(health + 5f, 100f);
        }
        
        // Kiểm tra xem heo có bị bệnh không
        if (health < 30f && CurrentState != PigState.Sick)
        {
            CurrentState = PigState.Sick;
        }
        
        // Kiểm tra xem heo có chết không
        if (health <= 0f)
        {
            Die();
        }
    }
    
    private void CheckGrowth()
    {
        if (level < 3) // Giả sử có 3 cấp độ
        {
            currentGrowthDay++;
            
            if (currentGrowthDay >= daysToGrow && hunger > 50f && health > 70f)
            {
                LevelUp();
            }
        }
    }
    
    private void LevelUp()
    {
        level++;
        currentGrowthDay = 0;
        baseValue *= 2; // Giá trị tăng theo cấp độ
        
        // Tăng kích thước heo
        transform.localScale = new Vector3(
            transform.localScale.x * 1.2f,
            transform.localScale.y * 1.2f,
            transform.localScale.z
        );
        
        // Cập nhật hình ảnh
        UpdateVisual();
        
        // Thông báo
        OnPigLevelUp?.Invoke();
    }
    
    private void Die()
    {
        // Thông báo
        OnPigDeath?.Invoke();
        
        // Xóa khỏi game
        Destroy(gameObject);
    }
    
    public int GetValue()
    {
        // Giá trị dựa trên cấp độ và các chỉ số
        float healthMultiplier = health / 100f;
        float happinessMultiplier = happiness / 100f;
        
        return Mathf.RoundToInt(baseValue * level * healthMultiplier * happinessMultiplier);
    }
}
