using UnityEngine;
using UnityEngine.XR;

public class LaserBoxSelector : MonoBehaviour
{
    public Transform laserOrigin; // Il GameObject laser (cilindro o cubo)
    public LayerMask boxLayerMask; // Layer dei box selezionabili
    public float maxDistance = 10f;

    private SelectableBox currentSelected;

    void Update()
    {
        Ray ray = new Ray(laserOrigin.position, laserOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, boxLayerMask))
        {
            var selectable = hit.collider.GetComponent<SelectableBox>();

            if (selectable != null)
            {
                if (currentSelected != null && currentSelected != selectable)
                    currentSelected.Deselect();

                // Se premi il grilletto destro Oculus (adatta se usi altro input)
                if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
                {
                    selectable.Select();
                    currentSelected = selectable;
                }
            }
        }
        else if (currentSelected != null)
        {
            currentSelected.Deselect();
            currentSelected = null;
        }
    }
}
