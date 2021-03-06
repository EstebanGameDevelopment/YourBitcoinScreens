using UnityEngine;
using YourBitcoinController;
using YourCommonTools;
using YourNetworkingTools;

namespace YourBitcoinManager
{

	/******************************************
	 * 
	 * ScreenController
	 * 
	 * ScreenManager controller that handles all the screens's creation and disposal for Bitcoin
	 * 
	 * 	To get Bitcoins in the Main Network:
	 *  
	 *  https://buy.blockexplorer.com/
	 *  
	 *  Or in the TestNet Network:
	 *  
	 *  https://testnet.manu.backend.hamburg/faucet
	 *
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinController : FunctionsScreenController
    {
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const int MAXIMUM_NUMBER_OF_STACKED_SCREENS = 20;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static ScreenBitcoinController _instance;

		public static ScreenBitcoinController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(ScreenBitcoinController)) as ScreenBitcoinController;
					DontDestroyOnLoad(_instance);
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public TextAsset ReadMeFile;

		public string ScreenToLoad = "";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------			
		private string m_screenToLoad = "";
		private object[] m_optionalParams = null;


        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	


        // -------------------------------------------
        /* 
		* Awake
		*/
        public override void Awake()
        {
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            base.Awake();
		}

		// -------------------------------------------
		/* 
		 * Initialitzation listener
		 */
		public override void Start()
		{
			base.Start();

#if DEBUG_MODE_DISPLAY_LOG
            Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
#endif


#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        Screen.SetResolution(550, 900, false);
#endif

#if ENABLE_BITCOIN            
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);

			if (ScreenToLoad.Length > 0)
			{
				LanguageController.Instance.Initialize();

				InitializeBitcoin(UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, ScreenToLoad);
			}
#else
            EnableProcessEvents = false;
#endif
        }

        // -------------------------------------------
        /* 
		 * StartSplashScreen
		 */
        public override void StartSplashScreen()
        {
        }

        // -------------------------------------------
        /* 
		 * InitializeBitcoin
		 */
        public virtual void InitializeBitcoin(UIScreenTypePreviousAction _typeAction, string _screenToLoad = "", params object[] _optionalParams)
		{
			m_screenToLoad = _screenToLoad;
			m_optionalParams = _optionalParams;
			if (m_hasBeenInitialized)
			{
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, m_screenToLoad, _typeAction, true, m_optionalParams);

            }
			else
			{
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INITIAL_CONNECTION, _typeAction, LanguageController.Instance.GetText("message.your.bitcoin.manager.title"), LanguageController.Instance.GetText("message.connecting.to.blockchain"), null, null);

                Invoke("InitializeRealBitcoin", 0.1f);
			}
		}

		// -------------------------------------------
		/* 
		 * InitializeRealBitcoin
		 */
		public void InitializeRealBitcoin()
		{
			BitCoinController.Instance.Init();
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		public override void Destroy()
		{
			base.Destroy();

#if ENABLE_BITCOIN
			BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;

			LanguageController.Instance.Destroy();
			CommController.Instance.Destroy();
			BitCoinController.Instance.Destroy();
#endif

            Destroy(_instance);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected virtual void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_ALL_DATA_COLLECTED)
			{
				if (!m_hasBeenInitialized)
				{
					m_hasBeenInitialized = true;
					BitCoinController.Instance.LoadPrivateKeys(true);

					if (BitCoinController.Instance.CurrentPrivateKey.Length == 0)
					{
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenBitcoinPrivateKeyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
                    }
					else
					{
						if (m_screenToLoad.Length > 0)
						{
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, m_screenToLoad, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true, m_optionalParams);
                        }
					}
				}
				BitcoinEventController.Instance.DispatchBitcoinEvent(BitCoinController.EVENT_BITCOINCONTROLLER_ALL_DATA_INITIALIZED);
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected override void OnUIEvent(string _nameEvent, params object[] _list)
		{
            if (!PreProcessScreenEvents(_nameEvent, _list)) return;

            base.OnUIEvent(_nameEvent, _list);

			if (_nameEvent == ScreenController.EVENT_FORCE_DESTRUCTION_POPUP)
			{
                DestroyScreensFromLayerPool();
			}
			
		}
	}
}