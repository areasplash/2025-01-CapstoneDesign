using UnityEngine;

public class ToolVisual : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector3 handPosition = new Vector3(-0.3f, 0f, 0f);

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(ItemData data) {
        if (spriteRenderer != null) {
            spriteRenderer.sprite = data?.HoldingSprite;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (spriteRenderer != null) {
            transform.localPosition = new Vector3(spriteRenderer.flipX ? -handPosition.x : handPosition.x, handPosition.y, handPosition.z);
        }
    }
}
