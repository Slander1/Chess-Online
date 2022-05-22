using Unity.Networking.Transport;

namespace Net.NetMassage
{
    public class NetStartGame : NetMessage
    {
        public int AssignedTeam { get; set; }

        public NetStartGame()
        {
            Code = Opcode.START_GAME;
        }
        public NetStartGame(DataStreamReader dataStreamReader)
        {
            Code = Opcode.START_GAME;
            Desirialize(dataStreamReader);
        }

        public override void Serialize(ref DataStreamWriter dataStreamWriter)
        {
            dataStreamWriter.WriteByte((byte) Code);
        }

        public override void Desirialize(DataStreamReader dataStreamReader)
        {
            //AssignedTeam = dataStreamReader.ReadInt();
        }

        public override void RecivedOnClient()
        {
            NetUtility.CStartgame?.Invoke(this);
        }

        public override void RecivedOnServer(NetworkConnection networkConnection)
        {
            NetUtility.SStartgame?.Invoke(this, networkConnection);
        }
    }
}