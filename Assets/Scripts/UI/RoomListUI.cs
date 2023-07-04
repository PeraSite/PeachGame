using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PeachGame.Client.UI.Elements;
using PeachGame.Client.Utils;
using PeachGame.Common.Models;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeachGame.Client.UI {
	public class RoomListUI : MonoBehaviour,
		IPacketHandler<ServerResponseRoomListPacket>,
		IPacketHandler<ServerResponseCreateRoomPacket>,
		IPacketHandler<ServerResponseJoinRoomPacket> {
		[Header("방 목록")]
		[SerializeField] private RoomListElement _roomListElementPrefab;
		[SerializeField] private RectTransform _roomListParent;
		[SerializeField] private float _autoRefreshTime = 1f;

		[Header("닉네임 설정")]
		[SerializeField] private TMP_InputField _nicknameInput;

		[Header("방 생성")]
		[SerializeField] private TMP_InputField _roomNameInput;
		[SerializeField] private GameObject _createRoomPanel;

		private void Awake() {
			_roomListElements = new List<RoomListElement>();
			CloseCreateRoomPanel();
		}

		private void Start() {
			SetupNickname();
			RefreshRoomList();
			StartRoomRefreshTimer();
		}

		private void OnEnable() {
			this.RegisterPacketHandler<ServerResponseRoomListPacket>();
			this.RegisterPacketHandler<ServerResponseCreateRoomPacket>();
			this.RegisterPacketHandler<ServerResponseJoinRoomPacket>();
		}

		private void OnDisable() {
			this.UnregisterPacketHandler<ServerResponseRoomListPacket>();
			this.UnregisterPacketHandler<ServerResponseCreateRoomPacket>();
			this.UnregisterPacketHandler<ServerResponseJoinRoomPacket>();
		}

#region 닉네임 설정
		private void SetupNickname() {
			// Join 씬에서 정한 닉네임 설정
			_nicknameInput.text = NetworkManager.Instance.Nickname;
		}

		public void UpdateNickname() {
			var newNickname = _nicknameInput.text;
			if (string.IsNullOrEmpty(newNickname)) {
				return;
			}

			// 새로운 닉네임으로 값 업데이트 후 패킷 전송
			NetworkManager.Instance.Nickname = newNickname;
			NetworkManager.Instance.SendPacket(new ClientPingPacket(NetworkManager.Instance.ClientId, newNickname));
		}
#endregion

#region 방 목록
		private List<RoomListElement> _roomListElements;

		public void RefreshRoomList() {
			NetworkManager.Instance.SendPacket(new ClientRequestRoomListPacket());
		}

		public void Handle(ServerResponseRoomListPacket packet) {
			_roomListElements.ForEach(x => Destroy(x.gameObject));
			_roomListElements.Clear();
			packet.InfoList.ForEach(roomInfo => {
				var roomListElement = Instantiate(_roomListElementPrefab, _roomListParent);
				roomListElement.Setup(roomInfo, JoinRoom);
				_roomListElements.Add(roomListElement);
			});
		}

		private void StartRoomRefreshTimer() {
			RoomRefreshTimer().Forget();
		}

		private async UniTaskVoid RoomRefreshTimer() {
			RefreshRoomList();
			await UniTask.Delay(TimeSpan.FromSeconds(_autoRefreshTime), cancellationToken: this.GetCancellationTokenOnDestroy());
			StartRoomRefreshTimer();
		}
#endregion

#region 방 생성
		public void OpenCreateRoomPanel() {
			_createRoomPanel.SetActive(true);
		}

		public void CloseCreateRoomPanel() {
			_createRoomPanel.SetActive(false);
		}

		public void CreateRoom() {
			var roomName = _roomNameInput.text;

			if (string.IsNullOrWhiteSpace(roomName)) {
				Debug.LogError("방 이름을 입력해주세요.");
				return;
			}

			NetworkManager.Instance.SendPacket(new ClientRequestCreateRoomPacket(roomName));
		}

		public void Handle(ServerResponseCreateRoomPacket packet) {
			NetworkManager.Instance.CurrentRoomId = packet.RoomId;
			SceneManager.LoadScene("Lobby");
		}
#endregion

#region 방 접속
		private void JoinRoom(RoomInfo roomInfo) {
			if (roomInfo.CurrentPlayers >= roomInfo.MaxPlayers) {
				Debug.LogError("방이 꽉 찼습니다.");
				return;
			}

			if (roomInfo.State != RoomState.Waiting) {
				Debug.LogError("방이 대기 상태가 아닙니다.");
				return;
			}

			NetworkManager.Instance.SendPacket(new ClientRequestJoinRoomPacket(roomInfo.RoomId));
		}

		public void Handle(ServerResponseJoinRoomPacket packet) {
			if (packet.Success) {
				NetworkManager.Instance.CurrentRoomId = packet.RoomId;
				SceneManager.LoadScene("Lobby");
			} else {
				Debug.LogError("방 접속에 실패했습니다:" + packet.ErrorMessage);
				NetworkManager.Instance.SendPacket(new ClientRequestRoomListPacket());
			}
		}
  #endregion
	}
}
