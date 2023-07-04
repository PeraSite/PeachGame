using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PeachGame.Client.Behaviour {
	public class ReplicatedPeachSelector : MonoBehaviour {
		[Header("컴포넌트")]
		[SerializeField] private Image _selectBoxImage;

		private Rect _selectRect;

		private void Start() {
			_selectRect = Rect.zero;
			_selectBoxImage.gameObject.SetActive(false);
		}

		private void OnDestroy() {
			_selectBoxImage.rectTransform.DOKill();
		}

		public void SetState(float minX, float maxX, float minY, float maxY, bool active) {
			// 주어진 값으로 Rect 변경
			_selectRect = Rect.MinMaxRect(minX, minY, maxX, maxY);

			// 선택 박스 이미지 크기 변경
			RectTransform rect = _selectBoxImage.rectTransform;
			if (!_selectBoxImage.gameObject.activeInHierarchy && active) {
				// 새롭게 드래그했다면 즉시 이동
				rect.offsetMin = _selectRect.min;
				rect.offsetMax = _selectRect.max;
			} else {
				// 이전 애니메이션 캔슬하고 이동
				rect.DOKill();
				rect.DOAnchorPos(_selectRect.position, 0.1f);
				rect.DOSizeDelta(_selectRect.size, 0.1f);
			}

			// 드래그 중 상태 값으로 Active 설정
			_selectBoxImage.gameObject.SetActive(active);

		}
	}
}
