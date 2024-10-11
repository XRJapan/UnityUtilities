using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;


/// <summary> Manages a list of collision events which can have conditions that are required to invoke a collision event</summary>
public class CollisionEventManager : MonoBehaviour
{
    public List<CollisionEvent> events = new List<CollisionEvent>();

    void Reset()
    {
        AddNewEvent();
    }

    public void AddNewEvent()
    {
        var collisionEvent = gameObject.AddComponent<CollisionEvent>();
        collisionEvent.hideFlags = HideFlags.HideInInspector;
        events.Add(collisionEvent);
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(CollisionEventManager), true)]
[CanEditMultipleObjects]
public class CollisionEventManagerEditor : Editor
{
    private CollisionEventManager manager;
    private List<Editor> cachedEditors = new List<Editor>();
    private List<bool> showEvents = new List<bool>();
    private UnityEvent garbageCollector = new UnityEvent();

    public override void OnInspectorGUI()
    {
        if (manager == null) manager = (CollisionEventManager)target;
        GUI.color = Color.green;
        if (GUILayout.Button("Add event"))
            manager.AddNewEvent();
        GUI.color = Color.white;

        for (int i = 0; i < manager.events.Count; i++)
        {
            if (cachedEditors.Count < manager.events.Count) cachedEditors.Add(new Editor());
            if (showEvents.Count < manager.events.Count) showEvents.Add(false);
            showEvents[i] = EditorGUILayout.Foldout(showEvents[i], "Event " + (i + 1));
            if (showEvents[i])
            {
                GUI.color = Color.red;
                if (GUILayout.Button("Delete")) RemoveEvent(manager.events[i]);
                GUI.color = Color.white;
                Editor tempEditor = cachedEditors[i];
                CreateCachedEditor(manager.events[i], typeof(EventTriggerEditor), ref tempEditor);
                if (tempEditor != null) tempEditor.OnInspectorGUI();
                cachedEditors[i] = tempEditor;
            }
        }

        //Garbage collector? overkill? yes. Unnecessary? maybe. Better alternatives? Definitely.
        if (garbageCollector != null)
        {
            garbageCollector.Invoke();
            garbageCollector.RemoveAllListeners();
        }

        EditorUtility.SetDirty(target);
    }

    private void RemoveEvent(CollisionEvent collision3DEvent)
    {
        garbageCollector.AddListener(() => manager.events.Remove(collision3DEvent));
    }
}
#endif