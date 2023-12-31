﻿using Cysharp.Threading.Tasks;
using PeachGame.Client.Utils;
using PeachGame.Common.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeachGame.Client.UI {
	public class JoinUI : MonoBehaviour, IPacketHandler<ServerPongPacket> {
		[SerializeField] private TMP_InputField _inputField;

		private void OnEnable() {
			this.RegisterPacketHandler();
		}

		private void OnDisable() {
			this.UnregisterPacketHandler();
		}

		public void OnJoinButton() {
			var nickname = _inputField.text;

			if (string.IsNullOrEmpty(nickname)) {
				return;
			}

			UniTask.RunOnThreadPool(() => NetworkManager.Instance.JoinServer(nickname)).Forget();
		}

		public void Handle(ServerPongPacket packet) {
			NetworkManager.Instance.Nickname = _inputField.text;
			NetworkManager.Instance.ClientId = packet.ClientId;
			SceneManager.LoadScene("RoomList");
		}
	}
}
