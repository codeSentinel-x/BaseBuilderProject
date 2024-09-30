using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public static PlayerMovement _I;
    public float _speed;
    Vector2Int _lastPosition;
    Vector3 _desiredPos;
    List<int2> 
    void Awake() {
        _I = this;
    }
    public void Move(Vector3 newPos) {
        _desiredPos = new Vector3(newPos.x, transform.position.y, newPos.z);
    }
    void Update() {
        if (Time.time % 5 == 0) CheckPosition();
        if (transform.position != _desiredPos && _desiredPos!= Vector3.zero) transform.position = Vector3.Lerp(transform.position, _desiredPos, _speed * Time.deltaTime);
        
    }
    public void CheckPosition() {
        var cPos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z));
        if (_lastPosition != cPos) {
            OnPositionChange();
            _lastPosition = cPos;
        }
    }
    public void OnPositionChange() {
        Debug.Log($"Current player cell is cell with position: {GridSystem._I._grid.GetGridCell(_lastPosition.x, _lastPosition.y)._pos}");
    }
}
