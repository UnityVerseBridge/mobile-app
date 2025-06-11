using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityVerseBridge.Core;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// Mobile 앱에서 활성 room 목록을 가져와 선택할 수 있도록 함
    /// </summary>
    public class RoomListFetcher : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform roomListContainer;
        [SerializeField] private GameObject roomItemPrefab;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Text statusText;
        
        [Header("Settings")]
        [SerializeField] private ConnectionConfig connectionConfig;
        [SerializeField] private MobileAppInitializer appInitializer;
        [SerializeField] private float autoRefreshInterval = 5f;
        
        private Coroutine autoRefreshCoroutine;
        
        void Start()
        {
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(FetchRoomList);
            }
            
            // 자동 새로고침 시작
            if (autoRefreshInterval > 0)
            {
                autoRefreshCoroutine = StartCoroutine(AutoRefresh());
            }
            
            // 초기 목록 가져오기
            FetchRoomList();
        }
        
        private void FetchRoomList()
        {
            StartCoroutine(FetchRoomListCoroutine());
        }
        
        private IEnumerator FetchRoomListCoroutine()
        {
            if (connectionConfig == null)
            {
                ShowStatus("ConnectionConfig not set", Color.red);
                yield break;
            }
            
            // Extract base URL from WebSocket URL
            string baseUrl = connectionConfig.signalingServerUrl
                .Replace("ws://", "http://")
                .Replace("wss://", "https://");
            
            string roomsUrl = $"{baseUrl}/rooms";
            
            ShowStatus("Fetching room list...", Color.yellow);
            
            using (UnityWebRequest request = UnityWebRequest.Get(roomsUrl))
            {
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    ShowStatus($"Failed to fetch rooms: {request.error}", Color.red);
                    yield break;
                }
                
                try
                {
                    string jsonResponse = request.downloadHandler.text;
                    RoomListResponse response = JsonUtility.FromJson<RoomListResponse>(jsonResponse);
                    
                    UpdateRoomList(response.rooms);
                    ShowStatus($"Found {response.rooms.Length} active rooms", Color.green);
                }
                catch (System.Exception e)
                {
                    ShowStatus($"Failed to parse response: {e.Message}", Color.red);
                }
            }
        }
        
        private void UpdateRoomList(RoomInfo[] rooms)
        {
            // Clear existing list
            if (roomListContainer != null)
            {
                foreach (Transform child in roomListContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            // Add room items
            foreach (var room in rooms)
            {
                if (roomItemPrefab != null && roomListContainer != null)
                {
                    GameObject item = Instantiate(roomItemPrefab, roomListContainer);
                    
                    // Setup room item UI
                    Text[] texts = item.GetComponentsInChildren<Text>();
                    if (texts.Length > 0)
                    {
                        texts[0].text = $"Room: {room.roomId}";
                        if (texts.Length > 1)
                        {
                            texts[1].text = $"Host: {room.hostType} | Guests: {room.guestCount}";
                        }
                    }
                    
                    // Add click handler
                    Button button = item.GetComponent<Button>();
                    if (button != null)
                    {
                        string roomId = room.roomId; // Capture for closure
                        button.onClick.AddListener(() => JoinRoom(roomId));
                    }
                }
            }
            
            // Show empty state if no rooms
            if (rooms.Length == 0 && roomListContainer != null)
            {
                GameObject emptyText = new GameObject("EmptyText");
                emptyText.transform.SetParent(roomListContainer);
                Text text = emptyText.AddComponent<Text>();
                text.text = "No active rooms found";
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.gray;
            }
        }
        
        private void JoinRoom(string roomId)
        {
            if (connectionConfig != null)
            {
                // Disable session room ID and set the selected room
                connectionConfig.useSessionRoomId = false;
                connectionConfig.roomId = roomId;
                
                ShowStatus($"Joining room: {roomId}", Color.yellow);
                
                // Start connection
                if (appInitializer != null)
                {
                    appInitializer.StartConnection();
                }
            }
        }
        
        private IEnumerator AutoRefresh()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoRefreshInterval);
                FetchRoomList();
            }
        }
        
        private void ShowStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
            Debug.Log($"[RoomListFetcher] {message}");
        }
        
        void OnDestroy()
        {
            if (autoRefreshCoroutine != null)
            {
                StopCoroutine(autoRefreshCoroutine);
            }
        }
        
        [System.Serializable]
        private class RoomListResponse
        {
            public RoomInfo[] rooms;
            public string timestamp;
        }
        
        [System.Serializable]
        private class RoomInfo
        {
            public string roomId;
            public string hostType;
            public long createdAt;
            public int guestCount;
        }
    }
}