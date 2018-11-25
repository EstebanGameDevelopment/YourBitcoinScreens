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
	 * ScreenBitcoinSendView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinSendView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SEND";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENBITCOINSEND_USER_CONFIRMED_RUN_TRANSACTION = "EVENT_SCREENBITCOINSEND_USER_CONFIRMED_RUN_TRANSACTION";

		// ----------------------------------------------
		// SUBS
		// ----------------------------------------------	
		private const string SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_TRANSACTION	= "SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_TRANSACTION";
		private const string SUB_EVENT_SCREENBITCOIN_CONTINUE_WITH_LOW_FEE			= "SUB_EVENT_SCREENBITCOIN_CONTINUE_WITH_LOW_FEE";
		private const string SUB_EVENT_SCREENBITCOIN_USER_CONFIRMATION_MESSAGE		= "SUB_EVENT_SCREENBITCOIN_USER_CONFIRMATION_MESSAGE";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private InputField m_publicAddressInput;
		private string m_publicAddressToSend;
		private bool m_validPublicAddressToSend = false;
		private GameObject m_validAddress;
		private GameObject m_saveAddress;

		private InputField m_amountInput;
		private string m_amountInCurrency = "0";
		private decimal m_amountInCryptocurrency = 0;
		private Dropdown m_currencies;
		private string m_currencySelected;
		private decimal m_exchangeToBitcoin;

		private InputField m_feeInput;
		private string m_feeInCurrency = "0";
		private decimal m_feeInBitcoins = 0;
		private Dropdown m_fees;

		private InputField m_messageInput;

		private bool m_hasChanged = false;
		private bool m_transactionSuccess = false;
		private string m_transactionIDHex = "";

		private int m_idUser = -1;
		private string m_passwordUser = "";
		private long m_idRequest = -1;

		public bool HasChanged
		{
			get { return m_hasChanged; }
			set
			{
				m_hasChanged = value;
			}
		}
		public bool ValidPublicKeyToSend
		{
            get { return m_validPublicAddressToSend; }
			set
			{
				m_validPublicAddressToSend = value;
				string labelAddress = BitCoinController.Instance.AddressToLabel(m_publicAddressToSend);
				if (labelAddress != m_publicAddressToSend)
				{
					m_container.Find("Address/Label").GetComponent<Text>().text = labelAddress;
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.red;
				}
				else
				{
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.black;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			string publicKeyAddress = "";
			string amountTransaction = "0";
			string messageTransaction = LanguageController.Instance.GetText("screen.send.explain.please");

			if (_list != null)
			{
				if (_list.Length > 0)
				{
					publicKeyAddress = (string)_list[0];					
					if (_list.Length > 2)
					{
						amountTransaction = (string)_list[1];
						BitCoinController.Instance.CurrentCurrency = (string)_list[2];
						if (_list.Length > 3)
						{
							messageTransaction = (string)_list[3];
						}
					}					
				}
			}

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			// DOLLAR
			m_currencySelected = BitCoinController.Instance.CurrentCurrency;
			m_exchangeToBitcoin = BitCoinController.Instance.CurrenciesExchange[m_currencySelected];

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(OnBackButton);

			// YOUR WALLET			
			m_container.Find("YourWallet").GetComponent<Button>().onClick.AddListener(OnCheckWallet);
			UpdateWalletButtonInfo();

			// PUBLIC KEY TO SEND
			m_saveAddress = m_container.Find("Address/SaveAddress").gameObject;
			m_saveAddress.GetComponent<Button>().onClick.AddListener(OnSaveAddress);
			m_saveAddress.SetActive(false);

			m_validAddress = m_container.Find("Address/ValidAddress").gameObject;
			m_validAddress.GetComponent<Button>().onClick.AddListener(OnAddressValid);
			m_validAddress.SetActive(false);

			m_container.Find("Address/Label").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.send.write.destination.address");
			m_publicAddressInput = m_container.Find("Address/PublicKey").GetComponent<InputField>();
			m_publicAddressInput.onValueChanged.AddListener(OnValuePublicKeyChanged);			
			m_container.Find("Address/SelectAddress").GetComponent<Button>().onClick.AddListener(OnSelectAddress);
			m_container.Find("Address/SelectAddress/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.addresses");
#if !ENABLE_FULL_WALLET
			m_container.Find("Address/SelectAddress").gameObject.SetActive(false);
#endif
			m_publicAddressInput.text = publicKeyAddress;

			// AMOUNT
			m_container.Find("Amount/Label").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.send.amount.to.send");
			m_amountInput = m_container.Find("Amount/Value").GetComponent<InputField>();
			m_amountInput.onValueChanged.AddListener(OnValueAmountChanged);
			m_amountInCurrency = amountTransaction;
			m_amountInput.text = m_amountInCurrency;

			// CURRENCIES
			m_currencies = m_container.Find("Amount/Currency").GetComponent<Dropdown>();
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

			// FEE
			m_container.Find("Fee/Label").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.send.fee.to.miners");
			m_feeInput = m_container.Find("Fee/Value").GetComponent<InputField>();
			m_feeInput.onValueChanged.AddListener(OnValueFeeChanged);
			m_feeInCurrency = "0";
			m_feeInput.text = m_feeInCurrency;

			// FEE SUGGESTED
			m_fees = m_container.Find("Fee/Type").GetComponent<Dropdown>();
			m_fees.onValueChanged.AddListener(OnFeeSuggestedChanged);
			m_fees.options = new List<Dropdown.OptionData>();
			for (int i = 0; i < BitCoinController.FEES_SUGGESTED.Length; i++)
			{
				m_fees.options.Add(new Dropdown.OptionData(BitCoinController.FEES_SUGGESTED[i]));
			}

			// MESSAGE
			m_container.Find("Pay/MessageTitle").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.send.write.description.transaction");
			m_messageInput = m_container.Find("Pay/Message").GetComponent<InputField>();
			m_messageInput.text = messageTransaction;
			m_container.Find("Pay/ExecutePayment").GetComponent<Button>().onClick.AddListener(OnExecutePayment);
			
			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);			
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);

			// UPDATE SELECTION CURRENCY
			m_currencies.value = 1;
			m_currencies.value = 0;
			m_currencySelected = BitCoinController.Instance.CurrentCurrency;
			if (indexCurrentCurrency != -1)
			{
				m_currencies.value = indexCurrentCurrency;
			}
			m_exchangeToBitcoin = BitCoinController.Instance.CurrenciesExchange[m_currencySelected];

			// UPDATE SELECTION FEE
			m_fees.itemText.text = BitCoinController.FEES_SUGGESTED[BitCoinController.FEES_SUGGESTED.Length - 1];
			m_fees.value = BitCoinController.FEES_SUGGESTED.Length - 1;
			m_feeInCurrency = (BitCoinController.Instance.FeesTransactions[m_fees.itemText.text] * (decimal)m_exchangeToBitcoin).ToString();
			m_feeInput.text = m_feeInCurrency;

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

			UIEventController.Instance.UIEvent -= OnUIEvent;
			BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnValuePublicKeyChanged
		 */
		private void OnValuePublicKeyChanged(string _newValue)
		{
			if ((_newValue.Length > 0) && (BitCoinController.Instance.CurrentPublicKey != m_publicAddressToSend))
			{
				m_publicAddressToSend = m_publicAddressInput.text;
                ValidPublicKeyToSend = BitCoinController.Instance.ValidatePublicKey(m_publicAddressToSend);
#if ENABLE_FULL_WALLET
				bool enableButtonSaveAddress = true;
				if (BitCoinController.Instance.ContainsAddress(m_publicAddressToSend))
				{
					enableButtonSaveAddress = false;
				}
				if (enableButtonSaveAddress)
				{
					m_saveAddress.SetActive(true);
				}
#endif

                m_validAddress.SetActive(true);
				m_validAddress.transform.Find("IconValid").gameObject.SetActive(m_validPublicAddressToSend);
				m_validAddress.transform.Find("IconError").gameObject.SetActive(!m_validPublicAddressToSend);
			}
		}

		// -------------------------------------------
		/* 
		 * OnValueAmountChanged
		 */
		private void OnValueAmountChanged(string _newValue)
		{
			if (_newValue.Length > 0)
			{
				m_amountInCurrency = m_amountInput.text;
				m_amountInCryptocurrency = decimal.Parse(m_amountInCurrency) / m_exchangeToBitcoin;
			}
		}

		// -------------------------------------------
		/* 
		 * OnCurrencyChanged
		 */
		private void OnCurrencyChanged(int _index)
		{
			BitcoinEventController.Instance.DispatchBitcoinEvent(BitCoinController.EVENT_BITCOINCONTROLLER_NEW_CURRENCY_SELECTED, m_currencies.options[_index].text);

			m_currencySelected = m_currencies.options[_index].text;
			m_exchangeToBitcoin = (decimal)BitCoinController.Instance.CurrenciesExchange[m_currencySelected];

			// UPDATE AMOUNT
			m_amountInCurrency = (m_amountInCryptocurrency * m_exchangeToBitcoin).ToString();
			m_amountInput.text = m_amountInCurrency;

			// UPDATE FEE
			m_feeInCurrency = (m_feeInBitcoins * m_exchangeToBitcoin).ToString();
			m_feeInput.text = m_feeInCurrency;

			// UPDATE WALLET
			UpdateWalletButtonInfo();
		}

		// -------------------------------------------
		/* 
		 * UpdateWalletButtonInfo
		 */
		private void UpdateWalletButtonInfo()
		{
			string messageButton = "";
			string label = BitCoinController.Instance.AddressToLabel(BitCoinController.Instance.CurrentPublicKey);
			if (label != BitCoinController.Instance.CurrentPublicKey)
			{
				messageButton = label;
			}
			decimal bitcoins = BitCoinController.Instance.PrivateKeys[BitCoinController.Instance.CurrentPrivateKey];
			m_exchangeToBitcoin = BitCoinController.Instance.CurrenciesExchange[m_currencySelected];
			if (messageButton.Length > 0)
			{
				messageButton += "/\n"; 
			}
			messageButton += Utilities.Trim(bitcoins.ToString()) + " BTC / \n";
			messageButton += Utilities.Trim((m_exchangeToBitcoin * bitcoins).ToString()) + " " + m_currencySelected;
			m_container.Find("YourWallet/Text").GetComponent<Text>().text = messageButton;
		}

		// -------------------------------------------
		/* 
		 * OnCheckWallet
		 */
		private void OnCheckWallet()
		{
			ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
			Invoke("OnRealCheckWallet", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * OnRealCheckWallet
		 */
		public void OnRealCheckWallet()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
#if ENABLE_FULL_WALLET
			ScreenBitcoinController.Instance.CreateNewScreen(ScreenBitcoinPrivateKeyView.SCREEN_NAME, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, true);
#else
			ScreenBitcoinController.Instance.CreateNewScreen(ScreenBitcoinPrivateKeyView.SCREEN_NAME, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, true, BitCoinController.Instance.CurrentPublicKey);
#endif
		}


		// -------------------------------------------
		/* 
		 * OnSelectAddress
		 */
		private void OnSelectAddress()
		{
#if ENABLE_FULL_WALLET
			ScreenBitcoinController.Instance.CreateNewScreen(ScreenSelectAddressFromView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false);
#endif
		}

		// -------------------------------------------
		/* 
		 * OnSaveAddress
		 */
		private void OnSaveAddress()
		{
#if ENABLE_FULL_WALLET
			ScreenBitcoinController.Instance.CreateNewScreen(ScreenEnterEmailView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, LanguageController.Instance.GetText("screen.enter.new.label.address"));
#endif
		}

		// -------------------------------------------
		/* 
		 * OnValueFeeChanged
		 */
		private void OnValueFeeChanged(string _newValue)
		{
			if (_newValue.Length > 0)
			{
				m_feeInCurrency = m_feeInput.text;
				m_feeInBitcoins = decimal.Parse(m_feeInCurrency) / m_exchangeToBitcoin;
			}
		}

		// -------------------------------------------
		/* 
		 * OnFeeSuggestedChanged
		 */
		private void OnFeeSuggestedChanged(int _index)
		{
			m_feeInCurrency = (BitCoinController.Instance.FeesTransactions[m_fees.options[_index].text] * (decimal)m_exchangeToBitcoin).ToString();
			m_feeInput.text = m_feeInCurrency;
			m_feeInBitcoins = decimal.Parse(m_feeInCurrency) / m_exchangeToBitcoin;
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
				ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_TRANSACTION);
			}
			else
			{
				Destroy();
			}
		}

		// -------------------------------------------
		/* 
		 * OnAddressValid
		 */
		private void OnAddressValid()
		{
			string description = "";
			if (m_validPublicAddressToSend)
			{
				description = LanguageController.Instance.GetText("screen.bitcoin.send.valid.address");
				ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), description, null, "");
			}
			else
			{
				description = LanguageController.Instance.GetText("screen.bitcoin.send.invalid.address");
				ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), description, null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * OnExecutePayment
		 */
		private void OnExecutePayment()
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("m_messageInput.text=" + m_messageInput.text);
			Debug.Log("m_publicAddressToSend=" + m_publicAddressToSend);
			Debug.Log("m_amountInCryptocurrency=" + m_amountInCryptocurrency);
			Debug.Log("m_feeInBitcoins=" + m_feeInBitcoins);
