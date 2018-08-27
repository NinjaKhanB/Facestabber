using UnityEngine;
using UnityEngine.UI;

namespace Face.UI
{
	/// <summary>
	/// Lobby info panel - pop-up modal for LobbyScene
	/// </summary>
	public class LobbyInfoPanel : MonoBehaviour
	{
		[SerializeField]
		protected Text m_InfoText;
		[SerializeField]
		protected Text m_ButtonText;
		[SerializeField]
		protected Button m_SingleButton;

		public void Display(string info, UnityEngine.Events.UnityAction buttonClbk, bool displayButton = true)
		{
			m_InfoText.text = info;

			m_SingleButton.gameObject.SetActive(displayButton);
			m_SingleButton.onClick.RemoveAllListeners();
			if (buttonClbk != null)
			{
				m_SingleButton.onClick.AddListener(buttonClbk);
			}

			m_SingleButton.onClick.AddListener(() =>
				{
					gameObject.SetActive(false);
				});

			gameObject.SetActive(true);
		}
	}
}