using UnityEngine;
using UnityEngine.UI;
using UnityVerseBridge.Core;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// Mobile 앱에서 Room ID를 수동으로 입력하거나 QR 스캔 결과를 받아 처리
    /// </summary>
    public class RoomIdInput : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InputField roomIdInputField;
        [SerializeField] private Button connectButton;
        [SerializeField] private Text statusText;
        
        [Header("Settings")]
        [SerializeField] private ConnectionConfig connectionConfig;
        [SerializeField] private UnityVerseBridgeManager bridgeManager;
        
        void Start()
        {
            if (connectButton != null)
            {
                connectButton.onClick.AddListener(OnConnectButtonClicked);
            }
            
            // ConnectionConfig의 세션 Room ID 기능 비활성화 (수동 입력 사용)
            if (connectionConfig != null)
            {
                connectionConfig.useSessionRoomId = false;
            }
            
            // 기본값 표시
            if (roomIdInputField != null && connectionConfig != null)
            {
                roomIdInputField.text = connectionConfig.roomId;
            }
        }
        
        private void OnConnectButtonClicked()
        {
            if (string.IsNullOrEmpty(roomIdInputField.text))
            {
                ShowStatus("Room ID를 입력해주세요", Color.red);
                return;
            }
            
            // Room ID 설정
            string roomId = roomIdInputField.text.Trim();
            connectionConfig.roomId = roomId;
            ShowStatus($"Connecting to room: {roomId}", Color.yellow);
            
            // 연결 시작 using UnityVerseBridgeManager
            if (bridgeManager != null)
            {
                bridgeManager.SetRoomId(roomId);
                bridgeManager.Connect();
            }
            else
            {
                // Try to find UnityVerseBridgeManager in scene
                bridgeManager = FindFirstObjectByType<UnityVerseBridgeManager>();
                if (bridgeManager != null)
                {
                    bridgeManager.SetRoomId(roomId);
                    bridgeManager.Connect();
                }
                else
                {
                    Debug.LogError("[RoomIdInput] UnityVerseBridgeManager not found!");
                }
            }
        }
        
        /// <summary>
        /// QR 코드 스캔 결과를 처리
        /// </summary>
        public void ProcessQRCodeData(string qrData)
        {
            try
            {
                var data = JsonUtility.FromJson<RoomConnectionData>(qrData);
                
                // Room ID 설정
                if (roomIdInputField != null)
                {
                    roomIdInputField.text = data.roomId;
                }
                
                // 서버 URL 업데이트 (옵션)
                if (!string.IsNullOrEmpty(data.serverUrl))
                {
                    connectionConfig.signalingServerUrl = data.serverUrl;
                }
                
                ShowStatus($"QR 스캔 성공: {data.roomId}", Color.green);
                
                // 자동 연결 (옵션)
                OnConnectButtonClicked();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RoomIdInput] Failed to parse QR data: {e.Message}");
                ShowStatus("QR 코드 파싱 실패", Color.red);
            }
        }
        
        private void ShowStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
            Debug.Log($"[RoomIdInput] {message}");
        }
        
        [System.Serializable]
        private class RoomConnectionData
        {
            public string roomId;
            public string serverUrl;
            public string timestamp;
        }
    }
}