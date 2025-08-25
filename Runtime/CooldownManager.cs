using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CooldownManager
{
    private class HiddenMonobehaviour : MonoBehaviour { }

    private static MonoBehaviour mono;

    private static Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();

    //TO SUPPORT DOMAIN RELOADING WHEN DISABLED
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        GameObject obj = new GameObject("Cooldown Manager");
        UnityEngine.Object.DontDestroyOnLoad(obj);
        mono = obj.AddComponent<HiddenMonobehaviour>();
        coroutines = new Dictionary<string, Coroutine>();
    }

    /// <summary>Delays <b>action</b> by <b>cooldownDurations (in seconds)</b></summary>
    /// <param name="cooldownDuration">Duration in seconds</param>
    public static CoroutineHandler Cooldown(float cooldownDuration, Action action)
    {
        return mono.StartCoroutine(InternalCooldown(cooldownDuration, action));
    }

    /// <summary>Cooldown will delay the action by the cooldownDuration given (in seconds). If another coroutine with same name is running it will stop that one</summary>
    /// <param name="cooldownDuration">Duration in seconds</param>
    public static void Cooldown(float cooldownDuration, Action action, string name)
    {
        if (coroutines.ContainsKey(name))
            mono.StopCoroutine(coroutines[name]);

        coroutines[name] = mono.StartCoroutine(InternalCooldown(cooldownDuration, action));
    }

    public static CoroutineHandler Cooldown<T>(float cooldownDuration, Action<T> action, T parameter)
    {
        return mono.StartCoroutine(InternalCooldown(cooldownDuration, action, parameter));
    }

    public static CoroutineHandler Cooldown<T, TU>(float cooldownDuration, Action<T, TU> action, T param1, TU param2)
    {
        return mono.StartCoroutine(InternalCooldown(cooldownDuration, action, param1, param2));
    }

    public static CoroutineHandler Cooldown<T, TU, TX>(float cooldownDuration, Action<T, TU, TX> action, T param1, TU param2, TX param3)
    {
        return mono.StartCoroutine(InternalCooldown(cooldownDuration, action, param1, param2, param3));
    }

    private static IEnumerator InternalCooldown(float cooldownDuration, Action action)
    {
        yield return new WaitForSeconds(cooldownDuration);
        action.Invoke();
    }

    private static IEnumerator InternalCooldown<T>(float cooldownDuration, Action<T> action, T parameter)
    {
        yield return new WaitForSeconds(cooldownDuration);
        action.Invoke(parameter);
    }

    private static IEnumerator InternalCooldown<T, TU>(float cooldownDuration, Action<T, TU> action, T parameter1, TU parameter2)
    {
        yield return new WaitForSeconds(cooldownDuration);
        action.Invoke(parameter1, parameter2);
    }

    private static IEnumerator InternalCooldown<T, TU, TX>(float cooldownDuration, Action<T, TU, TX> action, T parameter1, TU parameter2, TX parameter3)
    {
        yield return new WaitForSeconds(cooldownDuration);
        action.Invoke(parameter1, parameter2, parameter3);
    }

    public static CoroutineHandler OnNextFrame(Action action)
    {
        return mono.StartCoroutine(InternalNextFrame(action));
    }

    private static IEnumerator InternalNextFrame(Action action)
    {
        yield return 0;
        action.Invoke();
    }

    public static CoroutineHandler Repeat(Action action, float repeatInterval, float duration = -1)
    {
        return mono.StartCoroutine(InternalIterateOverTime(action, repeatInterval, duration));
    }

    private static IEnumerator InternalIterateOverTime(Action action, float repeatInterval, float duration = -1)
    {
        WaitForSeconds wait = new WaitForSeconds(repeatInterval);
        float startTime = Time.time;

        while (duration == -1)
        {
            action.Invoke();
            yield return wait;
        }

        for (float timePassed = 0; timePassed < duration; timePassed = Time.time - startTime)
        {
            action.Invoke();
            yield return wait;
        }
    }

    public static CoroutineHandler Cooldown(this MonoBehaviour behaviour, float cooldownDuration, Action action)
    {
        return behaviour.StartCoroutine(InternalCooldown(cooldownDuration, action));
    }

    public class CoroutineHandler
    {
        public Coroutine Coroutine { get; }

        private CoroutineHandler(Coroutine coroutine)
        {
            Coroutine = coroutine;
        }

        public void StopCoroutine()
        {
            if (mono == null) 
                return;

            mono.StopCoroutine(Coroutine);
        }

        public static implicit operator CoroutineHandler(Coroutine coroutine) => new CoroutineHandler(coroutine);
    }
}