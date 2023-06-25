using PeachGame.Common.Packets;

namespace PeachGame.Client.Utils {
	public static class PacketHandlerExtensions {
		public static void RegisterPacketHandler<TPacket>(this IPacketHandler<TPacket> self) where TPacket : IPacket, new() {
			NetworkManager.Instance.RegisterPacketHandler(self);
		}

		public static void UnregisterPacketHandler<TPacket>(this IPacketHandler<TPacket> self) where TPacket : IPacket, new() {
			NetworkManager.Instance.UnregisterPacketHandler(self);
		}
	}
}
