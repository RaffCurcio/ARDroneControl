// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using System.Collections.Generic;
using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.Events;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    [MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class DetectionManager : MonoBehaviour
    {
        [SerializeField] private WebCamTextureManager m_webCamTextureManager;

        [Header("Controls configuration")]
        [SerializeField] private OVRInput.RawButton m_actionButton = OVRInput.RawButton.A;

        [Header("Ui references")]
        [SerializeField] private DetectionUiMenuManager m_uiMenuManager;

        [Header("Sentis inference ref")]
        [SerializeField] private SentisInferenceRunManager m_runInference;
        [SerializeField] private SentisInferenceUiManager m_uiInference;
        [SerializeField] private EnvironmentRayCastSampleManager m_environmentRaycast;

        [Space(10)]
        public UnityEvent<int> OnObjectsIdentified;

        private bool m_isPaused = true;
        private bool m_isStarted = false;
        private bool m_isSentisReady = false;
        private float m_delayPauseBackTime = 0;
        private float inferenceCooldown = 0.5f;
        private float nextInferenceTime = 0f;

        #region Unity Functions

        private void Awake()
        {
            OVRManager.display.RecenteredPose += () =>
            {
                OnObjectsIdentified?.Invoke(-1);
            };
        }

        private IEnumerator Start()
        {
            var sentisInference = FindAnyObjectByType<SentisInferenceRunManager>();
            while (!sentisInference.IsModelLoaded)
            {
                yield return null;
            }
            m_isSentisReady = true;
        }

        private void Update()
        {
            var hasWebCamTextureData = m_webCamTextureManager.WebCamTexture != null;

            if (!m_isStarted)
            {
                if (hasWebCamTextureData && m_isSentisReady)
                {
                    m_uiMenuManager.OnInitialMenu(m_environmentRaycast.HasScenePermission());
                    m_isStarted = true;
                }
            }
            else
            {
                // Pulsante A premuto dopo avvio per funzioni future (non più spawn marker)
                if (OVRInput.GetUp(m_actionButton) && m_delayPauseBackTime <= 0)
                {
                    OnObjectsIdentified?.Invoke(0); // Azione alternativa, ora vuota
                }

                m_delayPauseBackTime -= Time.deltaTime;
                if (m_delayPauseBackTime <= 0)
                {
                    m_delayPauseBackTime = 0;
                }
            }

            if (m_isPaused || !hasWebCamTextureData)
            {
                if (m_isPaused)
                {
                    m_delayPauseBackTime = 0.1f;
                }
                return;
            }

            if (!m_runInference.IsRunning() && Time.time >= nextInferenceTime)
            {
                m_runInference.RunInference(m_webCamTextureManager.WebCamTexture);
                nextInferenceTime = Time.time + inferenceCooldown;
            }
        }

        #endregion

        #region Public Functions

        public void OnPause(bool pause)
        {
            m_isPaused = pause;
        }

        #endregion
    }
}
