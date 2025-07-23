using UnityEngine;

public class FarmPlot : MonoBehaviour, IInteractable
{
    public enum PlotState { Dry, Normal, Wet, GemFertilized }

    private SpriteRenderer spriteRenderer;

    [SerializeField] private PlotState currentPlotState;
    public PlotState CurrentPlotState => currentPlotState;

    [SerializeField] private Sprite[] stateSprites;
    private ToolManager playerToolManager;

    // interactable
    public InteractionType InteractionType => InteractionType.Interact;
    public bool IsInteractable {
        get {
            if (currentPlant == null) { 
                return playerToolManager?.EquippedTool is SeedTool;
            }
            return currentPlant.IsHarvestable();
        }
    }

    // Fields
    [SerializeField] private GameObject plantPrefab;

    private Plant currentPlant;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool PlantSeed(PlantData plantData) {
        if (currentPlant != null) {
            Debug.Log("이미 식물이 심어져 있음");
            return false;
        }
        // Plant 오브젝트 생성
        GameObject plantObject = Instantiate(plantPrefab, transform.position, Quaternion.identity);
        plantObject.transform.SetParent(transform);
        currentPlant = plantObject.GetComponent<Plant>();
        currentPlant.SetPlantData(plantData);
        currentPlant.SetFarmPlot(this);

        return true;
    }

    public float GetGrowthSpeedMultiplier() {
        return currentPlotState switch {
            PlotState.Dry => 0f,
            PlotState.Normal => 0.5f,
            PlotState.Wet => 1f,
            PlotState.GemFertilized => 2f,
            _ => 1f
        };
    }

    public void SetPlotState(PlotState newState) {
        if (currentPlotState == newState) { return; }
        currentPlotState = newState;

        // 상태 스프라이트 적용
        spriteRenderer.sprite = stateSprites[(int)newState];
    }

    private void Init() {
        SetPlotState(PlotState.Normal);
    }

    void Start() {
        Init();
        playerToolManager = GameManager.Player.GetComponent<ToolManager>();
        // TODO 테스트용 당근 심기 삭제 필요
        // PlantSeed(PlantDataBase.Instance.GetPlantData("Carrot"));
    }

    void Update() {

    }

    public void Interact(GameObject interactor) {
        Debug.Log("interact!");
        if (currentPlant == null) {
            SeedTool seed = (SeedTool)playerToolManager?.EquippedTool;
            PlantSeed(PlantDataBase.Instance.GetPlantData(seed.GetPlantId()));
        }
        else if (currentPlant.IsHarvestable()) {
            // TODO 아이템 획득 로직 추가 필요
            
            Debug.Log("수확!");
            RemovePlant();
        }
        // TODO 시든 식물이 추가되면 처리 필요
    }

    private void RemovePlant() {
        if (currentPlant != null) {
            Destroy(currentPlant.gameObject);
            currentPlant = null;
        }
    }
}
