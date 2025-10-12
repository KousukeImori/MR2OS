using System;
using UnityEngine;

public class UpperBodyFullTracker : MonoBehaviour
{
    [Header("Right Arm Transforms")]
    [SerializeField] private Transform rightShoulder;
    [SerializeField] private Transform rightUpperArm;
    [SerializeField] private Transform rightForearm;
    [SerializeField] private Transform rightWrist;

    [Header("Left Arm Transforms")]
    [SerializeField] private Transform leftShoulder;
    [SerializeField] private Transform leftUpperArm;
    [SerializeField] private Transform leftForearm;
    [SerializeField] private Transform leftWrist;

    [Header("Head Transforms")]
    [SerializeField] private Transform neckBase;
    [SerializeField] private Transform head;

    private TextMesh infoText;
    private Camera mainCam;

    // 平均化
    private int counter = 0;
    private int sampling_freq = 40;

    private int rightServo1Sum, rightServo2Sum;
    private int leftServo1Sum, leftServo2Sum;
    private int rightWristTwistSum, leftWristTwistSum;
    private int neckYawSum, neckPitchSum;
    private int rightElbowSum, leftElbowSum;

    void Start()
    {
        mainCam = Camera.main;

        GameObject go = new GameObject("JointAngleText");
        infoText = go.AddComponent<TextMesh>();
        infoText.fontSize = 64;
        infoText.color = Color.yellow;
        infoText.anchor = TextAnchor.MiddleCenter;
        infoText.alignment = TextAlignment.Center;
        go.transform.localScale = Vector3.one * 0.01f;
    }

    void Update()
    {
        if (mainCam == null) return;

        // ====================== 右腕 ======================
        int rightShoulder1, rightShoulder2;
        int rightElbow = CalcElbowAngle(rightUpperArm, rightForearm, rightWrist);
        (rightShoulder1, rightShoulder2) = CalcShoulderServoAngles(rightShoulder, rightUpperArm);
        int rightWristTwist = CalcWristTwist(rightForearm, rightWrist); // Y軸

        // ====================== 左腕 ======================
        int leftShoulder1, leftShoulder2;
        int leftElbow = CalcElbowAngle(leftUpperArm, leftForearm, leftWrist);
        (leftShoulder1, leftShoulder2) = CalcShoulderServoAngles(leftShoulder, leftUpperArm);
        int leftWristTwist = CalcWristTwist(leftForearm, leftWrist); // Y軸

        // ====================== 首 ======================
        (int neckYaw, int neckPitch) = CalcNeckAngles(neckBase, head);

        // 平均化用に加算
        if (counter < sampling_freq)
        {
            rightServo1Sum += rightShoulder1;
            rightServo2Sum += rightShoulder2;
            rightElbowSum += rightElbow;
            rightWristTwistSum += rightWristTwist;

            leftServo1Sum += leftShoulder1;
            leftServo2Sum += leftShoulder2;
            leftElbowSum += leftElbow;
            leftWristTwistSum += leftWristTwist;

            neckYawSum += neckYaw;
            neckPitchSum += neckPitch;
        }
        else if (counter == sampling_freq)
        {
            int avgRightServo1 = rightServo1Sum / sampling_freq;
            int avgRightServo2 = rightServo2Sum / sampling_freq;
            int avgRightElbow = rightElbowSum / sampling_freq;
            int avgRightWrist = rightWristTwistSum / sampling_freq;

            int avgLeftServo1 = leftServo1Sum / sampling_freq;
            int avgLeftServo2 = leftServo2Sum / sampling_freq;
            int avgLeftElbow = leftElbowSum / sampling_freq;
            int avgLeftWrist = leftWristTwistSum / sampling_freq;

            int avgNeckYaw = neckYawSum / sampling_freq;
            int avgNeckPitch = neckPitchSum / sampling_freq;

            Debug.Log($"Right Shoulder: servo1={avgRightServo1}°, servo2={avgRightServo2}°, Elbow={avgRightElbow}°, Wrist(Y)={avgRightWrist}°");
            Debug.Log($"Left Shoulder: servo1={avgLeftServo1}°, servo2={avgLeftServo2}°, Elbow={avgLeftElbow}°, Wrist(Y)={avgLeftWrist}°");
            Debug.Log($"Neck: Yaw={avgNeckYaw}°, Pitch={avgNeckPitch}°");

            // リセット
            counter = 0;
            rightServo1Sum = rightServo2Sum = 0;
            rightElbowSum = rightWristTwistSum = 0;
            leftServo1Sum = leftServo2Sum = 0;
            leftElbowSum = leftWristTwistSum = 0;
            neckYawSum = neckPitchSum = 0;
        }

        // ====================== 表示 ======================
        infoText.text =
            $"Right Shoulder: {rightShoulder1}°, {rightShoulder2}°\n" +
            $"Right Elbow: {rightElbow}°\n" +
            $"Right Wrist Twist(Y): {rightWristTwist}°\n\n" +
            $"Left Shoulder: {leftShoulder1}°, {leftShoulder2}°\n" +
            $"Left Elbow: {leftElbow}°\n" +
            $"Left Wrist Twist(Y): {leftWristTwist}°\n\n" +
            $"Neck Yaw: {neckYawSum / Mathf.Max(1, counter)}°, Pitch: {neckPitchSum / Mathf.Max(1, counter)}°";

        infoText.transform.position = mainCam.transform.position + mainCam.transform.forward * 1.0f;
        infoText.transform.rotation = Quaternion.LookRotation(infoText.transform.position - mainCam.transform.position);

        counter++;
    }

