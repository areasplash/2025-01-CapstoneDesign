using UnityEngine;

public class MapObjectsParent : MonoBehaviour
{
    private static float spritePPU = 16f;
    private float tileWidth = 32f / spritePPU;
    private float tileHeight = 16f / spritePPU;

    public void SnapAllChildrenToGrid()
    {
        foreach (Transform child in transform)
        {
            Vector3 offset = new Vector3(tileWidth / 2f, 0f, 0f);
            child.position = SnapToIsometricGrid(child.position, tileWidth, tileHeight) + offset;
        }
    }

    private Vector3 SnapToIsometricGrid(Vector3 rawPosition, float tileWidth, float tileHeight)
    {
        float gridX = Mathf.Round((rawPosition.x / (tileWidth / 2) + rawPosition.y / (tileHeight / 2)) / 2f);
        float gridY = Mathf.Round((rawPosition.y / (tileHeight / 2) - rawPosition.x / (tileWidth / 2)) / 2f);

        float snappedX = (gridX - gridY) * (tileWidth / 2f);
        float snappedY = (gridX + gridY) * (tileHeight / 2f);

        return new Vector3(snappedX, snappedY, rawPosition.z);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SnapAllChildrenToGrid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
