using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PeachGenerator : MonoBehaviour {
	[Title("Generation")]
	[SerializeField] private Peach _peachPrefab;
	[SerializeField] private Transform _peachGrid;
	[SerializeField] private int _peachCount;

	private Dictionary<Peach, int> _peaches;

	private void Awake() {
		_peaches = new Dictionary<Peach, int>();
		for (int i = 1; i <= _peachCount; i++) {
			var peach = Instantiate(_peachPrefab, _peachGrid);
			peach.name = $"Peach {i}";

			var number = Random.Range(1, 9);
			peach.Init(number);
			_peaches[peach] = number;
		}
	}
}
