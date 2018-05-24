﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBitcoin;
using UnityEngine;
using UnityEngine.UI;
using YourBitcoinController;
using YourCommonTools;
using YourNetworkingTools;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenBitcoinPrivateKeyView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinPrivateKeyView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_WALLET";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENPROFILE_SERVER_REQUEST_RESET_PASSWORD_CONFIRMATION	= "EVENT_SCREENPROFILE_SERVER_REQUEST_RESET_PASSWORD_CONFIRMATION";
		public const string EVENT_SCREENPROFILE_LOAD_SCREEN_EXCHANGE_TABLES_INFO			= "EVENT_SCREENPROFILE_LOAD_SCREEN_EXCHANGE_TABLES_INFO";
		public const string EVENT_SCREENPROFILE_LOAD_CHECKING_KEY_PROCESS					= "EVENT_SCREENPROFILE_LOAD_CHECKING_KEY_PROCESS";
		public const string EVENT_SCREENBITCOINPRIVATEKEY_SEND_PRIVATE_KEY_EMAIL			= "EVENT_SCREENBITCOINPRIVATEKEY_SEND_PRIVATE_KEY_EMAIL";

		// ----------------------------------------------
		// SUBS
		// ----------------------------------------------	
		public const string SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_WITHOUT_SAVE	= "SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_WITHOUT_SAVE";
		public const string SUB_EVENT_SCREENBITCOINPRIVATEKEY_CONFIRMATION_DELETE	= "SUB_EVENT_SCREENBITCOINPRIVATEKEY_CONFIRMATION_DELETE";
		public const string SUB_EVENT_SCREENBITCOINPRIVATEKEY_VIDEO_TUTORIAL		= "SUB_EVENT_SCREENBITCOINPRIVATEKEY_VIDEO_TUTORIAL";
		public const string SUB_EVENT_SCREENBITCOINPRIVATEKEY_BURN_KEY_CONFIRMATION = "SUB_EVENT_SCREENBITCOINPRIVATEKEY_BURN_KEY_CONFIRMATION";
		public const string SUB_EVENT_SCREENBITCOINPRIVATEKEY_HIDE_INFO_BUTTONS		= "SUB_EVENT_SCREENBITCOINPRIVATEKEY_HIDE_INFO_BUTTONS";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const int TOTAL_SIZE_PRIVATE_KEY = 4 * 13;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private InputField m_labelKey;
		private bool m_hasBeenInitialized = false;

		private GameObject m_patternsContainer;
		private GameObject m_seesContainer;

		private InputField m_completeKey;
		private GameObject m_seeComplete;
		private GameObject m_emailPrivateKey;

		private GameObject m_outputTransactionHistory;
		private GameObject m_inputTransactionHistory;

		private bool m_requestCheckValidKey = false;
		private GameObject m_greenLight;
		private GameObject m_redCross;
		private Text m_approveMessage;
		private Text m_balance;
		private decimal m_balanceValue = -1m;
		private GameObject m_buttonBalance;
		private GameObject m_createNewWallet;
		
		private bool m_hasChanged = false;
		private GameObject m_buttonSave;

		public bool HasChanged
		{
			get { return m_hasChanged; }
			set { m_hasChanged = value; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_labelKey = m_container.Find("LabelKey").GetComponent<InputField>();
			m_container.Find("InfoLabelKey").GetComponent<Button>().onClick.AddListener(OnInfoLabelKey);

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.your.private.address");
			m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.location.reasuring.message.private.key");

			m_greenLight = m_container.Find("IconValid").gameObject;
			m_redCross = m_container.Find("IconWrong").gameObject;
			m_approveMessage = m_container.Find("Validated").GetComponent<Text>();
			m_greenLight.SetActive(false);
			m_redCross.SetActive(false);
			m_approveMessage.text = "";

			m_completeKey = m_container.Find("CompleteKey").GetComponent<InputField>();
			m_completeKey.text = LanguageController.Instance.GetText("screen.bitcoin.write.here.your.private.key");
			m_completeKey.onValueChanged.AddListener(OnValueMainKeyChanged);
			m_completeKey.onEndEdit.AddListener(OnEditedMainKeyChanged);
			m_container.Find("InfoPrivateKey").GetComponent<Button>().onClick.AddListener(OnInfoPrivateKey);

			m_emailPrivateKey = m_container.Find("EmailPrivateKey").gameObject;
			m_emailPrivateKey.GetComponent<Button>().onClick.AddListener(OnEmailPrivateKey);
			m_emailPrivateKey.SetActive(false);

			m_seeComplete = m_container.Find("SeeComplete").gameObject;

			m_outputTransactionHistory = m_container.Find("OutputTransactions").gameObject;
			m_outputTransactionHistory.GetComponent<Button>().onClick.AddListener(OnCheckOutputTransactions);
			m_outputTransactionHistory.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.check.output.history");
			m_outputTransactionHistory.SetActive(false);

			m_inputTransactionHistory = m_container.Find("InputTransactions").gameObject;
			m_inputTransactionHistory.GetComponent<Button>().onClick.AddListener(OnCheckInputTransactions);
			m_inputTransactionHistory.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.check.input.history");
			m_inputTransactionHistory.SetActive(false);

			m_buttonBalance = m_container.Find("Balance").gameObject;
			m_balance = m_buttonBalance.transform.Find("Text").GetComponent<Text>();
			m_balance.text = "";
			m_buttonBalance.GetComponent<Button>().onClick.AddListener(OnAddFunds);
			m_buttonBalance.SetActive(false);

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(OnBackButton);

			m_buttonSave = m_container.Find("Button_Save").gameObject;
			m_buttonSave.GetComponent<Button>().onClick.AddListener(OnSaveButton);
			m_buttonSave.SetActive(false);
			HasChanged = false;


			m_createNewWallet = m_container.Find("CreatePrivateKey").gameObject;
			m_createNewWallet.GetComponent<Button>().onClick.AddListener(OnCreateNewWallet);
			m_createNewWallet.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.create.new.wallet");

			BitcoinManagerEventController.Instance.BitcoinManagerEvent += new BitcoinManagerEventHandler(OnBitcoinManagerEvent);
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);

			LoadDataPrivateKey();

			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + BitCoinController.Instance.Network.ToString();

			BitcoinManagerEventController.Instance.DispatchBasicEvent(ScreenBitcoinInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			BitCoinController.Instance.RestoreCurrentPrivateKey();

			BitcoinManagerEventController.Instance.BitcoinManagerEvent -= OnBitcoinManagerEvent;
			BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;

			BitcoinManagerEventController.Instance.DispatchBasicEvent(ScreenBitcoinController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnEditedMainKeyChanged
		 */
		private void OnEditedMainKeyChanged(string _newValue)
		{
			if (m_balanceValue > 0)
			{
				m_balanceValue = -1;
				m_balance.text = LanguageController.Instance.GetText("screen.bitcoin.manager.check.valid.key");
				m_requestCheckValidKey = true;
			}
		}

		// -------------------------------------------
		/* 
		 * OnValueMainKeyChanged
		 */
		private void OnValueMainKeyChanged(string _newValue)
		{
			if (_newValue.Length == TOTAL_SIZE_PRIVATE_KEY)
			{
				m_buttonBalance.SetActive(true);					
				m_balance.text = LanguageController.Instance.GetText("screen.bitcoin.manager.check.valid.key");
				m_requestCheckValidKey = true;
			}
		}

		// -------------------------------------------
		/* 
		 * OnEditedMainKeyChanged
		 */
		private void OnEditedLabelKeyChanged(string _newValue)
		{
			if (_newValue.Length == TOTAL_SIZE_PRIVATE_KEY)
			{
				HasChanged = true;
			}
		}

		// -------------------------------------------
		/* 
		 * OnValueMainKeyChanged
		 */
		private void OnValueLabelKeyChanged(string _newValue)
		{
			if (_newValue.Length > 0)
			{
				HasChanged = true;
			}
		}

		// -------------------------------------------
		/* 
		 * OnInfoLabelKey
		 */
		public void OnInfoLabelKey()
		{
			string info = LanguageController.Instance.GetText("message.info");
			string description = LanguageController.Instance.GetText("screen.bitcoin.wallet.name.as.you.want");
			ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, info, description, null, "");
		}

		// -------------------------------------------
		/* 
		 * OnInfoPrivateKey
		 */
		public void OnInfoPrivateKey()
		{
			string info = LanguageController.Instance.GetText("message.info");
			string description = LanguageController.Instance.GetText("screen.bitcoin.wallet.fill.the.inputfield.with.your.private.key");
			ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_CONFIRMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, info, description, null, SUB_EVENT_SCREENBITCOINPRIVATEKEY_VIDEO_TUTORIAL);
		}

		// -------------------------------------------
		/* 
		 * OnEmailPrivateKey
		 */
		public void OnEmailPrivateKey()
		{
			string info = LanguageController.Instance.GetText("message.warning");
			string description = LanguageController.Instance.GetText("screen.bitcoin.wallet.send.private.key.warning");
			ScreenBitcoinController.Instance.CreateNewScreen(ScreenEmailPrivateKeyView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false);
		}

		// -------------------------------------------
		/* 
		 * OnCompleteKeyChanged
		 */
		public void CheckKeyEnteredInMainField()
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("CHECKING THE PRIVATE KEY IN MAIN FIELD*********");
#endif
			FillPrivateKeyInputs(m_completeKey.text);
			HasChanged = true;
		}

		// -------------------------------------------
		/* 
		 * LoadDataPrivateKey
		 */
		private void LoadDataPrivateKey()
		{
			if (BitCoinController.Instance.CurrentPrivateKey.Length > 0)
			{
				string deencryptedKey = BitCoinController.Instance.CurrentPrivateKey;

				if (!FillPrivateKeyInputs(deencryptedKey))
				{
					m_completeKey.text = "";

					m_greenLight.SetActive(false);
					m_redCross.SetActive(false);
					m_approveMessage.text = LanguageController.Instance.GetText("screen.location.key.is.not.defined.yet");
				}
			}
			else
			{
				m_completeKey.text = LanguageController.Instance.GetText("screen.bitcoin.write.here.your.private.key");

				m_greenLight.SetActive(false);
				m_redCross.SetActive(false);
				m_approveMessage.text = LanguageController.Instance.GetText("screen.location.key.is.not.defined.yet");
			}
		}

		// -------------------------------------------
		/* 
		 * FillPrivateKeyInputs
		 */
		private bool FillPrivateKeyInputs(string _decryptedKey)
		{
			if (_decryptedKey == null)
			{
				return false;
			}
			else
			{
				if (_decryptedKey.Length == TOTAL_SIZE_PRIVATE_KEY)
				{
					UpdateValidationVisualizationKey(_decryptedKey);
					m_completeKey.text = _decryptedKey;
					return true;
				}
				else
				{
					if (_decryptedKey.Length > 0)
					{
						BitcoinManagerEventController.Instance.DispatchBasicEvent(ScreenBitcoinInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT);
						string warning = LanguageController.Instance.GetText("message.error");
						string description = LanguageController.Instance.GetText("message.location.key.is.not.valid.blockchain");
						ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, "");
					}
					return false;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnBackButton
		 */
		private void OnBackButton()
		{
			if (HasChanged)
			{
				string warning = LanguageController.Instance.GetText("message.warning");
				string description = LanguageController.Instance.GetText("message.exit.without.apply.changes");
				ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_CONFIRMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_WITHOUT_SAVE);
			}
			else
			{
				Destroy();
			}
		}

		// -------------------------------------------
		/* 
		 * OnAddFunds
		 */
		private void OnAddFunds()
		{
			if (m_requestCheckValidKey)
			{
				m_requestCheckValidKey = false;
				ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
				Invoke("CheckKeyEnteredInMainField", 0.1f);
			}
			else
			{
				ScreenBitcoinController.Instance.CreateNewScreen(ScreenBitcoinAddFundsKeyView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, BitCoinController.Instance.CurrentPublicKey);
			}
		}

		// -------------------------------------------
		/* 
		 * OnCreateNewWallet
		 */
		private void OnCreateNewWallet()
		{
			ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");

			Invoke("OnRealCreateNewWallet", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * OnRealCreateNewWallet
		 */
		public void OnRealCreateNewWallet()
		{
			Key newKey = new Key();
			BitcoinSecret newPrivatteKey = newKey.GetBitcoinSecret(BitCoinController.Instance.Network);

			m_completeKey.text = newPrivatteKey.ToString();
			FillPrivateKeyInputs(m_completeKey.text);
			HasChanged = true;

			m_createNewWallet.SetActive(false);

			BitcoinManagerEventController.Instance.DispatchBasicEvent(ScreenBitcoinInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_POPUP);
		}

		// -------------------------------------------
		/* 
		 * OnCheckInputTransactions
		 */
		private void OnCheckInputTransactions()
		{
			ScreenBitcoinController.Instance.CreateNewScreen(ScreenBitcoinTransactionsView.SCREEN_NAME, TypePreviousActionEnum.HIDE_CURRENT_SCREEN, true, ScreenBitcoinTransactionsView.TRANSACTION_CONSULT_INPUTS);
		}

		// -------------------------------------------
		/* 
		 * OnCheckInputTransactions
		 */
		private void OnCheckOutputTransactions()
		{
			ScreenBitcoinController.Instance.CreateNewScreen(ScreenBitcoinTransactionsView.SCREEN_NAME, TypePreviousActionEnum.HIDE_CURRENT_SCREEN, true, ScreenBitcoinTransactionsView.TRANSACTION_CONSULT_OUTPUTS);
		}		

		// -------------------------------------------
		/* 
		 * OnSaveButton
		 */
		private void OnSaveButton()
		{
			string warning = LanguageController.Instance.GetText("message.warning");
			string description = LanguageController.Instance.GetText("screen.bitcoin.wallet.once.you.set.up.done");
			ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_CONFIRMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENBITCOINPRIVATEKEY_BURN_KEY_CONFIRMATION);
		}

		// -------------------------------------------
		/* 
		 * OnRealSaveButton
		 */
		public void OnRealSaveButton()
		{
			string privateKey = "";
			privateKey = m_completeKey.text;
			bool validationRightKey = (privateKey.Length == TOTAL_SIZE_PRIVATE_KEY) && BitCoinController.Instance.ValidatePrivateKey(privateKey);

			if (validationRightKey)
			{
				m_buttonSave.SetActive(false);

				BitCoinController.Instance.AddPrivateKey(privateKey, true);
				BitCoinController.Instance.SavePrivateKeys();
				if (m_labelKey.text.Length > 0)
				{
					BitCoinController.Instance.SaveAddresses(BitCoinController.Instance.GetPublicKey(privateKey), m_labelKey.text);
				}
				UpdateValidationVisualizationKey(privateKey);
				m_hasChanged = false;
				BitCoinController.Instance.CurrentPrivateKey = privateKey;
				BitcoinManagerEventController.Instance.DispatchBasicEvent(BitCoinController.EVENT_BITCOINCONTROLLER_UPDATE_ACCOUNT_DATA);

				// UPDATE DATABASE
				// HOLA PUTA
			}
			else
			{
				ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("message.location.key.is.not.valid.blockchain"), null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * OnDeleteButton
		 */
		private void OnDeleteButton()
		{
			string warning = LanguageController.Instance.GetText("message.warning");
			string description = LanguageController.Instance.GetText("screen.bitcoin.do.you.really.want.to.delete.wallet");
			ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_CONFIRMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENBITCOINPRIVATEKEY_CONFIRMATION_DELETE);
		}

		// -------------------------------------------
		/* 
		 * UpdateValidationVisualizationKey
		 */
		private void UpdateValidationVisualizationKey(string _privateKey)
		{
			m_greenLight.SetActive(false);
			m_redCross.SetActive(false);
			BitcoinManagerEventController.Instance.DispatchBasicEvent(ScreenBitcoinInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT);
			if (BitCoinController.Instance.ValidatePrivateKey(_privateKey))
			{
				m_greenLight.SetActive(true);
				m_emailPrivateKey.SetActive(true);
				m_approveMessage.text = LanguageController.Instance.GetText("screen.location.key.valid");
				m_buttonSave.SetActive(true);
				SetNewPrivateKey(_privateKey);
			}
			else
			{
				m_redCross.SetActive(true);
				m_approveMessage.text = LanguageController.Instance.GetText("screen.location.key.wrong");
			}
			if (!m_hasBeenInitialized)
			{
				m_hasBeenInitialized = true;
				m_labelKey.onValueChanged.AddListener(OnValueLabelKeyChanged);
				m_labelKey.onEndEdit.AddListener(OnEditedLabelKeyChanged);
			}
		}

		// -------------------------------------------
		/* 
		 * SetNewPrivateKey
		 */
		private void SetNewPrivateKey(string _privateKey)
		{
			BitCoinController.Instance.CurrentPrivateKey = _privateKey;
			string labelResult = BitCoinController.Instance.AddressToLabel(BitCoinController.Instance.CurrentPublicKey);
			if (labelResult != BitCoinController.Instance.CurrentPublicKey)
			{
				m_labelKey.text = labelResult;
			}
		}

		// -------------------------------------------
		/* 
		 * OnBitcoinEvent
		 */
		private void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_BALANCE_WALLET)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("EVENT_BITCOINCONTROLLER_BALANCE_WALLET::m_balanceValue=" + m_balanceValue);
#endif
				m_balanceValue = (decimal)((float)_list[0]);
				m_buttonBalance.SetActive(true);
				m_outputTransactionHistory.SetActive(true);
				m_inputTransactionHistory.SetActive(true);
				m_createNewWallet.SetActive(false);
				float balanceInCurrency = (float)(m_balanceValue * BitCoinController.Instance.GetCurrentExchange());
				m_balance.text = m_balanceValue.ToString() + " BTC" + " /\n" + balanceInCurrency + " " + BitCoinController.Instance.CurrentCurrency;
				m_requestCheckValidKey = false;
			}
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_CURRENCY_CHANGED)
			{
				float balanceInCurrency = (float)(m_balanceValue * BitCoinController.Instance.GetCurrentExchange());
				m_balance.text = m_balanceValue.ToString() + " BTC" + " /\n" + balanceInCurrency + " " + BitCoinController.Instance.CurrentCurrency;
			}
		}
		
		// -------------------------------------------
		/* 
		 * OnBitcoinManagerEvent
		 */
		private void OnBitcoinManagerEvent(string _nameEvent, params object[] _list)
		{
			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == ScreenBitcoinInformationView.EVENT_SCREENINFORMATION_CONFIRMATION_POPUP)
			{
				string subEvent = (string)_list[2];
				if (subEvent == SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_WITHOUT_SAVE)
				{
					if ((bool)_list[1])
					{
						Destroy();
					}
				}
				if (subEvent == SUB_EVENT_SCREENBITCOINPRIVATEKEY_CONFIRMATION_DELETE)
				{
					if ((bool)_list[1])
					{
						BitCoinController.Instance.RemovePrivateKey(BitCoinController.Instance.CurrentPrivateKey);
						BitcoinManagerEventController.Instance.DispatchBasicEvent(BitCoinController.EVENT_BITCOINCONTROLLER_UPDATE_ACCOUNT_DATA);
						BitCoinController.Instance.BackupCurrentPrivateKey = "";
						Destroy();
					}
				}
				if (subEvent == SUB_EVENT_SCREENBITCOINPRIVATEKEY_VIDEO_TUTORIAL)
				{
					if ((bool)_list[1])
					{
						Application.OpenURL("https://www.youtube.com/watch?v=wSwt2hYeAmE");
					}
				}
				if (subEvent == SUB_EVENT_SCREENBITCOINPRIVATEKEY_BURN_KEY_CONFIRMATION)
				{
					if ((bool)_list[1])
					{
						ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenBitcoinInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
						Invoke("OnRealSaveButton", 0.1f);
					}
				}
				if (subEvent == SUB_EVENT_SCREENBITCOINPRIVATEKEY_HIDE_INFO_BUTTONS)
				{
					m_buttonSave.SetActive(false);
					m_outputTransactionHistory.SetActive(false);
					m_inputTransactionHistory.SetActive(false);
					m_buttonBalance.SetActive(false);
				}
			}
			if (_nameEvent == ScreenEmailPrivateKeyView.EVENT_SCREENENTEREMAIL_PRIVATE_KEY_CONFIRMATION)
			{
				Application.OpenURL("mailto:" + (string)_list[0] + "?subject=" + LanguageController.Instance.GetText("message.private.address") + "&body=" + LanguageController.Instance.GetText("screen.bitcoin.wallet.send.private.key.warning") + ":" + BitCoinController.Instance.CurrentPrivateKey);
			}
			if (_nameEvent == ButtonEventCustom.EVENT_BUTTON_CUSTOM_PRESSED_DOWN)
			{
				GameObject sButtonSee = (GameObject)_list[0];
				if (m_seeComplete == sButtonSee)
				{
					m_completeKey.contentType = UnityEngine.UI.InputField.ContentType.Standard;
					m_completeKey.lineType = UnityEngine.UI.InputField.LineType.MultiLineNewline;
					m_completeKey.ForceLabelUpdate();
				}
			}
			if (_nameEvent == ButtonEventCustom.EVENT_BUTTON_CUSTOM_RELEASE_UP)
			{
				m_completeKey.contentType = UnityEngine.UI.InputField.ContentType.Password;
				m_completeKey.ForceLabelUpdate();
			}
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_BALANCE_WALLET)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("EVENT_BITCOINCONTROLLER_BALANCE_WALLET::m_balanceValue=" + m_balanceValue);
#endif
				m_balanceValue = (decimal)((float)_list[0]);
				m_buttonBalance.SetActive(true);
				m_outputTransactionHistory.SetActive(true);
				m_inputTransactionHistory.SetActive(true);
				m_createNewWallet.SetActive(false);
				float balanceInCurrency = (float)(m_balanceValue * BitCoinController.Instance.CurrenciesExchange[BitCoinController.Instance.CurrentCurrency]);
				m_balance.text = m_balanceValue.ToString() + " BTC" + " /\n" + balanceInCurrency + " " + BitCoinController.Instance.CurrentCurrency;
				m_requestCheckValidKey = false;
			}
			if (_nameEvent == EVENT_SCREENPROFILE_LOAD_SCREEN_EXCHANGE_TABLES_INFO)
			{
				YourBitcoinController.CommController.Instance.GetBitcoinExchangeRatesTable();
			}
			if (_nameEvent == EVENT_SCREENPROFILE_LOAD_CHECKING_KEY_PROCESS)
			{
				CheckKeyEnteredInMainField();
			}
			if (_nameEvent == ScreenBitcoinController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				OnBackButton();
			}
		}
	}
}