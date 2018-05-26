using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourBitcoinController;
using YourCommonTools;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenBitcoinTransactionsView
	 * 
	 * It will show a list with the transactions
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinTransactionsView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_TRANSACTIONS";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const int TRANSACTION_CONSULT_ALL		= 0;
		public const int TRANSACTION_CONSULT_INPUTS		= 1;
		public const int TRANSACTION_CONSULT_OUTPUTS	= 2;
		

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		public GameObject PrefabSlotTransaction;
		public GameObject[] Tabs;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private GameObject m_root;
		private Transform m_container;
		private Transform m_listKeys;
		private int m_transactionConsultType;
		private Dropdown m_currencies;
		private GameObject m_prefabSlotTransaction;
		private bool m_informationReady = false;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_transactionConsultType = (int)_list[0];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.your.bitcoin.manager.title");

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);

			// CURRENCIES
			m_currencies = m_container.Find("Currency").GetComponent<Dropdown>();
			m_currencies.onValueChanged.AddListener(OnCurrencyChanged);
			m_currencies.options = new List<Dropdown.OptionData>();
			int indexCurrentCurrency = -1;
			for (int i = 0; i < BitCoinController.CURRENCY_CODE.Length; i++)
			{
				if (BitCoinController.Instance.CurrentCurrency == BitCoinController.CURRENCY_CODE[i])
				{
					indexCurrentCurrency = i;
				}
				m_currencies.options.Add(new Dropdown.OptionData(BitCoinController.CURRENCY_CODE[i]));
			}

			m_currencies.value = 1;
			m_currencies.value = 0;
			if (indexCurrentCurrency != -1)
			{
				m_currencies.value = indexCurrentCurrency;
			}			

			BitcoinManagerEventController.Instance.BitcoinManagerEvent += new BitcoinManagerEventHandler(OnBasicEvent);
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);

			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + BitCoinController.Instance.Network.ToString();

			Tabs[TRANSACTION_CONSULT_ALL].GetComponent<Button>().onClick.AddListener(OnConsultAll);
			Tabs[TRANSACTION_CONSULT_INPUTS].GetComponent<Button>().onClick.AddListener(OnConsultInputs);
			Tabs[TRANSACTION_CONSULT_OUTPUTS].GetComponent<Button>().onClick.AddListener(OnConsultOutputs);

			m_listKeys = m_container.Find("ListItems");
			m_listKeys.GetComponent<SlotManagerView>().ClearCurrentGameObject(true);
			Invoke("UpdateListItems", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			if (m_listKeys!=null) m_listKeys.GetComponent<SlotManagerView>().Destroy();
			m_listKeys = null;

			BitcoinManagerEventController.Instance.BitcoinManagerEvent -= OnBasicEvent;
			BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;

			BitcoinManagerEventController.Instance.DispatchBasicEvent(ScreenBitcoinController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnCurrencyChanged
		 */
		private void OnCurrencyChanged(int _index)
		{
			BitCoinController.Instance.CurrentCurrency = m_currencies.options[_index].text;
			BitcoinManagerEventController.Instance.DispatchBasicEvent(BitCoinController.EVENT_BITCOINCONTROLLER_CURRENCY_CHANGED);
		}

		// -------------------------------------------
		/* 
		 * UpdateListItems
		 */
		public void UpdateListItems()
		{
			switch (m_transactionConsultType)
			{
				case TRANSACTION_CONSULT_INPUTS:
					m_container.Find("ListItems/Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.transactions.inputs");
					break;

				case TRANSACTION_CONSULT_OUTPUTS:
					m_container.Find("ListItems/Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.transactions.outputs");
					break;

				case TRANSACTION_CONSULT_ALL:
					m_container.Find("ListItems/Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.transactions.all");
					break;
			}

			for (int i = 0; i < Tabs.Length; i++)
			{
				if (m_transactionConsultType == i)
				{
					Tabs[i].transform.Find("Background").gameObject.SetActive(true);
				}
				else
				{
					Tabs[i].transform.Find("Background").gameObject.SetActive(false);
				}
			}

			if (!m_informationReady)
			{
				m_informationReady = true;
				BitCoinController.Instance.GetAllInformation(BitCoinController.Instance.GetPublicKey(BitCoinController.Instance.CurrentPrivateKey));				
			}

			
			List<ItemMultiObjectEntry> items = null;
			switch (m_transactionConsultType)
			{
				case TRANSACTION_CONSULT_INPUTS:
					items = BitCoinController.Instance.InTransactionsHistory;
					break;

				case TRANSACTION_CONSULT_OUTPUTS:
					items = BitCoinController.Instance.OutTransactionsHistory;
					break;

				case TRANSACTION_CONSULT_ALL:
					items = BitCoinController.Instance.AllTransactionsHistory;
					break;
			}

			m_listKeys.GetComponent<SlotManagerView>().ClearCurrentGameObject(true);
			m_listKeys.GetComponent<SlotManagerView>().Initialize(4, items, PrefabSlotTransaction, null);
		}

		// -------------------------------------------
		/* 
		* OnConsultAll
		*/
		private void OnConsultAll()
		{
			m_transactionConsultType = TRANSACTION_CONSULT_ALL;
			UpdateListItems();
		}

		// -------------------------------------------
		/* 
		* OnConsultInputs
		*/
		private void OnConsultInputs()
		{
			m_transactionConsultType = TRANSACTION_CONSULT_INPUTS;
			UpdateListItems();
		}

		// -------------------------------------------
		/* 
		* OnConsultOutputs
		*/
		private void OnConsultOutputs()
		{
			m_transactionConsultType = TRANSACTION_CONSULT_OUTPUTS;
			UpdateListItems();
		}

		// -------------------------------------------
		/* 
		* BackPressed
		*/
		private void BackPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		private void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_UPDATE_ACCOUNT_DATA)
			{
				m_listKeys.GetComponent<SlotManagerView>().ClearCurrentGameObject(true);
				Invoke("UpdateListItems", 0.1f);
			}
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == ScreenBitcoinController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				Destroy();
			}
		}
	}
}