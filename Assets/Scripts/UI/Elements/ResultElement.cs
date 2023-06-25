using TMPro;
using UnityEngine;

namespace PeachGame.Client.UI.Elements {
	public class ResultElement : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI _nicknameText;
		[SerializeField] private TextMeshProUGUI _scoreText;

		public void Setup(string nickname, int score) {
			_nicknameText.text = nickname;
			_scoreText.text = $"{score}점";
		}
	}
}
