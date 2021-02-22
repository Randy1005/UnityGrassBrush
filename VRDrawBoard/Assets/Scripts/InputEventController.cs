using UnityEngine;

public class InputEventController : MonoBehaviour
{

    public static InputEventController instance
    {
        get
        {
            if (_instance == null)
            {
                // find an input event controller first
                _instance = FindObjectOfType(typeof(InputEventController)) as InputEventController;

                if (_instance == null)
                {
                    // create a new instance of input event controller
                    GameObject inputEventGameObject = new GameObject("InputEventController");
                    _instance = inputEventGameObject.AddComponent<InputEventController>();
                }
            }

            _instance.drawEvent = new InputEvent();
            _instance.uiEvent = new InputEvent();
            _instance.objectPlacementEvent = new InputEvent();

            return _instance;
        }
    }

    void DetectEvent(InputEvent i_inputEvent)
    {
        if (Input.GetMouseButtonDown(0))
        {
            i_inputEvent.onMouseLeftClick.Invoke();
        }

    }

    private void Awake()
    {
        currentInputMode = InputMode.PlaceObject;

    }

    private void Update()
    {
        switch (currentInputMode)
        {
            case InputMode.Draw:
                DetectEvent(drawEvent);
                break;
            case InputMode.PlaceObject:
                DetectEvent(objectPlacementEvent);
                break;
            case InputMode.UI:
                DetectEvent(uiEvent);
                break;
            default:
                break;
        }
    }


    // define input modes
    static InputEventController _instance;
    public InputEvent drawEvent = new InputEvent();
    public InputEvent uiEvent = new InputEvent();
    public InputEvent objectPlacementEvent = new InputEvent();

    // mode should be changeable on UI menu script??
    public InputMode currentInputMode;

    public enum InputMode
    {
        Draw,
        PlaceObject,
        UI
    }

}
