using UnityEngine;
using System.Collections.Generic;

public enum ItemType {
    Special,
    Tool,
    Seed,
    Food,
    Equipment,
    MapObject,
    ETC
}

public class ItemDataBase : MonoSingleton<ItemDataBase>
{
    // 인스펙터에서 관리 용
    public List<ItemData> itemDatas;
    // 런타임 용 (해시맵)
    private Dictionary<string, ItemData> itemDict;

    protected override void Awake() {
        base.Awake();
        // 인스펙터에 등록한 리스트 기반으로 Dictionary 제작
        itemDict = new Dictionary<string, ItemData>();
        foreach (var item in itemDatas)
        {
            itemDict[item.ItemId] = item;
        }
    }

    public ItemData GetItemData(string itemId) {
        return itemDict.ContainsKey(itemId) ? itemDict[itemId] : null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
