using Unity.Networking.Transport;

namespace Net.NetMassage
{
    public class NetWelcome : NetMessage
    {
        public int AssignedTeam { get; set; }

        public NetWelcome()
        {
            Code = Opcode.WELCOME;
        }
        public NetWelcome(DataStreamReader dataStreamReader)
        {
            Code = Opcode.WELCOME;
            Desirialize(dataStreamReader);
        }

        public override void Serialize(ref DataStreamWriter dataStreamWriter)
        {
            dataStreamWriter.WriteByte((byte) Code);
            dataStreamWriter.WriteInt(AssignedTeam);
        }

        public override void Desirialize(DataStreamReader dataStreamReader)
        {
            AssignedTeam = dataStreamReader.ReadInt();
        }

        public override void RecivedOnClient(NetworkConnection networkConnection)
        {
            NetUtility.CWelcome?.Invoke(this);
        }

        public override void RecivedOnServer()
        {
            NetUtility.SWelcome?.Invoke(this);
        }
    }
}
