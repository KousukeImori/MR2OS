using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JoystickManager : MonoBehaviour
{
    public FixedJoystick inputMove; //JoyStickの値を取得

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //スティックでの縦移動
        this.transform.position += this.transform.forward * inputMove.Vertical* Time.deltaTime;
        //スティックでの横移動
        this.transform.position += this.transform.right * inputMove.Horizontal * Time.deltaTime; 
    }
/*
.Vertical,.HorizonalがJoyStickの垂直方向、水平方向の入力に対応しており、
JoyStickの傾き具合でそれぞれ-1~1のFloat型で返す。
*/
}
