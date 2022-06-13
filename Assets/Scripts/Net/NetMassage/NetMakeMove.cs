using Unity.Networking.Transport;
using UnityEngine;

namespace Net.NetMassage
{
    public class NetMakeMove : NetMessage
    {

        public Vector2Int originalMove;
        public Vector2Int distanationMove;
        public int teamId;
    
    
        public NetMakeMove()
        {
            Code = Opcode.MAKE_MOVE;
        }
        public NetMakeMove(DataStreamReader dataStreamReader)
        {
            Code = Opcode.MAKE_MOVE;
            Desirialize(dataStreamReader);
        }

        public override void Serialize(ref DataStreamWriter dataStreamWriter)
        {
            dataStreamWriter.WriteByte((byte) Code);
            dataStreamWriter.WriteInt(originalMove.x);
            dataStreamWriter.WriteInt(originalMove.y);
            dataStreamWriter.WriteInt(distanationMove.x);
            dataStreamWriter.WriteInt(distanationMove.y);
            dataStreamWriter.WriteInt(teamId);

        }

        public override void Desirialize(DataStreamReader dataStreamReader)
        {
            originalMove.x = dataStreamReader.ReadInt();
            originalMove.y = dataStreamReader.ReadInt();
            distanationMove.x = dataStreamReader.ReadInt();
            distanationMove.y = dataStreamReader.ReadInt();
            teamId = dataStreamReader.ReadInt();
        }

        public override void RecivedOnClient(NetworkConnection networkConnection)
        {
            NetUtility.CMakeMove?.Invoke(this);
        }

        public override void RecivedOnServer()
        {
            NetUtility.SMakeMove?.Invoke(this);
        }
    }
}