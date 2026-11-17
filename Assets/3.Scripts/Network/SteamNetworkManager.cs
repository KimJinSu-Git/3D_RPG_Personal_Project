using System;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using FishNet.Managing;

namespace Bird.Networking
{
    public class SteamNetworkManager : MonoBehaviour
    {
        [Header("네트워크 엔진 연결")]
        [Tooltip("Fish-Net의 메인 NetworkManager 프리팹을 끌어다 놓으세요")]
        [SerializeField] private NetworkManager networkManager;

        // 테스트용 기본 App ID (SpaceWar). 실제 출시 시 고유 ID로 변경합니다
        private const uint SteamAppId = 480;
        
        // ==========================================
        // 방 설정 데이터 구조체 정의 (GC가 발생하지 않는 Value Type)
        // ==========================================
        public struct RoomSettings
        {
            public string roomName;
            public string password; // 비어 있으면 공개방
            public string difficulty; // "Easy", "Normal", "Hard"
            public int maxPlayers; // 최대 4인
        }

        private void Start()
        {
            InitializeSteam();
        }

        private void InitializeSteam()
        {
            try
            {
                if (!SteamClient.IsValid)
                {
                    // 스팀 클라이언트를 초기화합니다
                    SteamClient.Init(SteamAppId, true);
                    Debug.Log($"스팀 초기화 성공. 접속된 계정 : {SteamClient.Name}");
                }
                else
                {
                    Debug.Log($"스팀이 이미 초기화되어 연결 유지 중입니다. 계정: {SteamClient.Name}"); 
                }
                
                // 이벤트 중복 구독 방지
                SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
                SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
                
                // 스팀 로비 관련 이벤트 구독
                SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
                SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            }
            catch (Exception e)
            {
                Debug.LogError($"스팀 클라이언트 초기화 실패 (스팀이 켜져 있는지 확인하세요): {e.Message}");
            }
        }
        
        // ==========================================
        // 방장 (Host) - 문패(Metadata)를 달고 로비 생성
        // ==========================================

        /// <summary>
        /// UI에서 '방 생성 확인' 버튼을 누르면 설정값(settings)과 함께 호출됩니다.
        /// </summary>
        public async void CreateLobbyAsync(RoomSettings settings)
        {
            Debug.Log("스팀 로비 생성 요청 중");
            
            // 1. 4인용 로비를 비동기로 생성합니다 (생성 성공 시 OnLobbyCreated 콜백이 자동 발생함)
            Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(settings.maxPlayers);

            if (!lobby.HasValue)
            {
                Debug.LogError("로비 생성에 실패했습니다");
                return;
            }
            
            // 스팀 로비에 메타데이터 달기
            lobby.Value.SetData("RoomName", settings.roomName);
            lobby.Value.SetData("Difficulty", settings.difficulty);
            
            // 비밀번호가 있다면 True, 없다면 False로 상태 기록
            bool hasPassword = !string.IsNullOrEmpty(settings.password);
            lobby.Value.SetData("HasPassword", hasPassword ? "T" : "F");

            if (hasPassword)
            {
                // 주의 : 실제 상용화 시에는 암호화해서 올려야 함.
                lobby.Value.SetData("Password", settings.password);
            }

            lobby.Value.SetData("GameID", "BirdDefenseRPG");
            
            Debug.Log($"로비 생성 완료! [방 이름: {settings.roomName} / 난이도: {settings.difficulty}]");
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            if (result != Result.OK)
            {
                Debug.LogError($"로비 생성 에러: {result}");
                return;
            }
            
            // 친구 및 방 목록에서 검색할 수 있도록 로비를 공개(Public) 상태로 설정합니다.
            lobby.SetPublic();
            lobby.SetJoinable(true);
            
            //Fish-Net의 서버와 클라이언트를 동시에 구동하여 방장(Host) 역할을 시작합니다.
            networkManager.ServerManager.StartConnection();
            networkManager.ClientManager.StartConnection();
            
            Debug.Log($"로비 생성 완료. Host 모드로 게임을 시작합니다. 로비 ID : {lobby.Id}");
        }
        
        // ==========================================
        // 참가자 (Client) - 로비 접속
        // ==========================================

        /// <summary>
        /// 스팀 오버레이(친구 초대) 또는 UI에서 로비 참가를 수락했을 때 호출됩니다.
        /// </summary>
        /// <param name="lobby"></param>
        public async void JoinLobbyAsync(Lobby lobby)
        {
            await lobby.Join();
        }
        
        private void OnLobbyEntered(Lobby lobby)
        {
            // 내가 방장(Host)이라면(서버가 켜져 있다면), 자신이 만든 방에 들어간 것이므로 접속 로직을 무시합니다.
            if (networkManager.IsServer) return;

            // 방장(Owner)의 스팀 고유 ID를 가져옵니다.
            string hostAddress = lobby.Owner.Id.ToString();
            
            Debug.Log($"로비 입장 완료. 방장({hostAddress})에게 P2P 연결을 시도합니다");
            
            // 방장의 스팀 ID를 IP 주소처럼 사용하여 Fish-Net 클라이언트를 접속시킵니다.
            networkManager.ClientManager.StartConnection(hostAddress);
        }
        
        // ==========================================
        // 메모리 해제 (최적화)
        // ==========================================
        
        private void OnDestroy()
        {
            // 메모리 누수 방지용
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            
            // 스팀 클라이언트 종료 처리
            SteamClient.Shutdown();
        }
    }
}

