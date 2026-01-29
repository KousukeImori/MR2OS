using TMPro;
using UnityEngine;

public class Quest3ControllerInputDisplay : MonoBehaviour
{
    [SerializeField]
    //private TMP_Text textComponent;  // TextMeshProで出力

    void Update()
    {
        // --- 右コントローラ ---
        bool isAButtonPressed = OVRInput.Get(OVRInput.RawButton.A);
        bool isAButtonTouched = OVRInput.Get(OVRInput.RawTouch.A);
        bool isBButtonPressed = OVRInput.Get(OVRInput.RawButton.B);
        bool isBButtonTouched = OVRInput.Get(OVRInput.RawTouch.B);

        float rightTrigger = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger); // 0.0～1.0
        float rightGrip = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);     // 0.0～1.0
        Vector2 rightStick = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);   // (-1～1)

        // --- 左コントローラ ---
        bool isXButtonPressed = OVRInput.Get(OVRInput.RawButton.X);
        bool isYButtonPressed = OVRInput.Get(OVRInput.RawButton.Y);
        float leftTrigger = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
        float leftGrip = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger);
        Vector2 leftStick = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);

        // --- テキストに表示 ---
        /*Debug.Log(
            $"【右コントローラAボタン押下: {isAButtonPressed}\n" +
            $"Aボタンタッチ: {isAButtonTouched}\n" +
            $"Bボタン押下: {isBButtonPressed}\n" +
            $"Bボタンタッチ: {isBButtonTouched}\n" +
            $"トリガー値: {rightTrigger:F2}\n" +
            $"グリップ値: {rightGrip:F2}\n" +
            $"スティック: {rightStick}\n\n" +

            $"【左コントローラ】\n" +
            $"Xボタン押下: {isXButtonPressed}\n" +
            $"Yボタン押下: {isYButtonPressed}\n" +
            $"トリガー値: {leftTrigger:F2}\n" +
            $"グリップ値: {leftGrip:F2}\n" +
            $"スティック: {leftStick}");*/
        Debug.Log($"右トリガ値 {rightTrigger}");
        Debug.Log($"右グリップ値 {rightGrip}");
    }
}
