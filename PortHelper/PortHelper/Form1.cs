using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
namespace PortHelper
{
    public partial class Form1 : Form
    {
        private SerialPort SerialPort = new SerialPort();

        private long received_count = 0;//接收计数
        public Form1()
        {
            InitializeComponent();
            InitConfig();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void InitConfig()
        {
            cbSerial.Items.AddRange(SerialPort.GetPortNames());
            if(cbSerial.Items.Count > 0)
            {
                cbSerial.SelectedIndex = 0;
            }
            else
            {
                cbSerial.Text = "未检测到串口";
            }
            string[] baudData = new string[] { "9600", "19200", "38400","57600", "115200","Custom"};
            cbBaudRate.Items.AddRange(baudData);
            cbBaudRate.SelectedIndex = 0;
            
            cbDataBits.SelectedIndex = 3;
            cbStop.SelectedIndex = 0;
            cbParity.SelectedIndex = 0;
            //所有的下拉框都不能随意输入内容,只能当选中特定项时才能输入
            cbSerial.DropDownStyle = ComboBoxStyle.DropDownList;
            cbBaudRate.DropDownStyle = ComboBoxStyle.DropDownList;
            cbDataBits.DropDownStyle = ComboBoxStyle.DropDownList;
            cbStop.DropDownStyle = ComboBoxStyle.DropDownList;
            cbParity.DropDownStyle = ComboBoxStyle.DropDownList;
        
            //默认状态下,编码方式设置为ASCII
            radioButton1.Checked = true;

            SerialPort.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);
        }

        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //添加了这个等待时间再去读取就不会出现一个完整的内容要读两次;一定要写在开头,不然也没用
            System.Threading.Thread.Sleep(50);
            //开辟缓冲区
            byte[] reDatas = new byte[SerialPort.BytesToRead];
            //从缓冲区读取数据到byte[]
            SerialPort.Read(reDatas, 0, reDatas.Length);
            
            //实现数据解码
            AddData(reDatas);
        }

        public void AddData(byte[] Data)
        {
            if(radioButton1.Checked)
            {
                AddContent(new ASCIIEncoding().GetString(Data));
                
            }
            else if(radioButton2.Checked)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Data.Length; i++)
                {
                    sb.AppendFormat("{0:x2}" + " ", Data[i]);
                }
                AddContent(sb.ToString().ToUpper());
            }
            /*  else
            {
                string text = Encoding.GetEncoding("GB2312").GetString(Data);
                AddContent(text);
            }*/
        }

        private void AddContent(string content)
         {
            BeginInvoke(new MethodInvoker(delegate
            {
                txtRecv.AppendText(content + "\r\n");
                SerialPort.DiscardInBuffer();
            }));
            
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            if(SerialPort.IsOpen == false)
            {
                
                //设置串口相关属性
                SerialPort.PortName = cbSerial.SelectedItem.ToString();
                SerialPort.BaudRate = Convert.ToInt32(cbBaudRate.Text.ToString());
                SerialPort.DataBits = Convert.ToInt32(cbDataBits.SelectedItem.ToString());
                SerialPort.StopBits = (StopBits)Convert.ToInt32(cbStop.SelectedItem.ToString());
                SerialPort.Parity = (Parity)Convert.ToInt32(cbParity.SelectedIndex.ToString());
                try
                {
                    SerialPort.Open();
                    send_button.Enabled = true;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("未能成功开启串口");
                    return;
                }

                start_button.Text = "关闭";
            }
            else
            {
                try
                {
                    SerialPort.Close();
                    send_button.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("关闭失败");
                    
                }
                start_button.Text = "开启";
            }

            cbSerial.Enabled = !SerialPort.IsOpen;
            cbBaudRate.Enabled = !SerialPort.IsOpen;
            cbDataBits.Enabled = !SerialPort.IsOpen;
            cbStop.Enabled = !SerialPort.IsOpen;
            cbParity.Enabled =!SerialPort.IsOpen;
        }

        private void send_button_Click(object sender, EventArgs e)
        {
            byte[] sendData = null;
            if(radioButton1.Checked)
            {
                sendData = Encoding.ASCII.GetBytes(txtSend.Text.Trim());
            }
            else if(radioButton2.Checked)
            {
                sendData = ToHexFromString(txtSend.Text.Trim());
            }
            //else
            //{
            //    sendData = Encoding.Default.GetBytes(txtSend.Text.Trim());
            //}

            SerialPort.Write(sendData, 0, sendData.Length);

        }

        private byte[] ToHexFromString(string str)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);
            StringBuilder builder = new StringBuilder();
            for(int i=0;i<data.Length; i++)
            {
                builder.AppendFormat("{0:X2}" + " ", data[i]);
            }
            string strTemp = builder.ToString().Trim();
            string[] chars = strTemp.Split(new char[] {' '});

            byte[] hexByte = new byte[chars.Length];
            for(int i=0; i<chars.Length; i++)
            {
                hexByte[i] = Convert.ToByte(chars[i],16);
            }
            return hexByte;
        }
        private void clear_btn_Click(object sender, EventArgs e)
        {
            txtRecv.Clear();
        }
    }
}
