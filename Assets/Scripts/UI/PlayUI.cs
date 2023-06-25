using PeachGame.Client.Utils;
using PeachGame.Common.Models;
using PeachGame.Common.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeachGame.Client.UI {
	public class PlayUI : MonoBehaviour,
		IPacketHandler<ServerRoomStatePacket> {
		private const int PLAY_TIME = 60 * 2; // 2분

		[SerializeField] private TextMeshProUGUI _leftTimeText;
		[SerializeField] private Image _leftTimeBar;

		private void OnEnable() {
			this.RegisterPacketHandler<ServerRoomStatePacket>();
		}

		private void OnDisable() {
			this.UnregisterPacketHandler<ServerRoomStatePacket>();
		}

		public void Handle(ServerRoomStatePacket packet) {
			RoomInfo roomInfo = packet.RoomInfo;

			if (roomInfo.State == RoomState.Ending) {
				Debug.Log("Game Over!");
				return;
			}

			// 남은 시간 업데이트
			_leftTimeText.text = $"{roomInfo.LeftTime}";
			_leftTimeBar.fillAmount = (float)roomInfo.LeftTime / PLAY_TIME;
		}
	}
}
