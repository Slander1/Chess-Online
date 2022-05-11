using Unity.Networking.Transport;


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
        AssignedTeam = dataStreamReader.ReadInt();
    }

    public override void RecivedOnClient()
    {
        NetUtility.C_STARTGAME?.Invoke(this);
    }

    public override void RecivedOnServer(NetworkConnection networkConnection)
    {
        NetUtility.S_STARTGAME?.Invoke(this, networkConnection);
    }
}