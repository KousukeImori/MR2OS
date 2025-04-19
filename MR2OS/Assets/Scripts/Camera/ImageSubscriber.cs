using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using UnityEngine.UI;

// 引用元　UnityとROS2で実践するロボットプログラミング　2024/5/21 発行　著　奥谷哲郎　p125

public class ImageSubscriber : MonoBehaviour
{
    [SerializeField] Image targetImage;
    [SerializeField] string topicName;
    ROSConnection ros;
    Texture2D texture;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<CompressedImageMsg>(topicName, ReceiveMsg);
        texture = new Texture2D(1, 1);        
    }

    void ReceiveMsg(CompressedImageMsg compressedImage)
    {
        byte[] imageData = compressedImage.data;
        RenderTexture(imageData);
    }

    void RenderTexture(byte[] data)
    {
        texture.LoadImage(data);
        targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
    }
}
