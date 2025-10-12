using System;
using System.Threading;
using UnityEngine;



public class ShoulderAndElbowAngleDisplay : MonoBehaviour
{
    int counter = 0;
    //角度平均値算出までのサンプリング個数
    int sampling_freq = 40; 

    int servo1_average = 0;
    int servo2_average = 0;

    [SerializeField] private Transform shoulder;
    [SerializeField] private Transform upperArm;
    [SerializeField] private Transform forearm;
    [SerializeField] private Transform wrist; // 手首ボーンを追加

    private TextMesh infoText;
    private Camera mainCam;



    void Start()
    {
        mainCam = Camera.main;

        // カメラ正面に表示用の3Dテキストを生成
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
        if (mainCam == null || shoulder == null || upperArm == null || forearm == null || wrist == null)
            return;

        // ---------- 肩回転（Shoulder基準でのUpperArm姿勢） ----------
        Quaternion relRot = Quaternion.Inverse(shoulder.rotation) * upperArm.rotation;
        Vector3 shoulderLocalEuler = relRot.eulerAngles;

        // ---------- 肘角度（UpperArm - Forearm - Wrist） ----------
        Vector3 upperToFore = (forearm.position - upperArm.position).normalized;
        Vector3 foreToWrist = (wrist.position - forearm.position).normalized;

        // 内積を使って角度計算（0〜180°）
        float angle = Vector3.Angle(upperToFore, foreToWrist);

        // 伸ばした時180°，曲げるほど小さく（＝反転）
        float elbowAngle = 180f - angle;

        // ---------- 表示 ----------
        infoText.text =
            $"Shoulder (UpperArm relative to Shoulder)\n" +
            $"X={shoulderLocalEuler.x:F1}, Y={shoulderLocalEuler.y:F1}, Z={shoulderLocalEuler.z:F1}\n\n" +
            $"Elbow Flexion Angle: {elbowAngle:F1}°";

        
        //サーボ回転角の計算
        int servo1_angle = 0;
        int servo2_angle = 0;
        
        float y_vec_x = (float)((Math.Cos(shoulderLocalEuler.z*Mathf.Deg2Rad) * Math.Sin(shoulderLocalEuler.y*Mathf.Deg2Rad) * Math.Sin(shoulderLocalEuler.x*Mathf.Deg2Rad)) - (Math.Sin(shoulderLocalEuler.z*Mathf.Deg2Rad) * Math.Cos(shoulderLocalEuler.x*Mathf.Deg2Rad)));
        float y_vec_y = (float)((Math.Sin(shoulderLocalEuler.z*Mathf.Deg2Rad) * Math.Sin(shoulderLocalEuler.y*Mathf.Deg2Rad) * Math.Sin(shoulderLocalEuler.x*Mathf.Deg2Rad)) + (Math.Cos(shoulderLocalEuler.z*Mathf.Deg2Rad) * Math.Cos(shoulderLocalEuler.x*Mathf.Deg2Rad)));
        float y_vec_z = (float)(Math.Cos(shoulderLocalEuler.y*Mathf.Deg2Rad) * Math.Sin(shoulderLocalEuler.x*Mathf.Deg2Rad));
        float y_vec_size = Mathf.Pow((y_vec_x * y_vec_x + y_vec_y * y_vec_y + y_vec_z * y_vec_z), 0.5f);
        

        float propo_cos = y_vec_y / y_vec_size;

        servo1_angle = (int)((Mathf.Atan2(y_vec_x, y_vec_z)) * Mathf.Rad2Deg);
        servo2_angle = (int)((Mathf.Acos(propo_cos)) * Mathf.Rad2Deg);
        // Debug.Log($"servo1={servo1_angle}°, servo2={servo2_angle}°");

        if (counter < sampling_freq)
        {
            if (servo1_angle < 0)
            {
                servo1_angle += 360; // 360°を加算し、負の角度による表現を避ける
            }
            if (servo2_angle < 0)
            {
                servo2_angle += 360;
            }
            servo1_average += servo1_angle;
            servo2_average += servo2_angle;
        }
        else if (counter == sampling_freq)
        {
            servo1_average /= sampling_freq;
            servo2_average /= sampling_freq;
            Debug.Log($"servo1={servo1_average}°, servo2={servo2_average}°");

            counter = 0;
        }


        // ---------- カメラ正面に固定 ----------
        infoText.transform.position = mainCam.transform.position + mainCam.transform.forward * 1.0f;
        infoText.transform.rotation = Quaternion.LookRotation(infoText.transform.position - mainCam.transform.position);

        // ---------- デバッグ可視化 ----------
        Debug.DrawLine(upperArm.position, forearm.position, Color.green); // 上腕
        Debug.DrawLine(forearm.position, wrist.position, Color.cyan);     // 前腕

        counter++;

    }

}
