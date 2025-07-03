using UnityEngine;
using System.Collections.Generic;

public class PlantDataBase : MonoSingleton<PlantDataBase>
{
    // 인스펙터에서 관리 용
    public List<PlantData> plantDatas;
    // 런타임 용 (해시맵)
    private Dictionary<string, PlantData> plantDict;

    protected override void Awake()
    {
        base.Awake();
        // 인스펙터에 등록한 리스트 기반으로 Dictionary 제작
        plantDict = new Dictionary<string, PlantData>();
        foreach (var plant in plantDatas)
        {
            plantDict[plant.PlantId] = plant;
        }
    }

    public PlantData GetPlantData(string plantId)
    {
        return plantDict.ContainsKey(plantId) ? plantDict[plantId] : null;
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
