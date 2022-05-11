using Unity.Networking.Transport;


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

    public override void RecivedOnClient()
    {
        NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }

    public override void RecivedOnServer(NetworkConnection networkConnection)
    {
        NetUtility.S_KEEP_ALIVE?.Invoke(this, networkConnection);
    }
}