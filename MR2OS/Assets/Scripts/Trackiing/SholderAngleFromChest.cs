using UnityEngine;

public class ShoulderAngleTrackerFromChest: MonoBehaviour
{
    [SerializeField] private Transform chest;
    [SerializeField] private Transform upperArm;
    [SerializeField] private Transform foreArm;

    void Update()
    {
        // --- 上腕ベクトル（肩→肘方向）---
        Vector3 upperDirWorld = (foreArm.position - upperArm.position).normalized;

        // --- 胸ローカル空間に変換 ---
        Vector3 localDir = chest.InverseTransformDirection(upperDirWorld).normalized;
        // chest.forward = z, chest.up = y, chest.right = x

        // === ピッチ（前後の上げ下げ）===
        // 前方(z)へ上げた時に正角度、真下で0°
        float pitchRad = Mathf.Atan2(localDir.y, Mathf.Abs(localDir.z));
        float pitchDeg = pitchRad * Mathf.Rad2Deg;

        // 制限処理
        if (localDir.z < 0f) // 背中側に回ったら0°
            pitchDeg = 0f;
        pitchDeg = Mathf.Clamp(pitchDeg, 0f, 135f);

        // === ロール（内外転）===
        // 外へ開く→+方向、内へ閉じる→-方向
        float rollRad = Mathf.Atan2(localDir.x, Mathf.Abs(localDir.z));
        float rollDeg = rollRad * Mathf.Rad2Deg;
        rollDeg = Mathf.Clamp(rollDeg, -20f, 135f);

        // === デバッグ出力 ===
        Debug.Log($"Pitch: {pitchDeg:F1}°, Roll: {rollDeg:F1}° (LocalDir: {localDir:F3})");
    }
}
