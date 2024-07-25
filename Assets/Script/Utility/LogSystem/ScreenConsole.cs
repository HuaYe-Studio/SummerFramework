using System.Collections.Generic;
using UnityEngine;

namespace Utility.LogSystem
{
    public class ScreenConsole : MonoBehaviour
    {
        [SerializeField] private List<LogMessage> logMessages = new List<LogMessage>();
        private Vector2 _scrollPosition;
        private GUIStyle _guiStyle;

        private void OnGUI()
        {
            if (!LogSystem.Instance.outputOnScreen) return;
            var scrollViewRect = new Rect(10, 10, Screen.width / 6.0f, Screen.height - 20);
            _guiStyle ??= new GUIStyle(GUI.skin.textArea)
            {
                wordWrap = true,
                fontSize = 32,
            };
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, _guiStyle,
                GUILayout.Width(scrollViewRect.width), GUILayout.Height(scrollViewRect.height));
            var endIndex = logMessages.Count;
            {
                var startIndex = Mathf.Max(0,
                    logMessages.Count - Mathf.FloorToInt(scrollViewRect.height / _guiStyle.lineHeight));
                for (var i = startIndex; i < endIndex; i++)
                {
                    var line = logMessages[i];
                    var color = LogSystem.LogColor(line.logLevel);
                    GUI.color = color;
                    GUILayout.TextArea(line.logInfo, _guiStyle);
                }
            }
            GUILayout.EndScrollView();
        }

        public void Log(LogMessage logMessage)
        {
            logMessages.Add(logMessage);
        }
    }
}