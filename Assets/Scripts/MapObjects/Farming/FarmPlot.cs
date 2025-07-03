using UnityEngine;

public class FarmPlot : MonoBehaviour
{
    public enum PlotState { Default, Cultivated, Planted, Growing, ReadyToHarvest }

    // Fields
    [SerializeField] private GameObject plantPrefab;
    private Plant currentPlant;

    public void PlantSeed(PlantData plantData)
    {
        if (currentPlant != null)
        {
            Debug.Log("이미 식물이 심어져 있음");
            return;
        }
        // Plant 오브젝트 생성
        GameObject plantObject = Instantiate(plantPrefab, transform.position, Quaternion.identity);
        plantObject.transform.SetParent(transform);
        currentPlant = plantObject.GetComponent<Plant>();
        currentPlant.SetPlantData(plantData);
        currentPlant.SetFarmPlot(this);
    }

    void Start()
    {
        // 테스트용 당근 심기
        PlantSeed(PlantDataBase.Instance.GetPlantData("Carrot"));
    }

    void Update()
    {

    }
}
