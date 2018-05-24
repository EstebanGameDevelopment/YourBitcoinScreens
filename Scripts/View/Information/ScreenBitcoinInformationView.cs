using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourBitcoinManager
{

	/******************************************
	 * 
	 * ScreenInformationView
	 * 
	 * Screen used to display informative or confirmation pop-up messages.
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinInformationView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_INFORMATION			= "SCREEN_INFORMATION";
		public const string SCREEN_INFORMATION_IMAGE	= "SCREEN_INFORMATION_IMAGE";
		public const string SCREEN_WAIT					= "SCREEN_WAIT";
		public const string SCREEN_CONFIRMATION			= "SCREEN_CONFIRMATION";
		public const string SCREEN_INITIAL_CONNECTION	= "SCREEN_INITIAL_CONNECTION";
		public const string SCREEN_UNLOCK_BITCOIN		= "SCREEN_UNLOCK_BITCOIN";
		public const string SCREEN_CHANGE_NETWORK		= "SCREEN_CHANGE_NETWORK";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENINFORMATION_CHANGED_PAGE_POPUP = "EVENT_SCREENINFORMATION_CHANGED_PAGE_POPUP";
		public const string EVENT_SCREENINFORMATION_CONFIRMATION_POPUP = "EVENT_SCREENINFORMATION_CONFIRMATION_POPUP";
		public const string EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_POPUP = "EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_POPUP";
		public const string EVENT_SCREENINFORMATION_FORCE_TRIGGER_OK_BUTTON = "EVENT_SCREENINFORMATION_FORCE_TRIGGER_OK_BUTTON";
		public const string EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT = "EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private Button m_ok;
		private Button m_cancel;
		private Button m_next;
		private Button m_previous;
		private Button m_abort;
		private Text m_text;
		private Text m_title;
		private Image m_imageContent;

		private int m_page = 0;
		private List<PageInformation> m_pagesInfo = new List<PageInformation>();
		private bool m_forceLastPage = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool ForceLastPage
		{
			get { return m_forceLastPage; }
			set { m_forceLastPage = value; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			List<PageInformation> listPages = (List<PageInformation>)_list[0];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			if (m_container.Find("Button_OK") != null)
			{
				m_ok = m_container.Find("Button_OK").GetComponent<Button>();
				m_ok.gameObject.GetComponent<Button>().onClick.AddListener(OkPressed);
			}
			if (m_container.Find("Button_Cancel") != null)
			{
				m_cancel = m_container.Find("Button_Cancel").GetComponent<Button>();
				m_cancel.gameObject.GetComponent<Button>().onClick.AddListener(CancelPressed);
			}
			if (m_container.Find("Button_Next") != null)
			{
				m_next = m_container.Find("Button_Next").GetComponent<Button>();
				m_next.gameObject.GetComponent<Button>().onClick.AddListener(NextPressed);
			}
			if (m_container.Find("Button_Previous") != null)
			{
				m_previous = m_container.Find("Button_Previous").GetComponent<Button>();
				m_previous.gameObject.GetComponent<Button>().onClick.AddListener(PreviousPressed);
			}
			if (m_container.Find("Button_Abort") != null)
			{
				m_abort = m_container.Find("Button_Abort").GetComponent<Button>();
				m_abort.gameObject.GetComponent<Button>().onClick.AddListener(AbortPressed);
			}

			if (m_container.Find("Text") != null)
			{
				m_text = m_container.Find("Text").GetComponent<Text>();
			}
			if (m_container.Find("Title") != null)
			{
				m_title = m_container.Find("Title").GetComponent<Text>();
			}

			if (m_container.Find("Image") != null)
			{
				m_imageContent = m_container.Find("Image").GetComponent<Image>();
			}

			if (listPages != null)
			{
				for (int i = 0; i < listPages.Count; i++)
				{
					m_pagesInfo.Add(((PageInformation)listPages[i]).Clone());
				}
			}

			BitcoinManagerEventController.Instance.BitcoinManagerEvent += new BitcoinManagerEventHandler(OnBasicEvent);

			ChangePage(0);
		}


		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			BitcoinManagerEventController.Instance.BitcoinManagerEvent -= OnBasicEvent;
			BitcoinManagerEventController.Instance.DispatchBasicEvent(ScreenBitcoinController.EVENT_SCREENMANAGER_DESTROY_OVERLAY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OkPressed
		 */
		private void OkPressed()
		{
			if (m_page + 1 < m_pagesInfo.Count)
			{
				ChangePage(1);
				return;
			}

			BitcoinManagerEventController.Instance.DispatchBasicEvent(EVENT_SCREENINFORMATION_CONFIRMATION_POPUP, this.gameObject, true, m_pagesInfo[m_page].EventData);
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * CancelPressed
		 */
		private void CancelPressed()
		{
			BitcoinManagerEventController.Instance.DispatchBasicEvent(EVENT_SCREENINFORMATION_CONFIRMATION_POPUP, this.gameObject, false, m_pagesInfo[m_page].EventData);
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * AbortPressed
		 */
		private void AbortPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * NextPressed
		 */
		private void NextPressed()
		{
			ChangePage(1);
		}

		// -------------------------------------------
		/* 
		 * PreviousPressed
		 */
		private void PreviousPressed()
		{
			ChangePage(-1);
		}

		// -------------------------------------------
		/* 
		 * Chage the information page
		 */
		private void ChangePage(int _value)
		{
			m_page += _value;
			if (m_page < 0) m_page = 0;
			if (m_pagesInfo.Count == 0)
			{
				return;
			}
			else
			{
				if (m_page >= m_pagesInfo.Count - 1)
				{
					m_page = m_pagesInfo.Count - 1;
				}
			}

			if ((m_page >= 0) && (m_page < m_pagesInfo.Count))
			{
				if (m_title != null) m_title.text = m_pagesInfo[m_page].MyTitle;
				if (m_text != null) m_text.text = m_pagesInfo[m_page].MyText;
				if (m_ok != null)
				{
					if (m_ok.gameObject.transform.Find("Text") != null)
					{
						if (m_pagesInfo[m_page].OkButtonText.Length > 0)
						{
							m_ok.gameObject.transform.Find("Text").GetComponent<Text>().text = m_pagesInfo[m_page].OkButtonText;
						}						
					}
				}
				if (m_cancel != null)
				{
					if (m_cancel.gameObject.transform.Find("Text") != null)
					{
						if (m_pagesInfo[m_page].CancelButtonText.Length > 0)
						{
							m_cancel.gameObject.transform.Find("Text").GetComponent<Text>().text = m_pagesInfo[m_page].CancelButtonText;
						}
					}
				}
				if (m_pagesInfo[m_page].MyText.Length > 0)
				{
					this.gameObject.name = m_pagesInfo[m_page].MyText;
				}
				if (m_imageContent != null)
				{
					if (m_pagesInfo[m_page].MySprite != null)
					{
						m_imageContent.sprite = m_pagesInfo[m_page].MySprite;
					}
				}
			}

			if (m_cancel != null) m_cancel.gameObject.SetActive(true);
			if (m_pagesInfo.Count == 1)
			{
				if (m_next != null) m_next.gameObject.SetActive(false);
				if (m_previous != null) m_previous.gameObject.SetActive(false);
				if (m_ok != null) m_ok.gameObject.SetActive(true);
			}
			else
			{
				if (m_page == 0)
				{
					if (m_previous != null) m_previous.gameObject.SetActive(false);
					if (m_next != null) m_next.gameObject.SetActive(true);
				}
				else
				{
					if (m_page == m_pagesInfo.Count - 1)
					{
						if (m_previous != null) m_previous.gameObject.SetActive(true);
						if (m_next != null) m_next.gameObject.SetActive(false);
					}
					else
					{
						if (m_previous != null) m_previous.gameObject.SetActive(true);
						if (m_next != null) m_next.gameObject.SetActive(true);
					}
				}

				BitcoinManagerEventController.Instance.DispatchBasicEvent(EVENT_SCREENINFORMATION_CHANGED_PAGE_POPUP, this.gameObject, m_pagesInfo[m_page].EventData);
			}
		}

		// -------------------------------------------
		/* 
		 * SetTitle
		 */
		public void SetTitle(string _text)
		{
			if (m_title != null)
			{
				m_title.text = _text;
			}
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{			
			if (_nameEvent == EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_POPUP)
			{
				Destroy();
			}
			if (_nameEvent == EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT)
			{
				Destroy();
			}
			if (_nameEvent == EVENT_SCREENINFORMATION_FORCE_TRIGGER_OK_BUTTON)
			{
				OkPressed();
			}
			if (_nameEvent == ScreenBitcoinController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				if (m_cancel != null)
				{
					CancelPressed();
				}
				else
				{
					OkPressed();
				}
			}
		}
	}
}