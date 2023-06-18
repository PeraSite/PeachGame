using PeachGame.Common.Packets;

namespace PeachGame.Client {
	public interface IPacketHandler<in T> where T : IPacket {
		void Handle(T packet);
	}
}
