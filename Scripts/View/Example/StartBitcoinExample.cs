using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourBitcoinManager;

public class StartBitcoinExample : MonoBehaviour {

	private bool m_hasBeenInitializedBitcoin = false;

	// -------------------------------------------
	/* 
	* Buttons to open the bitcoin managment
	*/
	void OnGUI()
	{
		if (m_hasBeenInitializedBitcoin)
		{
			if (ScreenBitcoinController.Instance.ScreensEnabled > 0)
			{
				return;
			}
		}
		

		float fontSize = 1.2f * 15;
		float yGlobalPosition = 10;
		if (GUI.Button(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "OPEN WALLET"))
		{
			ScreenBitcoinController.Instance.InitializeBitcoin(YourCommonTools.UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, ScreenBitcoinPrivateKeyView.SCREEN_NAME);
			m_hasBeenInitializedBitcoin = true;
		}
		yGlobalPosition += 2.2f * fontSize;

		if (GUI.Button(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "SEND MONEY"))
		{
			ScreenBitcoinController.Instance.InitializeBitcoin(YourCommonTools.UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, ScreenBitcoinSendView.SCREEN_NAME);
			m_hasBeenInitializedBitcoin = true;
		}
	}
}
