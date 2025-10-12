using UnityEngine;

namespace raspberly.ovr
{
    public class HumanoidJointAngles : MonoBehaviour
    {
        [Header("ボーン割り当て")]
        public Transform neck;
        public Transform head;
        public Transform pelvis;
        public Transform chest;

        public Transform rightShoulder;
        public Transform rightLowerArm;
        public Transform rightHand;

        public Transform leftShoulder;
        public Transform leftLowerArm;
        public Transform leftHand;

        [Header("量子化設定")]
        [Tooltip("角度を何度単位で丸めるか（例:5）」")]
        public float quantizeStep = 5f;
        [Tooltip("trueなら量子化後にヒステリシスを使って小刻みな変化を抑える")]
        public bool useHysteresis = true;
        // ヒステリシスに使う閾値（量子化ステップの半分がデフォルト）
        public float hysteresisThreshold = -1f;

        // 前フレームのservo配列（ヒステリシス用）
        private float[] prevServoAngles;

        void Start()
        {
            // servo角は 11 要素の想定（NeckPitch, NeckYaw, Waist, R:Pitch,Roll,Elbow,Wrist, L:Pitch,Roll,Elbow,Wrist）
            prevServoAngles = new float[11];
            if (hysteresisThreshold <= 0f) hysteresisThreshold = quantizeStep / 2f;
        }

        void Update()
        {
            // --- 角度取得（元の高精度な角度） ---
            Vector3 neckEuler = GetRelativeEuler(neck, head);
            float neckPitch = neckEuler.x;
            float neckYaw = neckEuler.y;

            Vector3 waistEuler = GetRelativeEuler(pelvis, chest);
            float waistPitch = waistEuler.x;

            Vector3 rShoulderEuler = GetRelativeEuler(chest, rightShoulder);
            float rShoulderPitch = rShoulderEuler.x;
            float rShoulderRoll = rShoulderEuler.z;
            float rElbow = GetJointAngle(rightShoulder, rightLowerArm, rightHand);
            Vector3 rWristEuler = GetRelativeEuler(rightLowerArm, rightHand);
            float rWrist = rWristEuler.x;

            Vector3 lShoulderEuler = GetRelativeEuler(chest, leftShoulder);
            float lShoulderPitch = lShoulderEuler.x;
            float lShoulderRoll = lShoulderEuler.z;
            float lElbow = GetJointAngle(leftShoulder, leftLowerArm, leftHand);
            Vector3 lWristEuler = GetRelativeEuler(leftLowerArm, leftHand);
            float lWrist = lWristEuler.x;

            // --- 量子化（5度単位） ---
            float[] rawServoAngles = new float[]
            {
                neckPitch, neckYaw,
                waistPitch,
                rShoulderPitch, rShoulderRoll, rElbow, rWrist,
                lShoulderPitch, lShoulderRoll, lElbow, lWrist
            };

            float[] qServoAngles = new float[rawServoAngles.Length];
            for (int i = 0; i < rawServoAngles.Length; i++)
            {
                qServoAngles[i] = QuantizeAngle(rawServoAngles[i], quantizeStep);
            }

            // --- ヒステリシス適用（必要なら） ---
            if (useHysteresis)
            {
                for (int i = 0; i < qServoAngles.Length; i++)
                {
                    float prev = prevServoAngles[i];
                    float now = qServoAngles[i];

                    // only accept change if difference >= threshold
                    if (Mathf.Abs(now - prev) < hysteresisThreshold)
                    {
                        // 変化が小さければ前回値を維持
                        qServoAngles[i] = prev;
                    }
                    else
                    {
                        prevServoAngles[i] = now;
                    }
                }
            }
            else
            {
                // ヒステリシス無効なら前フレームを上書き
                prevServoAngles = (float[])qServoAngles.Clone();
            }

            // --- デバッグ ---
            Debug.Log($"Quantized angles (step {quantizeStep}°): " +
                $"NeckP:{qServoAngles[0]}, NeckY:{qServoAngles[1]}, Waist:{qServoAngles[2]} ...");
            // --- デバッグ ---
Debug.Log(
    $"Right Shoulder Pitch:{qServoAngles[3]}°, " +
    $"Right Shoulder Roll:{qServoAngles[4]}°, " +
    $"Right Elbow:{qServoAngles[5]}°, " +
    $"Right Wrist:{qServoAngles[6]}°"
);

            // --- ここで qServoAngles をシリアル/ROSに送る／ロボットモデルに適用する ---
        }

        /// <summary>
        /// 指定ステップで角度を丸める（最近接）。角度は -180..180 の想定。
        /// </summary>
        private float QuantizeAngle(float angle, float step)
        {
            angle = NormalizeAngle(angle); // -180..180
            if (step <= 0f) return angle;
            float quant = Mathf.Round(angle / step) * step;
            // 例えば -182 → 178 になってしまうことを防ぐため正規化
            return NormalizeAngle(quant);
        }

        /// <summary>
        /// 角度を -180..180 に正規化
        /// </summary>
        private float NormalizeAngle(float ang)
        {
            float a = ang % 360f;
            if (a > 180f) a -= 360f;
            if (a <= -180f) a += 360f;
            return a;
        }

        /// <summary>
        /// 親ボーンに対する子ボーンの相対回転をEuler角に変換（正規化）
        /// </summary>
        private Vector3 GetRelativeEuler(Transform parent, Transform child)
        {
            Quaternion relative = Quaternion.Inverse(parent.rotation) * child.rotation;
            Vector3 euler = relative.eulerAngles;

            if (euler.x > 180) euler.x -= 360;
            if (euler.y > 180) euler.y -= 360;
            if (euler.z > 180) euler.z -= 360;

            return euler;
        }

        /// <summary>
        /// 3点 (A-B-C) からBを頂点とした角度を計算（単軸用）
        /// </summary>
        private float GetJointAngle(Transform a, Transform b, Transform c)
        {
            Vector3 ab = a.position - b.position;
            Vector3 cb = c.position - b.position;
            return Vector3.Angle(ab, cb);
        }
    }
}
