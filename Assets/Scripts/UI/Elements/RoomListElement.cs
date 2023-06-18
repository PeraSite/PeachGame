using PeachGame.Common.Models;
using PeachGame.Common.Packets.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeachGame.Client.UI.Elements {
	public class RoomListElement : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI _roomNameText;
		[SerializeField] private TextMeshProUGUI _playersText;
		[SerializeField] private TextMeshProUGUI _stateText;
		[SerializeField] private Button _joinButton;

		public void Setup(RoomInfo info) {
			_roomNameText.text = info.Name;
			_playersText.text = $"플레이어 : {info.CurrentPlayers}/{info.MaxPlayers}";

			var state = info.State switch {
				RoomState.Waiting => "대기",
				RoomState.Playing => "플레이",
				RoomState.Ending => "종료",
				_ => "알 수 없음"
			};
			_stateText.text = $"상태 : {state}";

			_joinButton.onClick.AddListener(() => {
				if (info.CurrentPlayers >= info.MaxPlayers) {
					Debug.LogError("방이 꽉 찼습니다.");
					return;
				}

				if (info.State != RoomState.Waiting) {
					Debug.LogError("방이 대기 상태가 아닙니다.");
					return;
				}

				NetworkManager.Instance.SendPacket(new ClientRequestJoinRoomPacket(info.RoomId));
			});
		}
	}
}
