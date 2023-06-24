using System.Collections.Generic;
using PeachGame.Client.UI.Elements;
using PeachGame.Common.Models;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeachGame.Client.UI {
	public class LobbyUI : MonoBehaviour,
		IPacketHandler<ServerRoomStatePacket>,
		IPacketHandler<ServerResponseQuitRoomPacket>,
		IPacketHandler<ClientChatPacket>,
		IPacketHandler<ServerLobbyAnnouncePacket> {
		[Header("방 정보")]
		[SerializeField] private TextMeshProUGUI _roomNameText;
		[SerializeField] private TextMeshProUGUI _playerCountText;
		[SerializeField] private TextMeshProUGUI _stateText;

		[Header("플레이어 목록")]
		[SerializeField] private PlayerListElement _playerListElementPrefab;
		[SerializeField] private RectTransform _playerListElementParent;

		[Header("채팅")]
		[SerializeField] private TextMeshProUGUI _chatLog;
		[SerializeField] private TMP_InputField _chatInput;

		private List<PlayerListElement> _instantiatedPlayerListElements;

		private void Awake() {
			_instantiatedPlayerListElements = new List<PlayerListElement>();
			_chatInput.onSubmit.AddListener(_ => OnChatSend());
		}

		private void OnEnable() {
			NetworkManager.Instance.RegisterPacketHandler<ServerRoomStatePacket>(this);
			NetworkManager.Instance.RegisterPacketHandler<ServerResponseQuitRoomPacket>(this);
			NetworkManager.Instance.RegisterPacketHandler<ClientChatPacket>(this);
			NetworkManager.Instance.RegisterPacketHandler<ServerLobbyAnnouncePacket>(this);
		}

		private void OnDisable() {
			NetworkManager.Instance.UnregisterPacketHandler<ServerRoomStatePacket>(this);
			NetworkManager.Instance.UnregisterPacketHandler<ServerResponseQuitRoomPacket>(this);
			NetworkManager.Instance.UnregisterPacketHandler<ClientChatPacket>(this);
			NetworkManager.Instance.UnregisterPacketHandler<ServerLobbyAnnouncePacket>(this);
		}

		public void OnStartButton() {
			NetworkManager.Instance.SendPacket(new ClientRequestStartPacket());
		}

		public void OnQuitButton() {
			NetworkManager.Instance.SendPacket(new ClientRequestQuitRoomPacket(NetworkManager.Instance.CurrentRoomId));
		}

		public void Handle(ServerRoomStatePacket packet) {
			RoomInfo info = packet.RoomInfo;
			_roomNameText.text = $"이름 : {info.Name}";
			_playerCountText.text = $"플레이어 : {info.CurrentPlayers}/{info.MaxPlayers}";

			var state = info.State switch {
				RoomState.Waiting => "대기",
				RoomState.Playing => "플레이",
				RoomState.Ending => "종료",
				_ => "알 수 없음"
			};
			_stateText.text = $"상태 : {state}";

			// 플레이어 목록 업데이트
			_instantiatedPlayerListElements.ForEach(x => Destroy(x.gameObject));
			_instantiatedPlayerListElements.Clear();
			info.Players.ForEach(playerInfo => {
				var element = Instantiate(_playerListElementPrefab, _playerListElementParent);
				element.Setup(playerInfo);
				_instantiatedPlayerListElements.Add(element);
			});
		}

		public void Handle(ServerResponseQuitRoomPacket packet) {
			SceneManager.LoadScene("RoomList");
		}

		public void OnChatSend() {
			var text = _chatInput.text;
			if (string.IsNullOrEmpty(text)) {
				return;
			}

			NetworkManager.Instance.SendPacket(new ClientChatPacket(NetworkManager.Instance.Nickname, text));
			_chatInput.text = "";
			_chatInput.ActivateInputField();
		}

		public void Handle(ClientChatPacket packet) {
			_chatLog.text += $"{packet.Nickname} : {packet.Message}\n";
		}

		public void Handle(ServerLobbyAnnouncePacket packet) {
			_chatLog.text += $"<color=#E3C565>{packet.Message}</color>\n";
		}
	}
}
