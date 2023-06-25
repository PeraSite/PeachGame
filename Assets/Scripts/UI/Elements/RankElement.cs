using TMPro;
using UnityEngine;

namespace PeachGame.Client.UI.Elements {
	public class RankElement : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI _rankText;
		[SerializeField] private TextMeshProUGUI _nicknameText;
		[SerializeField] private TextMeshProUGUI _scoreText;

		public void Set(int rank, string nickname, int score) {
			_rankText.text = rank switch {
				1 => "1st",
				2 => "2nd",
				3 => "3rd",
				4 => "4th",
				_ => $"{rank}th"
			};

			_nicknameText.text = nickname;
			_scoreText.text = $"{score}점";
		}
	}
}
