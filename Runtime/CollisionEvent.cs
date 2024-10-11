using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UltEvents;
#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif
using UnityEngine.Events;


[Serializable]
public class CollisionEvent : MonoBehaviour //This should really not be a scriptable object
{
    public enum CollisionTypes { Tag, Script, Name, Object }
    private Type type;
    [HideInInspector] public CollisionTypes collisionType;
    [HideInInspector] public string typeName = "";
    [HideInInspector] public string tagName = "";
    [HideInInspector] public string objectName = "";
    [HideInInspector] public GameObject targetObject;
    [HideInInspector] public bool activateWithAnything = false;
    [HideInInspector] public bool showStayEvents;

    public UltEvent<GameObject> onTriggerEnter = new();
    public UltEvent<GameObject> onTriggerStay = new();
    public UltEvent<GameObject> onTriggerExit = new();

    public UltEvent<GameObject> onCollisionEnter = new();
    public UltEvent<GameObject> onCollisionStay = new();
    public UltEvent<GameObject> onCollisionExit = new();

    public void HandleEvent(UltEvent<GameObject> unityEvent, GameObject col)
    {
        //No functions anyway
        if (unityEvent == null)
            return;

        //Always activate if true
        if (activateWithAnything)
        {
            unityEvent.Invoke(col);
            return;
        }

        switch (collisionType)
        {
            case CollisionTypes.Tag:
                try
                {
                    if (col.CompareTag(tagName)) unityEvent.Invoke(col);
                }
                catch (Exception e)
                {
                    Debug.LogError("Does the tag: " + tagName + " exist? Otherwhise the function contains an error. Error message from object: " + name, this);
                    Debug.LogError(e.ToString());
                }
                break;
            case CollisionTypes.Script:
                InvokeByType(unityEvent, col);
                break;
            case CollisionTypes.Name:
                if (col.name == objectName) unityEvent.Invoke(col);
                break;
            case CollisionTypes.Object:
                if (col.gameObject == targetObject) unityEvent.Invoke(col);
                break;
        }
    }

    private void InvokeByType(UltEvent<GameObject> unityEvent, GameObject col)
    {
        if (type == null && typeName != null)
            type = Type.GetType(typeName);

        if (col.transform.GetComponent(type))
            unityEvent.Invoke(col);
    }

    #region 3D
    private void OnTriggerEnter(Collider other)
    {
        HandleEvent(onTriggerEnter, other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        HandleEvent(onTriggerStay, other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        HandleEvent(onTriggerExit, other.gameObject);
    }

    private void OnCollisionEnter(Collision col)
    {
        HandleEvent(onCollisionEnter, col.gameObject);
    }

    private void OnCollisionStay(Collision col)
    {
        HandleEvent(onCollisionStay, col.gameObject);
    }

    private void OnCollisionExit(Collision col)
    {
        HandleEvent(onCollisionExit, col.gameObject);
    }
    #endregion

    #region 2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleEvent(onTriggerEnter, other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandleEvent(onTriggerStay, other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        HandleEvent(onTriggerExit, other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        HandleEvent(onCollisionEnter, other.gameObject);
    }
    private void OnCollisionStay2D(Collision2D other)
    {
        HandleEvent(onCollisionStay, other.gameObject);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        HandleEvent(onCollisionExit, other.gameObject);
    }

    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(CollisionEvent), true)]
public class EventTriggerEditor : Editor
{
    private CollisionEvent trigger;
    private bool showTriggerEvents = false;
    private bool showCollisionEvents = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (trigger == null)
            trigger = target as CollisionEvent;

        trigger.activateWithAnything = EditorGUILayout.Toggle("Activate with anything", trigger.activateWithAnything);
        if (!trigger.activateWithAnything)
        {
            trigger.collisionType = (CollisionEvent.CollisionTypes)EditorGUILayout.EnumPopup("Type", trigger.collisionType);

            switch (trigger.collisionType)
            {
                case CollisionEvent.CollisionTypes.Tag:
                    SelectCollisionByTag();
                    break;
                case CollisionEvent.CollisionTypes.Script:
                    SelectCollisionByType();
                    break;
                case CollisionEvent.CollisionTypes.Name:
                    SelectCollisionByName();
                    break;
                case CollisionEvent.CollisionTypes.Object:
                    SelectCollisionByObject();
                    break;
            }
        }

        //Unity events
        GUI.color = Color.grey * 0.5f;
        GUILayout.BeginVertical("Box");
        GUI.color = Color.white;

        showTriggerEvents = EditorGUILayout.Foldout(showTriggerEvents, "Show trigger events");
        if (showTriggerEvents)
        {
            //Trigger events
            SerializedProperty onTriggerEnterProperty = serializedObject.FindProperty("onTriggerEnter");
            EditorGUILayout.PropertyField(onTriggerEnterProperty);
            SerializedProperty onTriggerStayProperty = serializedObject.FindProperty("onTriggerStay");
            EditorGUILayout.PropertyField(onTriggerStayProperty);
            SerializedProperty onTriggerExitProperty = serializedObject.FindProperty("onTriggerExit");
            EditorGUILayout.PropertyField(onTriggerExitProperty);
        }

        showCollisionEvents = EditorGUILayout.Foldout(showCollisionEvents, "Show collision events");
        if (showCollisionEvents)
        {
            //Collision events
            SerializedProperty onCollisionEnterProperty = serializedObject.FindProperty("onCollisionEnter");
            EditorGUILayout.PropertyField(onCollisionEnterProperty);
            SerializedProperty onCollisionStayProperty = serializedObject.FindProperty("onCollisionStay");
            EditorGUILayout.PropertyField(onCollisionStayProperty);
            SerializedProperty onCollisionExitProperty = serializedObject.FindProperty("onCollisionExit");
            EditorGUILayout.PropertyField(onCollisionExitProperty);
        }
        GUILayout.EndVertical();


        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private int selectedTagIndex;
    private void SelectCollisionByTag()
    {
        selectedTagIndex = EditorGUILayout.Popup("Tag name", selectedTagIndex, InternalEditorUtility.tags);
        trigger.tagName = InternalEditorUtility.tags[selectedTagIndex];
    }

    private void SelectCollisionByName()
    {
        trigger.objectName = EditorGUILayout.TextField("Name", trigger.objectName);
    }

    private void SelectCollisionByType()
    {
        Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

        List<Type> types = new List<Type>();
        List<string> enumStrings = new List<string>();
        foreach (Type type in allTypes)
        {
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                types.Add(type);
                enumStrings.Add(type.FullName);
            }
        }

        int selected = trigger.typeName != null ? types.IndexOf(Type.GetType(trigger.typeName)) : 0;
        int id = EditorGUILayout.Popup("Script Type", selected, enumStrings.ToArray());
        if (id == -1) return;
        trigger.typeName = types[id].Name;
    }

    private void SelectCollisionByObject()
    {
        trigger.targetObject = (GameObject)EditorGUILayout.ObjectField(trigger.targetObject, typeof(GameObject), true);
    }
}
#endif