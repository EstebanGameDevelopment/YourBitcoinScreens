using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YourCommonTools;

namespace YourBitcoinManager
{

	/******************************************
	 * 
	 * ButtonCustom
	 * 
	 * @author Esteban Gallardo
	 */
	public class ButtonEventCustom : Button
	{
		public const string EVENT_BUTTON_CUSTOM_PRESSED_DOWN = "EVENT_BUTTON_CUSTOM_PRESSED_DOWN";
		public const string EVENT_BUTTON_CUSTOM_RELEASE_UP = "EVENT_BUTTON_CUSTOM_RELEASE_UP";

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);

			UIEventController.Instance.DispatchUIEvent(EVENT_BUTTON_CUSTOM_PRESSED_DOWN, this.gameObject);
		}


		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);

			UIEventController.Instance.DispatchUIEvent(EVENT_BUTTON_CUSTOM_RELEASE_UP, this.gameObject);
		}

	}
}