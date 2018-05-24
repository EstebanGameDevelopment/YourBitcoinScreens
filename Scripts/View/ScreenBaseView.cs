using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenBaseView
	 * 
	 * Base class that will allow special management of the activation of the screen
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBaseView : MonoBehaviour
	{
		protected bool m_hasBeenDestroyed = false;

		// -------------------------------------------
		/* 
		 * This functions needs to be overridden in certain classes in order 
		 * to discard/listen events or reload data
		 */
		public virtual void SetActivation(bool _activation)
		{
			this.gameObject.SetActive(_activation);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public virtual bool Destroy()
		{
			if (m_hasBeenDestroyed) return true;
			m_hasBeenDestroyed = true;

			return false;
		}
	}
}