#endif
			if (!m_validPublicAddressToSend)
			{
				ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.bitcoin.send.no.valid.address.to.send"), null, "");
			}
			else
			{
				decimal amountFeeUSD = m_feeInBitcoins * BitCoinController.Instance.CurrenciesExchange[BitCoinController.CODE_DOLLAR];
				if (amountFeeUSD < 0.19m)
				{
					ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("screen.bitcoin.send.fee.too.low"), null, SUB_EVENT_SCREENBITCOIN_CONTINUE_WITH_LOW_FEE);
				}
				else
				{					
					if (m_amountInCryptocurrency == 0) 
					{
						ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.bitcoin.send.amount.no.zero"), null, "");						
					}
					else
					{
						decimal amountTotalUSD = m_amountInCryptocurrency * BitCoinController.Instance.CurrenciesExchange[BitCoinController.CODE_DOLLAR];
						if (amountTotalUSD < 1m)
						{
							ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("screen.bitcoin.send.amount.too.low"), null, SUB_EVENT_SCREENBITCOIN_CONTINUE_WITH_LOW_FEE);
						}
						else
						{
							SummaryTransactionForLastConfirmation();
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * SummaryTransaction
		 */
		private void SummaryTransactionForLastConfirmation()
		{
			ScreenBitcoinController.Instance.CreateNewScreen(ScreenTransactionSummaryView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, m_amountInCryptocurrency, m_feeInBitcoins, m_currencySelected, BitCoinController.Instance.AddressToLabel(m_publicAddressToSend), m_messageInput.text);
		}

		// -------------------------------------------
		/* 
		 * OnExecutePayment
		 */
		private void OnExecuteRealPayment()
		{
#if ENABLE_BITCOIN
            BitCoinController.Instance.Pay(BitCoinController.Instance.CurrentPrivateKey,
								m_publicAddressToSend,
								m_messageInput.text,
								m_amountInCryptocurrency,
								m_feeInBitcoins);
#endif
        }

        // -------------------------------------------
        /* 
		 * OnUIEvent
		 */
        protected void OnUIEvent(string _nameEvent, params object[] _list)
        {
#if ENABLE_FULL_WALLET
			if (_nameEvent == ScreenEnterEmailView.EVENT_SCREENENTEREMAIL_CONFIRMATION)
			{
				string label = (string)_list[0];
				BitCoinController.Instance.SaveAddresses(m_publicAddressToSend, label);
				ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.bitcoin.send.address.saved"), null, "");
				m_saveAddress.SetActive(false);
				if (label.Length > 0)
				{
					m_container.Find("Address/Label").GetComponent<Text>().text = label;
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.red;
				}
			}
#endif
            if (_nameEvent == EVENT_SCREENBITCOINSEND_USER_CONFIRMED_RUN_TRANSACTION)
            {
                ScreenBitcoinController.Instance.DestroyScreensAboveLayerPool();
                ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
                Invoke("OnExecuteRealPayment", 0.1f);
            }

            if (!this.gameObject.activeSelf) return;

            if (_nameEvent == ScreenController.EVENT_CONFIRMATION_POPUP)
            {
                string subEvent = (string)_list[2];
                if (subEvent == SUB_EVENT_SCREENBITCOIN_CONFIRMATION_EXIT_TRANSACTION)
                {
                    if ((bool)_list[1])
                    {
                        Destroy();
                    }
                }
                if (subEvent == SUB_EVENT_SCREENBITCOIN_CONTINUE_WITH_LOW_FEE)
                {
                    if ((bool)_list[1])
                    {
                        SummaryTransactionForLastConfirmation();
                    }
                }
                if (subEvent == SUB_EVENT_SCREENBITCOIN_USER_CONFIRMATION_MESSAGE)
                {
                    BitcoinEventController.Instance.DispatchBitcoinEvent(BitCoinController.EVENT_BITCOINCONTROLLER_TRANSACTION_USER_ACKNOWLEDGE, m_transactionSuccess, m_transactionIDHex);
                }
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
            {
                OnBackButton();
            }
        }

        // -------------------------------------------
        /* 
		 * OnBitcoinEvent
		 */
        private void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_TRANSACTION_DONE)
			{
				UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
				m_transactionSuccess = (bool)_list[0];
				m_transactionIDHex = "";
				if ((bool)_list[0])
				{
					HasChanged = false;
					BitCoinController.Instance.RefreshBalancePrivateKeys();
					m_transactionIDHex = (string)_list[1];					 
					ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.bitcoin.send.transaction.success"), null, SUB_EVENT_SCREENBITCOIN_USER_CONFIRMATION_MESSAGE);
				}
				else
				{								
					string messageError = LanguageController.Instance.GetText("screen.bitcoin.send.transaction.error");
					if (_list.Length >= 2)
					{
						messageError = (string)_list[1];
					}
					ScreenBitcoinController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), messageError, null, "");
				}
			}
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY)
			{
				string publicKeyAddress = (string)_list[0];
				HasChanged = true;
				m_publicAddressInput.text = publicKeyAddress;
				m_publicAddressToSend = publicKeyAddress;
				ValidPublicKeyToSend = BitCoinController.Instance.ValidatePublicKey(m_publicAddressToSend);
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY::PUBLIC KEY ADDRESS=" + publicKeyAddress);
#endif

				string labelAddress = BitCoinController.Instance.AddressToLabel(publicKeyAddress);
				if ((labelAddress.Length > 0) && (labelAddress != publicKeyAddress))
				{
					m_container.Find("Address/Label").GetComponent<Text>().text = labelAddress;
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.red;
				}
			}
		}
    }
}