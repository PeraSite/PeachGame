using Cysharp.Threading.Tasks;
using PeachGame.Common.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeachGame.Client.UI {
	public class JoinUI : MonoBehaviour, IPacketHandler<ServerPongPacket> {
		[SerializeField] private TMP_InputField _inputField;

		private void OnEnable() {
			NetworkManager.Instance.RegisterPacketHandler(this);
		}

		private void OnDisable() {
			NetworkManager.Instance.UnregisterPacketHandler(this);
		}

		public void OnJoinButton() {
			var nickname = _inputField.text;

			if (string.IsNullOrEmpty(nickname)) {
				return;
			}

			UniTask.RunOnThreadPool(() => NetworkManager.Instance.JoinServer()).Forget();
		}

		public void Handle(ServerPongPacket packet) {
			NetworkManager.Instance.ClientId = packet.ClientId;
			SceneManager.LoadScene("RoomList");
		}
	}
}
