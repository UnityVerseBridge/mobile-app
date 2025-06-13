using UnityEngine;
using UnityEngine.UI;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.UI;
using UnityVerseBridge.Core.Networking;
using System.Collections;

namespace UnityVerseBridge.MobileApp.UI
{
    /// <summary>
    /// Mobile 앱의 메뉴 시스템을 관리하는 컨트롤러
    /// UI cleanup, 연결 해제, 디버깅 기능 등을 제공
    /// </summary>
    public class MobileMenuController : MonoBehaviour
    {
        [Header("Menu UI")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button menuToggleButton;
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button cleanupUIButton;
        [SerializeField] private Button refreshRoomsButton;
        [SerializeField] private Toggle debugModeToggle;
        [SerializeField] private Text connectionStatusText;
        [SerializeField] private Text cleanupStatusText;
        
        [Header("References")]
        [SerializeField] private UnityVerseBridgeManager bridgeManager;
        [SerializeField] private MobileRoomUIAdapter roomUIAdapter;
        [SerializeField] private RoomDiscovery roomDiscovery;
        
        [Header("Settings")]
        [SerializeField] private bool autoHideMenu = true;
        [SerializeField] private float autoHideDelay = 3f;
        
        private bool isMenuVisible = false;
        private Coroutine autoHideCoroutine;
        
        void Start()
        {
            // Find components if not assigned
            if (bridgeManager == null)
                bridgeManager = FindFirstObjectByType<UnityVerseBridgeManager>();
                
            if (roomUIAdapter == null)
                roomUIAdapter = FindFirstObjectByType<MobileRoomUIAdapter>();
                
            if (roomDiscovery == null)
                roomDiscovery = FindFirstObjectByType<RoomDiscovery>();
            
            // Setup UI callbacks
            SetupUICallbacks();
            
            // Track menu UI with UI manager
            if (menuPanel != null)
                UIManager.Instance.TrackGameObject(menuPanel);
            
            // Initially hide menu
            if (menuPanel != null)
                menuPanel.SetActive(false);
            
            // Start status update
            StartCoroutine(UpdateConnectionStatus());
        }
        
        private void SetupUICallbacks()
        {
            if (menuToggleButton != null)
                menuToggleButton.onClick.AddListener(ToggleMenu);
                
            if (disconnectButton != null)
                disconnectButton.onClick.AddListener(DisconnectAndCleanup);
                
            if (cleanupUIButton != null)
                cleanupUIButton.onClick.AddListener(CleanupUI);
                
            if (refreshRoomsButton != null)
                refreshRoomsButton.onClick.AddListener(RefreshRooms);
                
            if (debugModeToggle != null)
                debugModeToggle.onValueChanged.AddListener(SetDebugMode);
        }
        
        private void ToggleMenu()
        {
            isMenuVisible = !isMenuVisible;
            
            if (menuPanel != null)
            {
                menuPanel.SetActive(isMenuVisible);
                
                if (isMenuVisible && autoHideMenu)
                {
                    // Reset auto-hide timer
                    if (autoHideCoroutine != null)
                        StopCoroutine(autoHideCoroutine);
                    autoHideCoroutine = StartCoroutine(AutoHideMenu());
                }
            }
        }
        
        private IEnumerator AutoHideMenu()
        {
            yield return new WaitForSeconds(autoHideDelay);
            
            if (isMenuVisible && menuPanel != null)
            {
                menuPanel.SetActive(false);
                isMenuVisible = false;
            }
        }
        
        private void DisconnectAndCleanup()
        {
            // Debug.Log("[MobileMenuController] Disconnecting and cleaning up...");
            
            // Disconnect WebRTC
            if (bridgeManager != null)
            {
                bridgeManager.Disconnect();
            }
            
            // Cleanup UI
            CleanupUI();
            
            UpdateCleanupStatus("Disconnected and cleaned up");
        }
        
        private void CleanupUI()
        {
            // Debug.Log("[MobileMenuController] Cleaning up UI...");
            
            // Cleanup all tracked UI
            UIManager.Instance.CleanupAll();
            
            // Also cleanup through room UI adapter if available
            if (roomUIAdapter != null)
            {
                roomUIAdapter.CleanupUI();
            }
            
            UpdateCleanupStatus("UI cleaned up successfully");
            
            // Hide menu after cleanup
            if (autoHideMenu && menuPanel != null)
            {
                menuPanel.SetActive(false);
                isMenuVisible = false;
            }
        }
        
        private void RefreshRooms()
        {
            if (roomDiscovery != null)
            {
                roomDiscovery.RefreshRoomList();
                UpdateCleanupStatus("Room list refreshed");
            }
        }
        
        private void SetDebugMode(bool enabled)
        {
            // Enable debug mode on video extensions
            var videoExtensions = FindObjectsByType<UnityVerseBridge.Core.Extensions.Mobile.MobileVideoExtension>(FindObjectsSortMode.None);
            foreach (var ext in videoExtensions)
            {
                ext.SetDebugMode(enabled);
            }
            
            UpdateCleanupStatus($"Debug mode: {(enabled ? "ON" : "OFF")}");
        }
        
        private IEnumerator UpdateConnectionStatus()
        {
            while (true)
            {
                if (connectionStatusText != null && bridgeManager != null)
                {
                    bool isConnected = bridgeManager.IsConnected;
                    connectionStatusText.text = $"Connection: {(isConnected ? "Connected" : "Disconnected")}";
                    connectionStatusText.color = isConnected ? Color.green : Color.red;
                    
                    // Update button states
                    if (disconnectButton != null)
                        disconnectButton.interactable = isConnected;
                }
                
                yield return new WaitForSeconds(1f);
            }
        }
        
        private void UpdateCleanupStatus(string message)
        {
            if (cleanupStatusText != null)
            {
                cleanupStatusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
                StartCoroutine(FadeOutCleanupStatus());
            }
            
            // Debug.Log($"[MobileMenuController] {message}");
        }
        
        private IEnumerator FadeOutCleanupStatus()
        {
            yield return new WaitForSeconds(3f);
            
            if (cleanupStatusText != null)
            {
                cleanupStatusText.text = "";
            }
        }
        
        void OnDestroy()
        {
            // Stop coroutines
            if (autoHideCoroutine != null)
                StopCoroutine(autoHideCoroutine);
            
            StopAllCoroutines();
            
            // Remove menu panel from tracking since it's being destroyed with this component
            if (menuPanel != null)
                UIManager.Instance.UntrackGameObject(menuPanel);
        }
    }
}