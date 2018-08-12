using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    GameObject cameraPivot { get { return BoardManager.instance.cameraPivot; } }
    new Camera camera;

    public Vector2 cameraZoomLimits;

    public void Start()
    {
        camera = Camera.main;
    }

    public void RotateCamera(Vector3 rot)
    {
        cameraPivot.transform.Rotate(rot);
    }

    public void MoveCamera(Vector3 pos)
    {
        cameraPivot.transform.position += pos;
    }
    public void ZoomCamera(float zoom)
    {
        camera.orthographicSize += zoom;
    }
}
