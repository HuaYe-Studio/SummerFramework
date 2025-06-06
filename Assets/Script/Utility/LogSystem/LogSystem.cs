using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility.SingletonPatternSystem;

namespace Utility.LogSystem
{
    [Serializable]
    public struct LogMessage
    {
        public string logInfo;
        public LogLevelEnum logLevel;
        public string[] logTag;
    }

    public enum LogLevelEnum
    {
        Debug,
        Info,
        Warning,
        Error,
    }

    [RequireComponent(typeof(ScreenConsole))]
    public class LogSystem : MonoSingleton<LogSystem>
    {
        private ScreenConsole _screenConsole;

        #region 配置文件

        [ShowInInspector] [Header("配置文件")] [LabelText("是否在Unity控制台输出日志")]
        public bool outputOnUnityConsole = true;

        [LabelText("是否在文件中输出日志")] public bool outputOnFile = true;
        [LabelText("是否在屏幕上输出日志")] public bool outputOnScreen = true;

        #endregion

        #region Life

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            _screenConsole = GetComponent<ScreenConsole>();
        }

        #endregion

        #region Function

        public void Log(string message, LogLevelEnum logLevelEnum, string[] logTag = null)
        {
            var logMessage = new LogMessage
            {
                logInfo = message,
                logLevel = logLevelEnum,
                logTag = logTag
            };
            Log(logMessage);
            if (outputOnFile)
            {
                LogToFile(message);
            }
        }

        private void Log(LogMessage logMessage)
        {
            if (outputOnUnityConsole)
            {
#if UNITY_EDITOR
                Debug.Log(logMessage.logInfo);
#endif
            }

            if (!outputOnScreen) return;
            if (_screenConsole == null) return;
            _screenConsole.Log(logMessage);
        }


        public static Color LogColor(LogLevelEnum level)
        {
            return level switch
            {
                LogLevelEnum.Debug => Color.white,

                LogLevelEnum.Info => Color.green,
                LogLevelEnum.Warning => Color.yellow,
                LogLevelEnum.Error => Color.red,
                _ => Color.white
            };
        }

        void LogToFile(string logMessage)
        {
            string fileName = $"{Application.platform}_Log.txt";
            string filePath = $"{Application.persistentDataPath}/{fileName}";

            try
            {
                System.IO.File.AppendAllText(filePath, $"{DateTime.Now}: {logMessage}\n");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write log to file: {ex.Message}");
            }
        }

        #endregion
    }
}