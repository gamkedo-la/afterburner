using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour 
{
	private Dictionary<StandardEventName, UnityEvent> m_eventDictionary;
	private Dictionary<BooleanEventName, UnityEvent<bool>> m_eventWithBoolDictionary;
	private Dictionary<FloatEventName, UnityEvent<float>> m_eventWithFloatDictionary;
    private Dictionary<TwoFloatsEventName, UnityEvent<float, float>> m_eventWithTwoFloatsDictionary;
    private Dictionary<StringEventName, UnityEvent<string>> m_eventWithStringDictionary;
    private Dictionary<IntegerEventName, UnityEvent<int>> m_eventWithIntDictionary;
    private Dictionary<TransformEventName, UnityEvent<Transform>> m_eventWithTransformDictionary;

    private static EventManager m_eventManager;


	public static EventManager Instance
	{
		get
		{
			if (!m_eventManager)
			{
				m_eventManager = FindObjectOfType(typeof (EventManager)) as EventManager;
				
				if (!m_eventManager)
				{
					Debug.LogError ("There needs to be one active EventManager script on a GameObject in your scene.");
				}
				else
				{
					m_eventManager.Initialise(); 
				}
			}
			
			return m_eventManager;
		}
	}


	void Initialise()
	{
		if (m_eventDictionary == null)
			m_eventDictionary = new Dictionary<StandardEventName, UnityEvent>();

		if (m_eventWithBoolDictionary == null)
			m_eventWithBoolDictionary = new Dictionary<BooleanEventName, UnityEvent<bool>>();

		if (m_eventWithFloatDictionary == null)
			m_eventWithFloatDictionary = new Dictionary<FloatEventName, UnityEvent<float>>();

        if (m_eventWithTwoFloatsDictionary == null)
            m_eventWithTwoFloatsDictionary = new Dictionary<TwoFloatsEventName, UnityEvent<float, float>>();

        if (m_eventWithStringDictionary == null)
            m_eventWithStringDictionary = new Dictionary<StringEventName, UnityEvent<string>>();

        if (m_eventWithIntDictionary == null)
            m_eventWithIntDictionary = new Dictionary<IntegerEventName, UnityEvent<int>>();

        if (m_eventWithTransformDictionary == null)
            m_eventWithTransformDictionary = new Dictionary<TransformEventName, UnityEvent<Transform>>();
    }


	public static void StartListening (StandardEventName eventName, UnityAction listener)
	{
		UnityEvent thisEvent = null;
		if (Instance.m_eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.AddListener (listener);
		} 
		else
		{
			thisEvent = new UnityEvent();
			thisEvent.AddListener (listener);
			Instance.m_eventDictionary.Add (eventName, thisEvent);
		}
	}


	public static void StopListening (StandardEventName eventName, UnityAction listener)
	{
		if (m_eventManager == null) return;

		UnityEvent thisEvent = null;
		if (Instance.m_eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener (listener);
		}
	}


	public static void TriggerEvent (StandardEventName eventName)
	{
		UnityEvent thisEvent = null;
		if (Instance.m_eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke ();
		}
	}


	public static void StartListening(BooleanEventName eventName, UnityAction<bool> listener)
	{
		UnityEvent<bool> thisEvent = null;
		if (Instance.m_eventWithBoolDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.AddListener(listener);
		} 
		else
		{
			thisEvent = new BoolEvent();
			thisEvent.AddListener(listener);
			Instance.m_eventWithBoolDictionary.Add(eventName, thisEvent);
		}
	}


	public static void StopListening(BooleanEventName eventName, UnityAction<bool> listener)
	{
		if (m_eventManager == null) return;
		
		UnityEvent<bool> thisEvent = null;
		if (Instance.m_eventWithBoolDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
	}


	public static void TriggerEvent(BooleanEventName eventName, bool argument)
	{
		UnityEvent<bool> thisEvent = null;
		if (Instance.m_eventWithBoolDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke(argument);
		}
	}


    public static void StartListening(FloatEventName eventName, UnityAction<float> listener)
    {
        UnityEvent<float> thisEvent = null;
        if (Instance.m_eventWithFloatDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new FloatEvent();
            thisEvent.AddListener(listener);
            Instance.m_eventWithFloatDictionary.Add(eventName, thisEvent);
        }
    }


    public static void StopListening(FloatEventName eventName, UnityAction<float> listener)
    {
        if (m_eventManager == null) return;

        UnityEvent<float> thisEvent = null;
        if (Instance.m_eventWithFloatDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }


    public static void TriggerEvent(FloatEventName eventName, float argument)
    {
        UnityEvent<float> thisEvent = null;
        if (Instance.m_eventWithFloatDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(argument);
        }
    }


    public static void StartListening(TwoFloatsEventName eventName, UnityAction<float, float> listener)
	{
		UnityEvent<float, float> thisEvent = null;
		if (Instance.m_eventWithTwoFloatsDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.AddListener(listener);
		} 
		else
		{
			thisEvent = new TwoFloatsEvent();
			thisEvent.AddListener(listener);
			Instance.m_eventWithTwoFloatsDictionary.Add(eventName, thisEvent);
		}
	}
	
	
	public static void StopListening(TwoFloatsEventName eventName, UnityAction<float, float> listener)
	{
		if (m_eventManager == null) return;
		
		UnityEvent<float, float> thisEvent = null;
		if (Instance.m_eventWithTwoFloatsDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
	}


	public static void TriggerEvent(TwoFloatsEventName eventName, float argument1, float argument2)
	{
		UnityEvent<float, float> thisEvent = null;
		if (Instance.m_eventWithTwoFloatsDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke(argument1, argument2);
		}
	}


    public static void StartListening(StringEventName eventName, UnityAction<string> listener)
    {
        UnityEvent<string> thisEvent = null;
        if (Instance.m_eventWithStringDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new StringEvent();
            thisEvent.AddListener(listener);
            Instance.m_eventWithStringDictionary.Add(eventName, thisEvent);
        }
    }


    public static void StopListening(StringEventName eventName, UnityAction<string> listener)
    {
        if (m_eventManager == null) return;

        UnityEvent<string> thisEvent = null;
        if (Instance.m_eventWithStringDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }


    public static void TriggerEvent(StringEventName eventName, string argument)
    {
        UnityEvent<string> thisEvent = null;
        if (Instance.m_eventWithStringDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(argument);
        }
    }


    public static void StartListening(IntegerEventName eventName, UnityAction<int> listener)
    {
        UnityEvent<int> thisEvent = null;
        if (Instance.m_eventWithIntDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new IntEvent();
            thisEvent.AddListener(listener);
            Instance.m_eventWithIntDictionary.Add(eventName, thisEvent);
        }
    }


    public static void StopListening(IntegerEventName eventName, UnityAction<int> listener)
    {
        if (m_eventManager == null) return;

        UnityEvent<int> thisEvent = null;
        if (Instance.m_eventWithIntDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }


    public static void TriggerEvent(IntegerEventName eventName, int argument)
    {
        UnityEvent<int> thisEvent = null;
        if (Instance.m_eventWithIntDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(argument);
        }
    }


    public static void StartListening(TransformEventName eventName, UnityAction<Transform> listener)
    {
        UnityEvent<Transform> thisEvent = null;
        if (Instance.m_eventWithTransformDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new TransformEvent();
            thisEvent.AddListener(listener);
            Instance.m_eventWithTransformDictionary.Add(eventName, thisEvent);
        }
    }


    public static void StopListening(TransformEventName eventName, UnityAction<Transform> listener)
    {
        if (m_eventManager == null) return;

        UnityEvent<Transform> thisEvent = null;
        if (Instance.m_eventWithTransformDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }


    public static void TriggerEvent(TransformEventName eventName, Transform argument)
    {
        UnityEvent<Transform> thisEvent = null;
        if (Instance.m_eventWithTransformDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(argument);
        }
    }


    public class BoolEvent : UnityEvent<bool>
	{
		
	}


	public class FloatEvent : UnityEvent<float>
	{
		
	}


    public class TwoFloatsEvent : UnityEvent<float, float>
    {

    }


    public class StringEvent : UnityEvent<string>
    {

    }


    public class IntEvent : UnityEvent<int>
    {

    }


    public class TransformEvent : UnityEvent<Transform>
    {

    }
}
