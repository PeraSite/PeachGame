using System;
using System.Collections.Generic;
using PeachGame.Client.Utils;
using PeachGame.Common.Packets.Server;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

namespace PeachGame.Client.Behaviour {
	public class PeachGenerator : MonoBehaviour,
		IPacketHandler<ServerResponseDragPacket> {
		[Title("Generation")]
		[SerializeField] private Peach _peachPrefab;
		[SerializeField] private Transform _peachGrid;
		[SerializeField] private int _peachCount;
		[SerializeField] private int _peachColumn = 20;

		private Dictionary<(int x, int y), Peach> _peaches;

		private void Awake() {
			_peaches = new Dictionary<(int x, int y), Peach>();
			var random = new Random(NetworkManager.Instance.RandomSeed);

			for (var y = 0; y < _peachCount / _peachColumn; y++) {
				for (var x = 0; x < _peachColumn; x++) {
					var number = random.Next(1, 10);
					var peach = Instantiate(_peachPrefab, _peachGrid);
					peach.name = $"Peach - {x}/{y}";

					peach.Init(number, x, y);
					_peaches[(x, y)] = peach;
				}
				Console.WriteLine();
			}
		}


		private void OnEnable() {
			this.RegisterPacketHandler();
		}

		private void OnDisable() {
			this.UnregisterPacketHandler();
		}

		public void Handle(ServerResponseDragPacket packet) {
			(int x, int y)[] positions = packet.Positions;

			foreach (var (x, y) in positions) {
				var peach = _peaches[(x, y)];
				peach.Delete();
			}

			// Guid playerId = packet.PlayerId;
			// TODO: 내가 없앤 것이라면 이펙트?
		}
	}
}
