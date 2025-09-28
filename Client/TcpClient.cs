using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;

class Client
{
    static public void Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        string server_ip = "127.0.0.1";
        int port = 12345;

        try
        {
            using (TcpClient client = new TcpClient(server_ip, port))
            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine("Da ket noi den server " + server_ip + ":" + port);

                while (true)
                {
                    Console.Write("Nhap thong tin gui toi server : ");
                    string message = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Console.WriteLine("⚠️ Không thể gửi dữ liệu rỗng!");
                        continue;
                    }

                    if (message.ToLower() == "exit")
                    {
                        Console.WriteLine("Dong ket noi toi server ");
                        break;
                    }

                    byte[] dataToSend = Encoding.ASCII.GetBytes(message);
                    stream.Write(dataToSend, 0, dataToSend.Length);
                    Console.WriteLine("Da gui : " + message);

                    byte[] buff = new byte[256];
                    int bytesRead = stream.Read(buff, 0, buff.Length);
                    string response = Encoding.ASCII.GetString(buff, 0, bytesRead);
                    Console.WriteLine("Nhan tu server : " + response);
                }
            }
        }

        catch (SocketException ex)
        {
            Console.WriteLine("❌ Lỗi kết nối: " + ex.Message);
        }

        catch (IOException ex)
        {
            Console.WriteLine("❌ Lỗi IO: " + ex.Message);
        }

        catch (Exception ex)
        {
            Console.WriteLine("❌ Lỗi không xác định: " + ex.Message);
        }

        Console.WriteLine("Ứng dụng client đã thoát.");
    }
}

