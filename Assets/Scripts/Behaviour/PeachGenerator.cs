using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

namespace PeachGame.Client.Behaviour {
	public class PeachGenerator : MonoBehaviour {
		[Title("Generation")]
		[SerializeField] private Peach _peachPrefab;
		[SerializeField] private Transform _peachGrid;
		[SerializeField] private int _peachCount;

		private Dictionary<Peach, int> _peaches;

		private void Awake() {
			_peaches = new Dictionary<Peach, int>();
			var random = new Random(NetworkManager.Instance.RandomSeed);

			for (int i = 1; i <= _peachCount; i++) {
				var peach = Instantiate(_peachPrefab, _peachGrid);
				peach.name = $"Peach {i}";

				var number = random.Next(0, 9);
				peach.Init(number);
				_peaches[peach] = number;
			}
		}
	}
}
