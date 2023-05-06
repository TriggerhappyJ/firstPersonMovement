using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    public float mouseSensX;
    public float mouseSensY;

    public Transform orientation;
    public Transform cameraPosition;
    [SerializeField] private Transform camHolder;

    private float xRotation;
    private float yRotation;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    
    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensY * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(Vector3 camTilt)
    {
        transform.DOLocalRotate(new Vector3(camTilt.x, camTilt.y, camTilt.z), 0.25f);
    }
    
}
