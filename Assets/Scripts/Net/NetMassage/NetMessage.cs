using Unity.Networking.Transport;
using UnityEngine;



public class NetMessage : MonoBehaviour
{
    public Opcode Code { set; get; }

    public virtual void Serialize(ref DataStreamWriter dataStreamWriter)
    {
        dataStreamWriter.WriteByte((byte) Code);
    }

    public virtual void Desirialize(DataStreamReader dataStreamReader)
    {
        
    }

    public virtual void RecivedOnClient()
    {
        
    }

    public virtual void RecivedOnServer(NetworkConnection networkConnection)
    {
        
    }

}
