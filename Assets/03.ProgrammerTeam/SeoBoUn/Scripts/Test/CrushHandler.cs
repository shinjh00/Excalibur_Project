using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrushHandler : MonoBehaviour
{
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                {
                    Debug.Log($"<color=red>[CrashHandler] {type}</color>: {logString}");
                    Debug.Log($"<color=red>[CrashHandler]</color> Stack Trace: {stackTrace}");
                    //Debug.LogError($"<color=red>[CrashHandler] {type}</color>: {logString}");
                    //Debug.LogError($"<color=red>[CrashHandler]</color> Stack Trace: {stackTrace}");
                    break;
                }
            case LogType.Warning:
            case LogType.Log:
                break;
        }

        //if (type == LogType.Exception)
        //{
        //    Debug.LogError($"<color=red>[CrashHandler]</color> Exception: {logString}");
        //    Debug.LogError($"<color=red>[CrashHandler]</color> Stack Trace: {stackTrace}");
        //}
    }
}
