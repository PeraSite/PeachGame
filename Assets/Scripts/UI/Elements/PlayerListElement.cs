using PeachGame.Common.Models;
using TMPro;
using UnityEngine;

namespace PeachGame.Client.UI.Elements {
	public class PlayerListElement : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI _nicknameText;

		public void Setup(PlayerInfo playerInfo) {
			_nicknameText.text = $"{(playerInfo.IsOwner ? "★ " : "")} {playerInfo.Nickname}";
			_nicknameText.fontStyle = playerInfo.IsOwner ? FontStyles.Bold : FontStyles.Normal;
		}
	}
}
