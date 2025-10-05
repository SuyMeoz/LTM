# Client File TCP bằng C#

## Tổng Quan
Đây là một client file đơn giản dựa trên TCP được triển khai bằng C# sử dụng Windows Forms. Client kết nối đến máy chủ file (server) trên địa chỉ `127.0.0.1` (localhost) và cổng 8888, hỗ trợ các lệnh: liệt kê file (LIST), tải xuống file (DOWNLOAD), xóa file (DELETE), và tải lên file (UPLOAD). Nội dung phản hồi từ server được hiển thị trong một RichTextBox.

Client sử dụng lập trình bất đồng bộ để gửi yêu cầu và nhận phản hồi. Giao thức sử dụng định dạng `COMMAND|filename|data` tương tự server. Đây là phần giao diện người dùng (UI) để tương tác dễ dàng với server.

**Lưu ý:** Client này dành cho mục đích giáo dục. Trong môi trường sản xuất, hãy thêm xử lý lỗi chi tiết hơn, xác thực kết nối, và hỗ trợ file lớn hơn. Đảm bảo server đang chạy trước khi sử dụng client.

## Yêu Cầu
- .NET Framework 4.5+ hoặc .NET Core/.NET 5+ (tương thích với Windows Forms).
- Visual Studio (với Windows Forms App template).
- Server file (từ code trước) phải đang chạy trên localhost:8888.
- Thêm các control UI vào Form: 
  - TextBox: `txtFileName` (để nhập tên file).
  - RichTextBox: `rtbFileContent` (để hiển thị nội dung phản hồi).
  - Button: `btnGetFile` (tải xuống), `btnDelete` (xóa), `btnList` (liệt kê), `btnUpload` (tải lên).

## Cách Chạy
1. Tạo một dự án Windows Forms App mới trong Visual Studio.
2. Thêm code này vào file `Form1.cs` (partial class, giả định designer đã tạo các control UI).
3. Build dự án (Build > Build Solution).
4. Chạy server file trước (từ code trước).
5. Chạy client: Nhấn F5 hoặc `dotnet run`.
6. Sử dụng:
   - Nhập tên file vào `txtFileName` (mặc định: "Welcome.txt").
   - Nhấn "List" để liệt kê file.
   - Nhấn "Get File" để tải xuống và hiển thị nội dung.
   - Nhấn "Delete" để xóa file.
   - Nhấn "Upload" để chọn và tải lên file từ máy local.

Phản hồi từ server sẽ hiển thị trong `rtbFileContent`. Nếu có lỗi, một MessageBox sẽ hiện thông báo.

## Giải Thích Code
Dưới đây là phân tích từng phần của code. Tôi đã nhóm các dòng liên quan để dễ hiểu và giải thích mục đích của chúng. Code là một partial class cho Form Windows Forms, tập trung vào xử lý sự kiện và giao tiếp mạng.

### Các Lệnh Using (Dòng 1-12)
```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
```
- Những lệnh này nhập các namespace cần thiết:
  - `System`: Các kiểu dữ liệu cốt lõi như `Exception`, `String`.
  - `System.Collections.Generic`: Để sử dụng `List<T>` hoặc tương tự (không sử dụng trực tiếp ở đây).
  - `System.ComponentModel`: Để hỗ trợ thiết kế Windows Forms (ví dụ: `BackgroundWorker`).
  - `System.Data`: Để xử lý dữ liệu (không sử dụng trực tiếp).
  - `System.Drawing`: Để vẽ và xử lý đồ họa trong Forms (ví dụ: `Color`, `Font`).
  - `System.IO`: Để đọc/ghi file (`File`, `Path`).
  - `System.Linq`: Để sử dụng LINQ (không sử dụng trực tiếp).
  - `System.Net.Sockets`: Để kết nối TCP (`TcpClient`, `NetworkStream`).
  - `System.Text`: Để mã hóa và thao tác chuỗi (`Encoding`, `StringBuilder`).
  - `System.Threading.Tasks`: Để lập trình bất đồng bộ (`async`/`await`).
  - `System.Windows.Forms`: Để xây dựng giao diện Windows Forms (`Form`, `Button`, `TextBox`, v.v.).

