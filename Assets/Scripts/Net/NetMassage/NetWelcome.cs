using Unity.Networking.Transport;


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

    public override void RecivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }

    public override void RecivedOnServer(NetworkConnection networkConnection)
    {
        NetUtility.S_WELCOME?.Invoke(this, networkConnection);
    }
}
