using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Oculus.Interaction;
public class grabdebugger : MonoBehaviour
{
    RayInteractable rayInteractable;


    private void OnSelect()
    {
        Debug.Log($"[RayInteractable] Selected: {gameObject.name}");
        SendHaptic(0.7f, 0.2f);
    }

    private void OnUnselect()
    {
        Debug.Log($"[RayInteractable] Unselected: {gameObject.name}");
    }

    private void OnHover()
    {
        Debug.Log($"[RayInteractable] Hovered: {gameObject.name}");
    }

    private void OnUnhover()
    {
        Debug.Log($"[RayInteractable] Unhovered: {gameObject.name}");
    }

    private void SendHaptic(float amplitude, float duration)
    {
       
        OVRInput.SetControllerVibration(1.0f, amplitude, OVRInput.Controller.RTouch);
        Invoke(nameof(StopHaptics), duration);
    }

    private void StopHaptics()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}