### Khai Báo Namespace và Lớp (Dòng 14-16)
```csharp
namespace client1._1
{
    public partial class Form1 : Form
    {
```
- `namespace client1._1`: Tổ chức code dưới một namespace để tránh xung đột tên (tên namespace có dấu chấm, có thể là "client1.1").
- `public partial class Form1 : Form`: Định nghĩa lớp partial `Form1` kế thừa từ `Form` (lớp cơ bản cho Windows Forms). `partial` cho phép tách code với designer file (Form1.Designer.cs).

### Constructor (Dòng 18-22)
```csharp
public Form1()
{
    InitializeComponent();
    this.Text = "File Client";
    txtFileName.Text = "Welcome.txt";
}
```
- `public Form1()`: Constructor của form, được gọi khi tạo instance.
- `InitializeComponent();`: Khởi tạo các control UI từ file designer (tạo các button, textbox, v.v.).
- `this.Text = "File Client";`: Đặt tiêu đề của cửa sổ form thành "File Client".
- `txtFileName.Text = "Welcome.txt";`: Đặt giá trị mặc định cho TextBox `txtFileName` là "Welcome.txt" (tên file mẫu).

### Phương Thức SendCommandAsync (Dòng 24-45)
```csharp
private async Task SendCommandAsync(string command, string filename = "", string data = "")
{
    try
    {
        using (TcpClient client = new TcpClient())
        {
            await client.ConnectAsync("127.0.0.1", 8888);
            using (NetworkStream stream = client.GetStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                await writer.WriteLineAsync($"{command}|{filename}|{data}");

                var sb = new StringBuilder();
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line == "END") break; // dừng khi gặp END
                    sb.AppendLine(line);
                }
                rtbFileContent.Text = sb.ToString();
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Lỗi: " + ex.Message);
    }
}
```
- `private async Task SendCommandAsync(string command, string filename = "", string data = "")`: Phương thức bất đồng bộ riêng tư để gửi lệnh đến server. Tham số: `command` (bắt buộc), `filename` và `data` (tùy chọn, mặc định rỗng). Trả về `Task` để async.
- `try { ... }`: Bao quanh logic bằng try-catch để xử lý lỗi.
- `using (TcpClient client = new TcpClient())`: Tạo client TCP mới và giải phóng tự động khi hoàn thành.
- `await client.ConnectAsync("127.0.0.1", 8888);`: Kết nối bất đồng bộ đến localhost (127.0.0.1) trên cổng 8888.
- `using (NetworkStream stream = client.GetStream())`: Lấy luồng mạng để đọc/ghi.
- `using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })`: Tạo writer UTF-8 với tự động flush (gửi dữ liệu ngay).
- `using (var reader = new StreamReader(stream, Encoding.UTF8))`: Tạo reader UTF-8.
- `await writer.WriteLineAsync($"{command}|{filename}|{data}");`: Gửi yêu cầu theo định dạng lệnh|filename|data (sử dụng string interpolation).
- `var sb = new StringBuilder();`: Tạo StringBuilder để tích lũy phản hồi từ server.
- `string line; while ((line = await reader.ReadLineAsync()) != null) { if (line == "END") break; sb.AppendLine(line); }`: Đọc từng dòng bất đồng bộ từ server cho đến khi gặp "END" (dừng và không thêm "END" vào nội dung), sau đó thêm các dòng khác vào StringBuilder.
- `rtbFileContent.Text = sb.ToString();`: Hiển thị toàn bộ nội dung phản hồi trong RichTextBox `rtbFileContent`.
- `catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }`: Bắt lỗi (ví dụ: không kết nối được server) và hiển thị hộp thoại thông báo (bằng tiếng Việt: "Lỗi: ").

