using ChessPiaces;
using Unity.Networking.Transport;
using UnityEngine;


namespace Net.NetMassage
{
    public class NetChosePiece : NetMessage
    {
        public int teamId;
        public ChessPiece.Type type;
        public Vector2Int pos;
        
        public NetChosePiece()
        {
            Code = Opcode.CHOSEPIECEONCHANGE;
        }
        public NetChosePiece(DataStreamReader dataStreamReader)
        {
            Code = Opcode.CHOSEPIECEONCHANGE;
            Desirialize(dataStreamReader);
        }
        public override void Serialize(ref DataStreamWriter dataStreamWriter)
        {
            dataStreamWriter.WriteByte((byte) Code);
            dataStreamWriter.WriteByte((byte) type);
            dataStreamWriter.WriteInt(pos.x);
            dataStreamWriter.WriteInt(pos.y);
            dataStreamWriter.WriteInt(teamId);

        }
        public override void Desirialize(DataStreamReader dataStreamReader)
        {
            type = (ChessPiece.Type)dataStreamReader.ReadByte();
            pos.x = dataStreamReader.ReadInt();
            pos.y = dataStreamReader.ReadInt();
            teamId = dataStreamReader.ReadInt();
        }

        public override void RecivedOnClient(NetworkConnection networkConnection)
        {
            NetUtility.CChosePieceOnChange?.Invoke(this);
        }

        public override void RecivedOnServer()
        {
            NetUtility.SChosePieceOnChange?.Invoke(this);
        }

    }
}
