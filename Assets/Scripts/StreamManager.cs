using UnityEngine;
using LiveKit;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
//using Unity.WebRTC;

public class StreamManager : MonoBehaviour
{

    public enum StreamSource{
        CLOUD,
        LOCAL
    }
    public string roomToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3ODAwMDY0MDcsImlzcyI6IkFQSWFTNVVmeXJQS3VjOCIsIm5iZiI6MTc0ODQ3MDQwOCwic3ViIjoiMiIsInZpZGVvIjp7ImNhblB1Ymxpc2giOnRydWUsImNhblB1Ymxpc2hEYXRhIjp0cnVlLCJjYW5TdWJzY3JpYmUiOnRydWUsInJvb20iOiJhYmNkIiwicm9vbUpvaW4iOnRydWV9fQ.t3E8f_yQdCS7N9Z4UPCeZs85C9ftFhzNaMfxvLEfYX8";

    public string wsurl = "wss://test-ky7qsf6n.livekit.cloud";


    public string localroomToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoidXNlciIsInZpZGVvIjp7InJvb21Kb2luIjp0cnVlLCJyb29tIjoiYSIsImNhblB1Ymxpc2giOnRydWUsImNhblN1YnNjcmliZSI6dHJ1ZSwiY2FuUHVibGlzaERhdGEiOnRydWV9LCJzdWIiOiJpZGVudGl0eSIsImlzcyI6ImRldmtleSIsIm5iZiI6MTc0OTU4MjAwNCwiZXhwIjoxNzQ5NjAzNjA0fQ.GoijGX9h1SSjjpU9pvX2TCn-jUOmoRvyqG2D8ngVd2g";

    public string localwsurl = "ws://localhost:7880";
    private Room _room;
    private int _retryCount = 0;
    private const int MAX_RETRIES = 3;

    private Texture _pendingTexture;

    //private RenderTexture _displayRenderTexture;
    private RenderTexture _leftEyeRenderTexture;
    private RenderTexture _rightEyeRenderTexture;

    //private Renderer _displayRenderer;

    private Renderer _leftEyeRenderer;
    private Renderer _rightEyeRenderer;

    //private Material _displayMaterial;

    public Vector2 leftOffset = new Vector2(0.0f, 0.0f);
    public Vector2 rightOffset = new Vector2(0.5f, 0.0f);

    public Vector2 leftTiling = new Vector2(0.5f, 1.0f);
    public Vector2 rightTiling = new Vector2(0.5f, 1.0f);

    public StreamSource streamSource = StreamSource.LOCAL;

    public float opacity = 1.0f;

    // private MediaStream receiveStream;
    // private RTCPeerConnection pc;


    void Start()
    {

        // Ensure VR is properly initialized
        if (!OVRManager.isHmdPresent)
        {
            Debug.LogError("No VR headset detected. Please ensure your VR headset is properly connected.");
            return;
        }

        // Wait for VR initialization
        StartCoroutine(WaitForVRInitialization());

        _leftEyeRenderer = transform.Find("LeftEye").GetComponent<Renderer>();
        _rightEyeRenderer = transform.Find("RightEye").GetComponent<Renderer>();

        //_displayRenderer = GetComponent<Renderer>();

        // Initialize render textures
        InitializeRenderTextures();
    }

    private void InitializeRenderTextures()
    {
        //_displayRenderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        _leftEyeRenderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        _rightEyeRenderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);

        // _displayMaterial = new Material(Shader.Find("Shader Graphs/sbsShader"));
        // _leftEyeRenderer.material = _displayMaterial;
        // _rightEyeRenderer.material = _displayMaterial;

        // if (_displayRenderer != null) _displayRenderer.material.mainTexture = _displayRenderTexture;


        if (_leftEyeRenderer != null) {
            _leftEyeRenderer.material.mainTexture = _leftEyeRenderTexture;    
            _leftEyeRenderer.material.SetFloat("_isLeft", 1.0f);
        }
        if (_rightEyeRenderer != null) {
            _rightEyeRenderer.material.mainTexture = _rightEyeRenderTexture;
            _rightEyeRenderer.material.SetFloat("_isLeft", 0.0f);
        }


        Debug.Log("Render textures initialized successfully");
    }


    private IEnumerator WaitForVRInitialization()
    {
        // Wait for VR to be fully initialized
        while (!OVRManager.isHmdPresent || !OVRManager.hasVrFocus)
        {
            yield return null;
        }

        // Now that VR is initialized, proceed with room connection
        // if (streamSource == StreamSource.LIVEKIT) StartCoroutine(ConnectToRoom());
        // else if (streamSource == StreamSource.WEBRTC) StartCoroutine(ConnectToWebRTC());

        StartCoroutine(ConnectToRoom(streamSource, "abcd"));
    }
    public void FetchTokenCoroutine(string roomName,
                                    string url,
                                    Action<string> onSuccess,
                                    Action<string> onError = null)
    {
        StartCoroutine(_FetchToken(roomName, url, onSuccess, onError));
    }