### Sự Kiện btnGetFile_Click (Dòng 47-53)
```csharp
private async void btnGetFile_Click(object sender, EventArgs e)
{
    string fileName = txtFileName.Text.Trim();
    if (string.IsNullOrEmpty(fileName)) return;
    await SendCommandAsync("DOWNLOAD", fileName);
}
```
- `private async void btnGetFile_Click(object sender, EventArgs e)`: Xử lý sự kiện click cho button "Get File" (tải xuống). `async void` vì là event handler.
- `string fileName = txtFileName.Text.Trim();`: Lấy tên file từ TextBox và loại bỏ khoảng trắng thừa.
- `if (string.IsNullOrEmpty(fileName)) return;`: Kiểm tra nếu tên file rỗng thì thoát (không làm gì).
- `await SendCommandAsync("DOWNLOAD", fileName);`: Gửi lệnh DOWNLOAD với tên file.

### Sự Kiện btnDelete_Click (Dòng 55-61)
```csharp
private async void btnDelete_Click(object sender, EventArgs e)
{
    string fileName = txtFileName.Text.Trim();
    if (string.IsNullOrEmpty(fileName)) return;
    await SendCommandAsync("DELETE", fileName);
}
```
- Tương tự btnGetFile, nhưng gửi lệnh "DELETE" để xóa file.

### Sự Kiện btnList_Click (Dòng 63-66)
```csharp
private async void btnList_Click(object sender, EventArgs e)
{
    await SendCommandAsync("LIST");
}
```
- Xử lý click cho button "List" (liệt kê file).
- Gửi lệnh "LIST" mà không cần filename hoặc data (sử dụng giá trị mặc định rỗng).

### Sự Kiện btnUpload_Click (Dòng 68-76)
```csharp
private async void btnUpload_Click(object sender, EventArgs e)
{
    using (OpenFileDialog ofd = new OpenFileDialog())
    {
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            string fileName = Path.GetFileName(ofd.FileName);
            string content = File.ReadAllText(ofd.FileName, Encoding.UTF8);
            await SendCommandAsync("UPLOAD", fileName, content);
        }
    }
}
```
- `private async void btnUpload_Click(object sender, EventArgs e)`: Xử lý click cho button "Upload" (tải lên).
- `using (OpenFileDialog ofd = new OpenFileDialog())`: Tạo hộp thoại mở file và giải phóng tự động.
- `if (ofd.ShowDialog() == DialogResult.OK)`: Hiển thị hộp thoại; nếu người dùng chọn OK (chọn file).
- `string fileName = Path.GetFileName(ofd.FileName);`: Lấy tên file từ đường dẫn đầy đủ (chỉ tên, không đường dẫn).
- `string content = File.ReadAllText(ofd.FileName, Encoding.UTF8);`: Đọc toàn bộ nội dung file bằng UTF-8.
- `await SendCommandAsync("UPLOAD", fileName, content);`: Gửi lệnh UPLOAD với tên file và nội dung.

## Hạn Chế và Cải Tiến
- **Kết Nối:** Chỉ kết nối localhost; có thể thêm TextBox để thay đổi IP/port.
- **File Lớn:** `File.ReadAllText` tải toàn bộ file vào bộ nhớ, không phù hợp file lớn. Sử dụng Stream cho file binary.
- **Xử Lý Lỗi:** Chỉ hiển thị MessageBox cơ bản; thêm log hoặc cập nhật UI chi tiết hơn.
- **UI:** Không có progress bar cho upload/download lớn. Thêm validation cho tên file.
- **Bảo Mật:** Không mã hóa dữ liệu; xem xét SSL/TLS cho kết nối an toàn.
- **Đa Lệnh:** Hiện chỉ gửi một lệnh mỗi lần; có thể hỗ trợ nhiều lệnh liên tục trong một kết nối.

