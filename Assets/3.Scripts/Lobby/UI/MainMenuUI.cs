using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bird.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("메인 메뉴 버튼")]
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button joinRoomButton;

        [Header("연결할 팝업 패널")]
        [SerializeField] private GameObject createLobbyPanel;
        [SerializeField] private GameObject lobbyListPanel;

        private void Awake()
        {
            createLobbyPanel.SetActive(false);
            lobbyListPanel.SetActive(false);
        }

        private void Start()
        {
            createRoomButton.onClick.AddListener(OpenCreatePanel);
            joinRoomButton.onClick.AddListener(OpenLobbyListPanel);
        }

        private void OpenCreatePanel()
        {
            lobbyListPanel.SetActive(false);
            createLobbyPanel.SetActive(true);
        }

        private void OpenLobbyListPanel()
        {
            createLobbyPanel.SetActive(false);
            lobbyListPanel.SetActive(true);
        }

        private void OnDestroy()
        {
            createRoomButton.onClick.RemoveListener(OpenCreatePanel);
            joinRoomButton.onClick.RemoveListener(OpenLobbyListPanel);
        }
    }
}
