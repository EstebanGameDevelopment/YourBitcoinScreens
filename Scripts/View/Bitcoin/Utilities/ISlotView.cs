using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ISlotView
	 * 
	 * Interface for the slot component
	 * 
	 * @author Esteban Gallardo
	 */
	public interface ISlotView
	{
		// FUNCTIONS
		void Initialize(params object[] _list);
		bool Destroy();
	}
}