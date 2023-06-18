using TMPro;
using UnityEngine;

namespace PeachGame.Client.UI {
	public class RoomListUI : MonoBehaviour {
		[SerializeField] private TMP_InputField _nicknameInput;

		private void Start() {
			_nicknameInput.text = NetworkManager.Instance.Nickname;
		}
	}
}
