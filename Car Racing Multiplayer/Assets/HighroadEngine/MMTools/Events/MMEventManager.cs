using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{	
	public class FloatEvent : UnityEvent<float> { }
	public class IntEvent : UnityEvent<int> { }
	public class StringEvent : UnityEvent<string> { }

	/// <summary>
	/// Manages the various game events
	/// Trigger them to broadcast them to other classes that have registered to it
	/// </summary>

	public static class MMEventManager 
	{
		private static Dictionary <string, UnityEvent> eventDictionary;
		private static Dictionary <string, FloatEvent> floatEventDictionary;
		private static Dictionary <string, IntEvent> intEventDictionary;
		private static Dictionary <string, StringEvent> stringEventDictionary;

	    private static void Init ()
		{
	        if (eventDictionary == null)
	        {
	            eventDictionary = new Dictionary<string, UnityEvent>();
			}
			if (floatEventDictionary == null)
	        {
				floatEventDictionary = new Dictionary<string, FloatEvent>();
			}
			if (intEventDictionary == null)
	        {
				intEventDictionary = new Dictionary<string, IntEvent>();
			}
			if (stringEventDictionary == null)
	        {
				stringEventDictionary = new Dictionary<string, StringEvent>();
	        }
	    }

	    public static void StartListening (string eventName, UnityAction listener)
	    {
	    	Init();
			UnityEvent thisEvent = null;
	        if (eventDictionary.TryGetValue (eventName, out thisEvent))
	        {
	            thisEvent.AddListener (listener);
	        } 
	        else
	        {
	            thisEvent = new UnityEvent ();
	            thisEvent.AddListener (listener);
	            eventDictionary.Add (eventName, thisEvent);
	        }
	     }

		public static void StartListeningFloatEvent (string eventName, UnityAction<float> listener)
		{
	    	Init();
			FloatEvent thisFloatEvent = null;
			if (floatEventDictionary.TryGetValue (eventName, out thisFloatEvent))
	        {
				thisFloatEvent.AddListener (listener);
	        } 
	        else
	        {
				thisFloatEvent = new FloatEvent ();
				thisFloatEvent.AddListener (listener);
				floatEventDictionary.Add (eventName, thisFloatEvent);
	        }
	    }

		public static void StartListeningIntEvent (string eventName, UnityAction<int> listener)
		{
	    	Init();
			IntEvent thisIntEvent = null;
			if (intEventDictionary.TryGetValue (eventName, out thisIntEvent))
	        {
				thisIntEvent.AddListener (listener);
	        } 
	        else
	        {
				thisIntEvent = new IntEvent ();
				thisIntEvent.AddListener (listener);
				intEventDictionary.Add (eventName, thisIntEvent);
	        }
	    }

		public static void StartListeningStringEvent (string eventName, UnityAction<string> listener)
		{
	    	Init();
			StringEvent thisStringEvent = null;
			if (stringEventDictionary.TryGetValue (eventName, out thisStringEvent))
	        {
				thisStringEvent.AddListener (listener);
	        } 
	        else
	        {
				thisStringEvent = new StringEvent ();
				thisStringEvent.AddListener (listener);
				stringEventDictionary.Add (eventName, thisStringEvent);
	        }
		}	    	

		public static void StopListening (string eventName, UnityAction listener)
	    {
			Init();
			UnityEvent thisEvent = null;
	        if (eventDictionary.TryGetValue (eventName, out thisEvent))
	        {
	            thisEvent.RemoveListener (listener);
	        }
		} 	

		public static void StopListeningFloatEvent (string eventName, UnityAction<float> listener)
	    {
			Init();
			FloatEvent thisFloatEvent = null;
	        if (floatEventDictionary.TryGetValue (eventName, out thisFloatEvent))
	        {
	            thisFloatEvent.RemoveListener (listener);
	        }
		}

		public static void StopListeningIntEvent (string eventName, UnityAction<int> listener)
	    {
			Init();
			IntEvent thisIntEvent = null;
	        if (intEventDictionary.TryGetValue (eventName, out thisIntEvent))
	        {
	            thisIntEvent.RemoveListener (listener);
	        }
		}

		public static void StopListeningStringEvent (string eventName, UnityAction<string> listener)
	    {
			Init();
			StringEvent thisStringEvent = null;
	        if (stringEventDictionary.TryGetValue (eventName, out thisStringEvent))
	        {
	            thisStringEvent.RemoveListener (listener);
	        }
	   	}
					

				

	    public static void TriggerEvent (string eventName)
	    {
			Init();

	        UnityEvent thisEvent = null;
	        if (eventDictionary.TryGetValue (eventName, out thisEvent))
	        {
	            thisEvent.Invoke ();
	        }
		}

	    public static void TriggerFloatEvent (string eventName, float value)
	    {
			Init();

	        FloatEvent thisEvent = null;
	        if (floatEventDictionary.TryGetValue (eventName, out thisEvent))
	        {
				thisEvent.Invoke (value);
	        }
		}

	    public static void TriggerIntEvent (string eventName, int value)
	    {
			Init();

	        IntEvent thisEvent = null;
	        if (intEventDictionary.TryGetValue (eventName, out thisEvent))
	        {
				thisEvent.Invoke (value);
	        }
		}

	    public static void TriggerStringEvent (string eventName, string value)
	    {
			Init();

	        StringEvent thisEvent = null;
	        if (stringEventDictionary.TryGetValue (eventName, out thisEvent))
	        {
				thisEvent.Invoke (value);
	        }
	    }
		
	}
}