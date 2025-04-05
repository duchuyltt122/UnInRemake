using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    // Singleton pattern
    public static UIManager Instance { get; private set; }
    
    // UI chính
    [Header("Main UI")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI feedText;
    [SerializeField] private TextMeshProUGUI gemText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private Slider timeSlider;
    
    // UI theo dõi thú cưng
    [Header("Pet Monitor")]
    [SerializeField] private GameObject petMonitorPanel;
    [SerializeField] private TextMeshProUGUI pigNameText;
    [SerializeField] private Image pigImage;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider happinessSlider;
    [SerializeField] private Slider cleanlinessSlider;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Button feedButton;
    [SerializeField] private Button petButton;
    [SerializeField] private Button cleanButton;
    [SerializeField] private Button medicineButton;
    
    // UI cửa hàng
    [Header("Shop UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopItemContainer;
    [SerializeField] private GameObject shopItemPrefab;
    
    // UI kho đồ
    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryItemContainer;
    [SerializeField] private GameObject inventoryItemPrefab;
    
    // UI popup thông báo
    [Header("Notification")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    
    // UI menu chính
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    
    // UI cài đặt
    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    
    // Các biến lưu trữ
    private Pig selectedPig;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Kết nối với GameManager
        GameManager.Instance.OnGameStateChanged += UpdateUI;
        GameManager.Instance.OnDayChanged += UpdateDayText;
        GameManager.Instance.OnTimeChanged += UpdateTimeSlider;
        
        // Kết nối với ResourceManager
        ResourceManager.Instance.OnCoinsChanged += UpdateCoinText;
        ResourceManager.Instance.OnFeedChanged += UpdateFeedText;
        ResourceManager.Instance.OnGemsChanged += UpdateGemText;
        
        // Khởi tạo UI
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        // Khởi tạo giá trị ban đầu
        UpdateCoinText(ResourceManager.Instance.GetCoins());
        UpdateFeedText(ResourceManager.Instance.GetFeed());
        UpdateGemText(ResourceManager.Instance.GetGems());
        UpdateDayText(1);
        
        // Khởi tạo các panel
        petMonitorPanel.SetActive(false);
        shopPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        notificationPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        
        // Thiết lập các button
        feedButton.onClick.AddListener(() => {
            if (selectedPig != null) selectedPig.Feed();
        });
        
        petButton.onClick.AddListener(() => {
            if (selectedPig != null) selectedPig.Pet();
        });
        
        cleanButton.onClick.AddListener(() => {
            if (selectedPig != null) selectedPig.Clean();
        });
        
        medicineButton.onClick.AddListener(() => {
            if (selectedPig != null) selectedPig.GiveMedicine();
        });
        
        continueButton.onClick.AddListener(() => {
            mainMenuPanel.SetActive(false);
            GameManager.Instance.ResumeGame();
        });
        
        newGameButton.onClick.AddListener(() => {
            // TODO: Reset game
            mainMenuPanel.SetActive(false);
            GameManager.Instance.ResumeGame();
        });
        
        settingsButton.onClick.AddListener(() => {
            mainMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
        });
        
        quitButton.onClick.AddListener(() => {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        });
    }
    
    public void UpdateUI()
    {
        // Cập nhật UI dựa trên trạng thái game
        switch (GameManager.Instance.CurrentState)
        {
            case GameManager.GameState.Paused:
                mainMenuPanel.SetActive(true);
                break;
            case GameManager.GameState.Shopping:
                shopPanel.SetActive(true);
                RefreshShopItems();
                break;
            case GameManager.GameState.Playing:
                mainMenuPanel.SetActive(false);
                shopPanel.SetActive(false);
                settingsPanel.SetActive(false);
                break;
        }
    }
    
    #region Resource UI Updates
    
    private void UpdateCoinText(int value)
    {
        if (coinText != null)
            coinText.text = value.ToString("N0");
    }
    
    private void UpdateFeedText(int value)
    {
        if (feedText != null)
            feedText.text = value.ToString("N0");
    }
    
    private void UpdateGemText(int value)
    {
        if (gemText != null)
            gemText.text = value.ToString("N0");
    }
    
    private void UpdateDayText(int day)
    {
        if (dayText != null)
            dayText.text = "Ngày " + day.ToString();
    }
    
    private void UpdateTimeSlider(float normalizedTime)
    {
        if (timeSlider != null)
            timeSlider.value = normalizedTime;
    }
    
    #endregion
    
    #region Pig UI
    
    public void ShowPigInfo(Pig pig)
    {
        selectedPig = pig;
        
        if (petMonitorPanel != null)
        {
            petMonitorPanel.SetActive(true);
            
            // Cập nhật thông tin
            if (pigNameText != null)
                pigNameText.text = pig.name;
                
            if (pigImage != null)
                pigImage.sprite = pig.GetComponent<SpriteRenderer>()?.sprite;
                
            UpdatePigStatus();
            
            // Đăng ký sự kiện
            pig.OnPigStateChanged += UpdatePigStatus;
        }
    }
    
    public void HidePigInfo()
    {
        if (selectedPig != null)
        {
            selectedPig.OnPigStateChanged -= UpdatePigStatus;
            selectedPig = null;
        }
        
        if (petMonitorPanel != null)
            petMonitorPanel.SetActive(false);
    }
    
    private void UpdatePigStatus()
    {
        if (selectedPig == null)
            return;
            
        // Lấy giá trị từ các thuộc tính (dùng reflection vì thuộc tính private)
        System.Type type = selectedPig.GetType();
        float hunger = (float)type.GetField("hunger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(selectedPig);
        float happiness = (float)type.GetField("happiness", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(selectedPig);
        float cleanliness = (float)type.GetField("cleanliness", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(selectedPig);
        float health = (float)type.GetField("health", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(selectedPig);
        
        // Cập nhật giá trị vào slider
        if (hungerSlider != null)
            hungerSlider.value = hunger / 100f;
            
        if (happinessSlider != null)
            happinessSlider.value = happiness / 100f;
            
        if (cleanlinessSlider != null)
            cleanlinessSlider.value = cleanliness / 100f;
            
        if (healthSlider != null)
            healthSlider.value = health / 100f;
            
        // Cập nhật trạng thái các nút
        feedButton.interactable = ResourceManager.Instance.GetFeed() >= selectedPig.GetType().GetField("feedConsumption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(selectedPig) as int? ?? 2;
        medicineButton.interactable = ResourceManager.Instance.GetItemQuantity("medicine") > 0;
    }
    
    #endregion
    
    #region Shop UI
    
    public void OpenShop()
    {
        GameManager.Instance.EnterShopMode();
        shopPanel.SetActive(true);
        RefreshShopItems();
    }
    
    public void CloseShop()
    {
        shopPanel.SetActive(false);
        GameManager.Instance.ExitShopMode();
    }
    
    private void RefreshShopItems()
    {
        // Xóa các item cũ
        foreach (Transform child in shopItemContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Tạo các item cửa hàng
        CreateShopItem("feed_bag", "Bao Thức Ăn", "Thêm 20 đơn vị thức ăn", 200);
        CreateShopItem("medicine", "Thuốc", "Chữa bệnh cho heo", 500);
        CreateShopItem("normal_pig", "Heo Thường", "Heo bình thường", 1000);
        CreateShopItem("spotted_pig", "Heo Đốm", "Heo đốm quý hiếm", 2000);
        CreateShopItem("black_pig", "Heo Đen", "Heo đen quý hiếm", 3000);
        CreateShopItem("golden_pig", "Heo Vàng", "Heo vàng cực kỳ quý hiếm", 5000);
        CreateShopItem("pig_house_upgrade", "Nâng Cấp Chuồng", "Thêm chỗ cho 1 heo", 2500);
    }
    
    private void CreateShopItem(string itemId, string name, string description, int price)
    {
        GameObject itemObj = Instantiate(shopItemPrefab, shopItemContainer);
        ShopItemUI shopItem = itemObj.GetComponent<ShopItemUI>();
        
        if (shopItem != null)
        {
            shopItem.Initialize(itemId, name, description, price);
            shopItem.OnBuyClicked += BuyItem;
        }
    }
    
    private void BuyItem(string itemId, int price)
    {
        if (ResourceManager.Instance.GetCoins() >= price)
        {
            bool success = false;
            
            switch (itemId)
            {
                case "feed_bag":
                    success = ResourceManager.Instance.AddFeed(20);
                    break;
                case "medicine":
                    success = ResourceManager.Instance.AddItem("medicine");
                    break;
                case "normal_pig":
                    if (PigManager.Instance.CanAddPig())
                    {
                        PigManager.Instance.AddNewPig(Pig.PigType.Normal);
                        success = true;
                    }
                    break;
                case "spotted_pig":
                    if (PigManager.Instance.CanAddPig())
                    {
                        PigManager.Instance.AddNewPig(Pig.PigType.Spotted);
                        success = true;
                    }
                    break;
                case "black_pig":
                    if (PigManager.Instance.CanAddPig())
                    {
                        PigManager.Instance.AddNewPig(Pig.PigType.Black);
                        success = true;
                    }
                    break;
                case "golden_pig":
                    if (PigManager.Instance.CanAddPig())
                    {
                        PigManager.Instance.AddNewPig(Pig.PigType.Golden);
                        success = true;
                    }
                    break;
                case "pig_house_upgrade":
                    PigManager.Instance.UpgradeMaxPigs(1);
                    success = true;
                    break;
            }
            
            if (success)
            {
                ResourceManager.Instance.RemoveCoins(price);
                ShowNotification("Mua hàng thành công!");
            }
            else
            {
                ShowNotification("Không thể mua hàng.");
            }
        }
        else
        {
            ShowNotification("Không đủ tiền!");
        }
    }
    
    #endregion
    
    #region Inventory UI
    
    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        RefreshInventoryItems();
    }
    
    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }
    
    private void RefreshInventoryItems()
    {
        // Xóa các item cũ
        foreach (Transform child in inventoryItemContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Lấy danh sách item và hiển thị
        List<ResourceManager.InventoryItem> items = ResourceManager.Instance.GetAllItems();
        
        foreach (var item in items)
        {
            GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryItemContainer);
            InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
            
            if (itemUI != null)
            {
                itemUI.Initialize(item.itemId, item.itemName, item.quantity, item.icon);
                itemUI.OnUseClicked += UseItem;
            }
        }
    }
    
    private void UseItem(string itemId)
    {
        bool used = false;
        
        switch (itemId)
        {
            case "medicine":
                if (selectedPig != null)
                {
                    selectedPig.GiveMedicine();
                    used = true;
                }
                else
                {
                    ShowNotification("Hãy chọn một con heo trước!");
                }
                break;
                
            // Thêm các loại item khác ở đây
        }
        
        if (used)
        {
            ResourceManager.Instance.RemoveItem(itemId);
            RefreshInventoryItems();
        }
    }
    
    #endregion
    
    #region Notifications
    
    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);
            
            // Tự động ẩn thông báo sau một khoảng thời gian
            CancelInvoke("HideNotification");
            Invoke("HideNotification", duration);
        }
    }
    
    private void HideNotification()
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }
    
    #endregion
    
    public void OpenMainMenu()
    {
        GameManager.Instance.PauseGame();
        mainMenuPanel.SetActive(true);
    }
    
    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    
    public void ApplySettings()
    {
        // TODO: Lưu và áp dụng cài đặt
        CloseSettingsPanel();
    }
}

// Class hỗ trợ cho UI các item trong cửa hàng
public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button buyButton;
    
    private string itemId;
    private int price;
    
    public System.Action<string, int> OnBuyClicked;
    
    public void Initialize(string id, string name, string description, int itemPrice)
    {
        itemId = id;
        price = itemPrice;
        
        if (nameText != null)
            nameText.text = name;
            
        if (descriptionText != null)
            descriptionText.text = description;
            
        if (priceText != null)
            priceText.text = itemPrice.ToString("N0");
            
        // TODO: Load icon từ resources
        
        if (buyButton != null)
            buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke(itemId, price));
    }
}

// Class hỗ trợ cho UI các item trong kho đồ
public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button useButton;
    
    private string itemId;
    
    public System.Action<string> OnUseClicked;
    
    public void Initialize(string id, string name, int quantity, Sprite icon)
    {
        itemId = id;
        
        if (nameText != null)
            nameText.text = name;
            
        if (quantityText != null)
            quantityText.text = "x" + quantity.ToString();
            
        if (iconImage != null && icon != null)
            iconImage.sprite = icon;
            
        if (useButton != null)
            useButton.onClick.AddListener(() => OnUseClicked?.Invoke(itemId));
    }
}
