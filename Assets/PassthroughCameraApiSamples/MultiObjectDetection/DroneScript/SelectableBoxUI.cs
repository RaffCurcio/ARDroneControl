using UnityEngine;
using UnityEngine.UI;

public class SelectableBoxUI : MonoBehaviour
{
    public Button selectButton;
    public System.Action OnSelected;

    private void Awake()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() =>
            {
                OnSelected?.Invoke();
            });
        }
    }

    public void SetLabel(string text)
    {
        if (selectButton != null)
        {
            var txt = selectButton.GetComponentInChildren<Text>();
            if (txt != null)
                txt.text = text;
        }
    }
}
