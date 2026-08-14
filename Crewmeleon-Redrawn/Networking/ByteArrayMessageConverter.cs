using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Serialization;

namespace CrewmeleonRedrawn.Networking;

[MessageConverter]
public class ByteArrayMessageConverter : MessageConverter<byte[]>
{
    public override void Write(MessageWriter writer, byte[] value) => writer.WriteBytesAndSize(value);
    
    public override byte[] Read(MessageReader reader, Type objectType) => reader.ReadBytesAndSize();
}