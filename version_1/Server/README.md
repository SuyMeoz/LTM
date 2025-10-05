# TCP - SERVER
`using System.Net;` → Namespace cho các lớp mạng (IPAddress, IPEndPoint).

`using System.Net.Sockets;` → Namespace cho TCP/UDP socket (`TcpClient`, `TcpListener`).

`using System.IO;` → Namespace cho xử lý luồng dữ liệu (`IOException`).

```cs
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
```
xử lý tiếng Việt (hoặc các ngôn ngữ có dấu, ký tự đặc biệt) chính xác khi nhập/xuất dữ liệu trong console

```cs
string server_ip = "127.0.0.1";
int port = 12345;
```
`server_ip` → địa chỉ ip của server

`port` → cổng mà server lắng nghe 

```cs
TcpListener listener  = null;
```
Khai báo biến `listener` để quản lý socket server.

```cs
try
{
    listener = new TcpListener(IPAddress.Parse(ip), port);
    listener.Start();
    Console.WriteLine("Server dang lang nghe tai " + ip + ":" + port + "...");
```
`TcpListener` tạo một server TCP lắng nghe tại địa chỉ IP và port chỉ định.

`listener.Start()` → bắt đầu lắng nghe kết nối từ client.

In ra thông báo server đã sẵn sàng.

```cs
while (true)
{
    TcpClient client = listener.AcceptTcpClient();
    Console.WriteLine("Client da ket noi !");
```
Vòng lặp vô hạn để server luôn sẵn sàng nhận nhiều client.

`AcceptTcpClient()` → chặn (blocking) cho đến khi có client kết nối.

Khi có client kết nối, in ra thông báo.

```cs
using(NetworkStream stream = client.GetStream())
{
    byte[] buff = new byte[256];
    int bytesRead;
```
Lấy `NetworkStream` từ client để đọc/ghi dữ liệu.

`using` đảm bảo `stream` sẽ được giải phóng khi ra khỏi khối.

Tạo buffer 256 byte để chứa dữ liệu nhận từ client.

```cs
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
```
`stream.Read(...)` → đọc dữ liệu từ client, trả về số byte thực tế.

Nếu `bytesRead == 0` → client đã đóng kết nối.

Chuyển dữ liệu từ byte sang chuỗi bằng `Encoding.ASCII`.

In dữ liệu nhận được ra console.

Tạo phản hồi `"Server da nhan : ..."`.

Gửi phản hồi lại cho client bằng `stream.Write(...)`.

```cs
catch (IOException ex)
{
    Console.WriteLine("⚠️ Lỗi khi đọc/ghi dữ liệu: " + ex.Message);
}
```
Bắt lỗi khi có sự cố đọc/ghi dữ liệu (ví dụ client ngắt kết nối đột ngột).

```cs
client.Close();
Console.WriteLine("Da dong ket noi voi client");
```
Sau khi client ngắt kết nối, đóng socket và in thông báo.

```cs
catch(SocketException ex)
{
    Console.WriteLine("❌ Lỗi socket: " + ex.Message);
}

catch (Exception ex)
{
    Console.WriteLine("❌ Lỗi không xác định: " + ex.Message);
}
```
`SocketException`: lỗi liên quan đến socket (ví dụ port đã bị chiếm).

`Exception`: bắt mọi lỗi khác không xác định.

```cs
finally
{
    if (listener != null)
    {
        listener.Stop();
    }
    Console.WriteLine("Server đã dừng.");
}
```
`finally` luôn chạy, kể cả khi có lỗi.

Đảm bảo `listener.Stop()` được gọi để giải phóng tài nguyên.

In ra thông báo server đã dừng.