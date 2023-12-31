﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeachGame.Client.Behaviour {
	public class Peach : MonoBehaviour {
		[Header("Components")]
		[SerializeField] private TextMeshProUGUI _numberText;
		[SerializeField] private Image _peachImage;
		private CanvasGroup _canvasGroup;

		[Header("Sprites")]
		[SerializeField] private Sprite _defaultSprite;
		[SerializeField] private Sprite _selectedSprite;

		private bool _isSelected;
		public bool IsDeleted { get; private set; }

		public int Number { get; private set; }
		public (int x, int y) Position { get; private set; }

		private void Awake() {
			_canvasGroup = GetComponent<CanvasGroup>();
		}

		public void Init(int number, int x, int y) {
			_numberText.text = number.ToString();
			Number = number;
			Position = (x, y);
		}

		public void Select() {
			if (IsDeleted) return;
			if (_isSelected) return;
			_isSelected = true;
			_peachImage.sprite = _selectedSprite;
		}

		public void Deselect() {
			if (IsDeleted) return;
			if (!_isSelected) return;

			_isSelected = false;
			_peachImage.sprite = _defaultSprite;
		}

		public void Delete() {
			_canvasGroup.alpha = 0f;
			IsDeleted = true;
		}
	}
}
