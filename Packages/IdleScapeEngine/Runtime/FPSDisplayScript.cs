using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSDisplayScript : MonoBehaviour
{
    float timeA;
    int fps;
    int lastFPS;
    public GUIStyle textStyle;
    Vector2 pos;
    public bool displayFPS;
    public float averageFPS;

    // Use this for initialization
    void Start()
    {
        timeA = Time.timeSinceLevelLoad;
        textStyle.fontSize = 35;
        textStyle.normal.textColor = new Color(1, 0.9964f, 0);
        pos.x = Screen.width / 2;
        pos.y = 5;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad - timeA <= 1)
        {
            fps++;
        }
        else
        {
            lastFPS = fps + 1;
            timeA = Time.timeSinceLevelLoad;
            fps = 0;
        }

        if (averageFPS > 0)
        {
            averageFPS = (averageFPS + lastFPS) / 2;
        }
        else averageFPS = lastFPS;
    }
    void OnGUI()
    {
        if (displayFPS) GUI.Label(new Rect(pos.x, pos.y, 30, 30), "FPS " + lastFPS, textStyle);
    }
}