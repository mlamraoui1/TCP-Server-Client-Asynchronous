using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TCP_CLIENT
{    
    public static void StartSending()
    {

        // Data buffer for incoming data.  
        int i = 0;

        IPAddress  ipAdr = IPAddress.Parse("169.254.0.22");
        IPEndPoint distIP = new IPEndPoint(ipAdr, 9760);
        Socket socketClient;

        try
        {
            byte[] bufferEmission = new byte[24];

            while (true)
            {

                    socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    i++;
                    string com = "TO "+i.ToString();
                    bufferEmission = System.Text.Encoding.UTF8.GetBytes(com);
                    socketClient.Connect(distIP);
                    if ((socketClient.Send(bufferEmission, bufferEmission.Length, SocketFlags.None)) < 0)
                        Console.WriteLine("Can not send");
                    socketClient.Close();
                    System.Threading.Thread.Sleep(10);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Main(String[] args)
    {
        StartSending();
        return 0;
    }
}
