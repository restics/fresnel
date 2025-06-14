using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SettingsPanelUI : MonoBehaviour
{

    private float _originalPanelScaleX = 1.0f;
    private float _originalPanelScaleY = 1.0f;
    private float _originalPanelPositionY = 0.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        var streamManager = FindFirstObjectByType<StreamManager>();
        _originalPanelScaleX = streamManager.transform.localScale.x;
        _originalPanelScaleY = streamManager.transform.localScale.y;
        _originalPanelPositionY = streamManager.transform.localPosition.y;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpacitySliderUpdate(){
        var slider = GetComponent<Slider>();

        var streamManager = FindFirstObjectByType<StreamManager>();
        if (slider == null || streamManager == null){ 
            Debug.LogError("Slider or stream manager not found");
            return; 
        }

        streamManager.UpdateOpacity(slider.value);

        transform.GetComponentInChildren<TextMeshProUGUI>().text = $"Current Value : {slider.value}";
        Debug.Log($"Opacity updated to {slider.value:F2}");
    }


    public void OnOffsetSliderUpdate(){
        var slider = GetComponent<Slider>();

        var streamManager = FindFirstObjectByType<StreamManager>();
        if (slider == null || streamManager == null){ 
            Debug.LogError("Slider or stream manager not found");
            return; 
        }
        var mappedOffset = Mathf.Lerp(-0.25f, 0.25f, slider.value);
        streamManager.UpdateOffset(mappedOffset);

        Debug.Log($" Offset updated to {mappedOffset}");
    }

    public void OnVerticalOffsetSliderUpdate(){
        var slider = GetComponent<Slider>();

        var streamManager = FindFirstObjectByType<StreamManager>();
        if (slider == null || streamManager == null){ 
            Debug.LogError("Slider or stream manager not found");
            return; 
        }
        var mappedOffset = Mathf.Lerp(-0.05f, 0.05f, slider.value);
        streamManager.UpdateVerticalOffset(mappedOffset);

        Debug.Log($" Offset updated to {mappedOffset}");
    }

    public void OnScaleSliderUpdate(){
        var slider = GetComponent<Slider>();

        var streamManager = FindFirstObjectByType<StreamManager>();
        if (slider == null || streamManager == null){ 
            Debug.LogError("Slider or stream manager not found");
            return; 
        }
        var mappedScale = Mathf.Lerp(0.1f, 2.0f, slider.value);
        streamManager.transform.localScale = new Vector3(mappedScale*_originalPanelScaleX, mappedScale*_originalPanelScaleY, 0.01f);
        streamManager.transform.localPosition = new Vector3(0, _originalPanelPositionY * mappedScale, 0);

        Debug.Log($" Scale updated to {mappedScale}");
    }
    public void OnStereoModeUpdate(){
        var toggle = GetComponent<Toggle>();
        var streamManager = FindFirstObjectByType<StreamManager>();
        if (toggle == null || streamManager == null){ 
            Debug.LogError("Toggle or stream manager not found");
            return; 
        }
        streamManager.UpdateStereoMode(toggle.isOn);
        Debug.Log($" Stereo mode updated to {toggle.isOn}");
    }

    public void OnStreamSourceUpdate(TMP_InputField inputField){
        var toggle = GetComponent<Toggle>();
        var streamManager = FindFirstObjectByType<StreamManager>();
        if (toggle == null || streamManager == null){ 
            Debug.LogError("Toggle or stream manager not found");
            return; 
        }
        streamManager.ResetRoom();
        streamManager.streamSource = toggle.isOn ? StreamManager.StreamSource.CLOUD : StreamManager.StreamSource.LOCAL;
        StartCoroutine(streamManager.ConnectToRoom(streamManager.streamSource, inputField.text));

        Debug.Log($" Stream source updated to {streamManager.streamSource}");
    }

    public void ConnectToRoom(TMP_InputField inputField){
        var streamManager = FindFirstObjectByType<StreamManager>();
        if (streamManager == null){ 
            Debug.LogError("Stream manager not found");
            return; 
        }
        StartCoroutine(streamManager.ConnectToRoom(streamManager.streamSource, inputField.text));
    }
}
