﻿using System;
using System.Collections.Generic;
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
	 * ScreenBitcoinPrivateKeyView
	 * 
     * Screen that allows to manage an individual Bitcoin address
     * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinPrivateKeyView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_WALLET";

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREENPROFILE_SERVER_REQUEST_RESET_PASSWORD_CONFIRMATION	= "EVENT_SCREENPROFILE_SERVER_REQUEST_RESET_PASSWORD_CONFIRMATION";
		public const string EVENT_SCREENPROFILE_LOAD_SCREEN_EXCHANGE_TABLES_INFO			= "EVENT_SCREENPROFILE_LOAD_SCREEN_EXCHANGE_TABLES_INFO";
		public const string EVENT_SCREENPROFILE_LOAD_CHECKING_KEY_PROCESS					= "EVENT_SCREENPROFILE_LOAD_CHECKING_KEY_PROCESS";
		public const string EVENT_SCREENBITCOINPRIVATEKEY_SEND_PRIVATE_KEY_EMAIL			= "EVENT_SCREENBITCOINPRIVATEKEY_SEND_PRIVATE_KEY_EMAIL";
		public const string EVENT_SCREENBITCOINPRIVATEKEY_INITIALIZE_BITCOIN_WITH			= "EVENT_SCREENBITCOINPRIVATEKEY_INITIALIZE_BITCOIN_WITH";

        public const string EVENT_SCREENBITCOINPRIVATEKEY_CANCELATION = "EVENT_SCREENBITCOINPRIVATEKEY_CANCELATION";

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
        private GameObject m_btnInfoPrivateKey;

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
		private GameObject m_exchangeTable;

		private bool m_hasChanged = false;
		private GameObject m_buttonSave;
		private GameObject m_buttonDelete;

        private bool m_considerEnableEdition = false;
		private bool m_enableEdition = true;
		private bool m_enableDelete = false;
		private string m_currentPublicKey = "";

		public bool HasChanged
		{
			get { return m_hasChanged; }
			set {
				m_hasChanged = value;
#if ENABLE_FULL_WALLET
				m_buttonSave.SetActive(m_hasChanged);
#endif
			}
		}
		public bool EnableEdition
		{
			get { return m_enableEdition; }
			set
			{
				m_enableEdition = value;
				if (m_completeKey != null) m_completeKey.interactable = m_enableEdition;
				if (m_labelKey != null) m_labelKey.interactable = m_enableEdition;
				if (m_enableEdition)
				{
					if (m_buttonSave != null) EnableVisualSaveButton();
				}
			}
		}


		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
            base.Initialize(_list);

            object[] initialParams = (object[])_list[0];

#if ENABLE_FULL_WALLET
			if ((initialParams != null) && (initialParams.Length > 0))
			{
				m_enableEdition = (bool)initialParams[0];
				m_enableDelete = true;
			}
			else
			{
				m_enableEdition = true;
				m_enableDelete = false;
			}
#else
            if (initialParams != null)
			{
				if (initialParams.Length > 0)
				{
					m_currentPublicKey = (string)initialParams[0];
					m_considerEnableEdition = true;
				}
			}
			UpdateEnableEdition();
			m_enableDelete = false;
#endif

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
            m_btnInfoPrivateKey = m_container.Find("InfoPrivateKey").gameObject;
            m_btnInfoPrivateKey.GetComponent<Button>().onClick.AddListener(OnInfoPrivateKey);

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

			m_exchangeTable = m_container.Find("ExchangeTable").gameObject;
			m_exchangeTable.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.exchange.information");
			m_exchangeTable.GetComponent<Button>().onClick.AddListener(OnExchangeTableInfo);
			m_exchangeTable.SetActive(false);

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(OnBackButton);

			m_buttonSave = m_container.Find("Button_Save").gameObject;
			m_buttonSave.GetComponent<Button>().onClick.AddListener(OnSaveButton);
			m_buttonSave.SetActive(false);
			HasChanged = false;

            m_buttonDelete = m_container.Find("Button_Delete").gameObject;
			m_buttonDelete.GetComponent<Button>().onClick.AddListener(OnDeleteButton);
			m_buttonDelete.SetActive(false);

			m_createNewWallet = m_container.Find("CreatePrivateKey").gameObject;
			m_createNewWallet.GetComponent<Button>().onClick.AddListener(OnCreateNewWallet);
			m_createNewWallet.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.create.new.wallet");
			if (m_currentPublicKey.Length > 0)
			{
				m_createNewWallet.SetActive(false);
			}

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);			
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);

			LoadDataPrivateKey();

