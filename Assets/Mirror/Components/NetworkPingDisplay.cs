using UnityEngine;

namespace Mirror
{
    /// <summary>
    /// Component that will display the clients ping in milliseconds
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkPingDisplay")]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkPingDisplay.html")]
    public class NetworkPingDisplay : MonoBehaviour
    {
        public bool showPing = true;
        public Vector2 position = new Vector2(200, 0);
        public int fontSize = 24;
        public Color textColor = new Color32(255, 255, 255, 80);
        public string prefix;
        public string suffix;

        GUIStyle style;

        void Awake()
        {
            style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = fontSize;
            style.normal.textColor = textColor;
        }

        void OnGUI()
        {
            if (!showPing) { return; }

            string text = string.Format("{0}ms", (int)(NetworkTime.rtt * 1000));

            // leave here or create special method to update fontSize and textColor
            style.fontSize = fontSize;
            style.normal.textColor = textColor;

            int width = Screen.width;
            int height = Screen.height;
            Rect rect = new Rect(position.x, position.y, width - 200, height * 2 / 100);

            GUI.Label(rect,prefix + text + suffix, style);
        }
    }
}
