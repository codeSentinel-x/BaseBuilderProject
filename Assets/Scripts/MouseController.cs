using UnityEngine;

public class MouseController : MonoBehaviour {
    public GameObject _objectToSpawn;
    public LayerMask _targetLayer;
    void Update() {
        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _targetLayer)) {
                Debug.Log($"Clicked on cell with pos {GridSystem._I._grid.GetGridCell(hit.point)._pos}");
                PlayerMovement._I.Move(hit.point);
            }
            // Instantiate(_objectToSpawn, hit.point, Quaternion.identity);
        }
    }
}
