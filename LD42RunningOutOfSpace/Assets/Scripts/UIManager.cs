using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public Image MenuChainFoodTuto;
    int introState = 0;
    public List<Sprite> introImages;

    public Text EndGameScreen;
    public GameObject EndGame100Screen;
    public GameObject EndGame999Screen;
    public GameObject menu;
    public GameObject CreditsScreen;
    Vector3 NextTurnRotation = new Vector3(0,0,13);


    //public Image RotateRight;
    //public Image RotateLeft;

    public Image ActiveAnimalImage;
    public Image NextTurn;
    public GameObject NextTurnPulse;
    public Color freeTileColor;
    public Color unusableTileColor;

    public Text DaysText;
    public Text SaneText;

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

    public void ToggleForStart() {
        ToggleForNewGame();
        buttonsParent.SetActive(false);
        IntroScreen.transform.parent.gameObject.SetActive(false);
        CreditsScreen.SetActive(false);
        menu.SetActive(false);
    }

    public void ToggleForNewGame()
    {
        NewAnimalScreen.transform.parent.gameObject.SetActive(false);
        EndGameScreen.transform.parent.gameObject.SetActive(false);
        EndGame100Screen.SetActive(false);
        EndGame999Screen.SetActive(false);
        ActiveAnimalImage.gameObject.SetActive(false);
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

    public IEnumerator PulseNextTurn()
    {
        while (GrowthManager.instance.currentTurn == 0)
        {
            float scale = 1.5f + (Mathf.Sin(Time.time*3)*.25f);
            NextTurnPulse.transform.localScale = new Vector3(scale, scale);
            yield return null;
        }
    }

    public IEnumerator rotateTimeHolder()
    {
        while (NextTurn.transform.rotation.z >= 0 || NextTurn.transform.rotation.z < -20)
        {
            NextTurn.transform.Rotate(NextTurnRotation);
            yield return null;
        }
        NextTurn.transform.rotation = Quaternion.Euler(Vector3.zero);
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


    public void ChangeTutoImage(Sprite image)
    {
        MenuChainFoodTuto.sprite = image;
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
                StartCoroutine(PulseNextTurn());
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
            cameraPivot.transform.position += pos * camera.orthographicSize * .1f;

            // reevaluate buttons interactibility
            MoveLeft.raycastTarget = cameraPivot.transform.position.x > 0;
            MoveRight.raycastTarget = cameraPivot.transform.position.x < BoardManager.instance.width;
            MoveUp.raycastTarget = cameraPivot.transform.position.y < BoardManager.instance.length;
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

    IEnumerator ZoomCamera(float zoomFactor)
    {
        while (true)
        {
            zoom(zoomFactor);

            if (zoomFactor > 0 && !ZoomOut.raycastTarget)
            {
                StopMovement();
            }
            else if (zoomFactor < 0 && !ZoomIn.raycastTarget)
            {
                StopMovement();
            }
            yield return null;
        }
    }

    public void zoom(float zoomFactor)
    {
        // actual zoom
        camera.orthographicSize += camera.orthographicSize * zoomFactor ;

        // reevaluate buttons interactibility
        ZoomIn.raycastTarget = camera.orthographicSize > cameraZoomLimits.x;
        ZoomOut.raycastTarget = camera.orthographicSize < cameraZoomLimits.y;
        ZoomIn.color = ZoomIn.raycastTarget ? activeColor : inactiveColor;
        ZoomOut.color = ZoomOut.raycastTarget ? activeColor : inactiveColor;
    }


    private Vector3 mouseOrigin;

    private void Update()
    {
        // stop rotation
        if (Input.GetMouseButtonUp(1))
        {
            StopMovement();
        }


        if(EventSystem.current.IsPointerOverGameObject(-1))
        {
            return;
        }


        // zoom on wheel
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            zoom(Input.GetAxis("Mouse ScrollWheel"));
        }

        // start rotation on right click
        if (Input.GetMouseButtonDown(1))
        {
            if (Input.mousePosition.x < Screen.width * .5f)
            {
                StartRotation(-0.4f);
            }
            else
            {
                StartRotation(0.4f);
            }
        }


        // click drag move
        if (Input.GetMouseButtonDown(0))
        {
            mouseOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if(Input.GetMouseButton(0))
        {
            Vector3 pos = (cameraPivot.transform.position + (Camera.main.ScreenToWorldPoint(Input.mousePosition) - mouseOrigin) * camera.orthographicSize * -0.01f);

            cameraPivot.transform.position = pos;
        }
    }
}
