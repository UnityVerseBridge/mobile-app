using UnityEngine;
using UnityEngine.UI;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.UI;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// Adapter that connects the core RoomListUI and RoomInputUI to the mobile app
    /// Provides mobile-specific functionality while using core components
    /// </summary>
    public class MobileRoomUIAdapter : MonoBehaviour
    {
        [Header("Core UI Components")]
        [SerializeField] private RoomListUI roomListUI;
        [SerializeField] private RoomInputUI roomInputUI;
        
        [Header("Mobile App Integration")]
        [SerializeField] private UnityVerseBridgeManager bridgeManager;
        [SerializeField] private GameObject roomSelectionPanel;
        [SerializeField] private GameObject connectionPanel;
        
        [Header("Mobile-Specific Features")]
        [SerializeField] private bool enableQRScanning = true;
        [SerializeField] private bool autoConnectOnSelection = true;
        
        void Start()
        {
            // Find bridge manager if not assigned
            if (bridgeManager == null)
            {
                bridgeManager = FindFirstObjectByType<UnityVerseBridgeManager>();
            }
            
            SetupUICallbacks();
        }
        
        private void SetupUICallbacks()
        {
            // Room List UI callbacks
            if (roomListUI != null)
            {
                roomListUI.onRoomSelected.AddListener(OnRoomSelected);
                roomListUI.onRoomsUpdated.AddListener(OnRoomsUpdated);
            }
            
            // Room Input UI callbacks
            if (roomInputUI != null)
            {
                roomInputUI.onRoomIdSubmitted.AddListener(OnRoomIdSubmitted);
                
                if (enableQRScanning)
                {
                    roomInputUI.onScanQRRequested.AddListener(OnQRScanRequested);
                }
            }
        }
        
        private void OnRoomSelected(string roomId)
        {
            Debug.Log($"[MobileRoomUIAdapter] Room selected from list: {roomId}");
            
            // Update room input UI
            if (roomInputUI != null)
            {
                roomInputUI.SetRoomId(roomId);
            }
            
            // Auto-connect if enabled
            if (autoConnectOnSelection)
            {
                ConnectToRoom(roomId);
            }
            else
            {
                // Switch to connection panel
                ShowConnectionPanel();
            }
        }
        
        private void OnRoomIdSubmitted(string roomId)
        {
            Debug.Log($"[MobileRoomUIAdapter] Room ID submitted: {roomId}");
            ConnectToRoom(roomId);
        }
        
        private void OnRoomsUpdated(RoomListUI.RoomInfo[] rooms)
        {
            Debug.Log($"[MobileRoomUIAdapter] Room list updated: {rooms.Length} rooms");
            
            // Mobile-specific: Show notification if new rooms appear
            if (rooms.Length > 0 && !roomSelectionPanel.activeSelf)
            {
                ShowMobileNotification("New rooms available!");
            }
        }
        
        private void OnQRScanRequested()
        {
            Debug.Log("[MobileRoomUIAdapter] QR scan requested");
            
            // Mobile-specific QR scanning implementation
            // This would integrate with platform-specific QR libraries
#if UNITY_IOS || UNITY_ANDROID
            StartQRScanner();
#else
            Debug.LogWarning("[MobileRoomUIAdapter] QR scanning not supported on this platform");
#endif
        }
        
        private void ConnectToRoom(string roomId)
        {
            if (bridgeManager != null)
            {
                bridgeManager.SetRoomId(roomId);
                bridgeManager.Connect();
                
                // Hide UI panels during connection
                if (roomSelectionPanel != null)
                    roomSelectionPanel.SetActive(false);
            }
        }
        
        private void ShowConnectionPanel()
        {
            if (roomSelectionPanel != null)
                roomSelectionPanel.SetActive(false);
                
            if (connectionPanel != null)
                connectionPanel.SetActive(true);
        }
        
        private void ShowMobileNotification(string message)
        {
            // Mobile-specific notification
#if UNITY_IOS || UNITY_ANDROID
            // Use Unity Mobile Notifications package or native plugins
            Debug.Log($"[MobileNotification] {message}");
#endif
        }
        
        private void StartQRScanner()
        {
            // Placeholder for QR scanning implementation
            // Would integrate with ZXing.NET or platform-specific libraries
            Debug.Log("[MobileRoomUIAdapter] Starting QR scanner...");
            
            // Simulate QR scan for testing
            if (Application.isEditor)
            {
                string simulatedQR = "{\"roomId\":\"test-room-123\",\"serverUrl\":\"ws://localhost:8080\"}";
                if (roomInputUI != null)
                {
                    roomInputUI.ProcessQRCodeData(simulatedQR);
                }
            }
        }
        
        /// <summary>
        /// Switch between room list and manual input views
        /// </summary>
        public void ToggleInputMode()
        {
            if (roomListUI != null && roomInputUI != null)
            {
                bool showList = !roomListUI.gameObject.activeSelf;
                roomListUI.gameObject.SetActive(showList);
                roomInputUI.gameObject.SetActive(!showList);
            }
        }
        
        /// <summary>
        /// Refresh the room list
        /// </summary>
        public void RefreshRooms()
        {
            if (roomListUI != null)
            {
                roomListUI.RefreshRoomList();
            }
        }
    }
}