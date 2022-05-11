using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public enum Opcode
{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    REMATCH = 5
}

public static class NetUtility
{
    public static Action<NetMessage> C_KEEP_ALIVE;
    public static Action<NetMessage> C_WELCOME;
    public static Action<NetMessage> C_STARTGAME;
    public static Action<NetMessage> C_MAKE_MOVE;
    public static Action<NetMessage> C_REMATCH;
    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<NetMessage, NetworkConnection> S_WELCOME;
    public static Action<NetMessage, NetworkConnection> S_STARTGAME;
    public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
    public static Action<NetMessage, NetworkConnection> S_REMATCH;


    public static void OnData(DataStreamReader dataStreamReader, NetworkConnection networkConnection,
        Server server = null)
    {
        NetMessage msg = null;
        var opCode = (Opcode) dataStreamReader.ReadByte();
        switch (opCode)
        {
            case Opcode.KEEP_ALIVE: msg = new NetKeepAlive(dataStreamReader); break; 
            /*case Opcode.WELCOME: msg = new NetWeclome(dataStreamReader); break;
            case Opcode.START_GAME:msg = new NetStartGame(dataStreamReader); break;
            case Opcode.MAKE_MOVE:msg = new NetMakeMove(dataStreamReader); break;
            case Opcode.REMATCH:msg = new NetRematch(dataStreamReader); break;*/
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
