using Unity.Networking.Transport;

namespace Net.NetMassage
{
    public class NetKeepAlive : NetMessage
    {
        public NetKeepAlive()
        {
            Code = Opcode.KEEP_ALIVE;
        }
        public NetKeepAlive(DataStreamReader dataStreamReader)
        {
            Code = Opcode.KEEP_ALIVE;
            Desirialize(dataStreamReader);
        }

        public override void Serialize(ref DataStreamWriter dataStreamWriter)
        {
            dataStreamWriter.WriteByte((byte) Code);
        }

        public override void Desirialize(DataStreamReader dataStreamReader)
        {
        
        }

        public override void RecivedOnClient(NetworkConnection networkConnection)
        {
            NetUtility.CKeepAlive?.Invoke(this);
        }

        public override void RecivedOnServer()
        {
            NetUtility.SKeepAlive?.Invoke(this);
        }
    }
}