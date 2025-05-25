using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.Signaling;
using UnityVerseBridge.Core.Signaling.Data;
using UnityVerseBridge.Core.Signaling.Adapters;
using UnityVerseBridge.Core.Signaling.Messages;
using Unity.WebRTC;

namespace UnityVerseBridge.MobileApp
{
    public class MobileAppInitializer : MonoBehaviour
    {
        private string clientId;
        private SystemWebSocketAdapter webSocketAdapter;
        private SignalingClient signalingClient;

        [Header("Dependencies")]
        [SerializeField] private WebRtcManager webRtcManager;
        [SerializeField] private ConnectionConfig connectionConfig;
        [SerializeField] private WebRtcConfiguration webRtcConfiguration;

        void Start()
        {
            try
            {
                // Critical: WebRTC.Update() coroutine must be started first
                StartCoroutine(WebRTC.Update());
                
                // Note: In newer Unity WebRTC versions, explicit Initialize() is not needed
                // WebRTC initializes automatically when first used
                
                InitializeApp();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileAppInitializer] Failed to initialize: {ex.Message}");
            }
        }

        private void InitializeApp()
        {
            Debug.Log("[MobileAppInitializer] Starting initialization...");
            
            // Generate secure client ID
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            clientId = $"mobile_{GenerateHashedId(deviceId)}";

            if (!ValidateDependencies())
            {
                throw new InvalidOperationException("Required dependencies are missing");
            }

            webSocketAdapter = new SystemWebSocketAdapter();
            signalingClient = new SignalingClient();
            
            webRtcManager.SetRole(false); // Answerer
            webRtcManager.SetupSignaling(signalingClient);
            
            if (webRtcConfiguration != null)
            {
                webRtcManager.SetConfiguration(webRtcConfiguration);
            }

            // Set role as Answerer
            webRtcManager.SetRole(false);
            // Answerer doesn't need autoStartPeerConnection - it waits for offer
            webRtcManager.autoStartPeerConnection = false;

            // Handle disconnection and auto-reconnect
            signalingClient.OnDisconnected += HandleSignalingDisconnected;

            StartCoroutine(DelayedSignalingConnection(connectionConfig.signalingServerUrl));
        }

        private bool ValidateDependencies()
        {
            if (webRtcManager == null)
            {
                Debug.LogError("[MobileAppInitializer] WebRtcManager not assigned!");
                return false;
            }
            
            if (connectionConfig == null)
            {
                Debug.LogError("[MobileAppInitializer] ConnectionConfig not assigned!");
                return false;
            }
            
            return true;
        }

        private string GenerateHashedId(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 16).ToLower();
            }
        }

        private async void StartSignalingConnection(string serverUrl)
        {
            int retryCount = 0;
            int maxRetries = connectionConfig.maxReconnectAttempts;
            
            while (retryCount < maxRetries)
            {
                try
                {
                    // Authentication if required
                    string connectUrl = serverUrl;
                    if (connectionConfig.requireAuthentication)
                    {
                        Debug.Log("[MobileAppInitializer] Authenticating...");
                        bool authSuccess = await AuthenticationHelper.AuthenticateAsync(
                            serverUrl,
                            clientId, 
                            "mobile", 
                            connectionConfig.authKey
                        );
                        
                        if (!authSuccess)
                        {
                            throw new Exception("Authentication failed");
                        }
                        
                        // Add token to URL if authenticated
                        connectUrl = AuthenticationHelper.AppendTokenToUrl(serverUrl);
                    }
                    
                    await signalingClient.InitializeAndConnect(webSocketAdapter, connectUrl);
                    Debug.Log("[MobileAppInitializer] SignalingClient 연결 성공");
                    
                    await Task.Delay(500); // Give more time for initialization
                    await RegisterClient();
                    
                    signalingClient.OnSignalingMessageReceived += HandleSignalingMessage;
                    break; // Success
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Debug.LogError($"[MobileAppInitializer] Connection attempt {retryCount} failed: {ex.Message}");
                    
                    if (retryCount < maxRetries)
                    {
                        float delay = Mathf.Pow(2, retryCount - 1);
                        Debug.Log($"[MobileAppInitializer] Retrying in {delay} seconds...");
                        await Task.Delay((int)(delay * 1000));
                    }
                }
            }
        }

        // Removed AuthenticateAsync method - now using AuthenticationManager
        
        private async Task RegisterClient()
        {
            try
            {
                var registerMessage = new RegisterMessage
                {
                    peerId = clientId,
                    clientType = "mobile",
                    roomId = connectionConfig.GetRoomId()
                };
                
                string jsonMessage = JsonUtility.ToJson(registerMessage);
                Debug.Log($"[MobileAppInitializer] Registering client: {jsonMessage}");
                
                await webSocketAdapter.SendText(jsonMessage);
                Debug.Log("[MobileAppInitializer] Client registered successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileAppInitializer] Failed to register client: {ex.Message}");
                throw;
            }
        }

        private void HandleSignalingMessage(string type, string jsonData)
        {
            try
            {
                if (type == "registered")
                {
                    Debug.Log("[MobileAppInitializer] Registration confirmed by server");
                }
                else if (type == "error")
                {
                    var error = JsonUtility.FromJson<ErrorMessage>(jsonData);
                    Debug.LogError($"[MobileAppInitializer] Server error: {error.error} (context: {error.context})");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileAppInitializer] Failed to handle message: {ex.Message}");
            }
        }
        
        void Update()
        {
            webSocketAdapter?.DispatchMessageQueue();
            signalingClient?.DispatchMessages();
        }
        
        private IEnumerator DelayedSignalingConnection(string serverUrl)
        {
            // Wait for WebRTC initialization to complete
            yield return new WaitForSeconds(0.5f);
            StartSignalingConnection(serverUrl);
        }

        private void HandleSignalingDisconnected()
        {
            Debug.LogWarning("[MobileAppInitializer] Signaling disconnected. Attempting to reconnect...");
            StartCoroutine(ReconnectSignaling());
        }

        private IEnumerator ReconnectSignaling()
        {
            yield return new WaitForSeconds(2f); // Wait before reconnecting
            
            // Clean up previous connection
            if (signalingClient != null)
            {
                signalingClient.OnSignalingMessageReceived -= HandleSignalingMessage;
                signalingClient.OnDisconnected -= HandleSignalingDisconnected;
            }
            
            // WebSocket adapter doesn't need explicit disposal
            webSocketAdapter = null;
            
            // Create new instances
            webSocketAdapter = new SystemWebSocketAdapter();
            signalingClient = new SignalingClient();
            webRtcManager.SetupSignaling(signalingClient);
            
            if (connectionConfig != null)
            {
                StartSignalingConnection(connectionConfig.signalingServerUrl);
            }
        }

        void OnDestroy()
        {
            if (signalingClient != null)
            {
                signalingClient.OnSignalingMessageReceived -= HandleSignalingMessage;
                signalingClient.OnDisconnected -= HandleSignalingDisconnected;
            }
            
            // Note: In newer Unity WebRTC versions, explicit Dispose() is not needed
            // Resources are cleaned up automatically
        }
    }
}
