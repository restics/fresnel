using UnityEngine;
using LiveKit;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using LiveKit.Proto;

public class StreamManager : MonoBehaviour
{
    public string roomToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3ODAwMDY0MDcsImlzcyI6IkFQSWFTNVVmeXJQS3VjOCIsIm5iZiI6MTc0ODQ3MDQwOCwic3ViIjoiMiIsInZpZGVvIjp7ImNhblB1Ymxpc2giOnRydWUsImNhblB1Ymxpc2hEYXRhIjp0cnVlLCJjYW5TdWJzY3JpYmUiOnRydWUsInJvb20iOiJhYmNkIiwicm9vbUpvaW4iOnRydWV9fQ.t3E8f_yQdCS7N9Z4UPCeZs85C9ftFhzNaMfxvLEfYX8";
    private Room _room;
    private int _retryCount = 0;
    private const int MAX_RETRIES = 3;

    IEnumerator Start()
    {
        yield return ConnectToRoom();
    }

    private IEnumerator ConnectToRoom()
    {
        _room = new Room();
        _room.ConnectionStateChanged += OnConnectionStateChanged;
        _room.TrackSubscribed += TrackSubscribed;

        var options = new RoomOptions
        {
            AutoSubscribe = true,
            Dynacast = true,
            AdaptiveStream = true,
            JoinRetries = 3
        };

        Debug.Log($"Attempting to connect to LiveKit room (Attempt {_retryCount + 1}/{MAX_RETRIES})");
        var connect = _room.Connect("wss://test-ky7qsf6n.livekit.cloud", roomToken, options);
        yield return connect;

        if (!connect.IsError)
        {
            _retryCount = 0;
            var text = transform.parent.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Connected to " + _room.Name;
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
                StartCoroutine(ConnectToRoom());
            }
            else
            {
                Debug.LogError("Max retry attempts reached. Please check your connection and token.");
            }
        }
    }

    private void OnConnectionStateChanged(ConnectionState state)
    {
        Debug.Log($"Connection state changed to: {state}");
        switch (state)
        {
            case ConnectionState.Connecting:
                Debug.Log("Connecting to LiveKit...");
                break;
            case ConnectionState.Connected:
                Debug.Log("Connected to LiveKit!");
                break;
            case ConnectionState.Disconnected:
                Debug.Log("Disconnected from LiveKit");
                break;
            case ConnectionState.Failed:
                Debug.LogError("Failed to connect to LiveKit");
                break;
        }
    }

    void TrackSubscribed(IRemoteTrack track, RemoteTrackPublication publication, RemoteParticipant participant)
    {
        if (track is RemoteVideoTrack videoTrack)
        {
            var rawImage = GetComponent<RawImage>();
            var stream = new VideoStream(videoTrack);
            stream.TextureReceived += (tex) =>
            {
                rawImage.texture = tex;
            };
            StartCoroutine(stream.Update());
        }
        else if (track is RemoteAudioTrack audioTrack)
        {
            GameObject audObject = new GameObject(audioTrack.Sid);
            var source = audObject.AddComponent<AudioSource>();
            var stream = new AudioStream(audioTrack, source);
            // Audio is being played on the source ..
        }
    }

    void OnDestroy()
    {
        if (_room != null)
        {
            _room.ConnectionStateChanged -= OnConnectionStateChanged;
            _room.TrackSubscribed -= TrackSubscribed;
            _room.Disconnect();
        }
    }
}
