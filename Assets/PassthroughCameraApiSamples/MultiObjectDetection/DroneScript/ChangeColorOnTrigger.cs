using UnityEngine;
using PassthroughCameraSamples.MultiObjectDetection;

public class ChangeColorOnTrigger : MonoBehaviour
{
    public Transform rightController;
    public float rayLength = 60f;

    private bool isSelected = false;

    private static ChangeColorOnTrigger currentSelected = null;
    public static bool IsDroneSelected => currentSelected != null && currentSelected.isSelected;

    private void Update()
    {
        if (rightController == null) return;

        Ray ray = new Ray(rightController.position, rightController.forward);
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.green); // visualizza il raycast in editor

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
        {
            if (hit.collider.transform.IsChildOf(transform) &&
                OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                if (isSelected)
                {
                    Deselect();
                    currentSelected = null;
                }
                else
                {
                    if (currentSelected != null)
                    {
                        currentSelected.Deselect();
                    }

                    Select();
                    currentSelected = this;
                }
                FindObjectOfType<SentisInferenceUiManager>()?.RefreshBoxLabels();
            }
        }
    }

    private void Select()
    {
        isSelected = true;
    }

    private void Deselect()
    {
        isSelected = false;
    }

    private void OnDrawGizmos()
    {
        if (rightController == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(rightController.position, rightController.forward * rayLength);
    }
}
