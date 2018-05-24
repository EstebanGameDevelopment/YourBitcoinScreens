using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YourBitcoinController;
using YourCommonTools;

namespace YourBitcoinManager
{

	/******************************************
	 * 
	 * SlotTransactionView
	 * 
	 * Slot that will be used to display trasaction data
	 * 
	 * @author Esteban Gallardo
	 */
	public class SlotTransactionView : Button, ISlotView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SLOT_TRANSACTION_SELECTED = "EVENT_SLOT_TRANSACTION_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private Transform m_container;
		private string m_id;
		private string m_title;
		private decimal m_amount;
		private decimal m_fee;
		private DateTimeOffset m_date;

		private Dictionary<string, Transform> m_iconsCurrencies = new Dictionary<string, Transform>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			ItemMultiObjectEntry item = (ItemMultiObjectEntry)_list[0];

			m_container = this.gameObject.transform;
			
			m_id = (string)item.Objects[0];
			m_date = (DateTimeOffset)item.Objects[2];
			m_amount = (decimal)item.Objects[3];
			m_fee = (decimal)item.Objects[4];
			m_title = (string)item.Objects[5];

			List<ItemMultiTextEntry> transactionScriptPubKey = (List<ItemMultiTextEntry>)item.Objects[6];
			string addresses = "";
			for (int i = 0; i < transactionScriptPubKey.Count; i++)
			{
				ItemMultiTextEntry sitem = transactionScriptPubKey[i];
				if (addresses.Length > 0)
				{
					addresses += ":";
				}

				addresses += sitem.Items[1];
			}
			m_container.Find("Target").GetComponent<Text>().text = BitCoinController.Instance.AddressToLabel(addresses.Split(':'));

			m_container.Find("Title").GetComponent<Text>().text = m_title;
			string dateTrimmed = m_date.ToString();
			if (dateTrimmed.Length > 0)
			{
				dateTrimmed = dateTrimmed.Substring(0, dateTrimmed.IndexOf('+'));
			}
			m_container.Find("Date").GetComponent<Text>().text = dateTrimmed;

			m_container.Find("Bitcoins").GetComponent<Text>().text = m_amount.ToString();

			if (m_amount < 0)
			{
				m_container.GetComponent<Image>().color = new Color(188f / 255f, 83f / 255f, 141f / 255f);
				m_container.Find("IconsCalendar/Input").gameObject.SetActive(false);
				m_container.Find("IconsCalendar/Output").gameObject.SetActive(true);
				m_container.Find("IconsType/Sent").gameObject.SetActive(true);
				m_container.Find("IconsType/Received").gameObject.SetActive(false);
			}
			else
			{
				m_container.GetComponent<Image>().color = new Color(53f / 255f, 174f / 255f, 64f/255f);
				m_container.Find("IconsCalendar/Input").gameObject.SetActive(true);
				m_container.Find("IconsCalendar/Output").gameObject.SetActive(false);
				m_container.Find("IconsType/Sent").gameObject.SetActive(false);
				m_container.Find("IconsType/Received").gameObject.SetActive(true);
			}

			m_iconsCurrencies.Clear();
			for (int i = 0; i < BitCoinController.CURRENCY_CODE.Length; i++)
			{
				m_iconsCurrencies.Add(BitCoinController.CURRENCY_CODE[i], m_container.Find("IconsCurrency/" + BitCoinController.CURRENCY_CODE[i]));
			}


			UpdateCurrency();

			BitcoinManagerEventController.Instance.BitcoinManagerEvent += new BitcoinManagerEventHandler(OnBasicEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public bool Destroy()
		{
			BitcoinManagerEventController.Instance.BitcoinManagerEvent -= OnBasicEvent;
			GameObject.Destroy(this.gameObject);

			return true;
		}

		// -------------------------------------------
		/* 
		 * UpdateCurrency
		 */
		public void UpdateCurrency()
		{
			string balanceCurrencyWallet = (m_amount * BitCoinController.Instance.CurrenciesExchange[BitCoinController.Instance.CurrentCurrency]).ToString();
			m_container.Find("Price").GetComponent<Text>().text = balanceCurrencyWallet;
			m_container.Find("Currency").GetComponent<Text>().text = BitCoinController.Instance.CurrentCurrency;

			foreach (KeyValuePair<string, Transform> item in m_iconsCurrencies)
			{
				if (item.Key == BitCoinController.Instance.CurrentCurrency)
				{
					item.Value.gameObject.SetActive(true);
				}
				else
				{
					item.Value.gameObject.SetActive(false);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnPointerClick
		 */
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			BitcoinManagerEventController.Instance.DispatchBasicEvent(EVENT_SLOT_TRANSACTION_SELECTED, m_id);
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_CURRENCY_CHANGED)
			{
				UpdateCurrency();
			}
		}

	}
}