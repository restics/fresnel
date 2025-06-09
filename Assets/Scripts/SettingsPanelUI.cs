using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SettingsPanelUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
        Debug.Log($"Opacity updated to {slider.value}");
    }


    public void OnOffsetSliderUpdate(){
        var slider = GetComponent<Slider>();

        var streamManager = FindFirstObjectByType<StreamManager>();
        if (slider == null || streamManager == null){ 
            Debug.LogError("Slider or stream manager not found");
            return; 
        }
        var mappedOffset = Mathf.Lerp(-0.05f, 0.05f, slider.value);
        streamManager.UpdateOffset(mappedOffset);

        Debug.Log($" Offset updated to {mappedOffset}");
    }


    public void OnScaleSliderUpdate(){
        var slider = GetComponent<Slider>();

        var streamManager = FindFirstObjectByType<StreamManager>();
        if (slider == null || streamManager == null){ 
            Debug.LogError("Slider or stream manager not found");
            return; 
        }
        var mappedScale = Mathf.Lerp(0.1f, 1.0f, slider.value);
        streamManager.transform.localScale = new Vector3(mappedScale*160, mappedScale*90, 0.1f);

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
}
