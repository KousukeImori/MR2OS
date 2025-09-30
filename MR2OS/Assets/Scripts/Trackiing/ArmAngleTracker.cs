using UnityEngine;

namespace raspberly.ovr
{
    public class ArmAngleTracker : MonoBehaviour
    {
        [Header("左腕のボーン")]
        public Transform leftShoulder;
        public Transform leftElbow;
        public Transform leftWrist;

        [Header("右腕のボーン")]
        public Transform rightShoulder;
        public Transform rightElbow;
        public Transform rightWrist;

        void Update()
        {
            // 左腕の角度を計算
            float leftElbowAngle = CalculateAngle(leftShoulder, leftElbow, leftWrist);
            Debug.Log("左ひじ角度: " + leftElbowAngle);

            // 右腕の角度を計算
            float rightElbowAngle = CalculateAngle(rightShoulder, rightElbow, rightWrist);
            Debug.Log("右ひじ角度: " + rightElbowAngle);
        }

        /// <summary>
        /// 3点 (A-B-C) から、Bを頂点とした角度を求める
        /// </summary>
        private float CalculateAngle(Transform a, Transform b, Transform c)
        {
            Vector3 ab = a.position - b.position;
            Vector3 cb = c.position - b.position;

            float angle = Vector3.Angle(ab, cb);
            return angle;
        }
    }
}
