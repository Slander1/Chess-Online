using Unity.Networking.Transport;

namespace Net.NetMassage
{
    public class NetRematch : NetMessage
    {
        public int teamId;
        public byte wantRematch;
        
        public NetRematch()
        {
            Code = Opcode.REMATCH;
        }
        public NetRematch(DataStreamReader dataStreamReader)
        {
            Code = Opcode.REMATCH;
            Desirialize(dataStreamReader);
        }

        public override void Serialize(ref DataStreamWriter dataStreamWriter)
        {
            dataStreamWriter.WriteByte((byte) Code);
            dataStreamWriter.WriteInt(teamId);
            dataStreamWriter.WriteByte(wantRematch);

        }

        public override void Desirialize(DataStreamReader dataStreamReader)
        {
            teamId = dataStreamReader.ReadInt();
            wantRematch = dataStreamReader.ReadByte();
        }

        public override void RecivedOnClient()
        {
            NetUtility.CRematch?.Invoke(this);
        }

        public override void RecivedOnServer(NetworkConnection networkConnection)
        {
            NetUtility.SRematch?.Invoke(this, networkConnection);
        }
    }
}