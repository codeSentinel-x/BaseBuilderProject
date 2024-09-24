using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {
    public GameObject _objectToSpawn;
    void Update() {
        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) Instantiate(_objectToSpawn, hit.point, Quaternion.identity);
        }
    }
}
