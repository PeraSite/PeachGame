using System.Collections.Generic;
using PeachGame.Client.UI.Elements;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Packets.Server;
using TMPro;
using UnityEngine;

namespace PeachGame.Client.UI {
	public class RoomListUI : MonoBehaviour, IPacketHandler<ServerResponseRoomListPacket> {
		[Header("방 목록")]
		[SerializeField] private RoomListElement _roomListElementPrefab;
		[SerializeField] private RectTransform _roomListParent;


		[Header("닉네임 설정")]
		[SerializeField] private TMP_InputField _nicknameInput;

		private void Awake() {
			_roomListElements = new List<RoomListElement>();
		}

		private void Start() {
			SetupNickname();
			RefreshRoomList();
		}

		private void OnEnable() {
			NetworkManager.Instance.RegisterPacketHandler(this);
		}

		private void OnDisable() {
			NetworkManager.Instance.UnregisterPacketHandler(this);
		}

#region 닉네임 설정
		private void SetupNickname() {
			// Join 씬에서 정한 닉네임 설정
			_nicknameInput.text = NetworkManager.Instance.Nickname;
		}

		public void UpdateNickname() {
			// 새로운 닉네임으로 값 업데이트
			NetworkManager.Instance.Nickname = _nicknameInput.text;
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
				roomListElement.Setup(roomInfo);
				_roomListElements.Add(roomListElement);
			});
		}
#endregion
	}
}
