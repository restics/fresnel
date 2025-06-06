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


    public void OnOffsetSliderUpdate(bool isLeft){
        var slider = GetComponent<Slider>();

        var streamManager = FindFirstObjectByType<StreamManager>();
        if (slider == null || streamManager == null){ 
            Debug.LogError("Slider or stream manager not found");
            return; 
        }
        var mappedOffset = slider.value * 0.1f - 0.05f;
        streamManager.UpdateOffset(mappedOffset, isLeft);

        Debug.Log($" Offset updated to {mappedOffset} for {(isLeft ? "left" : "right")} eye");
    }
}
