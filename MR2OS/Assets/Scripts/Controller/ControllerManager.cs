using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using Oculus.Interaction.PoseDetection;

public class ControllerManager : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "crawler_ctrl";
    int rm = 0;
    int lm = 0;
    int rm_pre = 0;
    int lm_pre = 0;
    [SerializeField]
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Int8Msg>(topicName);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger)){
            //Debug.Log("RightTrigger Pushed");
            rm_pre = 1;
            //右モータ正転
        }
        if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger)){
            //Debug.Log("LeftTrigger Pushed");
            lm_pre = 1;
            //左モータ正転
        }

        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger)){
            //右モータ逆転
            rm_pre = -1;
        }
        if (OVRInput.Get(OVRInput.RawButton.LHandTrigger)){
            lm_pre = -1;
        }
        else{
            rm_pre = 0;
            lm_pre = 0;
        }

        if (rm_pre != rm)
        {
            rm = rm_pre;
            //右モータ制御情報パブリッシュ(正転、逆転、停止)
            if(rm == 0)
            {
                RightMotorStop();
            }else if(rm == 1){
                RightMotorForward_high();
            }else if (rm == -1){
                RightMotorBack_high();
            }
        }
        if (lm_pre != lm)
        {
            lm = lm_pre;
            //左モータ制御情報パブリッシュ
            if (lm == 0)
            {
                LeftMotorStop();
            }else if (lm == -1){
                LeftMotorBack_high();
            }else if(lm == 1){
                LeftMotorForward_high();
            }
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
}

