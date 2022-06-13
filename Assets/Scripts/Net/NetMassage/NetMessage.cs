using Unity.Networking.Transport;
using UnityEngine;

namespace Net.NetMassage
{
    public class NetMessage
    {
        public Opcode Code { set; get; }

        public virtual void Serialize(ref DataStreamWriter dataStreamWriter)
        {
            dataStreamWriter.WriteByte((byte) Code);
        }

        public virtual void Desirialize(DataStreamReader dataStreamReader)
        {
        
        }

        public virtual void RecivedOnClient(NetworkConnection networkConnection)
        {
        
        }

        public virtual void RecivedOnServer()
        {
        
        }

    }
}
