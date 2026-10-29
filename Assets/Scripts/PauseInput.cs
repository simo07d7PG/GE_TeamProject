using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInput : MonoBehaviour
{
    public void OnPause(InputValue value)
    {
        if (value.isPressed && PauseMenu.Instance != null) PauseMenu.Instance.Toggle();
    }
}
