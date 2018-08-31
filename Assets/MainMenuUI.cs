using System;
using UnityEngine;
using UnityEngine.Events;
using Face.Networking;
using Face.Utilities;

namespace Face.UI
{
	//Page in menu to return to
	public enum MenuPage
	{
		Home,
		Lobby,
	}

	public class MainMenuUI : Singleton<MainMenuUI>
	{

		#region Static config

		public static MenuPage s_ReturnPage;

		#endregion

		#region Fields

		[SerializeField]
		protected CanvasGroup m_DefaultPanel;
		[SerializeField]
		protected CanvasGroup m_CreateGamePanel;
		[SerializeField]
		protected CanvasGroup m_LobbyPanel;
		[SerializeField]
		protected CanvasGroup m_ServerListPanel;
		[SerializeField]
		protected LobbyInfoPanel m_InfoPanel;
		[SerializeField]
		protected LobbyPlayerList m_PlayerList;
		[SerializeField]
		protected GameObject m_QuitButton;

		private CanvasGroup m_CurrentPanel;

		private Action m_WaitTask;
		private bool m_ReadyToFireTask;

		#endregion

		public LobbyPlayerList playerList
		{
			get
			{
				return m_PlayerList;
			}
		}

		#region Methods

		// Use this for initialization
		void Start ()
		{
			LoadingModal modal = LoadingModal.s_Instance;
			if (modal != null) {
				modal.FadeOut ();
			}

			if (m_QuitButton != null) {
				m_QuitButton.SetActive (true);
			} else {
				Debug.LogError ("Missing quitButton from MainMenuUI");
			}

			//Used to return to correct page on return to menu
			switch (s_ReturnPage) {
			case MenuPage.Home:
			default:
				ShowDefaultPanel ();
				break;
			case MenuPage.Lobby:
				ShowLobbyPanel ();
				break;
			}
		}
	
		// Update is called once per frame
		void Update ()
		{
			if (m_ReadyToFireTask) {
				bool canFire = false;

				LoadingModal modal = LoadingModal.s_Instance;
				if (modal != null && modal.readyToTransition) {
					modal.FadeOut ();
					canFire = true;
				} else if (modal == null) {
					canFire = true;
				}

				if (canFire) {
					if (m_WaitTask != null) {
						m_WaitTask ();
						m_WaitTask = null;
					}

					m_ReadyToFireTask = false;
				}
			}
		}

		//Convenience function for showing panels
		public void ShowPanel (CanvasGroup newPanel)
		{
			if (m_CurrentPanel != null) {
				m_CurrentPanel.gameObject.SetActive (false);
			}

			m_CurrentPanel = newPanel;
			if (m_CurrentPanel != null) {
				m_CurrentPanel.gameObject.SetActive (true);
			}
		}

		public void ShowDefaultPanel ()
		{
			ShowPanel (m_DefaultPanel);
		}

		public void ShowLobbyPanel ()
		{
			ShowPanel (m_LobbyPanel);
		}

		public void ShowLobbyPanelForConnection ()
		{
			ShowPanel (m_LobbyPanel);
			NetworkManager.s_Instance.gameModeUpdated -= ShowLobbyPanelForConnection;
			HideInfoPopup ();
		}

		public void ShowServerListPanel ()
		{
			ShowPanel (m_ServerListPanel);
		}

		/// <summary>
		/// Shows the info popup with a callback
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="callback">Callback.</param>
		public void ShowInfoPopup(string label, UnityAction callback)
		{
			if (m_InfoPanel != null)
			{
				m_InfoPanel.Display(label, callback, true);
			}
		}

		public void ShowInfoPopup(string label)
		{
			if (m_InfoPanel != null)
			{
				m_InfoPanel.Display(label, null, false);
			}
		}

		public void ShowConnectingModal (bool reconnectMatchmakingClient)
		{
			ShowInfoPopup ("Connecting...", () => {
				if (NetworkManager.s_InstanceExists) {
					if (reconnectMatchmakingClient) {
						NetworkManager.s_Instance.Disconnect ();
						NetworkManager.s_Instance.StartMatchingmakingClient ();
					} else {
						NetworkManager.s_Instance.Disconnect ();
					}
				}
			});
		}

		public void HideInfoPopup()
		{
			if (m_InfoPanel != null)
			{
				m_InfoPanel.gameObject.SetActive(false);
			}
		}

		/// Wait for network to disconnect before performing an action
		public void DoIfNetworkReady (Action task)
		{
			if (task == null) {
				throw new ArgumentNullException ("task");
			}

			NetworkManager netManager = NetworkManager.s_Instance;

			if (netManager.isNetworkActive) {
				m_WaitTask = task;

				LoadingModal modal = LoadingModal.s_Instance;
				if (modal != null) {
					modal.FadeIn ();
				}

				m_ReadyToFireTask = false;
				netManager.clientStopped += OnClientStopped;
			} else {
				task ();
			}
		}

		//Event listener
		private void OnClientStopped ()
		{
			NetworkManager netManager = NetworkManager.s_Instance;
			netManager.clientStopped -= OnClientStopped;
			m_ReadyToFireTask = true;
		}

		private void GoToFindGamePanel ()
		{
			ShowServerListPanel ();
			NetworkManager.s_Instance.StartMatchingmakingClient ();
		}

		private void GoToCreateGamePanel ()
		{
			ShowPanel (m_CreateGamePanel);
		}

		#endregion


		#region Button events

		public void OnCreateGameClicked ()
		{
			DoIfNetworkReady (GoToCreateGamePanel);
		}

		public void OnFindGameClicked ()
		{
			// Set network into matchmaking search mode
			DoIfNetworkReady (GoToFindGamePanel);
		}

		public void OnQuitGameClicked ()
		{
			Debug.LogError ("Quitting");
			Application.Quit ();
		}

		#endregion
	}
}