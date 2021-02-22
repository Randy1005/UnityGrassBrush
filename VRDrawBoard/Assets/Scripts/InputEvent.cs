using UnityEngine;
using UnityEngine.Events;

public class InputEvent : MonoBehaviour
{   
    public void ClearEvents()
    {
        onMouseLeftClick.RemoveAllListeners();
    }


    public UnityEvent onMouseLeftClick = new UnityEvent();

}
