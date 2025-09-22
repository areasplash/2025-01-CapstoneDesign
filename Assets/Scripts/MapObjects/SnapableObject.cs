using UnityEngine;

public interface ISnappable {
    void SnapToGrid();
}

public class SnapableObject : MonoBehaviour, ISnappable {
    public void SnapToGrid() {
        transform.position = GridUtility.SnapToIsometricGrid(transform.position);
    }

    private void OnEnable() {
        //SnapToGrid();
    }
}