#if ENABLE_BITCOIN
			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + BitCoinController.Instance.Network.ToString();
#endif

			UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_WAIT);

			UpdateEnableEdition();
		}

        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public override bool Destroy()
		{
			if (base.Destroy()) return true;

			BitCoinController.Instance.RestoreCurrentPrivateKey();

			UIEventController.Instance.UIEvent -= OnMenuEvent;
			BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;

			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

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
		 * UpdateEnableEdition
		 */
		private void UpdateEnableEdition()
		{
#if !ENABLE_FULL_WALLET
			if (m_considerEnableEdition)
			{
				if ((m_currentPublicKey.Length > 0)
					|| (BitCoinController.Instance.CurrentPublicKey.Length > 0))
				{
					EnableEdition = BitCoinController.Instance.CurrentPublicKey != m_currentPublicKey;
				}
				else
				{
					EnableEdition = true;
				}
			}
#endif
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
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, info, description, null, "");
        }

		// -------------------------------------------
		/* 
		 * OnInfoPrivateKey
		 */
		public void OnInfoPrivateKey()
		{
			string info = LanguageController.Instance.GetText("message.info");
			string description = LanguageController.Instance.GetText("screen.bitcoin.wallet.fill.the.inputfield.with.your.private.key");
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, info, description, null, SUB_EVENT_SCREENBITCOINPRIVATEKEY_VIDEO_TUTORIAL);
        }

		// -------------------------------------------
		/* 
		 * OnEmailPrivateKey
		 */
		public void OnEmailPrivateKey()
		{
			string info = LanguageController.Instance.GetText("message.warning");
			string description = LanguageController.Instance.GetText("screen.bitcoin.wallet.send.private.key.warning");
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 1, null, ScreenEmailPrivateKeyView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false);
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
				m_buttonSave.SetActive(false);
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
						UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_WAIT);
						string warning = LanguageController.Instance.GetText("message.error");
						string description = LanguageController.Instance.GetText("message.location.key.is.not.valid.blockchain");
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, warning, description, null, "");
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
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_WITHOUT_SAVE);
            }
			else
			{
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_SCREENBITCOINPRIVATEKEY_CANCELATION);
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
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
                Invoke("CheckKeyEnteredInMainField", 0.5f);
			}
			else
			{
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 1, null, ScreenBitcoinAddFundsKeyView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, BitCoinController.Instance.CurrentPublicKey);
            }
		}

		// -------------------------------------------
		/* 
		 * OnCreateNewWallet
		 */
		private void OnCreateNewWallet()
		{
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");

            Invoke("OnRealCreateNewWallet", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * OnRealCreateNewWallet
		 */
		public void OnRealCreateNewWallet()
		{
#if ENABLE_BITCOIN
			Key newKey = new Key();
			BitcoinSecret newPrivatteKey = newKey.GetBitcoinSecret(BitCoinController.Instance.Network);

			m_completeKey.text = newPrivatteKey.ToString();
			FillPrivateKeyInputs(m_completeKey.text);
			HasChanged = true;

			m_createNewWallet.SetActive(false);
#endif
			UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
		}

		// -------------------------------------------
		/* 
		 * OnCheckInputTransactions
		 */
		private void OnCheckInputTransactions()
		{
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 1, null, ScreenBitcoinTransactionsView.SCREEN_NAME, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, false, ScreenBitcoinTransactionsView.TRANSACTION_CONSULT_INPUTS);
        }

		// -------------------------------------------
		/* 
		 * OnCheckInputTransactions
		 */
		private void OnCheckOutputTransactions()
		{
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 1, null, ScreenBitcoinTransactionsView.SCREEN_NAME, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, false, ScreenBitcoinTransactionsView.TRANSACTION_CONSULT_OUTPUTS);
        }		

		// -------------------------------------------
		/* 
		 * OnSaveButton
		 */
		private void OnSaveButton()
		{
			string warning = LanguageController.Instance.GetText("message.warning");
			string description = LanguageController.Instance.GetText("screen.bitcoin.wallet.once.you.set.up.done");
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENBITCOINPRIVATEKEY_BURN_KEY_CONFIRMATION);
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
				m_currentPublicKey = BitCoinController.Instance.CurrentPublicKey;
				UpdateEnableEdition();
				BitcoinEventController.Instance.DispatchBitcoinEvent(BitCoinController.EVENT_BITCOINCONTROLLER_UPDATE_ACCOUNT_DATA);
				BasicSystemEventController.Instance.DispatchBasicSystemEvent(BitCoinController.EVENT_BITCOINCONTROLLER_PUBLIC_KEY_SELECTED, BitCoinController.Instance.CurrentPublicKey);
			}
			else
			{
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("message.location.key.is.not.valid.blockchain"), null, "");
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
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENBITCOINPRIVATEKEY_CONFIRMATION_DELETE);
        }

		// -------------------------------------------
		/* 
		 * EnableVisualSaveButton
		 */
		private void EnableVisualSaveButton()
		{
			if (m_considerEnableEdition)
			{
				if (m_currentPublicKey == "")
				{
					if (m_greenLight.activeSelf)
					{
						m_buttonSave.SetActive(m_enableEdition);
					}
					else
					{
						m_buttonSave.SetActive(true);
					}
				}
				else
				{
					if (m_greenLight.activeSelf)
					{
						m_buttonSave.SetActive(m_enableEdition);
					}
					else
					{
						m_buttonSave.SetActive(false);
					}
				}
			}
			else
			{
				m_buttonSave.SetActive(true);
			}
		}

		// -------------------------------------------
		/* 
		 * UpdateValidationVisualizationKey
		 */
		private void UpdateValidationVisualizationKey(string _privateKey)
		{
			m_greenLight.SetActive(false);
			m_redCross.SetActive(false);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_WAIT);
			if (BitCoinController.Instance.ValidatePrivateKey(_privateKey))
			{
				m_greenLight.SetActive(true);
				m_emailPrivateKey.SetActive(true);
				m_approveMessage.text = LanguageController.Instance.GetText("screen.location.key.valid");
#if ENABLE_FULL_WALLET
				SetNewPrivateKey(_privateKey);
#else

				if (!m_considerEnableEdition)
				{
					SetNewPrivateKey(_privateKey);
					m_buttonSave.SetActive(true);
				}
				else
				{
					if (m_currentPublicKey.Length == 0)
					{
						SetNewPrivateKey(_privateKey);
						EnableVisualSaveButton();
					}
					else
					{
						if (m_currentPublicKey != BitCoinController.Instance.GetPublicKey(_privateKey))
						{
							string warning = LanguageController.Instance.GetText("message.error");
							string description = LanguageController.Instance.GetText("screen.bitcoin.wallet.not.the.same.public.key");
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENBITCOINPRIVATEKEY_HIDE_INFO_BUTTONS);
                        }
						else
						{
							m_buttonSave.SetActive((BitCoinController.Instance.CurrentPrivateKey != _privateKey));
							SetNewPrivateKey(_privateKey);
						}
					}
				}
