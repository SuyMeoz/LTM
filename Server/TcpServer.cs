using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;


class Server
{
    static public void Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        string ip = "127.0.0.1";
        int port = 12345;

        TcpListener listener  = null;

        try
        {
            listener = new TcpListener(IPAddress.Parse(ip), port);
            listener.Start();
            Console.WriteLine("Server dang lang nghe tai " + ip + ":" + port + "...");
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Client da ket noi !");

                    using(NetworkStream stream = client.GetStream())
                    {
                        byte[] buff = new byte[256];
                        int bytesRead;

                        try
                        {
                            while((bytesRead = stream.Read(buff, 0, buff.Length)) != 0)
                            {
                                string dataReceived = Encoding.ASCII.GetString(buff, 0, bytesRead);
                                Console.WriteLine("Nhan du lieu tu client : " + dataReceived);

                                string response = "Server da nhan : " + dataReceived;
                                byte[] dataToSend = Encoding.ASCII.GetBytes(response);
                                stream.Write(dataToSend, 0, dataToSend.Length);
                            }
                        }

                        catch (IOException ex)
                        {
                            Console.WriteLine("⚠️ Lỗi khi đọc/ghi dữ liệu: " + ex.Message);
                        }
                    }
                    client.Close();
                    Console.WriteLine("Da dong ket noi voi client");
                }
            }
        }

        catch(SocketException ex)
        {
            Console.WriteLine("❌ Lỗi socket: " + ex.Message);
        }

        catch (Exception ex)
        {
            Console.WriteLine("❌ Lỗi không xác định: " + ex.Message);
        }
        
        finally
        {
            if (listener != null)
            {
                listener.Stop();
            }
            Console.WriteLine("Server đã dừng.");
        }
    }
}