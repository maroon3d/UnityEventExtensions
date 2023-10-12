using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public static class UnityEventExtensions
{
    #region CONSTS
    // GLOBAL SETTINGS
    public const bool GLOBAL_DEBUG = false;

    private const string PREFIX_UNITYEVENT = "<color=#40f9ff>UnityEvent - </color>";

    // COLORS
    private const string COLOR_GAMEOBJECT = "#faa661";
    private const string COLOR_TYPE = "#bdc4ff";
    private const string COLOR_ARG_VOID = "#999999";
    private const string COLOR_ARG_INT = "#fffd8a";
    private const string COLOR_ARG_FLOAT = "#8abeff";
    private const string COLOR_ARG_STRING = "#f081b5";
    private const string COLOR_ARG_BOOL_TRUE = "#77FF50";
    private const string COLOR_ARG_BOOL_FALSE = "#ff4e6a";

    // TYPES
    private const string ARGUMENT_TYPE_INT = "Int32";
    private const string ARGUMENT_TYPE_FLOAT = "Single";
    private const string ARGUMENT_TYPE_STRING = "String";
    private const string ARGUMENT_TYPE_BOOL = "Boolean";
    #endregion

    public static void InvokeAndLog(this UnityEvent unityEvent, MonoBehaviour obj, string nameOfUnityEvent, bool debug = false)
    {
        unityEvent?.Invoke();

        if (debug || GLOBAL_DEBUG)
        {
#if UNITY_EDITOR
            DebugLogEditor(obj, nameOfUnityEvent);
#else
            DebugLog(unityEvent);
#endif
        }
    }

    // This is standard extension method, works in builds
    public static void DebugLog(this UnityEvent unityEvent)
    {
        int eventCount = unityEvent.GetPersistentEventCount();

        for (int i = 0; i < eventCount; i++)
        {
            UnityEngine.Object listenerObject = unityEvent.GetPersistentTarget(i);
            string methodName = unityEvent.GetPersistentMethodName(i);

            if (listenerObject == null)
            {
                Debug.Log($"<b>{PREFIX_UNITYEVENT} <color=red>Target object not found!</color></b>");   
            }
            else if (unityEvent.GetPersistentMethodName(i) == "")
            {
                Debug.Log($"<b>{PREFIX_UNITYEVENT} <color=red>Function not found on the target object.</color></b>");
            }
            else
            {
                Debug.Log($"<b>{PREFIX_UNITYEVENT}<color=orange>{listenerObject.name}</color>.{methodName}()</b>");
            }
        }
    }



#if UNITY_EDITOR
    // This is editor debugger, works only in editor
    private static void DebugLogEditor(MonoBehaviour obj, string nameOfUnityEvent)
    {
        SerializedObject serializedObject = new SerializedObject(obj);
        SerializedProperty unityEventProperty = serializedObject.FindProperty($"{nameOfUnityEvent}.m_PersistentCalls.m_Calls");

        // Iterate through the UnityEvent's methods
        for (int i = 0; i < unityEventProperty.arraySize; i++)
        {
            SerializedProperty methodProperty = unityEventProperty.GetArrayElementAtIndex(i);
            SerializedProperty methodNameProperty = methodProperty.FindPropertyRelative("m_MethodName");
            SerializedProperty targetObjectProperty = methodProperty.FindPropertyRelative("m_Target");

            // Extract method name and target object
            string methodName = methodNameProperty.stringValue;
            UnityEngine.Object targetObject = targetObjectProperty.objectReferenceValue;
            
            // Check if a target object is set
            if (targetObject != null)
            {
                // Get the type of the target object
                Type targetType = targetObject.GetType();

                // Find the method with the given name
                MethodInfo[] allMethods = targetType.GetMethods();
                MethodInfo methodInfo = allMethods.FirstOrDefault(m => m.Name == methodName);

                if (methodInfo != null)
                {
                    // Access the method's parameter information
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    string paramValue = "void";
                    string parameterLog = $"<color={COLOR_ARG_VOID}>{paramValue}</color>";


                    // Check if method has at least one parameter
                    if (parameters.Length > 0)
                    {
                        ParameterInfo param = parameters[0];

                        if (param.ParameterType.Name == ARGUMENT_TYPE_INT)
                        {
                            paramValue = methodProperty.FindPropertyRelative("m_Arguments.m_IntArgument").intValue.ToString();
                            parameterLog = $"<color={COLOR_ARG_INT}>{paramValue}</color>";
                        }
                        else if (param.ParameterType.Name == ARGUMENT_TYPE_STRING)
                        {
                            paramValue = methodProperty.FindPropertyRelative("m_Arguments.m_StringArgument").stringValue.ToString();
                            parameterLog = $"<color={COLOR_ARG_STRING}>'{paramValue}'</color>";
                        }
                        else if (param.ParameterType.Name == ARGUMENT_TYPE_FLOAT)
                        {
                            paramValue = methodProperty.FindPropertyRelative("m_Arguments.m_FloatArgument").floatValue.ToString();
                            parameterLog = $"<color={COLOR_ARG_FLOAT}>{paramValue}f</color>";
                        }
                        else if (param.ParameterType.Name == ARGUMENT_TYPE_BOOL && param.Name == "value")
                        {
                            bool boolValue = methodProperty.FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue;
                            paramValue = boolValue.ToString();
                            string boolColor = boolValue ? COLOR_ARG_BOOL_TRUE : COLOR_ARG_BOOL_FALSE;
                            parameterLog = $"<color={boolColor}>{paramValue}</color>";
                        }
                    }

                    // Get debug log data
                    string sourceObjectName = obj.gameObject.name;
                    Type sourceType = obj.GetType();
                    string targetObjectName = targetObject.name;

                    // Final log
                    Debug.Log($"<b>{PREFIX_UNITYEVENT} <color={COLOR_GAMEOBJECT}>{sourceObjectName}</color>.<color={COLOR_TYPE}>{sourceType}()</color>  <color=cyan>→</color>  <color={COLOR_GAMEOBJECT}>{targetObjectName}</color>.<color={COLOR_TYPE}>{targetType.Name}.</color>{methodName}({parameterLog}) </b>");
                }
                else
                {
                    Debug.Log($"<b>{PREFIX_UNITYEVENT} <color=red>Function not found on the target object.</color></b>");
                }
            }
            else
            {
                Debug.Log($"<b>{PREFIX_UNITYEVENT} <color=red>Target object not found!</color></b>");
            }
        }
    }
#endif
}

