using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using YourCommonTools;

namespace YourBitcoinManager
{
	public delegate void BitcoinManagerEventHandler(string _nameEvent, params object[] _list);

	/******************************************
	 * 
	 * BasicEventController
	 * 
	 * Class used to dispatch events through all the system
	 * 
	 * @author Esteban Gallardo
	 */
	public class BitcoinManagerEventController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_BASICEVENT_DELAYED_CALL			= "EVENT_BASICEVENT_DELAYED_CALL";
		public const string EVENT_BASICEVENT_TOTAL_UNITS_AVAILABLE	= "EVENT_BASICEVENT_TOTAL_UNITS_AVAILABLE";
		public const string EVENT_BASICEVENT_TOTAL_TIME				= "EVENT_BASICEVENT_TOTAL_TIME";

		public event BitcoinManagerEventHandler BitcoinManagerEvent;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static BitcoinManagerEventController _instance;

		public static BitcoinManagerEventController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(BitcoinManagerEventController)) as BitcoinManagerEventController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						string finalSingletonName = "^BitcoinManagerEventController^";
#if !ENABLE_MY_OFUSCATION || UNITY_EDITOR
						finalSingletonName = finalSingletonName.Replace("^", "");
#endif
						container.name = finalSingletonName;
						_instance = container.AddComponent(typeof(BitcoinManagerEventController)) as BitcoinManagerEventController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<TimedEventData> m_listEvents = new List<TimedEventData>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		private BitcoinManagerEventController()
		{
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * Will dispatch an event
		 */
		public void DispatchBasicEvent(string _nameEvent, params object[] _list)
		{
			if (BitcoinManagerEvent != null) BitcoinManagerEvent(_nameEvent, _list);
		}

		// -------------------------------------------
		/* 
		 * Will add a new delayed event to the queue
		 */
		public void DelayBasicEvent(string _nameEvent, float _time, params object[] _list)
		{
			m_listEvents.Add(new TimedEventData(_nameEvent, _time, _list));
		}

		// -------------------------------------------
		/* 
		 * Clone a delayed event
		 */
		public void DelayBasicEvent(TimedEventData _timeEvent)
		{
			m_listEvents.Add(new TimedEventData(_timeEvent.NameEvent, _timeEvent.Time, _timeEvent.List));
		}

		// -------------------------------------------
		/* 
		 * Will process the queue of delayed events 
		 */
		void Update()
		{
			// DELAYED EVENTS
			for (int i = 0; i < m_listEvents.Count; i++)
			{
				TimedEventData eventData = m_listEvents[i];
				eventData.Time -= Time.deltaTime;
				if (eventData.Time <= 0)
				{
					BitcoinManagerEvent(eventData.NameEvent, eventData.List);
					eventData.Destroy();
					m_listEvents.RemoveAt(i);
					break;
				}
			}
		}
	}
}
