using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using System;
using Unity.WebRTC;

public class StreamManager : MonoBehaviour
{

    public enum StreamSource{
        LIVEKIT,
        WEBRTC
    }
    //public string roomToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3ODAwMDY0MDcsImlzcyI6IkFQSWFTNVVmeXJQS3VjOCIsIm5iZiI6MTc0ODQ3MDQwOCwic3ViIjoiMiIsInZpZGVvIjp7ImNhblB1Ymxpc2giOnRydWUsImNhblB1Ymxpc2hEYXRhIjp0cnVlLCJjYW5TdWJzY3JpYmUiOnRydWUsInJvb20iOiJhYmNkIiwicm9vbUpvaW4iOnRydWV9fQ.t3E8f_yQdCS7N9Z4UPCeZs85C9ftFhzNaMfxvLEfYX8";

    //public string wsurl = "wss://test-ky7qsf6n.livekit.cloud";


    //public string localroomToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoidXNlciIsInZpZGVvIjp7InJvb21Kb2luIjp0cnVlLCJyb29tIjoiYSIsImNhblB1Ymxpc2giOnRydWUsImNhblN1YnNjcmliZSI6dHJ1ZSwiY2FuUHVibGlzaERhdGEiOnRydWV9LCJzdWIiOiJpZGVudGl0eSIsImlzcyI6ImRldmtleSIsIm5iZiI6MTc0OTU4MjAwNCwiZXhwIjoxNzQ5NjAzNjA0fQ.GoijGX9h1SSjjpU9pvX2TCn-jUOmoRvyqG2D8ngVd2g";

    public string localwsurl;

    public string roomName = "abcd";

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

    public StreamSource streamSource = StreamSource.WEBRTC;

    public float opacity = 1.0f;

    private MediaStream receiveStream;
    private RTCPeerConnection pc;

    private bool isConnecting = false;

    void Start()
    {
#if WEBRTC_3_0_0_PRE_5_OR_BEFORE
        WebRTC.Initialize();
#endif
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
        Connect(streamSource, roomName);

        //StartCoroutine(ConnectToRoom(streamSource, "abcd"));
    }

    public void Connect(StreamSource streamsource, string Roomname = "abcd")
    {
        if (isConnecting)
        {
            Debug.LogWarning("Already connecting to a room. Please wait.");
            return;
        }
        StartCoroutine(ConnectToWebRTC());
    }

    public void ResetRoom(){

    }
    
    public IEnumerator ConnectToWebRTC()
    {
        isConnecting = true;
        //transform.rotation = Quaternion.Euler(0, 0, 0);
        StartCoroutine(WebRTC.Update());

        pc = new RTCPeerConnection();
        receiveStream = new MediaStream();

        pc.OnIceCandidate = candidate =>
        {
            Debug.Log($"WebRTC: OnIceCandidate {candidate.ToString()}");
        };
        pc.OnIceConnectionChange = state =>
        {
            Debug.Log($"WebRTC: OnIceConnectionChange {state.ToString()}");
        };
        pc.OnTrack = e =>
        {
            receiveStream.AddTrack(e.Track);
        };

        receiveStream.OnAddTrack = e =>
        {
            Debug.Log($"WebRTC: OnAddTrack {e.ToString()}");
            if (e.Track is VideoStreamTrack videoTrack)
            {
                videoTrack.OnVideoReceived += texture =>
                {

                    var width = texture.width / 2.0f; // divide by 2 because we are using two eyes
                    var height = texture.height;

                    var aspectRatio = (float)width / (float)height;
                    Debug.Log($"Aspect ratio: {aspectRatio}");
                    transform.localScale = new Vector3(1000.0f * aspectRatio, 1000.0f, 0.01f); // guarantee 1000px height
                    if (texture != null)
                    {
                        _pendingTexture = texture;
                        Debug.Log($"Updated pending texture: {texture.width}x{texture.height}");
                    }
                };
            }
        };

        StartCoroutine(SetupPeerConnection());
        IEnumerator SetupPeerConnection()
        {
            RTCRtpTransceiverInit init = new RTCRtpTransceiverInit();
            init.direction = RTCRtpTransceiverDirection.RecvOnly;
            pc.AddTransceiver(TrackKind.Audio, init);
            pc.AddTransceiver(TrackKind.Video, init);

            yield return StartCoroutine(PeerNegotiationNeeded());
        }

        // Generate offer.
        IEnumerator PeerNegotiationNeeded()
        {
            var op = pc.CreateOffer();
            yield return op;

            Debug.Log($"WebRTC: CreateOffer done={op.IsDone}, hasError={op.IsError}, {op.Desc}");
            if (op.IsError) yield break;

            yield return StartCoroutine(OnCreateOfferSuccess(op.Desc));
        }

        // When offer is ready, set to local description.
        IEnumerator OnCreateOfferSuccess(RTCSessionDescription offer)
        {
            var op = pc.SetLocalDescription(ref offer);
            Debug.Log($"WebRTC: SetLocalDescription {offer.type} {offer.sdp}");
            yield return op;

            Debug.Log($"WebRTC: Offer done={op.IsDone}, hasError={op.IsError}");
            if (op.IsError) yield break;

            yield return StartCoroutine(ExchangeSDP(localwsurl, offer.sdp));
        }

        // Exchange SDP(offer) with server, got answer.
        IEnumerator ExchangeSDP(string url, string offer)
        {
            // Use Task to call async methods.
            var task = System.Threading.Tasks.Task<string>.Run(async () =>
            {
                System.Uri uri = new System.UriBuilder(url).Uri;
                Debug.Log($"WebRTC: Build uri {uri}");

                var content = new System.Net.Http.StringContent(offer);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/sdp");

                var client = new System.Net.Http.HttpClient();
                var res = await client.PostAsync(uri, content);
                res.EnsureSuccessStatusCode();

                string data = await res.Content.ReadAsStringAsync();
                Debug.Log($"WebRTC: Exchange SDP ok, answer is {data}");
                return data;
            });

            // Covert async to coroutine yield, wait for task to be completed.
            yield return new WaitUntil(() => task.IsCompleted);
            // Check async task exception, it won't throw it automatically.
            if (task.Exception != null)
            {
                Debug.Log($"WebRTC: Exchange SDP failed, url={url}, err is {task.Exception.ToString()}");
                yield break;
            }

            StartCoroutine(OnGotAnswerSuccess(task.Result));
        }

        // When got answer, set remote description.
        IEnumerator OnGotAnswerSuccess(string answer)
        {
            isConnecting = false;
            RTCSessionDescription desc = new RTCSessionDescription();
            desc.type = RTCSdpType.Answer;
            desc.sdp = answer;
            var op = pc.SetRemoteDescription(ref desc);
            yield return op;

            Debug.Log($"WebRTC: Answer done={op.IsDone}, hasError={op.IsError}");
            yield break;
        }
        yield return null;
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
        leftOffset.x = (isStereo) ? 0.5f : 0.0f;
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


    public void savePrefs()
    {
        PlayerPrefs.SetString("url", localwsurl);
    }


}
