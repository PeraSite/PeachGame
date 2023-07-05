using System.Collections.Generic;
using System.Linq;
using PeachGame.Common.Packets.Client;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeachGame.Client.Behaviour {
	public class PeachSelector : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
		[Header("컴포넌트")]
		[SerializeField] private Image _selectBoxImage;
		[SerializeField] private RectTransform _canvas;

		[Header("네트워킹")]
		[SerializeField] private float _broadcastTime = 0.1f;

		private List<Peach> _peaches;
		private bool _isDragging;
		private Vector2 _startPosition;
		private Rect _selectRect;
		private float _timer;

		private void Start() {
			_selectBoxImage.gameObject.SetActive(false);
			_peaches = new List<Peach>();
			var peaches = FindObjectsOfType<Peach>();
			foreach (var peach in peaches) {
				_peaches.Add(peach);
			}
		}

		private void Update() {
			_timer += Time.deltaTime;
			if (_timer >= _broadcastTime) {
				_timer = 0f;
				Tick();
			}
		}

		private void Tick() {
			// 선택 사각형 크기 Broadcast
			var canvasScale = _canvas.localScale;

			NetworkManager.Instance.SendPacket(new ClientSelectRangePacket(
				NetworkManager.Instance.ClientId, _isDragging,
				_selectRect.xMin / canvasScale.x,
				_selectRect.xMax / canvasScale.x,
				_selectRect.yMin / canvasScale.y,
				_selectRect.yMax / canvasScale.y
			));
		}

#region Selection Logic
		public void OnPointerDown(PointerEventData eventData) {
			_startPosition = eventData.position;
			_selectRect = new Rect();
		}

		public void OnBeginDrag(PointerEventData eventData) {
			_selectBoxImage.gameObject.SetActive(true);
			_isDragging = true;
		}

		public void OnDrag(PointerEventData eventData) {
			if (eventData.position.x < _startPosition.x) {
				_selectRect.xMin = eventData.position.x;
				_selectRect.xMax = _startPosition.x;
			} else {
				_selectRect.xMin = _startPosition.x;
				_selectRect.xMax = eventData.position.x;
			}

			if (eventData.position.y < _startPosition.y) {
				_selectRect.yMin = eventData.position.y;
				_selectRect.yMax = _startPosition.y;
			} else {
				_selectRect.yMin = _startPosition.y;
				_selectRect.yMax = eventData.position.y;
			}

			// 선택 사각형 이미지 크기 변경
			var canvasScale = _canvas.localScale;
			_selectBoxImage.rectTransform.offsetMin = _selectRect.min / canvasScale;
			_selectBoxImage.rectTransform.offsetMax = _selectRect.max / canvasScale;

			// 범위 내의 복숭아 선택/선택 해제
			foreach (var peach in _peaches) {
				var peachPosition = peach.transform.position;
				if (_selectRect.Contains(peachPosition)) {
					peach.Select();
				} else {
					peach.Deselect();
				}
			}
		}

		public void OnEndDrag(PointerEventData eventData) {
			_selectBoxImage.gameObject.SetActive(false);

			// 선택한 복숭아 찾기
			List<Peach> selectedPeaches = _peaches.Where(x => _selectRect.Contains(x.transform.position) && !x.IsDeleted).ToList();
			(int x, int y)[] selectedPeachPositions = selectedPeaches.Select(x => x.Position).ToArray();

			// 최소 2개는 선택해야 점수 획득 가능 => 별도 체크 
			if (selectedPeaches.Count >= 2) {
				// 선택한 복숭아 삭제 요청 패킷 전송
				NetworkManager.Instance.SendPacket(new ClientRequestDragPacket(selectedPeachPositions));
			}

			// 선택 해제
			selectedPeaches.ForEach(x => x.Deselect());

			_isDragging = false;
		}
  #endregion

#if UNITY_EDITOR
		[ContextMenu("Find Optimal Position")]
		private void FindOptimalPosition() {
			var currentPeach = _peaches.First(x => !x.IsDeleted);
			var targetNumber = 10 - currentPeach.Number;
			var targetPeach = _peaches.First(x => !x.IsDeleted && x.Number == targetNumber);

			Debug.Log(currentPeach.Position);
			Debug.Log(targetPeach.Position);

			NetworkManager.Instance.SendPacket(new ClientRequestDragPacket(new[] {currentPeach.Position, targetPeach.Position}));
		}
#endif
	}
}
