﻿using System.Collections.Generic;
using PeachGame.Client.UI.Elements;
using PeachGame.Client.Utils;
using PeachGame.Common.Models;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PeachGame.Client.UI {
	public class LobbyUI : MonoBehaviour,
		IPacketHandler<ServerRoomStatePacket>,
		IPacketHandler<ServerResponseQuitRoomPacket>,
		IPacketHandler<ClientChatPacket>,
		IPacketHandler<ServerLobbyAnnouncePacket>,
		IPacketHandler<ServerResponseStartPacket> {
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

		[Header("방장")]
		[SerializeField] private Button _startButton;

		private List<PlayerListElement> _instantiatedPlayerListElements;

		private void Start() {
			_instantiatedPlayerListElements = new List<PlayerListElement>();
			_chatInput.onSubmit.AddListener(_ => OnChatSend());
		}

		private void OnEnable() {
			this.RegisterPacketHandler<ServerRoomStatePacket>();
			this.RegisterPacketHandler<ServerResponseQuitRoomPacket>();
			this.RegisterPacketHandler<ClientChatPacket>();
			this.RegisterPacketHandler<ServerLobbyAnnouncePacket>();
			this.RegisterPacketHandler<ServerResponseStartPacket>();
		}

		private void OnDisable() {
			this.UnregisterPacketHandler<ServerRoomStatePacket>();
			this.UnregisterPacketHandler<ServerResponseQuitRoomPacket>();
			this.UnregisterPacketHandler<ClientChatPacket>();
			this.UnregisterPacketHandler<ServerLobbyAnnouncePacket>();
			this.UnregisterPacketHandler<ServerResponseStartPacket>();
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

			// 방장 정보 확인 후 시작 버튼 여부 변경
			_startButton.interactable = packet.RoomInfo.Owner == NetworkManager.Instance.ClientId;
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
			_chatLog.text += $"<color=#FF6E26>{packet.Message}</color>\n";
		}

		public void Handle(ServerResponseStartPacket packet) {
			// 공유 시드 값 설정
			NetworkManager.Instance.RandomSeed = packet.Seed;
			SceneManager.LoadScene("Play");
		}
	}
}