#endif
			}
			else
			{
				m_redCross.SetActive(true);
				m_approveMessage.text = LanguageController.Instance.GetText("screen.location.key.wrong");
			}
			if (!m_hasBeenInitialized)
			{
				m_hasBeenInitialized = true;
#if ENABLE_FULL_WALLET
				if (m_enableEdition)
				{
					m_labelKey.onValueChanged.AddListener(OnValueLabelKeyChanged);
					m_labelKey.onEndEdit.AddListener(OnEditedLabelKeyChanged);
				}
				else
				{
					m_labelKey.interactable = false;
				}
#else
				m_labelKey.onValueChanged.AddListener(OnValueLabelKeyChanged);
				m_labelKey.onEndEdit.AddListener(OnEditedLabelKeyChanged);
#endif
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
		 * OnExchangeTableInfo
		 */
		private void OnExchangeTableInfo()
		{
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
            if (m_balanceValue > 0)
			{
				UIEventController.Instance.DelayUIEvent(EVENT_SCREENPROFILE_LOAD_SCREEN_EXCHANGE_TABLES_INFO, 0.1f);
			}
			else
			{
				UIEventController.Instance.DelayUIEvent(EVENT_SCREENPROFILE_LOAD_CHECKING_KEY_PROCESS, 0.1f);
			}
		}

		// -------------------------------------------
		/* 
		 * OnBitcoinEvent
		 */
		private void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
            if (_nameEvent == EVENT_SCREENBITCOINPRIVATEKEY_INITIALIZE_BITCOIN_WITH)
            {
                m_completeKey.text = (string)_list[0];
                Invoke("CheckKeyEnteredInMainField", 0.1f);
            }
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_JSON_EXCHANGE_TABLE)
			{
#if ENABLE_FULL_WALLET
				UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
                List<object> exchangeParams = new List<object>();
                exchangeParams.Add(m_balanceValue);
                exchangeParams.Add((string)_list[0]);
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 1, null, ScreenExchangeTableView.SCREEN_EXCHANGE_TABLE, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, exchangeParams.ToArray());
#endif
            }
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_BALANCE_WALLET)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("EVENT_BITCOINCONTROLLER_BALANCE_WALLET::m_balanceValue=" + m_balanceValue);
#endif
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_WAIT);

                m_balanceValue = (decimal)((float)_list[0]);
				m_buttonBalance.SetActive(true);
				m_outputTransactionHistory.SetActive(true);
				m_inputTransactionHistory.SetActive(true);
				m_createNewWallet.SetActive(false);
