using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortTest
{
    public partial class Form1 : Form
    {
        public SerialPort SerialPort = new SerialPort();
        public bool ReadIsHex = false;
        public bool WriteIsHex = false;
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            SerialPort.DataReceived += SerialPort_DataReceived;
            SerialPort.ErrorReceived += SerialPort_ErrorReceived;

            //初始化端口号下拉框数据
            for (int i = 1; i < 256; i++)
            {
                comboBox1.Items.Add($"COM{i}");
            }

            //初始化波特率下拉框数据
            comboBox2.Items.Add(110);
            comboBox2.Items.Add(300);
            comboBox2.Items.Add(600);
            comboBox2.Items.Add(1200);
            comboBox2.Items.Add(2400);
            comboBox2.Items.Add(4800);
            comboBox2.Items.Add(9600);
            comboBox2.Items.Add(19200);
            comboBox2.Items.Add(38400);
            comboBox2.Items.Add(56000);
            comboBox2.Items.Add(57600);
            comboBox2.Items.Add(115200);

            //初始化数据位下拉框数据
            comboBox3.Items.Add(5);
            comboBox3.Items.Add(6);
            comboBox3.Items.Add(7);
            comboBox3.Items.Add(8);

            //初始化校验位下拉框数据
            foreach (var item in Enum.GetValues(typeof(Parity)))
            {
                comboBox4.Items.Add(item);
            }

            //初始化停止位下拉框数据
            foreach (var item in Enum.GetValues(typeof(StopBits)))
            {
                comboBox5.Items.Add(item);
            }

            //初始化编码格式下拉框数据
            foreach (var item in Encoding.GetEncodings())
            {
                comboBox6.Items.Add(item.Name);
            }

            //初始化赋值
            comboBox1.Text = "COM1";
            comboBox2.Text = "9600";
            comboBox3.Text = "8";
            comboBox4.Text = "None";
            comboBox5.Text = "One";
        }

        /// <summary>
        /// 端口发生错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            textBox1.Invoke(new Action(() =>
            {
                textBox1.Text += $"\r\n===========ERROR START===========\r\n";
            }));
            //读取缓存区数据
            while (SerialPort.BytesToRead > 0)
            {
                int length = SerialPort.BytesToRead;
                byte[] buffer = new byte[length];
                SerialPort.Read(buffer, 0, length);

                textBox1.Invoke(new Action(() =>
                {
                    if (ReadIsHex)
                    {
                        textBox1.Text += $"{buffer.ToHex()} ";
                    }
                    else
                    {
                        textBox1.Text += SerialPort.Encoding.GetString(buffer);
                    }
                }));
            }
            textBox1.Invoke(new Action(() =>
            {
                textBox1.Text += $"\r\n===========ERROR END===========\r\n";
            }));
        }

        /// <summary>
        /// 端口接收数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //读取缓存区数据
            while (SerialPort.BytesToRead > 0)
            {
                int length = SerialPort.BytesToRead;
                byte[] buffer = new byte[length];
                SerialPort.Read(buffer, 0, length);

                textBox1.Invoke(new Action(() =>
                {
                    if (ReadIsHex)
                    {
                        textBox1.Text += $"{buffer.ToHex()} ";
                    }
                    else
                    {
                        textBox1.Text += SerialPort.Encoding.GetString(buffer);
                    }
                }));
            }
        }

        /// <summary>
        /// 打开/关闭按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "打开")//关闭状态
            {
                string portName = comboBox1.Text.Trim();
                if (string.IsNullOrEmpty(portName))
                {
                    MessageBox.Show("端口号不能为空！");
                    return;
                }

                if (!int.TryParse(comboBox2.Text.Trim(), out int baudRate))
                {
                    MessageBox.Show("波特率错误！");
                    return;
                }

                if (!int.TryParse(comboBox3.Text.Trim(), out int dataBits))
                {
                    MessageBox.Show("数据位错误！");
                    return;
                }

                if (!Enum.TryParse(comboBox4.Text.Trim(), out Parity parity))
                {
                    MessageBox.Show("校验位错误！");
                    return;
                }

                if (!Enum.TryParse(comboBox5.Text.Trim(), out StopBits stopBits))
                {
                    MessageBox.Show("停止位错误！");
                    return;
                }

                Encoding encoding = Encoding.Default;
                string encodingName = comboBox6.Text.Trim();
                if (!string.IsNullOrEmpty(encodingName))
                {
                    foreach (var item in Encoding.GetEncodings())
                    {
                        if (string.Equals(item.Name, encodingName, StringComparison.OrdinalIgnoreCase))
                        {
                            encoding = item.GetEncoding();
                            break;
                        }
                    }
                }

                //串口基本信息设置
                SerialPort.PortName = portName;
                SerialPort.BaudRate = baudRate;
                SerialPort.DataBits = dataBits;
                SerialPort.Parity = parity;
                SerialPort.StopBits = stopBits;
                SerialPort.Encoding = encoding;

                //更多属性配置
                //ReadBufferSize 输入缓存区大小 默认：4096
                //WriteBufferSize 输出缓存区大小 默认：2048
                //ReadTimeout 读超时 默认：-1
                //WriteTimeout 写超时 默认：-1
                //RtsEnable 启用请求发送RTS信号 默认：False
                //DtrEnable 终端就绪DTR信号 默认：False

                if (!SerialPort.IsOpen)
                {
                    try
                    {
                        SerialPort.Open();
                        button1.Text = "关闭";
                        comboBox1.Enabled = false;
                        comboBox2.Enabled = false;
                        comboBox3.Enabled = false;
                        comboBox4.Enabled = false;
                        comboBox5.Enabled = false;
                        comboBox6.Enabled = false;
                        button2.Enabled = true;
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show($"串口打开失败：{exc.Message}");
                    }
                }
            }
            else//打开状态
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
                }
                SerialPort.Dispose();
                button1.Text = "打开";
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
                comboBox6.Enabled = true;
                button2.Enabled = false;
            }
        }

        /// <summary>
        /// 接收区HEX显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ReadIsHex = checkBox1.Checked;
        }

        /// <summary>
        /// 发送区HEX内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            WriteIsHex = checkBox2.Checked;
        }

        /// <summary>
        /// 发送按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button2_Click(object sender, EventArgs e)
        {
            byte[] buffer;
            if (WriteIsHex)
            {
                buffer = textBox2.Text.Trim().ReHexToBuffer();
            }
            else
            {
                buffer = SerialPort.Encoding.GetBytes(textBox2.Text);
            }

            if (buffer.Length > 0)
            {
                if (buffer.Length <= SerialPort.WriteBufferSize)
                {
                    SerialPort.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    MessageBox.Show("当前内容超出了输出缓存大小，发送失败！");
                }
            }
        }
    }

    /// <summary>
    /// 字符串拓展方法
    /// </summary>
    public static partial class StringExtension
    {
        /// <summary>
        /// 将指定字符串转换为十六进制Hex字符串形式。
        /// </summary>
        /// <param name="input">要转换的原始字符串。</param>
        /// <param name="e">编码</param>
        /// <returns>转换后的内容</returns>
        public static string ToHex(this string input, Encoding e = null)
        {
            e = e ?? Encoding.UTF8;
            byte[] byteArray = e.GetBytes(input);
            return BitConverter.ToString(byteArray).Replace('-', ' ');
        }

        /// <summary>
        /// 将十六进制Hex字符串转换为原始字符串。
        /// </summary>
        /// <param name="input">十六进制字符串内容</param>
        /// <param name="e">编码</param>
        /// <returns>原始字符串内容</returns>
        public static string ReHex(this string input, Encoding e = null)
        {
            e = e ?? Encoding.UTF8;
            input = Regex.Replace(input, "[^0-9A-F]", "");
            if (input.Length <= 0) return "";
            byte[] vBytes = new byte[input.Length / 2];
            for (int i = 0; i < vBytes.Length; i++)
                vBytes[i] = byte.Parse(input.Substring(i * 2, 2), NumberStyles.HexNumber);
            return e.GetString(vBytes);
        }

        /// <summary>
        /// 将十六进制Hex字符串转换为二进制数据。
        /// </summary>
        /// <param name="input">十六进制字符串内容</param>
        /// <returns>原始字符串内容</returns>
        public static byte[] ReHexToBuffer(this string input)
        {
            input = Regex.Replace(input, "[^0-9A-F]", "");
            if (string.IsNullOrEmpty(input)) return new byte[0];
            byte[] vBytes = new byte[input.Length / 2];
            for (int i = 0; i < vBytes.Length; i++)
                vBytes[i] = byte.Parse(input.Substring(i * 2, 2), NumberStyles.HexNumber);
            return vBytes;
        }
    }

    /// <summary>
    /// 二进制数据拓展方法
    /// </summary>
    public static partial class BufferExtension
    {
        /// <summary>
        /// 将指定二进制数据转换为十六进制Hex字符串形式。
        /// </summary>
        /// <param name="input">要转换的二进制数据。</param>
        /// <returns>转换后的内容</returns>
        public static string ToHex(this byte[] input)
        {
            return BitConverter.ToString(input).Replace('-', ' ');
        }
    }
}
