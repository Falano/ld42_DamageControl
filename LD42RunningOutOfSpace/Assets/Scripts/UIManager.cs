﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    GameObject cameraPivot { get { return BoardManager.instance.cameraPivot; } }
    new Camera camera;
    public GameObject buttonsParent;
    //Image[] buttons;

    public Vector2 cameraZoomLimits;
    public Color activeColor;
    public Color inactiveColor;

    public Image ZoomIn;
    public Image ZoomOut;

    public Image MoveUp;
    public Image MoveLeft;
    public Image MoveRight;
    public Image MoveDown;

    //public Image RotateRight;
    //public Image RotateLeft;

    public Image NextTurn;

    public void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        camera = Camera.main;
        //buttons = buttonsParent.GetComponentsInChildren<Image>();
    }

    public void ToggleNextTurn(bool state)
    {
        NextTurn.raycastTarget = state;
        NextTurn.color = state? activeColor: inactiveColor;
    }

    public void StopMovement()
    {
        StopAllCoroutines();
        //ReevaluateButtonsInteractibility();
    }

    /*
    void ReevaluateButtonsInteractibility()
    {
        ZoomIn.raycastTarget = camera.orthographicSize > cameraZoomLimits.x;
        ZoomOut.raycastTarget = camera.orthographicSize < cameraZoomLimits.y;


        foreach(Image img in buttons)
        {
            img.color = img.raycastTarget ? activeColor: inactiveColor;
        }
    }*/

    public void ToggleObj(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }

    public void StartZoom(float zoom)
    {
        StartCoroutine(ZoomCamera(zoom));
    }

    public void StartMoveHor(float mv)
    {
        StartCoroutine(MoveCamera(new Vector2(mv, 0)));
    }

    public void StartMoveVert(float mv)
    {
        StartCoroutine(MoveCamera(new Vector2(0, mv)));
    }

    public void StartRotation(float rot)
    {
        StartCoroutine(RotateCamera(new Vector3(0, 0, rot)));
    }

    IEnumerator RotateCamera(Vector3 rot)
    {
        while (true)
        {
            // actual movement
            cameraPivot.transform.Rotate(rot);
            yield return null;
        }
    }

    IEnumerator MoveCamera(Vector3 pos)
    {
        while (true)
        {
            // actual movement
            cameraPivot.transform.position += pos;

            // reevaluate buttons interactibility
            MoveLeft.raycastTarget = cameraPivot.transform.position.x > 0;
            MoveRight.raycastTarget = cameraPivot.transform.position.x < BoardManager.instance.length;
            MoveUp.raycastTarget = cameraPivot.transform.position.y < BoardManager.instance.width;
            MoveDown.raycastTarget = cameraPivot.transform.position.y > 0;

            MoveLeft.color = MoveLeft.raycastTarget ? activeColor : inactiveColor;
            MoveUp.color = MoveUp.raycastTarget ? activeColor : inactiveColor;
            MoveRight.color = MoveRight.raycastTarget ? activeColor : inactiveColor;
            MoveDown.color = MoveDown.raycastTarget ? activeColor : inactiveColor;

            if (pos.y > 0 && !MoveUp.raycastTarget)
            {
                StopMovement();
            }
            else if (pos.x > 0 && !MoveRight.raycastTarget)
            {
                StopMovement();
            }
            else if (pos.y < 0 && !MoveDown.raycastTarget)
            {
                StopMovement();
            }
            else if (pos.x < 0 && !MoveLeft.raycastTarget)
            {
                StopMovement();
            }

            yield return null;
        }
    }

    IEnumerator ZoomCamera(float zoom)
    {
        while (true)
        {
            // actual zoom
            camera.orthographicSize += camera.orthographicSize * zoom;

            // reevaluate buttons interactibility
            ZoomIn.raycastTarget = camera.orthographicSize > cameraZoomLimits.x;
            ZoomOut.raycastTarget = camera.orthographicSize < cameraZoomLimits.y;
            ZoomIn.color = ZoomIn.raycastTarget ? activeColor : inactiveColor;
            ZoomOut.color = ZoomOut.raycastTarget ? activeColor : inactiveColor;
            if (zoom > 0 && !ZoomOut.raycastTarget)
            {
                StopMovement();
            }
            else if (zoom < 0 && !ZoomIn.raycastTarget)
            {
                StopMovement();
            }
            yield return null;
        }
    }
}
