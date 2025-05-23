using UnityEngine;

public class LaserRaycastSelector : MonoBehaviour
{
    public Transform laserTransform; // il cubo o cilindro laser
    public LayerMask boxLayerMask;   // layer dei box da selezionare
    public float maxDistance = 10f;

    private SelectableBox currentSelected;

    void Update()
    {
        // Crea un raggio dal laser in avanti
        Ray ray = new Ray(laserTransform.position, laserTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, boxLayerMask))
        {
            var selectable = hit.collider.GetComponent<SelectableBox>();
            if (selectable != null)
            {
                // Qui puoi chiamare Select() o gestire la selezione come vuoi
                if (currentSelected != null && currentSelected != selectable)
                    currentSelected.Deselect();

                selectable.Select();
                currentSelected = selectable;
            }
        }
        else
        {
            if (currentSelected != null)
            {
                currentSelected.Deselect();
                currentSelected = null;
            }
        }
    }
}
