using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    Vector3 baseCamPivotRot;
    Vector3 baseCamPivotPos;
    float baseCamzoom;

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
    Transform MoveParent;

    public Image NewAnimalScreen;
    public Image IntroScreen;
    int introState = 0;
    public List<Sprite> introImages;

    public Text EndGameScreen;
    public GameObject menu;


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
        MoveParent = MoveDown.transform.parent;

        ToggleForStart();
    }

    void ToggleForStart() {
        buttonsParent.SetActive(true);
        NewAnimalScreen.transform.parent.gameObject.SetActive(false);
        IntroScreen.transform.parent.gameObject.SetActive(false);
        EndGameScreen.transform.parent.gameObject.SetActive(false);
        menu.SetActive(false);
    }

    public void SetCameraReset()
    {
        baseCamzoom = camera.orthographicSize;
        baseCamPivotPos = cameraPivot.transform.position;
        baseCamPivotRot = cameraPivot.transform.rotation.eulerAngles;
    }

    public void ResetCamera()
    {
        camera.orthographicSize = baseCamzoom;
        cameraPivot.transform.position = baseCamPivotPos;
        cameraPivot.transform.rotation = Quaternion.Euler(baseCamPivotRot);

        // reevaluate buttons interactibility
                        ZoomIn.raycastTarget = true;
        ZoomOut.raycastTarget = true;
        ZoomIn.color = activeColor;
        ZoomOut.color = activeColor;
        MoveLeft.raycastTarget = true;
        MoveRight.raycastTarget = true;
        MoveUp.raycastTarget = true;
        MoveDown.raycastTarget = true;
        MoveParent.rotation = Quaternion.Euler(Vector3.zero);

        MoveLeft.color = activeColor;
        MoveUp.color = activeColor;
        MoveRight.color = activeColor;
        MoveDown.color = activeColor;
    }

    public void ToggleNextTurn(bool state)
    {
        NextTurn.raycastTarget = state;
        NextTurn.color = state ? activeColor : inactiveColor;
    }

    public void ToggleEndGame(bool state)
    {
        EndGameScreen.transform.parent.gameObject.SetActive(state);
        EndGameScreen.text = "Day " + GrowthManager.instance.currentTurn +":";
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

    public void ToggleNewAnimal(Sprite image)
    {
        NewAnimalScreen.transform.parent.gameObject.SetActive(true);
        NewAnimalScreen.sprite = image;
    }

    /// <summary>
    /// the tuto story thingy to show at the beginning
    /// </summary>
    public void UpdateIntroImages()
    {
        if (IntroScreen.transform.parent.gameObject.activeSelf == false)
        {
            IntroScreen.transform.parent.gameObject.SetActive(true);
            introState = 0;
            IntroScreen.sprite = introImages[introState];
        }

        else
        {
            if (introState >= introImages.Count-1)
            {
                IntroScreen.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                introState++;
                IntroScreen.sprite = introImages[introState];

            }
        }
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
            MoveParent.Rotate(-rot);
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
