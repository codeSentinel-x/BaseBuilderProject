using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    public Transform cameraTransform;
    public float shiftSpeed;
    public float normalSpeed;
    private float movementSpeed;
    public float movementTime;
    public float rotationAmount;
    public Vector3 zoomAmount;
    public Vector3 scrollZoomAmount;

    private Quaternion newRotation;
    private Vector3 newPosition;
    private Vector3 newZoom;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;
    private Vector3 rotationStartPosition;
    private Vector3 rotationCurrentPosition;


    void Start() {
        newRotation = transform.rotation;
        newPosition = transform.position;
        newZoom = cameraTransform.localPosition;
    }

    void Update() {
        HandleMovementInput();
        HandleMouseInput();
    }

    private void HandleMouseInput() {
        if (Input.mouseScrollDelta.y != 0) {
            newZoom += Input.mouseScrollDelta.y * -scrollZoomAmount;
            if (newZoom.y < 0) newZoom = Vector3.zero;
        }
        if (Input.GetMouseButtonDown(2)) {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float entry)) {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(2)) {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float entry)) {
                dragCurrentPosition = ray.GetPoint(entry);
                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }
        if (Input.GetMouseButtonDown(1)) {
            rotationStartPosition = transform.position;
        }
        if (Input.GetMouseButton(1)) {
            rotationCurrentPosition = Input.mousePosition;
            Vector3 difference = rotationStartPosition - rotationCurrentPosition;
            rotationStartPosition = rotationCurrentPosition;
            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }

    }

    void HandleMovementInput() {
        if (Input.GetKey(KeyCode.LeftShift)) {
            movementSpeed = shiftSpeed;
        } else {
            movementSpeed = normalSpeed;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            newPosition += (transform.forward * movementSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            newPosition += (transform.forward * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            newPosition += (transform.right * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            newPosition += (transform.right * movementSpeed);
        }
        if (Input.GetKey(KeyCode.Q)) {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }
        if (Input.GetKey(KeyCode.E)) {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }
        if (Input.GetKey(KeyCode.PageDown)) {
            newZoom += zoomAmount;
        }
        if (Input.GetKey(KeyCode.PageUp)) {
            newZoom -= zoomAmount;
            if (newZoom.y < 0) newZoom = Vector3.zero;
        }
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime), Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime));
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }
}