#if ENABLE_FULL_WALLET
				m_exchangeTable.SetActive(true);
				if (m_enableEdition)
				{
					if (m_enableDelete)
					{
						m_buttonDelete.SetActive(true);
					}
				}
#endif
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
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

#if !(ENABLE_OCULUS || ENABLE_WORLDSENSE)
            if (!this.gameObject.activeSelf) return;
#endif

			if (_nameEvent == ScreenController.EVENT_CONFIRMATION_POPUP)
			{
				string subEvent = (string)_list[2];
				if (subEvent == SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_WITHOUT_SAVE)
				{
					if ((bool)_list[1])
					{
                        BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_SCREENBITCOINPRIVATEKEY_CANCELATION);
                        Destroy();
					}
				}
				if (subEvent == SUB_EVENT_SCREENBITCOINPRIVATEKEY_CONFIRMATION_DELETE)
				{
					if ((bool)_list[1])
					{
						BitCoinController.Instance.RemovePrivateKey(BitCoinController.Instance.CurrentPrivateKey);
						BitcoinEventController.Instance.DispatchBitcoinEvent(BitCoinController.EVENT_BITCOINCONTROLLER_UPDATE_ACCOUNT_DATA);
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
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
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
				Application.OpenURL("mailto:" + (string)_list[0] + "?subject=" + LanguageController.Instance.GetText("message.private.address") + "&body=" + LanguageController.Instance.GetText("screen.bitcoin.wallet.send.private.key.warning") + ":" + BitCoinController.Instance.CurrentPrivateKey + ", PUBLIC KEY=" + BitCoinController.Instance.CurrentPublicKey);
			}
			if (_nameEvent == ButtonEventCustom.EVENT_BUTTON_CUSTOM_PRESSED_DOWN)
			{
				GameObject sButtonSee = (GameObject)_list[0];
				if (m_seeComplete == sButtonSee)
				{
					m_completeKey.contentType = UnityEngine.UI.InputField.ContentType.Standard;
					m_completeKey.lineType = UnityEngine.UI.InputField.LineType.MultiLineNewline;
					m_completeKey.ForceLabelUpdate();
                    m_btnInfoPrivateKey.SetActive(false);
                    m_emailPrivateKey.SetActive(false);
                }
			}
			if (_nameEvent == ButtonEventCustom.EVENT_BUTTON_CUSTOM_RELEASE_UP)
			{
				m_completeKey.contentType = UnityEngine.UI.InputField.ContentType.Password;
				m_completeKey.ForceLabelUpdate();
                m_btnInfoPrivateKey.SetActive(true);
                m_emailPrivateKey.SetActive(true);
            }
			if (_nameEvent == EVENT_SCREENPROFILE_LOAD_SCREEN_EXCHANGE_TABLES_INFO)
			{
				CommsHTTPConstants.GetBitcoinExchangeRatesTable();
			}
			if (_nameEvent == EVENT_SCREENPROFILE_LOAD_CHECKING_KEY_PROCESS)
			{
				CheckKeyEnteredInMainField();
			}
            if (this.gameObject.activeSelf)
            {
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
                {
                    OnBackButton();
                }
            }
		}
	}
}