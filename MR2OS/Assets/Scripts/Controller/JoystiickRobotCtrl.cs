using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using Oculus.Interaction.PoseDetection;

public class JoystiickRobotCtrl : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "crawler_ctrl";
    public FixedJoystick inputMove; //JoyStickの値を取得

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Int8Msg>(topicName);
        Time.fixedDeltaTime = 0.2f;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (inputMove.Vertical == 0)
            if(inputMove.Horizontal == 0)
            {
                Debug.Log("stop");
                RightMotorStop();
                LeftMotorStop()
;            }
        //前進,後退（高速、低速）
        if(inputMove.Horizontal < 0.4)
            if(inputMove.Horizontal > -0.4)
                if(inputMove.Vertical > 0)//前進
                {
                    if (inputMove.Vertical > 0.3)
                    {
                        Debug.Log("Go front high");
                        RightMotorForward_high();
                        LeftMotorForward_high();
                    }else{
                        Debug.Log("Go front low");
                        RightMotorForward_low();
                        LeftMotorForward_low();
                    }

                }else if(inputMove.Vertical < 0)//後退
                {
                    if (inputMove.Vertical < -0.3)
                    {
                        Debug.Log("Go back high");
                        RightMotorBack_high();
                        LeftMotorBack_high();
                    }else{
                        Debug.Log("Go back low");
                        RightMotorBack_low();
                        LeftMotorBack_low();
                    }
                }
        // 右旋回
        if (inputMove.Horizontal >= 0.4)
            if(inputMove.Vertical > 0)
            {
                Debug.Log("Right front");
                LeftMotorForward_high(); //
                RightMotorForward_low();
            }else{
                Debug.Log("right back");
                LeftMotorBack_high();
                RightMotorBack_low();
            }
        // 左旋回
        if (inputMove.Horizontal <= -0.4 )
            if (inputMove.Vertical > 0)
            {
                Debug.Log("left front");
                RightMotorForward_high();
                LeftMotorForward_low();
            }else{
                Debug.Log("back left");
                RightMotorBack_high();
                LeftMotorBack_low();
            }
  
// 超新地旋回用　あとでコメントアウトするかも  
       /* if (inputMove.Vertical < 0.1)
            if (inputMove.Vertical > -0.1)
            {
                if (inputMove.Horizontal > 0)
                {
                    Debug.Log("Right turn");
                    LeftMotorForward_low();
                    RightMotorBack_low();
                }else if (inputMove.Horizontal < 0){
                    Debug.Log("left turn");
                    RightMotorForward_low();
                    LeftMotorBack_low();
                }
            }*/
            
        else{
        
        //    RightMotorStop();
          //  LeftMotorStop();
        }
    }

    public void RightMotorStop()
    {
        Int8Msg int8Message = new Int8Msg(0);
        ros.Publish(topicName, int8Message);
        Debug.Log($"Published: {0}");
    }

    public void RightMotorForward_low()
    {
        Int8Msg int8Message = new Int8Msg(1);
        ros.Publish(topicName, int8Message);
    }

    public void RightMotorForward_high()
    {
        Int8Msg int8Message = new Int8Msg(2);
        ros.Publish(topicName, int8Message);
    }

    public void RightMotorBack_low()
    {
        Int8Msg int8Message = new Int8Msg(3);
        ros.Publish(topicName, int8Message);
    }

    public void RightMotorBack_high()
    {
        Int8Msg int8Message = new Int8Msg(4);
        ros.Publish(topicName, int8Message);
    }

    public void LeftMotorStop()
    {
       Int8Msg int8Message = new Int8Msg(10);
       ros.Publish(topicName, int8Message);
       Debug.Log($"Published: {1}");
    }

    public void LeftMotorForward_low()
    {
        Int8Msg int8Message = new Int8Msg(11);
        ros.Publish(topicName, int8Message);
    }

    public void LeftMotorForward_high()
    {
        Int8Msg int8Message = new Int8Msg(12);
        ros.Publish(topicName, int8Message);
    }

    public void LeftMotorBack_low()
    {
        Int8Msg int8Message = new Int8Msg(13);
        ros.Publish(topicName, int8Message);
    }

    public void LeftMotorBack_high()
    {
        Int8Msg int8Message = new Int8Msg(14);
        ros.Publish(topicName, int8Message);
    }

    public void ShutdownGpioChip()
    {
        Int8Msg int8Message = new Int8Msg(20);
        ros.Publish(topicName, int8Message);
    }

    private void OnApplicationQuit()
    {
        RightMotorStop();
        ShutdownGpioChip();
        Debug.Log("stop Crawler-Ctrl-System");
    }
/*
.Vertical,.HorizonalがJoyStickの垂直方向、水平方向の入力に対応しており、
JoyStickの傾き具合でそれぞれ-1~1のFloat型で返す。
*/
}