    private IEnumerator _FetchToken(string roomName,
                                    string url,
                                    Action<string> onSuccess,
                                    Action<string> onError)
    {
        // Build POST body
        string jsonData = $"{{\"roomName\":\"{roomName}\"}}";
        byte[] bodyRaw   = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Create request
        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            // request.postData = bodyRaw;
            request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type",   "application/json");
            request.SetRequestHeader("X-Sandbox-ID",   "zero-knowledge-blockchain-iae8fm");

            // Send and wait
            yield return request.SendWebRequest();

            // Handle result
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Replace ResponseType with your real response-model struct/class
                ResponseType rsp = JsonUtility.FromJson<ResponseType>(request.downloadHandler.text);

                Debug.Log($"Connected to room {rsp.roomName} at {rsp.serverUrl} with token {rsp.participantToken} and participant name {rsp.participantName}");
                onSuccess?.Invoke(rsp.participantToken);     // callback with token
            }
            else
            {
                string err = $"FetchToken error: {request.error}";
                Debug.LogError(err);
                onError?.Invoke(err);
            }
        }
    }

    // POCO that matches the JSON your server returns
    [Serializable]
    private struct ResponseType
    {
        public string roomName;
        public string serverUrl;
        public string participantToken;

        public string participantName;
    }

    public void ResetRoom(){
        if (_room != null){
            _room.TrackSubscribed -= TrackSubscribed;
            _room.Disconnect();
        }
        _room = null;
        _retryCount = 0;
        _pendingTexture = null;
    }
    
    public IEnumerator ConnectToRoom(StreamSource source, string roomName)
    {

        _room = new Room();
        _room.TrackSubscribed += TrackSubscribed;

        var options = new RoomOptions
        {
            AutoSubscribe = true,
            Dynacast = true,
            AdaptiveStream = false,
            JoinRetries = 5
        };

        Debug.Log($"Attempting to connect to LiveKit room (Attempt {_retryCount + 1}/{MAX_RETRIES})");
        string url = (source == StreamSource.LOCAL) ? localwsurl : wsurl;
        
        if (source == StreamSource.CLOUD){
            FetchTokenCoroutine(roomName, "https://cloud-api.livekit.io/api/sandbox/connection-details", (token) => {
                roomToken = token;
            }, (error) => {
                Debug.LogError($"Failed to fetch token: {error}");
            }); 
        }

        string token = (source == StreamSource.LOCAL) ? localroomToken : roomToken;
        
        var connect = _room.Connect(url, token, options);
        
        Debug.Log($"Connecting to {source} room");
        yield return connect;

        if (!connect.IsError)
        {
            _retryCount = 0;
            var text = transform.parent.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "";
            }
            Debug.Log($"Successfully connected to room: {_room.Name}");
        }
        else
        {
            _retryCount++;
            var text = transform.parent.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"Failed to connect to room (Attempt {_retryCount}/{MAX_RETRIES})";
            }
            Debug.LogError($"Failed to connect to room: {connect.IsError}");

            if (_retryCount < MAX_RETRIES)
            {
                Debug.Log($"Retrying in 5 seconds...");
                yield return new WaitForSeconds(5);
                StartCoroutine(ConnectToRoom(source, roomName));    
            }
            else
            {
                Debug.LogError("Max retry attempts reached. Please check your connection and token.");
            }
        }
    }

    public void UpdateOpacity(float newOpacity){
        // if (_displayRenderer != null){
        //     _displayRenderer.material.SetFloat("_Opacity", newOpacity);
        // }
        if (_leftEyeRenderer != null){
            _leftEyeRenderer.material.SetFloat("_Opacity", newOpacity);
        }
        if (_rightEyeRenderer != null){
            _rightEyeRenderer.material.SetFloat("_Opacity", newOpacity);
        }
    }

    public void UpdateOffset(float newOffset){
        var leftEye = transform.Find("LeftEye");
        var rightEye = transform.Find("RightEye");
        leftEye.transform.localPosition = new Vector3(newOffset, leftEye.transform.localPosition.y, leftEye.transform.localPosition.z);
        rightEye.transform.localPosition = new Vector3(-newOffset, rightEye.transform.localPosition.y, rightEye.transform.localPosition.z);
    }

    public void UpdateVerticalOffset(float newOffset){
        var leftEye = transform.Find("LeftEye");
        var rightEye = transform.Find("RightEye");
        leftEye.transform.localPosition = new Vector3(leftEye.transform.localPosition.x, newOffset, leftEye.transform.localPosition.z);
        rightEye.transform.localPosition = new Vector3(rightEye.transform.localPosition.x, -newOffset, rightEye.transform.localPosition.z);
    }

    public void UpdateStereoMode(bool isStereo){
        if (isStereo){
            leftOffset.x = 0.5f;

        }
        else{
            leftOffset.x = 0.0f;
        }
    }


    private void TrackSubscribed(IRemoteTrack track, RemoteTrackPublication publication, RemoteParticipant participant)
    {
        if (track is RemoteVideoTrack videoTrack)
        {
            Debug.Log($"Received video track from participant: {participant.Identity}");
            
            // Create a new VideoStream instance
            var stream = new VideoStream(videoTrack);
            
            // Set up the texture received event
            stream.TextureReceived += (texture) => {
                Debug.Log($"TextureReceived event fired - Texture: {(texture != null ? $"{texture.width}x{texture.height}" : "null")}");


                var width = texture.width/2.0f; // divide by 2 because we are using two eyes
                var height = texture.height;

                var aspectRatio = (float)width / (float)height;
                Debug.Log($"Aspect ratio: {aspectRatio}");  
                transform.localScale = new Vector3(1000.0f * aspectRatio, 1000.0f, 0.01f); // guarantee 1000px height
                if (texture != null) {
                    _pendingTexture = texture;
                    Debug.Log($"Updated pending texture: {texture.width}x{texture.height}");
                }
            };

            // Start the video stream
            stream.Start();
            StartCoroutine(stream.Update());
            Debug.Log("Video stream started and update coroutine initiated");
        }
    }

    void LateUpdate()
    {
        if (_pendingTexture == null)
        {
            Debug.Log("No pending texture available");
            return;
        }


        Debug.Log($"Converting texture: {_pendingTexture.width}x{_pendingTexture.height}");
        Texture2D tex2D = ToTexture2D(_pendingTexture);
        // tex2D = flipTextureHorizontally(tex2D);
        if (tex2D == null)
        {
            Debug.LogError("Failed to convert texture to Texture2D");
            return;
        }
        if (_leftEyeRenderTexture != null && _rightEyeRenderTexture != null)
        {
            // Graphics.Blit(tex2D, _displayRenderTexture);
            Graphics.Blit(tex2D, _leftEyeRenderTexture);
            Graphics.Blit(tex2D, _rightEyeRenderTexture);
            _leftEyeRenderer.material.SetVector("_LeftOffset", leftOffset);
            _leftEyeRenderer.material.SetVector("_LeftTiling", leftTiling);
            _rightEyeRenderer.material.SetVector("_RightOffset", rightOffset);
            _rightEyeRenderer.material.SetVector("_RightTiling", rightTiling);
        }

        // Clean up the intermediate texture
        if (tex2D != null && tex2D != _pendingTexture)
        {
            Destroy(tex2D);
        }
    }

    void OnDestroy()
    {
        if (_room != null)
        {
            _room.TrackSubscribed -= TrackSubscribed;
            _room.Disconnect();
        }

        // Clean up render textures
        if (_leftEyeRenderTexture != null)
        {
            _leftEyeRenderTexture.Release();
            Destroy(_leftEyeRenderTexture);
        }
        if (_rightEyeRenderTexture != null)
        {
            _rightEyeRenderTexture.Release();
            Destroy(_rightEyeRenderTexture);
        }
        // if (_displayRenderTexture != null)
        // {
        //     _displayRenderTexture.Release();
        //     Destroy(_displayRenderTexture);
        // }
        if (_leftEyeRenderer != null)
        {
            _leftEyeRenderer.material.mainTexture = null;
        }
        if (_rightEyeRenderer != null)
        {
            _rightEyeRenderer.material.mainTexture = null;
        }

    }

    private Texture2D ToTexture2D(Texture texture)
    {
        if (texture == null)
        {
            //Debug.LogWarning("Input texture is null in ToTexture2D");
            return null;
        }
        
        // If it's already a Texture2D, return it directly
        if (texture is Texture2D texture2D)
        {
           //Debug.Log("Input is already a Texture2D");
            return texture2D;
        }

        //Debug.Log($"Converting texture to Texture2D: {texture.width}x{texture.height}");
        // Create a new Texture2D with the same dimensions
        Texture2D result = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        
        // Create a temporary RenderTexture
        RenderTexture rt = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;
        
        // Copy the texture to the RenderTexture
        Graphics.Blit(texture, rt);
        
        // Read the pixels from the RenderTexture
        result.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        result.Apply();
        
        // Clean up
        RenderTexture.active = null;    
        rt.Release();
        
        //Debug.Log("Texture conversion completed successfully");
        return result;
    }



}
