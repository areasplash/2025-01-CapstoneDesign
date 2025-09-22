using UnityEngine;

public static class GridUtility {
    private static float spritePPU = 16f;
    private static float tileWidth = 32f / spritePPU;
    private static float tileHeight = 16f / spritePPU;

    public static Vector3 SnapToIsometricGrid(Vector3 rawPosition) {
        float gridX = Mathf.Round((rawPosition.x / (tileWidth / 2) + rawPosition.y / (tileHeight / 2)) / 2f);
        float gridY = Mathf.Round((rawPosition.y / (tileHeight / 2) - rawPosition.x / (tileWidth / 2)) / 2f);

        float snappedX = (gridX - gridY) * (tileWidth / 2f);
        float snappedY = (gridX + gridY) * (tileHeight / 2f);
        
        Vector3 offset = new Vector3(tileWidth / 2f, 0f, 0f);

        return new Vector3(snappedX, snappedY, rawPosition.z) + offset;
    }
}
