using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenEnterEmailView
	 * 
	 * It allows the user to enter an email address
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinEnterEmailView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_ENTER_EMAIL";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENENTEREMAIL_CONFIRMATION = "EVENT_SCREENENTEREMAIL_CONFIRMATION";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
            base.Initialize(_list);

            m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = (string)_list[0];

			m_container.Find("Button_Save").GetComponent<Button>().onClick.AddListener(OnConfirmEmail);
			m_container.Find("Button_Cancel").GetComponent<Button>().onClick.AddListener(OnCancelEmail);

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
		 * OnSaveEmail
		 */
		private void OnConfirmEmail()
		{
			string email = m_container.Find("Email").GetComponent<InputField>().text;

			if (email.Length == 0)
			{
				string titleInfoError = LanguageController.Instance.GetText("message.error");
				string descriptionInfoError = LanguageController.Instance.GetText("screen.enter.email.empty.data");
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, titleInfoError, descriptionInfoError, null, "");
            }
			else
			{
				UIEventController.Instance.DispatchUIEvent(EVENT_SCREENENTEREMAIL_CONFIRMATION, email);
			}
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnCancelURL
		 */
		private void OnCancelEmail()
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
                    OnCancelEmail();
                }
            }
		}
	}
}