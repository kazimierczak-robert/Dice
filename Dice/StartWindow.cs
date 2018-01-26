using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiceClient
{
    public partial class StartWindow : Form
    {
        public StartWindow()
        {
            InitializeComponent();
            var Wireless80211IPAddresses = GetAllLocalIPv4(NetworkInterfaceType.Wireless80211);
            var EthernetIPAddresses = GetAllLocalIPv4(NetworkInterfaceType.Ethernet);
            var result = Wireless80211IPAddresses.Union(EthernetIPAddresses).ToList();
            result.Add("127.0.0.1");
            CBIPAddress.DataSource = result;
            if(result.Count == 0)
            {
                MessageBox.Show("Brak dostępnych interfejsów sieciowych!\nWłącz minimum jeden z nich i spróbuj ponownie.", "Błąd");
                DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void BConnect_Click(object sender, EventArgs e)
        {
            if (TBLogin.ForeColor == Color.Gray || TBServer.ForeColor == Color.Gray)
            {
                MessageBox.Show("Minimum jedno z wymaganych pól jest puste!", "Błąd");
                return;
            }
            Program.isServer = false;
            Program.playerName = TBLogin.Text;
            Program.IPAddress = TBServer.Text;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void BCreate_Click(object sender, EventArgs e)
        {
            if (TBLogin.ForeColor == Color.Gray)
            {
                MessageBox.Show("Nie podano loginu!", "Błąd");
                return;
            }

            Program.isServer = true;
            Program.playerName = TBLogin.Text;
            Program.IPAddress = CBIPAddress.SelectedValue.ToString();
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void Textbox_Enter(object sender, EventArgs e)
        {
            if ((((TextBox)sender).Name == "TBLogin" && ((TextBox)sender).Text == "Login") || (((TextBox)sender).Name == "TBServer" && ((TextBox)sender).Text == "Adres IPv4 serwera"))
            {
                ((TextBox)sender).Text = "";
                ((TextBox)sender).ForeColor = SystemColors.WindowText;
            }
        }

        private void Textbox_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(((TextBox)sender).Text))
            {
                if (((TextBox)sender).Name == "TBLogin")
                {
                    ((TextBox)sender).Text = "Login";
                }
                if (((TextBox)sender).Name == "TBServer")
                {
                    ((TextBox)sender).Text = "Adres IPv4 serwera";
                }
                ((TextBox)sender).ForeColor = Color.Gray;
            }
        }

        public string[] GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }
    }
}
