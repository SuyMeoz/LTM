# Máy Chủ File TCP bằng C#

## Tổng Quan
Đây là một máy chủ file đơn giản dựa trên TCP được triển khai bằng C#. Máy chủ lắng nghe trên cổng 8888 và xử lý các hoạt động file cơ bản từ client, bao gồm liệt kê file (LIST), tải xuống file (DOWNLOAD), xóa file (DELETE), và tải lên file (UPLOAD). Nó sử dụng lập trình bất đồng bộ để xử lý nhiều client đồng thời.

Máy chủ chạy trong ứng dụng console và hỗ trợ mã hóa UTF-8 cho các ký tự quốc tế. Nó xử lý các yêu cầu từ client theo giao thức dựa trên dòng, nơi mỗi yêu cầu được định dạng là `COMMAND|filename|data` (nếu áp dụng).

**Lưu ý:** Đây là triển khai cơ bản cho mục đích giáo dục. Trong môi trường sản xuất, hãy thêm các tính năng bảo mật như xác thực, xử lý lỗi, và kiểm tra đầu vào để tránh các vấn đề như tấn công path traversal.

## Yêu Cầu
- .NET Framework hoặc .NET Core/.NET 5+ (tương thích với mã code được cung cấp).
- Visual Studio hoặc bất kỳ trình biên dịch C# nào.
- Chạy máy chủ trong thư mục mà bạn muốn thực hiện các hoạt động file (ví dụ: thư mục làm việc hiện tại).

## Cách Chạy
1. Lưu code thành file `Program.cs` trong một dự án console mới.
2. Build và chạy dự án: `dotnet run` (hoặc nhấn F5 trong Visual Studio).
3. Máy chủ sẽ bắt đầu lắng nghe trên cổng 8888.
4. Sử dụng một client TCP (ví dụ: telnet, client tùy chỉnh, hoặc công cụ như netcat) để kết nối và gửi lệnh như:
   - `LIST` (để liệt kê file).
   - `DOWNLOAD|filename.txt` (để tải xuống file).
   - `DELETE|filename.txt` (để xóa file).
   - `UPLOAD|filename.txt|nội dung file ở đây` (để tải lên file).

Máy chủ sẽ phản hồi bằng các dòng kết thúc bằng "END" cho các hoạt động thành công hoặc thông báo lỗi.

## Giải Thích Code
Dưới đây là phân tích từng dòng (hoặc từng phần) của code. Tôi đã nhóm các dòng liên quan để dễ hiểu và giải thích mục đích của chúng. Code được cấu trúc như một ứng dụng console với các phương thức bất đồng bộ để xử lý kết nối TCP.

