using UnityEngine;

public class ChangeColorOnTrigger : MonoBehaviour
{
    public Transform rightController;
    public float rayLength = 60f;
    public Color selectedColor = Color.green;

    private Renderer rend;
    private Color originalColor;
    private bool isSelected = false;

    private static ChangeColorOnTrigger currentSelected = null;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material = new Material(rend.material); // importantissimo: crea un'istanza del materiale
        originalColor = rend.material.color;
    }

    private void Update()
    {
        if (rightController == null || rend == null) return;

        Ray ray = new Ray(rightController.position, rightController.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
        {
            if (hit.collider.transform.IsChildOf(transform) &&
                OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Active))
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
            }
        }
    }

    private void Select()
    {
        rend.material.color = selectedColor;
        isSelected = true;

    }

    private void Deselect()
    {
        rend.material.color = originalColor;
        isSelected = false;
    }

    private void OnDrawGizmos()
    {
        if (rightController == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(rightController.position, rightController.forward * rayLength);
    }
}
