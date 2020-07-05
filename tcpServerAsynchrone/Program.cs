/*SERVEUR ASYNCHRONE*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Resources;
using System.Globalization;


public class SERVEUR_ASYNCHRONE
{
    static public Thread listenerAsync;

    public static string data = null;
    static public class AsynchronousSocketListener
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static IPEndPoint m_localEndPoint;
        static bool threadAsyncAlive = false;
        static Socket listener;


        // State object for reading client data asynchronously  
        public class StateObject
        {
            // Client  socket.  
            public Socket workSocket = null;
            public IPEndPoint IPEmetteur;
            // buffer tampon
            public const int BufferSize = 32;
            public byte[] buffer = new byte[BufferSize];

            public int tailleAttendue;
            public int tailleRecue;
            public byte[] bufferReception;
            public string entete;
        }

        public static void endpointSetup(IPEndPoint listeningPoint)
        {
            m_localEndPoint = listeningPoint;
        }

        public static void closeListener()
        {
            try
            {
                threadAsyncAlive = false;
                listener.Close();
            }
            catch (Exception)
            {
            }
        }

        public static void StartListening()
        {
            try
            {   // Bind the socket to the local endpoint and listen for incoming connections.  
                IPEndPoint localEndPoint = m_localEndPoint;

                listener = new Socket(localEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localEndPoint);
                listener.Listen(1);

                threadAsyncAlive = true;

                while (true)
                {
                    allDone.Reset();// Set the event to nonsignaled state.  
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();// Wait until a connection is made before continuing.  
                }
            }
            catch (Exception e)
            {
                threadAsyncAlive = false;
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();// Signal the main thread to continue.  

                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                // Create the state object.  
                StateObject state = new StateObject();
                state.tailleRecue = 0;
                state.tailleAttendue = 0;
                state.workSocket = handler;
                state.IPEmetteur = (IPEndPoint)handler.RemoteEndPoint;
                if (state.IPEmetteur.Address.ToString() == "169.254.0.22")
                {
                    handler.BeginReceive(state.buffer, 0, 20, SocketFlags.None, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, 8, SocketFlags.None, new AsyncCallback(ReadCallback), state);
                }
            }
            catch (Exception excCallBack)
            {

            }
        }
        public static void traitementObjetRecu(string emetteur, string entete, byte[] trameRecue)
        {
            data = System.Text.Encoding.UTF8.GetString(trameRecue);
            Console.WriteLine(data);
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            try
            {
                // Retrieve the state object and the handler socket from the asynchronous state object.  
                Socket handler = state.workSocket;

                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    if (state.IPEmetteur.Address.ToString() == "169.254.0.22")
                    {
                        data = System.Text.Encoding.UTF8.GetString(state.buffer);
                        Console.WriteLine(data);
                    }
                    //else
                    //{
                    //    if ((bytesRead == 8) && (state.tailleRecue == 0)) //Entete
                    //    {
                    //        state.tailleAttendue = BitConverter.ToInt32(state.buffer, 4);

                    //        byte[] byteEntete = new byte[4];
                    //        Array.Copy(state.buffer, 0, byteEntete, 0, 4);
                    //        state.entete = Encoding.ASCII.GetString(byteEntete);

                    //        state.bufferReception = new byte[state.tailleAttendue];
                    //        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    //    }
                    //    else
                    //    {
                    //        System.Buffer.BlockCopy(state.buffer, 0, state.bufferReception, state.tailleRecue, bytesRead);
                    //        state.tailleRecue += bytesRead;

                    //        if (state.tailleRecue == state.tailleAttendue)
                    //        {   // All the data has been read from the client
                    //            IPAddress IPEmetteur = state.IPEmetteur.Address;
                    //            string emetteur = "FROM " + IPEmetteur.ToString();
                    //            //if (comTCP.listeConnexionRev.ContainsKey(IPEmetteur))
                    //            //    emetteur = comTCP.listeConnexionRev[IPEmetteur];
                    //            traitementObjetRecu(emetteur, state.entete, state.bufferReception);
                    //        }
                    //        else
                    //        {   // Not all data received. Get more.  
                    //            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception exc)
            {

            }
        }
    }

    public static int Main(String[] args)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 9761);
        AsynchronousSocketListener.endpointSetup(localEndPoint);
        listenerAsync = new Thread(new ThreadStart(AsynchronousSocketListener.StartListening));
        listenerAsync.Start();
        return 0;
    }

}