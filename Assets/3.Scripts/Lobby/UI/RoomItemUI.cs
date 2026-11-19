using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks.Data;
using Bird.Networking;

namespace Bird.UI
{
    public class RoomItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomNameText;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private TMP_Text playerCountText;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private Button joinButton;

        private Lobby currentLobby;
        private SteamNetworkManager networkManager;

        public void Setup(Lobby lobby, SteamNetworkManager manager)
        {
            currentLobby = lobby;
            networkManager = manager;

            // 스팀 문패(Metadata)에서 텍스트를 읽어와 UI에 반영
            roomNameText.text = lobby.GetData("RoomName");
            difficultyText.text = lobby.GetData("Difficulty");
            playerCountText.text = $"{lobby.MemberCount} / {lobby.MaxMembers}";
            
            // 비밀번호가 있다면 "T"로 기록해두었으므로, 이를 비교하여 자물쇠 아이콘을 켜거나 끕니다.
            bool isLocked = lobby.GetData("HasPassword") == "T";
            lockIcon.SetActive(isLocked);
            
            // 기존 리스너 초기화 후 입장 버튼 이벤트 연결
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnJoinClicked()
        {
            bool isLocked = currentLobby.GetData("HasPassword") == "T";
            if (isLocked)
            {
                // TODO: 팝업창 띄우기 로직 (비밀번호 검증용 Confirm 버튼 포함)
                Debug.Log($"[{roomNameText.text}] 방은 비밀번호가 필요합니다. 팝업을 띄우세요.");
            }
            else
            {
                // 공개 방이면 즉시 접속 시도
                networkManager.JoinLobbyAsync(currentLobby);
            }
        }
    }
}
