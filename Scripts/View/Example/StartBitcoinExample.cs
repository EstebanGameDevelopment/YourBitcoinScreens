using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourBitcoinManager;

public class StartBitcoinExample : MonoBehaviour {

	private bool m_buttonsVisible = true;

	// -------------------------------------------
	/* 
	* Buttons to open the bitcoin managment
	*/
	void OnGUI()
	{
		if (m_buttonsVisible)
		{
			float fontSize = 1.2f * 15;
			float yGlobalPosition = 10;
			if (GUI.Button(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "OPEN WALLET"))
			{
				ScreenBitcoinController.Instance.InitializeBitcoin(ScreenBitcoinPrivateKeyView.SCREEN_NAME);
				m_buttonsVisible = false;
			}
			yGlobalPosition += 2.2f * fontSize;

			if (GUI.Button(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "SEND MONEY"))
			{
				ScreenBitcoinController.Instance.InitializeBitcoin(ScreenBitcoinSendView.SCREEN_NAME);
				m_buttonsVisible = false;
			}
		}
	}
}
