using System;
using UnityEngine;
using UnityEngine.UI;
using Steamworks.Data;
using Bird.Networking;
using System.Threading.Tasks;

namespace Bird.UI
{
    public class LobbyListUI : MonoBehaviour
    {
        [SerializeField] private SteamNetworkManager steamNetworkManager;
        [SerializeField] private Transform contentRoot;     // Scroll View의 Content 오브젝트
        [SerializeField] private GameObject roomItemPrefab; // RoomItem 프리팹
        [SerializeField] private Button refreshButton;      // 새로고침 버튼

        private void Start()
        {
            refreshButton.onClick.AddListener(() => RefreshListAsync());
            RefreshListAsync(); // UI가 켜질 때 최초 1회 검색
        }

        private async void RefreshListAsync()
        {
            // 1. 스팀 서버에 방 목록 검색 요청 (비동기 대기)
            Lobby[] lobbies = await steamNetworkManager.GetLobbyListAsync();
            
            // 2. 기존에 생성되어 있던 리스트 초기화 (GC 발생)
            foreach (Transform child in contentRoot)
            {
                Destroy(child.gameObject);
            }
            
            // 3. 검색된 방의 개수만큼 프리팹을 복사하여 Content 아래에 붙임
            foreach (Lobby lobby in lobbies)
            {
                GameObject itemObj = Instantiate(roomItemPrefab, contentRoot);
                RoomItemUI itemUI = itemObj.GetComponent<RoomItemUI>();
                
                // 데이터 세팅
                itemUI.Setup(lobby, steamNetworkManager);
            }
            
            Debug.Log($"방 목록 새로고침 완료: 총 {lobbies.Length}개의 방을 찾았습니다.");
        }
    }
}

