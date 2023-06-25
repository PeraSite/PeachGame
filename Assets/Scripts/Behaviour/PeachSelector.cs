﻿using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeachGame.Client.Behaviour {
	public class PeachSelector : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
		[SerializeField] private Image _selectBoxImage;
		[SerializeField] private RectTransform _canvas;
		[SerializeField] private TextMeshProUGUI _scoreText;

		private List<Peach> _peaches;
		private Vector2 _startPosition;
		private Rect _selectRect;

		private void Awake() {
			_selectBoxImage.gameObject.SetActive(false);
			_peaches = new List<Peach>();
		}

		private void Start() {
			var peaches = FindObjectsOfType<Peach>();
			foreach (var peach in peaches) {
				_peaches.Add(peach);
			}
		}

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

			var selectedPeaches = _peaches.Where(x => _selectRect.Contains(x.transform.position)).ToList();
			var number = selectedPeaches.Sum(x => x.Number);

			if (number == 10) {
				selectedPeaches.ForEach(x => x.Delete());
			} else {
				selectedPeaches.ForEach(x => x.Deselect());
			}
		}
	}
}