    // ------------------------- 肩角度 -------------------------
    private (int, int) CalcShoulderServoAngles(Transform shoulder, Transform upperArm)
    {
        if (shoulder == null || upperArm == null) return (0, 0);

        Quaternion relRot = Quaternion.Inverse(shoulder.rotation) * upperArm.rotation;
        Vector3 euler = relRot.eulerAngles;

        float y_vec_x = (float)((Math.Cos(euler.z * Mathf.Deg2Rad) * Math.Sin(euler.y * Mathf.Deg2Rad) * Math.Sin(euler.x * Mathf.Deg2Rad)) -
                                 (Math.Sin(euler.z * Mathf.Deg2Rad) * Math.Cos(euler.x * Mathf.Deg2Rad)));
        float y_vec_y = (float)((Math.Sin(euler.z * Mathf.Deg2Rad) * Math.Sin(euler.y * Mathf.Deg2Rad) * Math.Sin(euler.x * Mathf.Deg2Rad)) +
                                 (Math.Cos(euler.z * Mathf.Deg2Rad) * Math.Cos(euler.x * Mathf.Deg2Rad)));
        float y_vec_z = (float)(Math.Cos(euler.y * Mathf.Deg2Rad) * Math.Sin(euler.x * Mathf.Deg2Rad));

        float size = Mathf.Sqrt(y_vec_x * y_vec_x + y_vec_y * y_vec_y + y_vec_z * y_vec_z);
        float propo_cos = y_vec_y / size;

        int servo1 = Mathf.RoundToInt(Mathf.Atan2(y_vec_x, y_vec_z) * Mathf.Rad2Deg);
        int servo2 = Mathf.RoundToInt(Mathf.Acos(propo_cos) * Mathf.Rad2Deg);

        if (servo1 < 0) servo1 += 360;
        if (servo2 < 0) servo2 += 360;

        return (servo1, servo2);
    }

    // ------------------------- 肘角度 -------------------------
    private int CalcElbowAngle(Transform upperArm, Transform forearm, Transform wrist)
    {
        if (upperArm == null || forearm == null || wrist == null) return 0;

        Vector3 upperToFore = (forearm.position - upperArm.position).normalized;
        Vector3 foreToWrist = (wrist.position - forearm.position).normalized;

        float angle = Vector3.Angle(upperToFore, foreToWrist);
        float elbowAngle = 180f - angle;
        return Mathf.RoundToInt(elbowAngle);
    }

    // ------------------------- 手首のひねり角(Y軸) -------------------------
    private int CalcWristTwist(Transform forearm, Transform wrist)
    {
        if (forearm == null || wrist == null) return 0;

        Quaternion relRot = Quaternion.Inverse(forearm.rotation) * wrist.rotation;
        Vector3 euler = relRot.eulerAngles;

        int twistY = Mathf.RoundToInt(NormalizeAngle(euler.y));
        return twistY;
    }

    // ------------------------- 首の回転角 -------------------------
    private (int, int) CalcNeckAngles(Transform neckBase, Transform head)
    {
        if (neckBase == null || head == null) return (0, 0);

        Quaternion relRot = Quaternion.Inverse(neckBase.rotation) * head.rotation;
        Vector3 euler = relRot.eulerAngles;

        int yaw = Mathf.RoundToInt(NormalizeAngle(euler.y));   // 左右
        int pitch = Mathf.RoundToInt(NormalizeAngle(euler.x)); // 上下

        return (yaw, pitch);
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180) angle -= 360;
        return angle;
    }
}
