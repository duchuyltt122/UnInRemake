using UnityEngine;
using System.Collections.Generic;
using System;

public class PigManager : MonoBehaviour
{
    // Singleton pattern
    public static PigManager Instance { get; private set; }
    
    // Events
    public event Action<Pig> OnPigAdded;
    public event Action<Pig> OnPigRemoved;
    
    // Danh sách heo
    [SerializeField] private List<Pig> pigs = new List<Pig>();
    
    // Prefabs
    [SerializeField] private GameObject normalPigPrefab;
    [SerializeField] private GameObject spottedPigPrefab;
    [SerializeField] private GameObject blackPigPrefab;
    [SerializeField] private GameObject goldenPigPrefab;
    
    // Chuồng heo
    [SerializeField] private int maxPigs = 5;
    [SerializeField] private Transform[] pigPositions;
    
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
        // Khởi tạo các con heo ban đầu
        if (pigs.Count == 0)
        {
            AddNewPig(Pig.PigType.Normal);
        }
    }
    
    public bool CanAddPig()
    {
        return pigs.Count < maxPigs;
    }
    
    public Pig AddNewPig(Pig.PigType pigType)
    {
        if (!CanAddPig())
        {
            Debug.LogWarning("Đã đạt đến số lượng heo tối đa!");
            return null;
        }
        
        // Chọn prefab dựa trên loại heo
        GameObject pigPrefab = null;
        switch (pigType)
        {
            case Pig.PigType.Normal:
                pigPrefab = normalPigPrefab;
                break;
            case Pig.PigType.Spotted:
                pigPrefab = spottedPigPrefab;
                break;
            case Pig.PigType.Black:
                pigPrefab = blackPigPrefab;
                break;
            case Pig.PigType.Golden:
                pigPrefab = goldenPigPrefab;
                break;
        }
        
        if (pigPrefab == null)
        {
            Debug.LogError("Không tìm thấy prefab cho loại heo: " + pigType);
            return null;
        }
        
        // Tạo vị trí cho heo mới
        int index = pigs.Count;
        Vector3 position = (index < pigPositions.Length) ? 
            pigPositions[index].position : 
            new Vector3(UnityEngine.Random.Range(-5f, 5f), 0, UnityEngine.Random.Range(-5f, 5f));
        
        // Tạo heo mới
        GameObject pigObject = Instantiate(pigPrefab, position, Quaternion.identity, transform);
        Pig newPig = pigObject.GetComponent<Pig>();
        
        // Đặt tên cho heo
        newPig.name = "Pig_" + (pigs.Count + 1);
        
        // Đăng ký sự kiện
        newPig.OnPigDeath += () => RemovePig(newPig);
        
        // Thêm vào danh sách
        pigs.Add(newPig);
        OnPigAdded?.Invoke(newPig);
        
        return newPig;
    }
    
    public void RemovePig(Pig pig)
    {
        if (pigs.Contains(pig))
        {
            pigs.Remove(pig);
            OnPigRemoved?.Invoke(pig);
        }
    }
    
    public void SellPig(Pig pig)
    {
        if (pigs.Contains(pig))
        {
            // Nhận tiền bán heo
            int value = pig.GetValue();
            ResourceManager.Instance.AddCoins(value);
            
            // Xóa heo
            RemovePig(pig);
            Destroy(pig.gameObject);
        }
    }
    
    public void UpdateAllPigs()
    {
        // Cập nhật tất cả các con heo mỗi ngày
        foreach (Pig pig in pigs.ToArray()) // ToArray để tránh lỗi nếu có heo bị xóa trong quá trình lặp
        {
            pig.DailyUpdate();
        }
    }
    
    public int GetPigCount()
    {
        return pigs.Count;
    }
    
    public List<Pig> GetAllPigs()
    {
        return pigs;
    }
    
    public void UpgradeMaxPigs(int additionalSlots)
    {
        maxPigs += additionalSlots;
    }
}
