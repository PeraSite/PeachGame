using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PeachGame.Client.Behaviour;
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
	public class PlayUI : MonoBehaviour,
		IPacketHandler<ServerRoomStatePacket>,
		IPacketHandler<ClientSelectRangePacket> {
		private const int PLAY_TIME = 60 * 2; // 2분

		[Header("남은 시간")]
		[SerializeField] private TextMeshProUGUI _leftTimeText;
		[SerializeField] private Image _leftTimeBar;

		[Header("점수 랭킹")]
		[SerializeField] private RankElement _rankElementPrefab;
		[SerializeField] private Transform _rankElementParent;
		private Dictionary<Guid, RankElement> _rankElements;
		[SerializeField] private float _elementWidth = 400f;
		[SerializeField] private float _elementSpacing = 30f;
		[SerializeField] private float _rankAnimationTime = 0.5f;

		[Header("리플리케이션")]
		[SerializeField] private ReplicatedPeachSelector _replicatedSelectorPrefab;
		[SerializeField] private Transform _replicatedSelectorParent;
		private Dictionary<Guid, ReplicatedPeachSelector> _replicatedSelectors;

		[Header("패널")]
		[SerializeField] private GameObject _playPanel;
		[SerializeField] private GameObject _endingPanel;

		[Header("결과")]
		[SerializeField] private ResultElement _resultElementPrefab;
		[SerializeField] private Transform _resultElementParent;

		private void Awake() {
			_rankElements = new Dictionary<Guid, RankElement>();
			_replicatedSelectors = new Dictionary<Guid, ReplicatedPeachSelector>();

			_playPanel.SetActive(true);
			_endingPanel.SetActive(false);
			_leftTimeBar.fillAmount = 1.0f;
		}

		private void OnEnable() {
			this.RegisterPacketHandler<ServerRoomStatePacket>();
			this.RegisterPacketHandler<ClientSelectRangePacket>();
		}

		private void OnDisable() {
			this.UnregisterPacketHandler<ServerRoomStatePacket>();
			this.UnregisterPacketHandler<ClientSelectRangePacket>();
		}

		public void Handle(ServerRoomStatePacket packet) {
			RoomInfo roomInfo = packet.RoomInfo;

			if (roomInfo.State == RoomState.Ending) {
				ShowEnding(roomInfo);
				return;
			}

			// 남은 시간 업데이트
			_leftTimeText.text = $"{roomInfo.LeftTime}";
			_leftTimeBar.DOFillAmount((float)roomInfo.LeftTime / PLAY_TIME, _rankAnimationTime);

			// 랭킹 업데이트 로직
			UpdateRankUI(roomInfo);

			// 리플리케이션 업데이트 로직
			UpdateSelectorUI(roomInfo);
		}

#region 선택 박스 UI
		private void UpdateSelectorUI(RoomInfo roomInfo) {
			// 만약 유저가 나가서 UI랑 플레이어-1(본인 제외)랑 다르다면 리빌딩
			if (roomInfo.Players.Count != _replicatedSelectors.Count) {
				RebuildSelectorUI(roomInfo);
				return;
			}
		}

		private void RebuildSelectorUI(RoomInfo roomInfo) {
			// 기존 프리팹 초기화
			foreach (var selector in _replicatedSelectors.Values) {
				Destroy(selector.gameObject);
			}
			_replicatedSelectors.Clear();

			// 자신을 제외한 캐릭터만 생성
			foreach (PlayerInfo player in roomInfo.Players /*.Where(x => x.Id != NetworkManager.Instance.ClientId)*/) {
				// 프리팹 생성
				var element = Instantiate(_replicatedSelectorPrefab, _replicatedSelectorParent);
				_replicatedSelectors[player.Id] = element;
			}
		}

		public void Handle(ClientSelectRangePacket packet) {
			if (_replicatedSelectors.TryGetValue(packet.ClientId, out var selector)) {
				selector.SetState(packet.MinX, packet.MaxX, packet.MinY, packet.MaxY, packet.Dragging);
			}
		}
#endregion

#region 랭킹 UI
		private void UpdateRankUI(RoomInfo roomInfo) {
			// 한번도 UI를 만들지 않았다면 리빌딩
			if (_rankElements.Count == 0) {
				RebuildRankUI(roomInfo);
				return;
			}

			// 만약 유저가 나가서 UI랑 플레이어랑 숫자가 다르다면 리빌딩
			if (roomInfo.Players.Count != _rankElements.Count) {
				RebuildRankUI(roomInfo);
				return;
			}

			// --- 기존 UI 수정 로직 ---

			// 점수 정렬 - Guid으로, 점수로
			var sortedScores = roomInfo.Score
				.OrderByDescending(x => x.Value)
				.ToList();

			// UI Tween 변수
			var elementDistance = _elementWidth + _elementSpacing;
			var startX = (roomInfo.Players.Count - 1) * -(elementDistance / 2);

			foreach (PlayerInfo player in roomInfo.Players) {
				var id = player.Id;

				// UI에 안그려져있다면 무시(=존재하지 않는 플레이어)
				if (!_rankElements.TryGetValue(id, out var element)) continue;

				// 점수, 순위 계산
				var score = roomInfo.Score.GetValueOrDefault(player.Id, 0);
				var rank = sortedScores.FindIndex(x => x.Key == player.Id);

				// UI 변경
				element.Set(rank + 1, player.Nickname, score);
				var rect = (RectTransform)element.transform;
				rect.DOAnchorPosX(startX + elementDistance * rank, _rankAnimationTime);
			}
		}

		private void RebuildRankUI(RoomInfo roomInfo) {
			// 기존 프리팹 초기화
			foreach (RankElement element in _rankElements.Values) {
				element.transform.DOKill();
				Destroy(element.gameObject);
			}
			_rankElements.Clear();

			// 점수 정렬 - Guid으로, 점수로
			var sortedScores = roomInfo.Score
				.OrderByDescending(x => x.Value)
				.ToList();

			var elementDistance = _elementWidth + _elementSpacing;
			var startX = (roomInfo.Players.Count - 1) * -(elementDistance / 2);

			foreach (PlayerInfo player in roomInfo.Players) {
				// 점수, 순위 계산
				var score = roomInfo.Score.GetValueOrDefault(player.Id, 0);
				var rank = sortedScores.FindIndex(x => x.Key == player.Id);

				// 프리팹 생성
				var element = Instantiate(_rankElementPrefab, _rankElementParent);
				element.Set(rank + 1, player.Nickname, score);

				// 지정된 X 좌표로 이동
				var rect = (RectTransform)element.transform;
				rect.anchoredPosition = new Vector2(startX + elementDistance * rank, rect.anchoredPosition.y);

				_rankElements[player.Id] = element;
			}
		}
  #endregion

		private void ShowEnding(RoomInfo roomInfo) {
			_playPanel.SetActive(false);
			_endingPanel.SetActive(true);

			var sortedScores = roomInfo.Players
				.ToDictionary(x => x.Nickname, x => roomInfo.Score[x.Id])
				.OrderByDescending(x => x.Value)
				.ToList();

			foreach (var (nickname, score) in sortedScores) {
				var go = Instantiate(_resultElementPrefab, _resultElementParent);
				go.Setup(nickname, score);
			}
		}

		public void ReturnLobby() {
			SceneManager.LoadScene("RoomList");
		}
	}
}
