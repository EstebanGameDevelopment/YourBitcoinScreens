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
	 * SlotManagerView
	 * 
	 * Class that allows a pagination system for 
	 * the requests' list to allow multiple pagination
	 * to avoid load too much data at the same time.
	 * 
	 * @author Esteban Gallardo
	 */
	public class SlotManagerView: MonoBehaviour
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const int DEFAULT_ITEMS_EACH_PAGE = 4;

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public GameObject ButtonNext;
		public GameObject ButtonPrevious;

		public GameObject LoadingIcon;
		public GameObject LoadingText;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_content;	
		private List<GameObject> m_gameObjects = new List<GameObject>();
		private List<ItemMultiObjectEntry> m_data;
		private GameObject m_slotPrefab;
		private GameObject m_createNewPrefab;
		private int m_currentPage = 0;
		private int m_totalPages = 0;
		private int m_itemsEachPage = 0;
		private Transform m_imageLoading;
		private Transform m_textLoading;

		private Transform m_buttonNext;
		private Transform m_buttonPrevious;

		// -------------------------------------------
		/* 
		 * Initialize
		 */
		public void Initialize(int _itemsEachPage, List<ItemMultiObjectEntry> _data, GameObject _slotPrefab, GameObject _createNewPrefab = null)
		{
			m_itemsEachPage = _itemsEachPage;
			m_data = _data;
			m_slotPrefab = _slotPrefab;
			m_createNewPrefab = _createNewPrefab;

			m_content = this.gameObject.transform.Find("ScrollContent/Entries").gameObject;
			m_buttonNext = ButtonNext.transform;
			m_buttonPrevious = ButtonPrevious.transform;

			if (m_buttonNext != null)
			{
				m_buttonNext.GetComponent<Button>().onClick.AddListener(OnNextPressed);
				m_buttonNext.gameObject.SetActive(false);
			}
			if (m_buttonPrevious != null)
			{
				m_buttonPrevious.GetComponent<Button>().onClick.AddListener(OnPreviousPressed);
				m_buttonPrevious.gameObject.SetActive(false);
			}

			m_imageLoading = LoadingIcon.transform;
			m_textLoading = LoadingText.transform;
			if (m_textLoading!=null)
			{
				m_textLoading.GetComponent<Text>().text = LanguageController.Instance.GetText("message.loading");
			}
			m_imageLoading.gameObject.SetActive(true);
			m_textLoading.gameObject.SetActive(true);

			m_totalPages = m_data.Count / m_itemsEachPage;

			LoadCurrentPage();
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			ClearCurrentGameObject(true);
			m_gameObjects = null;
		}

		// -------------------------------------------
		/* 
		 * ClearCurrentGameObject
		 */
		public void ClearCurrentGameObject(bool _resetPage)
		{
			for (int i = 0; i < m_gameObjects.Count; i++)
			{
				if (m_gameObjects[i] != null)
				{
					m_gameObjects[i].GetComponent<ISlotView>().Destroy();
					m_gameObjects[i] = null;
				}
			}
			m_gameObjects.Clear();

			if (m_imageLoading != null) m_imageLoading.gameObject.SetActive(true);
			if (m_textLoading != null)
			{
				m_textLoading.GetComponent<Text>().text = LanguageController.Instance.GetText("message.loading");
				m_textLoading.gameObject.SetActive(true);
			}

			if (_resetPage)
			{
				m_currentPage = 0;
				if (m_buttonNext != null) m_buttonNext.GetComponent<Button>().onClick.RemoveAllListeners();
				if (m_buttonPrevious != null) m_buttonPrevious.GetComponent<Button>().onClick.RemoveAllListeners();
			}
		}


		// -------------------------------------------
		/* 
		 * LoadCurrentPage
		 */
		public void LoadCurrentPage()
		{
			if ((m_buttonNext != null) && (m_buttonPrevious != null) && (m_data.Count > m_itemsEachPage))
			{
				ClearCurrentGameObject(false);
				this.gameObject.transform.Find("ScrollContent").GetComponent<ScrollRect>().verticalNormalizedPosition = 1;

				int initialItem = m_currentPage * m_itemsEachPage;
				int finalItem = initialItem + m_itemsEachPage;

				int i = initialItem;
				for (i = initialItem; i < finalItem; i++)
				{
					if (i < m_data.Count)
					{
						GameObject newSlot = Utilities.AddChild(m_content.transform, m_slotPrefab);
						newSlot.GetComponent<ISlotView>().Initialize(m_data[i]);
						m_gameObjects.Add(newSlot);
					}
				}
				bool endReached = (i >= m_data.Count);

				if (m_buttonNext != null) m_buttonNext.gameObject.SetActive(false);
				if (m_buttonPrevious != null) m_buttonPrevious.gameObject.SetActive(false);

				if (initialItem == 0)
				{
					if (m_data.Count > m_itemsEachPage)
					{
						if (m_buttonNext != null)
						{
							m_buttonNext.gameObject.SetActive(true);
						}
					}
					if (m_buttonPrevious != null)
					{
						m_buttonPrevious.gameObject.SetActive(false);
					}
				}
				else
				{
					if (endReached)
					{
						if ((m_buttonPrevious != null) && (initialItem != 0))
						{
							m_buttonPrevious.gameObject.SetActive(true);
						}
					}
					else
					{
						if (m_buttonNext != null) m_buttonNext.gameObject.SetActive(true);
						if (m_buttonPrevious != null) m_buttonPrevious.gameObject.SetActive(true);
					}
				}
			}
			else
			{
				if (m_buttonNext != null) m_buttonNext.gameObject.SetActive(false);
				if (m_buttonPrevious != null) m_buttonPrevious.gameObject.SetActive(false);

				ClearCurrentGameObject(false);
				for (int i = 0; i < m_data.Count; i++)
				{                
					GameObject newSlot = Utilities.AddChild(m_content.transform, m_slotPrefab);
					newSlot.GetComponent<ISlotView>().Initialize(m_data[i]);
					m_gameObjects.Add(newSlot);
				}
			}

			if (m_createNewPrefab != null)
			{
				if ((m_currentPage + 1) * m_itemsEachPage >= m_data.Count)
				{
					GameObject newSlot = Utilities.AddChild(m_content.transform, m_createNewPrefab);
					newSlot.GetComponent<ISlotView>().Initialize();
					m_gameObjects.Add(newSlot);
				}
			}
			if (m_data.Count == 0)
			{
				if (m_imageLoading != null) m_imageLoading.gameObject.SetActive(true);
				if (m_textLoading != null) m_textLoading.GetComponent<Text>().text = LanguageController.Instance.GetText("message.no.records");
			}
			else
			{
				if (m_imageLoading != null) m_imageLoading.gameObject.SetActive(false);
				if (m_textLoading != null) m_textLoading.gameObject.SetActive(false);
			}
		}

		// -------------------------------------------
		/* 
		 * OnPreviousPressed
		 */
		public void OnPreviousPressed()
		{
			m_currentPage--;
			if (m_currentPage < 0) m_currentPage = 0;
			LoadCurrentPage();
		}

		// -------------------------------------------
		/* 
		 * OnNextPressed
		 */
		public void OnNextPressed()
		{
			m_currentPage++;
			if (m_currentPage * m_itemsEachPage >= m_data.Count) m_currentPage--;
			LoadCurrentPage();
		}

		// -------------------------------------------
		/* 
		 * CheckSlotExisting
		 */
		public bool CheckSlotExisting(GameObject _slot)
		{
			if (_slot == null) return false;

			for (int i = 0; i < m_gameObjects.Count; i++)
			{
				if (_slot == m_gameObjects[i])
				{
					return true;
				}
			}
			return false;
		}
			
	}
}