﻿using System;
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
	 * ScreenTransactionSummaryView
	 * 
	 * It ask for the confirmation of the user to run the transaction
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenTransactionSummaryView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_TRANSACTION_SUMMARY";

		// ----------------------------------------------
		// SUBS
		// ----------------------------------------------	
		public const string SUB_EVENT_PREMIUM_POST_CONFIRMATION = "SUB_EVENT_PREMIUM_POST_CONFIRMATION";
		public const string SUB_EVENT_PREMIUM_POST_DESTROY      = "SUB_EVENT_PREMIUM_POST_DESTROY";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private bool m_priceBitcoinLoaded = false;
		private Dictionary<string, Transform> m_iconsCurrencies = new Dictionary<string, Transform>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
            base.Initialize(_list);

            List<object> paramsSummary = (List<object>)_list[0];

            decimal amount = (decimal)paramsSummary[0];
			decimal fee = (decimal)paramsSummary[1];
			string currency = (string)paramsSummary[2];
			string toAddressTarget = (string)paramsSummary[3];
			string subjectTransaction = (string)paramsSummary[4];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("To").GetComponent<Text>().text = LanguageController.Instance.GetText("message.to");
			m_container.Find("Subject").GetComponent<Text>().text = LanguageController.Instance.GetText("message.subject");

			m_container.Find("AddressTarget").GetComponent<Text>().text = toAddressTarget;
			m_container.Find("SubjectTransaction").GetComponent<Text>().text = subjectTransaction;

			m_container.Find("Amount").GetComponent<Text>().text = LanguageController.Instance.GetText("message.amount");
			m_container.Find("Fee").GetComponent<Text>().text = LanguageController.Instance.GetText("message.fee");

			m_container.Find("Button_Confirm").GetComponent<Button>().onClick.AddListener(OnConfirmationTransaction);
			m_container.Find("Button_Confirm/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.send.confirm.right.and.pay");

			m_container.Find("Button_Cancel").GetComponent<Button>().onClick.AddListener(OnCancelTransaction);
			m_container.Find("Button_Cancel/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.cancel");

			m_container.Find("PriceBitcoin").GetComponent<Text>().text = Utilities.Trim(amount.ToString()) + " BTC";
			m_container.Find("PriceCurrency").GetComponent<Text>().text = Utilities.Trim((amount * BitCoinController.Instance.CurrenciesExchange[currency]).ToString()) + " " + currency;

			m_container.Find("FeeBitcoin").GetComponent<Text>().text = Utilities.Trim(fee.ToString()) + " BTC";
			m_container.Find("FeeCurrency").GetComponent<Text>().text = Utilities.Trim((fee * BitCoinController.Instance.CurrenciesExchange[currency]).ToString()) + " " + currency;

			m_iconsCurrencies.Clear();
			for (int i = 0; i < BitCoinController.CURRENCY_CODE.Length; i++)
			{
				m_iconsCurrencies.Add(BitCoinController.CURRENCY_CODE[i], m_container.Find("IconsCurrency/" + BitCoinController.CURRENCY_CODE[i]));
				if (BitCoinController.Instance.CurrentCurrency == BitCoinController.CURRENCY_CODE[i])
				{
					m_iconsCurrencies[BitCoinController.CURRENCY_CODE[i]].gameObject.SetActive(true);
				}
				else
				{
					m_iconsCurrencies[BitCoinController.CURRENCY_CODE[i]].gameObject.SetActive(false);
				}
			}

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);			
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
		 * OnRentPurchase
		 */
		private void OnConfirmationTransaction()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenBitcoinSendView.EVENT_SCREENBITCOINSEND_USER_CONFIRMED_RUN_TRANSACTION);
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnClickCancel
		 */
		private void OnCancelTransaction()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

            if (this.gameObject.activeSelf)
            {
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
                {
                    OnCancelTransaction();
                }
            }
        }
    }
}