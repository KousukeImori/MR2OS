using UnityEngine;

public class ShoulderAngleTracker : MonoBehaviour
{
    [SerializeField] private Transform chest;       // 胸の中心ボーン
    [SerializeField] private Transform shoulderR;   // 右肩ボーン
    [SerializeField] private Transform upperArmR;   // 右上腕ボーン
    [SerializeField] private Transform forearmR;    // 右前腕ボーン
   // [SerializeField] private Transform elbowR;      // 右肘ボーン

    private float prevPitch = 0f;
    private float prevRoll = 0f;

    private const float quantizeStep = 5f;   // 5度刻み
    private const float hysteresis = 2.5f;   // 2.5度未満の変化は無視

    void Update()
    {
        // --- 基準座標系 ---
        Vector3 chestX = chest.forward.normalized;    // 胸の向き
        Vector3 chestZ = Vector3.up;                  // 鉛直上
        Vector3 chestY = Vector3.Cross(chestZ, chestX).normalized;

        // --- 上腕ベクトル (肩→上腕) ---
        Vector3 upperArm = (upperArmR.position - shoulderR.position).normalized;

        // --- 前腕ベクトル (肘→前腕) ---
    //    Vector3 forearm = (forearmR.position - elbowR.position).normalized;

        // --- ピッチ（下ろした腕を0°, 前方向に最大135°まで）---
        Vector3 projXZ = Vector3.ProjectOnPlane(upperArm, chestY).normalized;
        float rawPitch = Mathf.Atan2(
            Vector3.Dot(projXZ, chestX),
            Vector3.Dot(projXZ, chestZ)
        ) * Mathf.Rad2Deg;

        float pitch;
        if (rawPitch < 0f)
            pitch = 0f; // 後ろは0°
        else
            pitch = Mathf.Min(rawPitch, 135f); // 前は135°まで

        // --- ロール（0°基準、-20°～135°で制限）---
        Vector3 projYZ = Vector3.ProjectOnPlane(upperArm, chestX).normalized;
        float rawRoll = Mathf.Atan2(
            Vector3.Dot(projYZ, chestY),
            Vector3.Dot(projYZ, chestZ)
        ) * Mathf.Rad2Deg;

        float roll = Mathf.Clamp(rawRoll, -20f, 135f);

        // --- 量子化 ---
        float qPitch = Mathf.Round(pitch / quantizeStep) * quantizeStep;
        float qRoll  = Mathf.Round(roll  / quantizeStep) * quantizeStep;

        // --- ヒステリシス ---
        if (Mathf.Abs(qPitch - prevPitch) >= hysteresis)
            prevPitch = qPitch;
        else
            qPitch = prevPitch;

        if (Mathf.Abs(qRoll - prevRoll) >= hysteresis)
            prevRoll = qRoll;
        else
            qRoll = prevRoll;

        // --- デバッグログ ---
        Debug.Log($"Right Shoulder -> Pitch: {qPitch}°, Roll: {qRoll}°");
    }
}
