using UnityEngine;

public class FrameRateDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;
    GUIStyle style;

    void Start()
    {
        style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 20;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(new Rect(10, 10, 200, 20), text, style);
    }
}
