using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class FpsCounter : MonoBehaviour
{
    public TextMeshProUGUI fps;
    public TextMeshProUGUI particles;


    void Update()
    {
        fps.text = "FPS: " + (int)(1 / Time.deltaTime);
    }
}