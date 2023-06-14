using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Peach : MonoBehaviour {
	[Title("Components")]
	[SerializeField] private TextMeshProUGUI _numberText;
	[SerializeField] private Image _peachImage;
	private CanvasGroup _canvasGroup;

	[Title("Sprites")]
	[SerializeField] private Sprite _defaultSprite;
	[SerializeField] private Sprite _selectedSprite;

	private bool _isSelected;

	public int Number { get; private set; }

	private void Awake() {
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	public void Init(int number) {
		_numberText.text = number.ToString();
		Number = number;
	}

	public void Select() {
		if (_isSelected) return;
		_isSelected = true;
		_peachImage.sprite = _selectedSprite;
	}

	public void Deselect() {
		if (!_isSelected) return;

		_isSelected = false;
		_peachImage.sprite = _defaultSprite;
	}

	public void Delete() {
		_canvasGroup.alpha = 0f;
	}
}
