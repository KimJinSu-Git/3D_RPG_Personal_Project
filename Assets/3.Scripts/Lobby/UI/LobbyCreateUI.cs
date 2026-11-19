using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bird.Networking;

namespace Bird.UI
{
    public class LobbyCreateUI : MonoBehaviour
    {
        [Header("UI 구성요소")]
        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_Dropdown difficultyDropdown;
        [SerializeField] private Button confirmButton;

        [Header("네트워크 연결")]
        [SerializeField] private SteamNetworkManager steamNetworkManager;

        private void Start()
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        private void OnConfirmButtonClicked()
        {
            // 방 이름이 비어 있으면 생성을 막는 예외 처리
            if (string.IsNullOrWhiteSpace(roomNameInput.text))
            {
                Debug.LogWarning("방 이름을 입력해야 합니다.");
                return;
            }
            
            // Dropdown에서 현재 선택된 옵션의 텍스트("Easy", "Normal", "Hard")를 가져옵니다.
            string selectedDifficulty = difficultyDropdown.options[difficultyDropdown.value].text;
            
            // 앞서 만든 RoomSettings 구조체에 UI 입력값들을 포장합니다.
            SteamNetworkManager.RoomSettings newRoomSettings = new SteamNetworkManager.RoomSettings
            {
                roomName = roomNameInput.text,
                password = passwordInput.text, // 입력이 없으면 빈 문자열("")이 들어갑니다.
                difficulty = selectedDifficulty,
                maxPlayers = 4
            };
            
            // 네트워크 매니저에게 포장된 데이터를 넘기며 방 생성을 지시합니다
            steamNetworkManager.CreateLobbyAsync(newRoomSettings);
            
            // 방 생성 요청 후 팝업창을 닫습니다. (또는 로딩 UI 띄우기)
            gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            // 메모리 누수를 막기 위해 등록한 버튼 리스너 해제
            confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        }
    }
}
