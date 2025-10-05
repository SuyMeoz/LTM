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

namespace client1._1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "File Client";
            txtFileName.Text = "Welcome.txt";
        }

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

        private async void btnGetFile_Click(object sender, EventArgs e)
        {
            string fileName = txtFileName.Text.Trim();
            if (string.IsNullOrEmpty(fileName)) return;
            await SendCommandAsync("DOWNLOAD", fileName);
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            string fileName = txtFileName.Text.Trim();
            if (string.IsNullOrEmpty(fileName)) return;
            await SendCommandAsync("DELETE", fileName);
        }

        private async void btnList_Click(object sender, EventArgs e)
        {
            await SendCommandAsync("LIST");
        }

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
    }
}