### Các Lệnh Using (Dòng 1-10)
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;
```
- Những lệnh này nhập các namespace cần thiết:
  - `System`: Các kiểu dữ liệu cốt lõi như `Console`, `String`, v.v.
  - `System.Collections.Generic`: Để sử dụng các bộ sưu tập (mặc dù không sử dụng nhiều ở đây).
  - `System.Linq`: Để sử dụng các truy vấn LINQ (sử dụng ngầm trong một số hoạt động).
  - `System.Text`: Để mã hóa và thao tác chuỗi.
  - `System.Threading.Tasks`: Để lập trình bất đồng bộ (`async`/`await`).
  - `System.Net`: Để sử dụng các kiểu mạng như `IPAddress`.
  - `System.Net.Sockets`: Để thực hiện hoạt động socket TCP (`TcpListener`, `TcpClient`).
  - `System.IO`: Để thực hiện hoạt động file (`Directory`, `File`, `StreamReader`, `StreamWriter`).

### Khai Báo Namespace và Lớp (Dòng 12-14)
```csharp
namespace server1
{
    internal class Program
    {
```
- `namespace server1`: Tổ chức code dưới một namespace để tránh xung đột tên.
- `internal class Program`: Định nghĩa một lớp nội bộ tên `Program` (tiêu chuẩn cho ứng dụng console). `internal` nghĩa là chỉ có thể truy cập trong assembly.

### Phương Thức Main (Dòng 16-29)
```csharp
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
```
- `static async Task Main(string[] args)`: Điểm khởi đầu của chương trình. `async Task` cho phép thực thi bất đồng bộ. `args` dùng cho tham số dòng lệnh (không sử dụng ở đây).
- `Console.InputEncoding = Encoding.UTF8;`: Đặt mã hóa đầu vào console thành UTF-8 để xử lý ký tự không phải ASCII.
- `Console.OutputEncoding = Encoding.UTF8;`: Đặt mã hóa đầu ra console tương tự.
- `Console.Title = "File server";`: Đặt tiêu đề cửa sổ console.
- `TcpListener listener = new TcpListener(IPAddress.Any, 8888);`: Tạo một listener TCP gắn kết với tất cả địa chỉ IP có sẵn (`IPAddress.Any`) trên cổng 8888.
- `listener.Start();`: Bắt đầu lắng nghe các kết nối đến.
- `Console.WriteLine("Server dang lang nghe o Port : 8888, Dang cho client ket noi...");`: In thông báo khởi động (bằng tiếng Việt: "Server đang lắng nghe ở Port: 8888, Đang chờ client kết nối...").
- `while (true)`: Vòng lặp vô hạn để liên tục chấp nhận client.
- `TcpClient client = await listener.AcceptTcpClientAsync();`: Chấp nhận bất đồng bộ một kết nối TCP đến và trả về đối tượng `TcpClient`.
- `Console.WriteLine($"Client đã kết nối từ:{client.Client.RemoteEndPoint}");`: Ghi log điểm cuối từ xa của client (IP:cổng) khi kết nối (bằng tiếng Việt: "Client đã kết nối từ:").
- `_ = HandleClientRequestAsync(client);`: Bắt đầu xử lý client bất đồng bộ theo cách fire-and-forget (`_ =` loại bỏ task trả về). Điều này cho phép vòng lặp chính chấp nhận thêm client mà không bị chặn.

### Phương Thức HandleClientRequestAsync (Dòng 31-90)
```csharp
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
```
- `static async Task HandleClientRequestAsync(TcpClient client)`: Phương thức bất đồng bộ để xử lý yêu cầu của một client duy nhất. Trả về `Task` để tương thích với async.
- `using (client)`: Giải phóng `TcpClient` khi hoàn thành (đóng kết nối).
- `using (NetworkStream stream = client.GetStream())`: Lấy luồng mạng cơ bản để đọc/ghi dữ liệu. `using` đảm bảo giải phóng.
- `using(var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })`: Tạo writer văn bản UTF-8 trên luồng. `AutoFlush = true` đảm bảo dữ liệu được gửi ngay lập tức.
- `using(var reader = new StreamReader(stream, Encoding.UTF8))`: Tạo reader văn bản UTF-8 trên luồng.
- `try { ... }`: Bao quanh logic chính bằng try-catch để xử lý lỗi.
- `string request = await reader.ReadLineAsync();`: Đọc bất đồng bộ dòng đầu tiên từ client (yêu cầu lệnh).
- `string[] parts = request.Split(new char[] { '|' }, 3);`: Tách yêu cầu bằng dấu phân cách '|', giới hạn ở 3 phần (lệnh, tên file, dữ liệu).
- `string command = parts[0].ToUpperInvariant();`: Trích xuất và chuyển lệnh thành chữ hoa để khớp không phân biệt chữ hoa/thường.
- `string filename = parts.Length > 1 ? parts[1] : null;`: Trích xuất tên file nếu có.
- `string data = parts.Length > 2 ? parts[2] : null;`: Trích xuất dữ liệu (cho UPLOAD) nếu có.
- `switch (command) { ... }`: Chuyển hướng đến trình xử lý phù hợp dựa trên lệnh.
  - **Trường hợp LIST**:
    - `var files = Directory.GetFiles(Directory.GetCurrentDirectory());`: Lấy tất cả file trong thư mục hiện tại.
    - `foreach (var f in files) await writer.WriteLineAsync(Path.GetFileName(f));`: Gửi từng tên file (không có đường dẫn) đến client.
    - `await writer.WriteLineAsync("END");`: Báo hiệu kết thúc danh sách.
  - **Trường hợp DOWNLOAD**:
    - `if (File.Exists(filename)) { ... }`: Kiểm tra file có tồn tại không.
    - `foreach (var line in File.ReadLines(filename, Encoding.UTF8)) await writer.WriteLineAsync(line);`: Đọc và gửi nội dung file theo dòng.
    - `await writer.WriteLineAsync("END");`: Kết thúc phản hồi.
    - Ngược lại: Gửi thông báo lỗi và "END".
  - **Trường hợp DELETE**:
    - `if (File.Exists(filename)) { File.Delete(filename); ... }`: Xóa file nếu tồn tại và xác nhận.
    - Ngược lại: Gửi lỗi.
    - Luôn kết thúc bằng "END".
  - **Trường hợp UPLOAD**:
    - `if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(data)) { ... }`: Kiểm tra đầu vào hợp lệ.
    - `File.WriteAllText(filename, data, Encoding.UTF8);`: Ghi dữ liệu vào file mới.
    - Gửi thành công hoặc lỗi, sau đó "END".
- `catch (Exception ex) { Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}"); }`: Bắt bất kỳ ngoại lệ nào (ví dụ: lỗi mạng) và ghi log (bằng tiếng Việt: "Đã xảy ra lỗi:").
- `finally { Console.WriteLine($"Đã đóng kết nối với client."); }`: Luôn ghi log đóng kết nối (bằng tiếng Việt: "Đã đóng kết nối với client.") trước khi giải phóng tài nguyên.

## Hạn Chế và Cải Tiến
- **Bảo Mật:** Không kiểm tra tên file (dễ bị tấn công path traversal, ví dụ: `../secret.txt`). Hãy thêm kiểm tra làm sạch.
- **Đồng Thời:** Xử lý nhiều client nhưng không khóa hoạt động file, có thể gây race condition.
- **Kích Thước File:** UPLOAD giả định dữ liệu nhỏ trong một dòng; đối với file lớn, sử dụng luồng nhị phân.
- **Xử Lý Lỗi:** Cơ bản; mở rộng cho các ngoại lệ cụ thể.
- **Giao Thức:** Đơn giản dựa trên dòng; xem xét giao thức nhị phân để hiệu quả hơn.
