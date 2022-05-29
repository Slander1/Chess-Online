using System;
using Net.NetMassage;
using Unity.Networking.Transport;
using UnityEngine;

namespace Net
{
    public enum Opcode
    {
        KEEP_ALIVE = 1,
        WELCOME = 2,
        START_GAME = 3,
        MAKE_MOVE = 4,
        REMATCH = 5,
        CHOSEPIECEONCHANGE = 6
    }

    public static class NetUtility
    {
        public static Action<NetMessage> CKeepAlive;
        public static Action<NetMessage> CWelcome;
        public static Action<NetMessage> CStartgame;
        public static Action<NetMessage> CMakeMove;
        public static Action<NetMessage> CRematch;
        public static Action<NetMessage> CChosePieceOnChange;
        public static Action<NetMessage, NetworkConnection> SKeepAlive;
        public static Action<NetMessage, NetworkConnection> SWelcome;
        public static Action<NetMessage, NetworkConnection> SStartgame;
        public static Action<NetMessage, NetworkConnection> SMakeMove;
        public static Action<NetMessage, NetworkConnection> SRematch;
        public static Action<NetMessage, NetworkConnection> SChosePieceOnChange;


        public static void OnData(DataStreamReader dataStreamReader, NetworkConnection networkConnection,
            Server server = null)
        {
            NetMessage msg = null;
            var opCode = (Opcode) dataStreamReader.ReadByte();
            switch (opCode)
            {
                case Opcode.KEEP_ALIVE:
                    msg = new NetKeepAlive(dataStreamReader);
                    break;
                case Opcode.WELCOME:
                    msg = new NetWelcome(dataStreamReader);
                    break;
                case Opcode.START_GAME:
                    msg = new NetStartGame(dataStreamReader);
                    break;
                case Opcode.MAKE_MOVE:
                    msg = new NetMakeMove(dataStreamReader);
                    break;
                case Opcode.CHOSEPIECEONCHANGE:
                    msg = new NetChosePiece(dataStreamReader);
                    break;
                /*case Opcode.REMATCH:msg = new NetRematch(dataStreamReader); break;*/
                default:
                    Debug.LogError("Message recived had no OpCode");
                    break;
            }

            if (server != null)
                msg.RecivedOnServer(networkConnection);
            else
                msg.RecivedOnClient();
        }


    }
}
