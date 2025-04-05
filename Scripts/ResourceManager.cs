using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    // Singleton pattern
    public static ResourceManager Instance { get; private set; }
    
    // Events
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnFeedChanged;
    public event Action<int> OnGemsChanged;
    
    // Tài nguyên
    [SerializeField] private int coins = 5000;
    [SerializeField] private int feed = 50;
    [SerializeField] private int gems = 10;
    
    // Kho đồ
    [System.Serializable]
    public class InventoryItem
    {
        public string itemId;
        public string itemName;
        public Sprite icon;
        public int quantity;
        public int maxStack = 99;
    }
    
    [SerializeField] private List<InventoryItem> inventory = new List<InventoryItem>();
    
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
    
    #region Resource Management
    
    public int GetCoins()
    {
        return coins;
    }
    
    public int GetFeed()
    {
        return feed;
    }
    
    public int GetGems()
    {
        return gems;
    }
    
    public bool AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
        return true;
    }
    
    public bool RemoveCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            OnCoinsChanged?.Invoke(coins);
            return true;
        }
        return false;
    }
    
    public bool AddFeed(int amount)
    {
        feed += amount;
        OnFeedChanged?.Invoke(feed);
        return true;
    }
    
    public bool RemoveFeed(int amount)
    {
        if (feed >= amount)
        {
            feed -= amount;
            OnFeedChanged?.Invoke(feed);
            return true;
        }
        return false;
    }
    
    public bool AddGems(int amount)
    {
        gems += amount;
        OnGemsChanged?.Invoke(gems);
        return true;
    }
    
    public bool RemoveGems(int amount)
    {
        if (gems >= amount)
        {
            gems -= amount;
            OnGemsChanged?.Invoke(gems);
            return true;
        }
        return false;
    }
    
    #endregion
    
    #region Inventory Management
    
    public bool AddItem(string itemId, int quantity = 1)
    {
        // Tìm item trong kho
        InventoryItem existingItem = inventory.Find(item => item.itemId == itemId);
        
        if (existingItem != null)
        {
            // Kiểm tra xem có vượt quá stack tối đa không
            if (existingItem.quantity + quantity <= existingItem.maxStack)
            {
                existingItem.quantity += quantity;
                return true;
            }
            else
            {
                // Xử lý trường hợp vượt quá stack tối đa
                return false;
            }
        }
        else
        {
            // Tạo item mới nếu chưa có trong kho
            // TODO: Load item data từ database
            InventoryItem newItem = new InventoryItem
            {
                itemId = itemId,
                itemName = "Item " + itemId, // Placeholder
                quantity = quantity
            };
            
            inventory.Add(newItem);
            return true;
        }
    }
    
    public bool RemoveItem(string itemId, int quantity = 1)
    {
        InventoryItem existingItem = inventory.Find(item => item.itemId == itemId);
        
        if (existingItem != null && existingItem.quantity >= quantity)
        {
            existingItem.quantity -= quantity;
            
            // Xóa item khỏi kho nếu số lượng = 0
            if (existingItem.quantity <= 0)
            {
                inventory.Remove(existingItem);
            }
            
            return true;
        }
        
        return false;
    }
    
    public int GetItemQuantity(string itemId)
    {
        InventoryItem item = inventory.Find(i => i.itemId == itemId);
        return item != null ? item.quantity : 0;
    }
    
    public List<InventoryItem> GetAllItems()
    {
        return inventory;
    }
    
    #endregion
    
    public void EndOfDayUpdate()
    {
        // Cập nhật tài nguyên cuối ngày
        // Ví dụ: Thu hoạch tự động, tiền thuê đất,...
    }
}
