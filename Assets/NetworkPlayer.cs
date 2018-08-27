using System;
using UnityEngine;
using UnityEngine.Networking;
using LobbyPlayer = Face.UI.LobbyPlayer;
using Face.UI;

namespace Face.Networking
{
	public class NetworkPlayer : NetworkBehaviour
	{
		public event Action<NetworkPlayer> syncVarsChanged;
		// Server only event
		public event Action<NetworkPlayer> becameReady;

		public event Action gameDetailsReady;

		[SerializeField]
		protected GameObject m_StatePrefab;
		[SerializeField]
		protected GameObject m_LobbyPrefab;

		// Set by commands
		[SyncVar(hook = "OnMyName")]
		private string m_PlayerName = "";
		[SyncVar(hook = "OnMyColor")]
		private Color m_PlayerColor = Color.clear;
		[SyncVar(hook = "OnReadyChanged")]
		private bool m_Ready = false;

		// Set on the server only
		[SyncVar(hook = "OnHasInitialized")]
		private bool m_Initialized = false;
		[SyncVar]
		private int m_PlayerId;

		private IColorProvider m_ColorProvider = null;
		private NetworkManager m_NetManager;

		private bool lateSetupOfClientPlayer = false;

		/// <summary>
		/// Gets this player's id
		/// </summary>
		public int playerId
		{
			get { return m_PlayerId; }
		}

		/// <summary>
		/// Gets this player's name
		/// </summary>
		public string playerName
		{
			get { return m_PlayerName; }
		}

		/// <summary>
		/// Gets this player's colour
		/// </summary>
		public Color color
		{
			get { return m_PlayerColor; }
		}

		/// <summary>
		/// Gets whether this player has marked themselves as ready in the lobby
		/// </summary>
		public bool ready
		{
			get { return m_Ready; }
		}

		/// <summary>
		/// Gets the lobby object associated with this player
		/// </summary>
		public LobbyPlayer lobbyObject
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the local NetworkPlayer object
		/// </summary>
		public static NetworkPlayer s_LocalPlayer
		{
			get;
			private set;
		}

		/// <summary>
		/// Set initial values
		/// </summary>
		[Client]
		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();
			Debug.Log("Local Network Player start");

			s_LocalPlayer = this;
		}

		/// <summary>
		/// Register us with the NetworkManager
		/// </summary>
		[Client]
		public override void OnStartClient()
		{
			DontDestroyOnLoad(this);

			if (m_NetManager == null)
			{
				m_NetManager = NetworkManager.s_Instance;
			}

			base.OnStartClient();
			Debug.Log("Client Network Player start");

			m_NetManager.RegisterNetworkPlayer(this);
		}

		/// <summary>
		/// Get network manager
		/// </summary>
		protected virtual void Start()
		{
			if (m_NetManager == null)
			{
				m_NetManager = NetworkManager.s_Instance;
			}
		}

		/// <summary>
		/// Deregister us with the manager
		/// </summary>
		public override void OnNetworkDestroy()
		{
			base.OnNetworkDestroy();
			Debug.Log("Client Network Player OnNetworkDestroy");

			if (lobbyObject != null)
			{
				Destroy(lobbyObject.gameObject);
			}

			if (m_NetManager != null)
			{
				m_NetManager.DeregisterNetworkPlayer(this);
			}
		}

		/// <summary>
		/// Fired when we enter the game scene
		/// </summary>
		[Client]
		public void OnEnterGameScene()
		{
			if (hasAuthority)
			{
				CmdClientReadyInGameScene();
			}
		}

		/// <summary>
		/// Fired when we return to the lobby scene, or are first created in the lobby
		/// </summary>
		[Client]
		public void OnEnterLobbyScene()
		{
			Debug.Log("OnEnterLobbyScene");
			if (m_Initialized && lobbyObject == null)
			{
				CreateLobbyObject();
			}
		}


		[Server]
		public void ClearReady()
		{
			m_Ready = false;
		}


		[Server]
		public void SetPlayerId(int playerId)
		{
			this.m_PlayerId = playerId;
		}



		/// <summary>
		/// Clean up lobby object for us
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (lobbyObject != null)
			{
				Destroy(lobbyObject.gameObject);
			}
		}


		/// <summary>
		/// Create our lobby object
		/// </summary>
		private void CreateLobbyObject()
		{
			lobbyObject = Instantiate(m_LobbyPrefab).GetComponent<LobbyPlayer>();
			lobbyObject.Init(this);
		}


		[Server]
		private void LazyLoadColorProvider()
		{
			if (m_ColorProvider != null)
			{
				return;
			}

			if (true)
			{
				Debug.Log("Missing mode - assigning PlayerColorProvider by default");
				m_ColorProvider = new PlayerColorProvider();
				return;
			}

		}

		[Server]
		private void SelectColour()
		{
			LazyLoadColorProvider();

			if (m_ColorProvider == null)
			{
				Debug.LogWarning("Could not find color provider");
				return;
			}

			Color newPlayerColor = m_ColorProvider.ServerGetColor(this);

			m_PlayerColor = newPlayerColor;
		}

		[ClientRpc]
		public void RpcPrepareForLoad()
		{
			if (isLocalPlayer)
			{
				// Show loading screen
				LoadingModal loading = LoadingModal.s_Instance;

				if (loading != null)
				{
					loading.FadeIn();
				}
			}
		}

		protected void AddClientToServer()
		{
			Debug.Log("CmdClientReadyInScene");
			GameObject stateObject = Instantiate(m_StatePrefab);
			NetworkServer.SpawnWithClientAuthority(stateObject, connectionToClient);
			if (lateSetupOfClientPlayer)
			{
				lateSetupOfClientPlayer = false;
			}
		}

		#region Commands

		/// <summary>
		/// Create our tank
		/// </summary>
		[Command]
		private void CmdClientReadyInGameScene()
		{
			AddClientToServer();
		}

		[Command]
		private void CmdSetInitialValues(int tankType, int decorationIndex, int decorationMaterial, string newName)
		{
			Debug.Log("CmdChangePlayer");
			m_PlayerName = newName;
			SelectColour();
			m_Initialized = true;
		}

		[Command]
		public void CmdColorChange()
		{
			Debug.Log("CmdColorChange");
			SelectColour();
		}

		[Command]
		public void CmdNameChanged(string name)
		{
			Debug.Log("CmdNameChanged");
			m_PlayerName = name;
		}

		[Command]
		public void CmdSetReady()
		{
			Debug.Log("CmdSetReady");
			if (m_NetManager.hasSufficientPlayers)
			{
				m_Ready = true;

				if (becameReady != null)
				{
					becameReady(this);
				}
			}
		}

		#endregion


		#region Syncvar callbacks

		private void OnMyName(string newName)
		{
			m_PlayerName = newName;

			if (syncVarsChanged != null)
			{
				syncVarsChanged(this);
			}
		}

		private void OnMyColor(Color newColor)
		{
			m_PlayerColor = newColor;

			if (syncVarsChanged != null)
			{
				syncVarsChanged(this);
			}
		}

		private void OnReadyChanged(bool value)
		{
			m_Ready = value;

			if (syncVarsChanged != null)
			{
				syncVarsChanged(this);
			}
		}

		private void OnHasInitialized(bool value)
		{
			if (!m_Initialized && value)
			{
				m_Initialized = value;
				CreateLobbyObject();
			}
		}

		#endregion
	}
}