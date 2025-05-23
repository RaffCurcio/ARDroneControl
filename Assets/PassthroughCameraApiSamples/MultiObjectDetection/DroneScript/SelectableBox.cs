using UnityEngine;
using UnityEngine.UI;

public class SelectableBox : MonoBehaviour
{
    private Image _image;
    private Color _defaultColor;
    public Color selectedColor = Color.green;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if (_image != null)
            _defaultColor = _image.color;
    }

    public void Select()
    {
        if (_image != null)
            _image.color = selectedColor;
    }

    public void Deselect()
    {
        if (_image != null)
            _image.color = _defaultColor;
    }
}
