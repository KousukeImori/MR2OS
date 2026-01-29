using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;                     // ROS TCP Connector
using RosMessageTypes.Std;                               // std_msgs/Float32MultiArray

public class HumanoidControllerPublisher : MonoBehaviour
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

    [Header("Debug Settings")]
    [Tooltip("デバッグモード: インスペクターから角度を手動で指定します")]
    [SerializeField] private bool useManualOverride = false;

    [Tooltip("右肩サーボ1角度")]
    [SerializeField, Range(0, 270)] private float manualRightServo1 = 0f;
    [Tooltip("右肩サーボ2角度")]
    [SerializeField, Range(0, 270)] private float manualRightServo2 = 0f;
    [Tooltip("右肘角度")]
    [SerializeField, Range(0, 270)] private float manualRightElbow = 0f;
    [Tooltip("右手首ひねり角")]
    [SerializeField, Range(0, 270)] private float manualRightWrist = 0f;

    [Tooltip("左肩サーボ1角度")]
    [SerializeField, Range(0, 270)] private float manualLeftServo1 = 0f;
    [Tooltip("左肩サーボ2角度")]
    [SerializeField, Range(0, 270)] private float manualLeftServo2 = 0f;
    [Tooltip("左肘角度")]
    [SerializeField, Range(0, 270)] private float manualLeftElbow = 0f;
    [Tooltip("左手首ひねり角")]
    [SerializeField, Range(0, 270)] private float manualLeftWrist = 0f;

    [Tooltip("首Yaw角度")]
    [SerializeField, Range(-180, 180)] private float manualNeckYaw = 0f;
    [Tooltip("首Pitch角度")]
    [SerializeField, Range(-180, 180)] private float manualNeckPitch = 0f;

    private TextMesh infoText;
    private Camera mainCam;

    private int counter = 0;
    private float R_trig = 0;
    private int sampling_freq = 10;

    private float rightServo1Sum, rightServo2Sum;
    private float leftServo1Sum, leftServo2Sum;
    private float rightWristTwistSum, leftWristTwistSum;
    private float neckYawSum, neckPitchSum;
    private float rightElbowSum, leftElbowSum;

    private ROSConnection ros;
    [SerializeField] private string topicName = "/joint_angles";

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

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float32MultiArrayMsg>(topicName);
    }

    void Update()
    {
        if (mainCam == null) return;

        float rightShoulder1 = 0f, rightShoulder2 = 0f;
        float rightElbow = 0f;
        float rightWristTwist = 0f;
        float leftShoulder1 = 0f, leftShoulder2 = 0f;
        float leftElbow = 0f;
        float leftWristTwist = 0f;
        float neckYaw = 0f, neckPitch = 0f;

        if (!useManualOverride)
        {
            // ====================== 実測値 ======================
            rightElbow = CalcElbowAngle(rightUpperArm, rightForearm, rightWrist);
            (rightShoulder1, rightShoulder2) = CalcShoulderServoAngles(rightShoulder, rightUpperArm);
            //rightWristTwist = CalcWristTwist(rightForearm, rightWrist);

            leftElbow = CalcElbowAngle(leftUpperArm, leftForearm, leftWrist);
            (leftShoulder1, leftShoulder2) = CalcShoulderServoAngles(leftShoulder, leftUpperArm);
            //leftWristTwist = CalcWristTwist(leftForearm, leftWrist);

            (neckYaw, neckPitch) = CalcNeckAngles(neckBase, head);
            Debug.Log($"neckbase = {neckBase}");
        }
        else
        {
            // ====================== デバッグモード（インスペクタ指定値） ======================
            rightShoulder1 = manualRightServo1;
            rightShoulder2 = manualRightServo2;
            rightElbow = manualRightElbow;
            rightWristTwist = manualRightWrist;

            leftShoulder1 = manualLeftServo1;
            leftShoulder2 = manualLeftServo2;
            leftElbow = manualLeftElbow;
            leftWristTwist = manualLeftWrist;

            neckYaw = manualNeckYaw;
            neckPitch = manualNeckPitch;
        }

        // ====================== 平均化（通常時のみ） ======================
        if (!useManualOverride)
        {
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
                SendROSMessage(
                    rightServo1Sum / sampling_freq,
                    rightServo2Sum / sampling_freq,
                    rightElbowSum / sampling_freq,
                    rightWristTwistSum / sampling_freq,
                    leftServo1Sum / sampling_freq,
                    leftServo2Sum / sampling_freq,
                    leftElbowSum / sampling_freq,
                    leftWristTwistSum / sampling_freq,
                    neckYawSum / sampling_freq,
                    neckPitchSum / sampling_freq
                );

                counter = 0;
                rightServo1Sum = rightServo2Sum = 0f;
                rightElbowSum = rightWristTwistSum = 0f;
                leftServo1Sum = leftServo2Sum = 0f;
                leftElbowSum = leftWristTwistSum = 0f;
                neckYawSum = neckPitchSum = 0f;
            }
        }
        else
        {
            // デバッグモード時は平均化せず即送信
            SendROSMessage(
                rightShoulder1,
                rightShoulder2,
                rightElbow,
                rightWristTwist,
                leftShoulder1,
                leftShoulder2,
                leftElbow,
                leftWristTwist,
                neckYaw,
                neckPitch
            );
        }

        // ====================== 表示 ======================
        /*  infoText.text =
              $"Right Shoulder: {rightShoulder1:F1}°, {rightShoulder2:F1}°\n" +
              $"Right Elbow: {rightElbow:F1}°\n" +
              $"Right Wrist Twist(Y): {rightWristTwist:F1}°\n\n" +
              $"Left Shoulder: {leftShoulder1:F1}°, {leftShoulder2:F1}°\n" +
              $"Left Elbow: {leftElbow:F1}°\n" +
              $"Left Wrist Twist(Y): {leftWristTwist:F1}°\n\n" +
              $"Neck Yaw: {neckYaw:F1}°, Pitch: {neckPitch:F1}°";

          infoText.transform.position = mainCam.transform.position + mainCam.transform.forward * 1.0f;
          infoText.transform.rotation = Quaternion.LookRotation(infoText.transform.position - mainCam.transform.position);
         */
        counter++;
        
    }

    private void SendROSMessage(
        float avgRightServo1, float avgRightServo2, float avgRightElbow, float avgRightWrist,
        float avgLeftServo1, float avgLeftServo2, float avgLeftElbow, float avgLeftWrist,
        float avgNeckYaw, float avgNeckPitch)
    {




        //aveRightElbowの可動域制限
        if (avgRightElbow > 180.0f)
        {
            avgRightElbow = 180.0f;
        }
        else if (avgRightElbow < 54.0f)
        {
            avgRightElbow = 55.0f;
        }
        //aveRightServo2の数値処理と可動域の制限
        //avgRightServo2 *= 1.5f;
        //Debug.Log(avgRightServo2);
        avgRightServo2 = -1*avgRightServo2;
        avgRightServo2 += 135.0f;
        if (avgRightServo2 > 181.0f)
        {
            avgRightServo2 = 180.0f;
        }
        else if (avgRightServo2 < 19.0f)
        {
            avgRightServo2 = 20.0f;
        }
        //aveRightServo1の数値処理と可動域の制限
        avgRightServo1 = -1.2f*avgRightServo1;
        avgRightServo1 += 250.0f;  // 最初270にしてた
        if (avgRightServo1 > 271.0f)
        {
            avgRightServo1 = 270.0f;
        }
        else if (avgRightServo1 < 44.0f)
        {
            avgRightServo1 = 45.0f;
        }
        //aveLeftElbowの可動域制限
        if (avgLeftElbow > 180.0f)
        {
            avgLeftElbow = 180.0f;
        }
        else if (avgLeftElbow < 54.0f)
        {
            avgLeftElbow = 55.0f;
        }
        //aveLeftServo1の数値処理と可動域の制限
        //avgLeftServo1 = -1*avgLeftServo1;
        //avgLeftServo1 += 270.0f;
        if (avgLeftServo1 > 271.0f)
        {
            avgLeftServo1 = 270.0f;
        }
        else if (avgLeftServo1 < 44.0f)
        {
            avgLeftServo1 = 45.0f;
        }
        //aveLeftServo2の数値処理と可動域の制限
        //avgLeftServo2 = -1*avgLeftServo2;
        //avgLeftServo2 += 135.0f;
        if (avgLeftServo2 > 181.0f)
        {
            avgLeftServo2 = 180.0f;
        }
        else if (avgLeftServo2 < 19.0f)
        {
            avgLeftServo2 = 20.0f;
        }

        //avgRightWristの数値処理と可動域の制限
        float rightTrigger = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
        avgRightWrist = rightTrigger * 100.0f;
        avgRightWrist = 100-avgRightWrist;
        Debug.Log($"R_trig = {rightTrigger}");
        Debug.Log($"avgRightWrist = {avgRightWrist}");
        //avgRightWrist = 0;

       /* Debug.Log($"[ROS] Send Joint Angles: R({avgRightServo1},{avgRightServo2},{avgRightElbow},{avgRightWrist}), " +
                  $"L({avgLeftServo1},{avgLeftServo2},{avgLeftElbow},{avgLeftWrist}), Neck({avgNeckYaw},{avgNeckPitch})");*/
        Debug.Log($"[ROS] Send Joint Angles Neck({avgNeckYaw},{avgNeckPitch})");
        int iRightServo1 = Mathf.RoundToInt(avgRightServo1);
        int iRightServo2 = Mathf.RoundToInt(avgRightServo2);
        int iRightElbow = Mathf.RoundToInt(avgRightElbow);
        int iRightWrist = Mathf.RoundToInt(avgRightWrist);

        int iLeftServo1 = Mathf.RoundToInt(avgLeftServo1);
        int iLeftServo2 = Mathf.RoundToInt(avgLeftServo2);
        int iLeftElbow = Mathf.RoundToInt(avgLeftElbow);
        int iLeftWrist = Mathf.RoundToInt(avgLeftWrist);

        int iNeckYaw = Mathf.RoundToInt(avgNeckYaw);
        int iNeckPitch = Mathf.RoundToInt(avgNeckPitch);

        float[] jointAngles = new float[]
        {
            iRightServo1, iRightServo2, iRightElbow, iRightWrist,
            iLeftServo1, iLeftServo2, iLeftElbow, iLeftWrist,
            iNeckYaw, iNeckPitch
        };

        Float32MultiArrayMsg msg = new Float32MultiArrayMsg();
        msg.data = jointAngles;
        ros.Publish(topicName, msg);
    }

    private (float, float) CalcShoulderServoAngles(Transform shoulder, Transform upperArm)
    {
        if (shoulder == null || upperArm == null) return (0f, 0f);
        Quaternion relRot = Quaternion.Inverse(shoulder.rotation) * upperArm.rotation;
        Vector3 euler = relRot.eulerAngles;

        float y_vec_x = (float)((Math.Cos(euler.z * Mathf.Deg2Rad) * Math.Sin(euler.y * Mathf.Deg2Rad) * Math.Sin(euler.x * Mathf.Deg2Rad)) -
                                (Math.Sin(euler.z * Mathf.Deg2Rad) * Math.Cos(euler.x * Mathf.Deg2Rad)));
        float y_vec_y = (float)((Math.Sin(euler.z * Mathf.Deg2Rad) * Math.Sin(euler.y * Mathf.Deg2Rad) * Math.Sin(euler.x * Mathf.Deg2Rad)) +
                                (Math.Cos(euler.z * Mathf.Deg2Rad) * Math.Cos(euler.x * Mathf.Deg2Rad)));
        float y_vec_z = (float)(Math.Cos(euler.y * Mathf.Deg2Rad) * Math.Sin(euler.x * Mathf.Deg2Rad));

        float size = Mathf.Sqrt(y_vec_x * y_vec_x + y_vec_y * y_vec_y + y_vec_z * y_vec_z);
        float propo_cos = y_vec_y / size;

        float servo1 = Mathf.Atan2(y_vec_x, y_vec_z) * Mathf.Rad2Deg;
        float servo2 = Mathf.Acos(propo_cos) * Mathf.Rad2Deg;

        if (servo1 < 0) servo1 += 360f;
        if (servo2 < 0) servo2 += 360f;

        return (servo1, servo2);
    }

    private float CalcElbowAngle(Transform upperArm, Transform forearm, Transform wrist)
    {
        if (upperArm == null || forearm == null || wrist == null) return 0f;

        Vector3 upperToFore = (forearm.position - upperArm.position).normalized;
        Vector3 foreToWrist = (wrist.position - forearm.position).normalized;

        float angle = Vector3.Angle(upperToFore, foreToWrist);
        float elbowAngle = 180f - angle;
        return elbowAngle;
    }

    private float CalcWristTwist(Transform forearm, Transform wrist)
    {
        if (forearm == null || wrist == null) return 0f;
        Quaternion relRot = Quaternion.Inverse(forearm.rotation) * wrist.rotation;
        Vector3 euler = relRot.eulerAngles;
        return NormalizeAngle(euler.y);
    }

    private (float, float) CalcNeckAngles(Transform neckBase, Transform head)
    {
        if (neckBase == null || head == null) return (0f, 0f);
        Quaternion relRot = Quaternion.Inverse(neckBase.rotation) * head.rotation;
        Vector3 euler = relRot.eulerAngles;
        float yaw = NormalizeAngle(euler.y);
        float pitch = NormalizeAngle(euler.x);
        return (yaw, pitch);
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
