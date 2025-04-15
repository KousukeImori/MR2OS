using TMPro;
using UnityEngine;

public class ControllerButtonChecker : MonoBehaviour
{
    [SerializeField]
    private TMP_Text textComponent;
    
    void Update()
    {
        // コントローラのAボタンが押されたかどうかを取得
        bool isAButtonPressed = OVRInput.Get(OVRInput.RawButton.A);
        // コントローラのAボタンに指が振れているかどうかを取得
        bool isAButtonTouched = OVRInput.Get(OVRInput.RawTouch.A);
        
        // テキストに表示
        textComponent.text = $"isAButtonPressed: {isAButtonPressed}\nisAButtonTouched: {isAButtonTouched}";
    }
}