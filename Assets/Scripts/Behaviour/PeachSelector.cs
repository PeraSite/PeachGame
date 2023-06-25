using System.Collections.Generic;
using System.Linq;
using PeachGame.Common.Packets.Client;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeachGame.Client.Behaviour {
	public class PeachSelector : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
		[SerializeField] private Image _selectBoxImage;
		[SerializeField] private RectTransform _canvas;

		private List<Peach> _peaches;
		private Vector2 _startPosition;
		private Rect _selectRect;

		private void Start() {
			_selectBoxImage.gameObject.SetActive(false);
			_peaches = new List<Peach>();
			var peaches = FindObjectsOfType<Peach>();
			foreach (var peach in peaches) {
				_peaches.Add(peach);
			}
		}

#region Selection Logic
		public void OnPointerDown(PointerEventData eventData) {
			_startPosition = eventData.position;
			_selectRect = new Rect();
		}

		public void OnBeginDrag(PointerEventData eventData) {
			_selectBoxImage.gameObject.SetActive(true);
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

			var canvasScale = _canvas.localScale;
			_selectBoxImage.rectTransform.offsetMin = _selectRect.min / canvasScale;
			_selectBoxImage.rectTransform.offsetMax = _selectRect.max / canvasScale;

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
			List<Peach> selectedPeaches = _peaches.Where(x => _selectRect.Contains(x.transform.position)).ToList();
			(int x, int y)[] selectedPeachPositions = selectedPeaches.Select(x => x.Position).ToArray();

			// 최소 2개는 선택해야 점수 획득 가능 => 별도 체크 
			if (selectedPeaches.Count >= 2) {
				// 선택한 복숭아 삭제 요청 패킷 전송
				NetworkManager.Instance.SendPacket(new ClientRequestDragPacket(selectedPeachPositions));
			}

			// 선택 해제
			selectedPeaches.ForEach(x => x.Deselect());
		}
  #endregion
	}
}
