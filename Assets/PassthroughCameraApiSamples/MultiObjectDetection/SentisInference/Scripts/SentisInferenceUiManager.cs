using System.Collections.Generic;
using Meta.XR.Samples;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PassthroughCameraSamples.MultiObjectDetection;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    [MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class SentisInferenceUiManager : MonoBehaviour
    {
        [Header("Placement configuration")]
        [SerializeField] private EnvironmentRayCastSampleManager m_environmentRaycast;
        [SerializeField] private WebCamTextureManager m_webCamTextureManager;
        private PassthroughCameraEye CameraEye => m_webCamTextureManager.Eye;

        [Header("UI display references")]
        [SerializeField] private SentisObjectDetectedUiManager m_detectionCanvas;
        [SerializeField] private RawImage m_displayImage;
        [SerializeField] private Sprite m_boxTexture;
        [SerializeField] private Color m_boxColor;
        [SerializeField] private Font m_font;
        [SerializeField] private Color m_fontColor;
        [SerializeField] private int m_fontSize = 80;

        [Header("Ray Interactable Object")]
        [SerializeField] private GameObject rayInteractableInstance;

        [Space(10)]
        public UnityEvent<int> OnObjectsDetected;

        public List<BoundingBox> BoxDrawn = new();

        private string[] m_labels;
        private List<GameObject> m_boxPool = new();
        private Transform m_displayLocation;

        public struct BoundingBox
        {
            public float CenterX;
            public float CenterY;
            public float Width;
            public float Height;
            public string Label;
            public Vector3? WorldPos;
            public string ClassName;
        }

        private void Start()
        {
            m_displayLocation = m_displayImage.transform;
            if (rayInteractableInstance != null)
            {
                rayInteractableInstance.SetActive(false);
            }
        }

        public void OnObjectDetectionError()
        {
            ClearAnnotations();
            OnObjectsDetected?.Invoke(0);
        }

        public void SetLabels(TextAsset labelsAsset)
        {
            m_labels = labelsAsset.text.Split('\n');
        }

        public void SetDetectionCapture(Texture image)
        {
            m_displayImage.texture = image;
            m_detectionCanvas.CapturePosition();
        }

        public void DrawUIBoxes(Tensor<float> output, Tensor<int> labelIDs, float imageWidth, float imageHeight)
        {
            m_detectionCanvas.UpdatePosition();
            ClearAnnotations();

            var displayWidth = m_displayImage.rectTransform.rect.width;
            var displayHeight = m_displayImage.rectTransform.rect.height;

            var scaleX = displayWidth / imageWidth;
            var scaleY = displayHeight / imageHeight;

            var halfWidth = displayWidth / 2;
            var halfHeight = displayHeight / 2;

            var boxesFound = output.shape[0];
            if (boxesFound <= 0)
            {
                OnObjectsDetected?.Invoke(0);
                return;
            }

            var maxBoxes = Mathf.Min(boxesFound, 2);
            OnObjectsDetected?.Invoke(maxBoxes);

            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(CameraEye);
            var camRes = intrinsics.Resolution;

            for (var n = 0; n < maxBoxes; n++)
            {
                var centerX = output[n, 0] * scaleX - halfWidth;
                var centerY = output[n, 1] * scaleY - halfHeight;
                var perX = (centerX + halfWidth) / displayWidth;
                var perY = (centerY + halfHeight) / displayHeight;

                var classname = m_labels[labelIDs[n]].Replace(" ", "_");
                var centerPixel = new Vector2Int(Mathf.RoundToInt(perX * camRes.x), Mathf.RoundToInt((1.0f - perY) * camRes.y));
                var ray = PassthroughCameraUtils.ScreenPointToRayInWorld(CameraEye, centerPixel);
                var worldPos = m_environmentRaycast.PlaceGameObjectByScreenPos(ray);

                string stato = ChangeColorOnTrigger.IsDroneSelected ? "Selezionato" : "Non selezionato";

                var box = new BoundingBox
                {
                    CenterX = centerX,
                    CenterY = centerY,
                    ClassName = classname,
                    Width = output[n, 2] * scaleX,
                    Height = output[n, 3] * scaleY,
                    Label = $"Drone {BoxDrawn.Count + 1} - {stato}",
                    WorldPos = worldPos,
                };

                BoxDrawn.Add(box);
                DrawBox(box, n);

                // 🔵 Posiziona e attiva il cubo Ray Interactable
                if (worldPos.HasValue && rayInteractableInstance != null)
                {
                    rayInteractableInstance.transform.position = worldPos.Value;
                    rayInteractableInstance.SetActive(true);
                }
            }
        }

        private void ClearAnnotations()
        {
            foreach (var box in m_boxPool)
            {
                box?.SetActive(false);
            }

            // 🔴 Disattiva anche il cubo Ray Interactable
            if (rayInteractableInstance != null)
            {
                rayInteractableInstance.SetActive(false);
            }

            BoxDrawn.Clear();
        }

        private void DrawBox(BoundingBox box, int id)
        {
            GameObject panel;
            if (id < m_boxPool.Count)
            {
                panel = m_boxPool[id];
                if (panel == null)
                {
                    panel = CreateNewBox(m_boxColor);
                }
                else
                {
                    panel.SetActive(true);
                }
            }
            else
            {
                panel = CreateNewBox(m_boxColor);
            }

            panel.transform.localPosition = new Vector3(box.CenterX, -box.CenterY, box.WorldPos.HasValue ? box.WorldPos.Value.z : 0.0f);
            panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - m_detectionCanvas.GetCapturedCameraPosition());

            var rt = panel.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(box.Width, box.Height);

            var label = panel.GetComponentInChildren<Text>();
            label.text = box.Label;
            label.fontSize = 20;
        }

        private GameObject CreateNewBox(Color color)
        {
            var panel = new GameObject("ObjectBox");
            _ = panel.AddComponent<CanvasRenderer>();
            var img = panel.AddComponent<Image>();
            img.color = color;
            img.sprite = m_boxTexture;
            img.type = Image.Type.Sliced;
            img.fillCenter = false;
            panel.transform.SetParent(m_displayLocation, false);

            var text = new GameObject("ObjectLabel");
            _ = text.AddComponent<CanvasRenderer>();
            text.transform.SetParent(panel.transform, false);
            var txt = text.AddComponent<Text>();
            txt.font = m_font;
            txt.color = m_fontColor;
            txt.fontSize = m_fontSize;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;

            var rt2 = text.GetComponent<RectTransform>();
            rt2.offsetMin = new Vector2(20, rt2.offsetMin.y);
            rt2.offsetMax = new Vector2(0, rt2.offsetMax.y);
            rt2.offsetMin = new Vector2(rt2.offsetMin.x, 0);
            rt2.offsetMax = new Vector2(rt2.offsetMax.x, 30);
            rt2.anchorMin = new Vector2(0, 0);
            rt2.anchorMax = new Vector2(1, 1);

            m_boxPool.Add(panel);
            return panel;
        }
    }
}
