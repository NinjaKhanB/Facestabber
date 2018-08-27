using UnityEngine;
using NetworkPlayer = Face.Networking.NetworkPlayer;

namespace Face.UI
{
	/// <summary>
	/// Interface to source colours for a given player or team from the server.
	/// </summary>
	public interface IColorProvider
	{
		Color ServerGetColor(NetworkPlayer player);
		void Reset();
	}
}