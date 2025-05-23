// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.Events;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class DetectionUiMenuManager : MonoBehaviour
    {
        [Header("UI configuration")]
        [SerializeField] private OVRInput.RawButton m_actionButton = OVRInput.RawButton.A;
        [SerializeField] private GameObject initialText;
        [SerializeField] private AudioSource m_buttonSound;

        public bool IsInputActive { get; set; } = true;

        public UnityEvent<bool> OnPause;

        private bool m_initialMenu = false;

        public bool IsPaused { get; private set; } = true;

        private void Start()
        {
            // Mostra il pannello iniziale all'avvio
            if (initialText != null)
            {
                initialText.SetActive(true);
            }

            m_initialMenu = true;
            IsPaused = true;
        }

        private void Update()
        {
            if (!IsInputActive)
                return;

            if (m_initialMenu)
            {
                InitialMenuUpdate();
            }
        }

        private void InitialMenuUpdate()
        {
            if (OVRInput.GetUp(m_actionButton) || Input.GetKeyDown(KeyCode.Return))
            {
                m_buttonSound?.Play();

                if (initialText != null)
                {
                    initialText.SetActive(false); // Nasconde il pannello
                }

                OnPauseMenu(false); // Continua
            }
        }

        private void OnPauseMenu(bool visible)
        {
            m_initialMenu = false;
            IsPaused = visible;

            if (initialText != null)
            {
                initialText.SetActive(false);
            }

            OnPause?.Invoke(visible);
        }
        public void OnInitialMenu(bool hasScenePermission)
        {
            if (initialText != null && hasScenePermission)
            {
                initialText.SetActive(true);
                m_initialMenu = true;
                IsPaused = true;
            }
            else
            {
                m_initialMenu = false;
                IsPaused = false;
            }
        }
    }
}
