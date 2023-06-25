using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using PeachGame.Common.Packets;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Serialization;
using UnityEngine;

namespace PeachGame.Client {
	public class NetworkManager : MonoBehaviour {
		public static NetworkManager Instance { get; private set; }

		[Header("네트워킹 관련")]
		[SerializeField] private string _ip = "127.0.0.1";
		[SerializeField] private int _port = 9000;

		// 네트워킹 객체
		private TcpClient _client;
		private NetworkStream _stream;
		private BinaryReader _reader;
		private BinaryWriter _writer;

		// 패킷 처리 Queue
		private ConcurrentQueue<IPacket> _packetQueue;

		// 로컬 클라이언트 값
		public string Nickname { get; set; }
		public Guid ClientId { get; set; }
		public int CurrentRoomId { get; set; }
		public int RandomSeed { get; set; }

		// 패킷 처리
		public static event Action<IPacket> OnPacketReceived;
		private Dictionary<(Type, Type), Action<IPacket>> _packetHandlerCache;

#region Unity Lifecycle
		private void Awake() {
			// Singleton 로직
			if (Instance != null && Instance != this) {
				Destroy(this.gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(this.gameObject);

			// 네트워킹 변수 초기화
			_client = new TcpClient();
			_packetQueue = new ConcurrentQueue<IPacket>();
			_packetHandlerCache = new Dictionary<(Type, Type), Action<IPacket>>();

			// 로컬 값 초기화
			Nickname = string.Empty;
			ClientId = Guid.Empty;
			CurrentRoomId = -1;
		}

		private void OnDestroy() {
			_reader?.Close();
			_writer?.Close();
			_stream?.Close();
			_client?.Close();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStatic() {
			OnPacketReceived = null;
			Instance = null;
		}

		private void Update() {
			if (_packetQueue.TryDequeue(out var incomingPacket)) {
				Debug.Log($"[S -> C] {incomingPacket}");
				HandlePacket(incomingPacket);
			}
		}
#endregion

#region Packet Handling
		private void HandlePacket(IPacket incomingPacket) {
			OnPacketReceived?.Invoke(incomingPacket);
		}

		public void RegisterPacketHandler<T>(IPacketHandler<T> handler) where T : IPacket {
			void Action(IPacket packet) {
				if (packet is T tPacket) {
					handler.Handle(tPacket);
				}
			}

			(Type handlerType, Type packetType) handlingType = (handler.GetType(), typeof(T));
			if (_packetHandlerCache.ContainsKey(handlingType)) {
				throw new Exception($"Can't register already registered handler: {handlingType.handlerType} - {handlingType.packetType}");
			}

			_packetHandlerCache[handlingType] = Action;
			OnPacketReceived += Action;
		}

		public void UnregisterPacketHandler<T>(IPacketHandler<T> handler) where T : IPacket {
			(Type handlerType, Type packetType) handlingType = (handler.GetType(), typeof(T));
			if (!_packetHandlerCache.ContainsKey(handlingType)) {
				throw new Exception($"Can't register not registered handler: {handlingType.handlerType} - {handlingType.packetType}");
			}

			Action<IPacket> action = _packetHandlerCache[(handler.GetType(), typeof(T))];
			OnPacketReceived -= action;
		}
#endregion

		public async UniTask JoinServer(string nickname) {
			if (_client.Connected) {
				Debug.Log("Can't join twice!");
				return;
			}

			await _client.ConnectAsync(IPAddress.Parse(_ip), _port);
			_stream = _client.GetStream();
			_writer = new BinaryWriter(_stream);
			_reader = new BinaryReader(_stream);

			ClientId = Guid.NewGuid();

			SendPacket(new ClientPingPacket(ClientId, nickname));

			while (_client.Connected) {
				try {
					var packetID = _reader.BaseStream.ReadByte();

					// 읽을 수 없다면(데이터가 끝났다면 리턴)
					if (packetID == -1) break;

					var packetType = (PacketType)packetID;

					// 타입에 맞는 패킷 객체 생성 후 큐에 추가
					var basePacket = packetType.CreatePacket(_reader);
					_packetQueue.Enqueue(basePacket);
				}
				catch (Exception) {
					break;
				}
			}
		}

		public void SendPacket(IPacket packet) {
			if (!_client.Connected) {
				Debug.LogError("서버에 연결되지 않았습니다!");
				return;
			}
			Debug.Log($"[C -> S] {packet}");

			_writer.Write(packet);
		}
	}
}
