using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourBitcoinController;
using YourCommonTools;
#if ENABLE_BITCOIN
using NBitcoin;
#endif

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenBitcoinAddFundsKeyView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinAddFundsKeyView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_ADD_FUNDS";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private string SUBEVENT_CONFIRMATION_OPEN_URL_TO_ADD_BITCOINS = "SUBEVENT_CONFIRMATION_OPEN_URL_TO_ADD_BITCOINS";
		private string SUBEVENT_CONFIRMATION_OPEN_URL_BITCOINS_TO_PAYPAL = "SUBEVENT_CONFIRMATION_OPEN_URL_BITCOINS_TO_PAYPAL";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private string m_publicKey = "";

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			m_publicKey = (string)_list[0];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(OnBackButton);

			m_container.Find("Enter").GetComponent<Button>().onClick.AddListener(OnEnterBitcoins);
			m_container.Find("Enter/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.enter.bitcoins");

			m_container.Find("Withdraw").GetComponent<Button>().onClick.AddListener(OnWithdrawBitcoins);
			m_container.Find("Withdraw/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.withdraw.bitcoins");

			m_container.Find("PublicKeyLabel").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.copy.paste.public.address");
			m_container.Find("PublicKeyInput").GetComponent<InputField>().text = BitCoinController.Instance.CurrentPublicKey;

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);

#if ENABLE_BITCOIN
			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + BitCoinController.Instance.Network.ToString();
#endif
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			UIEventController.Instance.UIEvent -= OnMenuEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}


		// -------------------------------------------
		/* 
		 * OnBackButton
		 */
		private void OnBackButton()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnEnterBitcoins
		 */
		private void OnEnterBitcoins()
		{
			Utilities.Clipboard = m_publicKey;
			string title = LanguageController.Instance.GetText("message.info");
			string description = LanguageController.Instance.GetText("screen.bitcoin.copied.public.key.clipboard");
			ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, title, description, null, SUBEVENT_CONFIRMATION_OPEN_URL_TO_ADD_BITCOINS);
		}

		// -------------------------------------------
		/* 
		 * OnWithdrawBitcoins
		 */
		private void OnWithdrawBitcoins()
		{
			string title = LanguageController.Instance.GetText("message.info");
			List<PageInformation> pages = new List<PageInformation>();
			pages.Add(new PageInformation(title, LanguageController.Instance.GetText("screen.bitcoin.choose.your.own.method.bitcoins.to.paypal"), null, ""));
			if (BitCoinController.Instance.IsMainNetwork)
			{
				pages.Add(new PageInformation(title, LanguageController.Instance.GetText("screen.bitcoin.choose.your.own.method.bitcoins.to.paypal.2"), null, SUBEVENT_CONFIRMATION_OPEN_URL_BITCOINS_TO_PAYPAL));
			}
			else
			{
				pages.Add(new PageInformation(title, LanguageController.Instance.GetText("screen.bitcoin.choose.your.own.method.bitcoins.to.paypal.2"), null, ""));
				pages.Add(new PageInformation(title, LanguageController.Instance.GetText("screen.bitcoin.choose.your.own.method.bitcoins.to.paypal.3"), null, SUBEVENT_CONFIRMATION_OPEN_URL_BITCOINS_TO_PAYPAL));
			}
			ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, pages);			
		}

		// -------------------------------------------
		/* 
		 * OnMenuEvent
		 */
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == ScreenController.EVENT_CONFIRMATION_POPUP)
			{
				string subEvent = (string)_list[2];
				if (subEvent == SUBEVENT_CONFIRMATION_OPEN_URL_TO_ADD_BITCOINS)
				{
					if (BitCoinController.Instance.IsMainNetwork)
					{
						Application.OpenURL("https://buy.blockexplorer.com/");
					}
					else
					{
						Application.OpenURL("https://testnet.manu.backend.hamburg/faucet");
					}
				}
				if (subEvent == SUBEVENT_CONFIRMATION_OPEN_URL_BITCOINS_TO_PAYPAL)
				{
					Application.OpenURL("https://www.coinbase.com");
				}				
			}
			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				OnBackButton();
			}
		}
	}
}