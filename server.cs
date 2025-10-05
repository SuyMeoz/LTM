using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;

namespace server1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "File server";

            TcpListener listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();

            Console.WriteLine("Server dang lang nghe o Port : 8888, Dang cho client ket noi...");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine($"Client đã kết nối từ:{client.Client.RemoteEndPoint}");

                _ = HandleClientRequestAsync(client);
            }
        }

        static async Task HandleClientRequestAsync(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            using(var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
            using(var reader = new StreamReader(stream, Encoding.UTF8))
            {
                try
                {
                    string request = await reader.ReadLineAsync();
                    string[] parts = request.Split(new char[] { '|' }, 3);
                    string command = parts[0].ToUpperInvariant();
                    string filename = parts.Length > 1 ? parts[1] : null;
                    string data = parts.Length > 2 ? parts[2] : null;

                    switch (command) {
                        case "LIST":
                            var files = Directory.GetFiles(Directory.GetCurrentDirectory());
                            foreach (var f in files)
                                await writer.WriteLineAsync(Path.GetFileName(f));
                            await writer.WriteLineAsync("END");
                            break;

                        case "DOWNLOAD":
                            if (File.Exists(filename))
                            {
                                foreach (var line in File.ReadLines(filename, Encoding.UTF8))
                                    await writer.WriteLineAsync(line);
                                await writer.WriteLineAsync("END");
                            }
                            else
                            {
                                await writer.WriteLineAsync($"ERROR: File '{filename}' not found.");
                                await writer.WriteLineAsync("END");
                            }
                            break;

                        case "DELETE":
                            if (File.Exists(filename))
                            {
                                File.Delete(filename);
                                await writer.WriteLineAsync($"File '{filename}' deleted.");
                            }
                            else
                            {
                                await writer.WriteLineAsync($"ERROR: File '{filename}' not found.");
                            }
                            await writer.WriteLineAsync("END");
                            break;

                        case "UPLOAD":
                            if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(data))
                            {
                                await writer.WriteLineAsync("ERROR: Missing filename or data.");
                            }
                            else
                            {
                                File.WriteAllText(filename, data, Encoding.UTF8);
                                await writer.WriteLineAsync($"File '{filename}' uploaded successfully.");
                            }
                            await writer.WriteLineAsync("END");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine($"Đã đóng kết nối với client.");
                }
            }
        }
    }
}
