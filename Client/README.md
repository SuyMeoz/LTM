# TCP - CLIENT

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
try
{
    using (TcpClient client = new TcpClient(server_ip, port))
    using (NetworkStream stream = client.GetStream())
```
`try { ... }` → Bọc code để bắt lỗi nếu có sự cố.

`TcpClient client = new TcpClient(server_ip, port)` → Tạo kết nối TCP tới server.

`NetworkStream stream = client.GetStream()` → Lấy luồng dữ liệu để gửi/nhận.

`using` → đảm bảo `client` và `stream` sẽ được giải phóng tự động khi ra khỏi khối lệnh.

```cs
Console.WriteLine("Da ket noi den server " + server_ip + ":" + port);
```
In ra thông báo đã kết nối thành công tới server.

```cs
while (true)
{
    Console.Write("Nhap thong tin gui toi server : ");
    string message = Console.ReadLine();
```
Vòng lặp vô hạn để liên tục gửi dữ liệu.

`Console.ReadLine()` → Đọc dữ liệu người dùng nhập từ bàn phím.

```cs
if (string.IsNullOrWhiteSpace(message))
{
    Console.WriteLine("⚠️ Không thể gửi dữ liệu rỗng!");
    continue;
}
```
Kiểm tra nếu chuỗi rỗng hoặc chỉ có khoảng trắng → không gửi, yêu cầu nhập lại.

```cs
if (message.ToLower() == "exit")
{
    Console.WriteLine("Dong ket noi toi server ");
    break;
}
```
Nếu người dùng nhập `"exit"` → thoát vòng lặp, đóng kết nối.

```cs
byte[] dataToSend = Encoding.ASCII.GetBytes(message);
stream.Write(dataToSend, 0, dataToSend.Length);
Console.WriteLine("Da gui : " + message);
```
Chuyển chuỗi message thành mảng byte bằng ASCII encoding.

`stream.Write(...)` → Gửi dữ liệu tới server.

In ra thông báo đã gửi.

```cs
byte[] buff = new byte[256];
int bytesRead = stream.Read(buff, 0, buff.Length);
string response = Encoding.ASCII.GetString(buff, 0, bytesRead);
Console.WriteLine("Nhan tu server : " + response);
```
Tạo buffer 256 byte để nhận dữ liệu từ server.

`stream.Read(...)` → Đọc dữ liệu từ server, trả về số byte thực tế đọc được.

`Encoding.ASCII.GetString(...)` → Chuyển byte nhận được thành chuỗi.

In ra phản hồi từ server.

```cs
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
```
`catch (SocketException)` → Bắt lỗi khi không kết nối được tới server (ví dụ server chưa chạy).

`catch (IOException)` → Bắt lỗi khi đọc/ghi dữ liệu qua stream.

`catch (Exception)` → Bắt mọi lỗi khác không xác định.