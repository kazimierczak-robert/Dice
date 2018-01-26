using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiceClient
{
    static class Program
    {
        public static bool isServer = false;
        public static string playerName = "";
        public static string IPAddress = "";
        public static SynchronousClient client = null;
        public static AsynchronousSocketListener server = null;
        public static Thread workerThread = null;
        public static PlayWindow playWindow = null;
        public static volatile bool allowToRun = false;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            StartWindow startWindow;

            DialogResult dialogResult = DialogResult.No;

            short assignedSeat = -1;

            while (dialogResult != DialogResult.Cancel)
            {
                if (dialogResult == DialogResult.No)
                {
                    isServer = false;
                    playerName = "";
                    IPAddress = "";
                    client = null;
                    server = null;
                    workerThread = null;
                    startWindow = new StartWindow();
                    allowToRun = true;
                    dialogResult = startWindow.ShowDialog();
                }
                if (dialogResult == DialogResult.Yes)
                {
                    allowToRun = true;
                    if (isServer)
                    {
                        server = new AsynchronousSocketListener(System.Net.IPAddress.Parse(IPAddress));
                        workerThread = new Thread(server.StartListening);
                        workerThread.Start();
                        assignedSeat = 0;
                        Thread.Sleep(500);
                        if (allowToRun)
                        {
                            playWindow = new PlayWindow(assignedSeat);
                            dialogResult = playWindow.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nie można uruchomić więcej niż 1 serwer!", "Błąd!");
                            dialogResult = DialogResult.No;
                        }
                    }
                    else
                    {
                        try
                        {
                            client = new SynchronousClient(IPAddress);
                            string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("00 " + playerName)) + " <EOF>";
                            client.Send(message);
                            string response = client.Receive();
                            if (response.Substring(0,2) != "OK")
                            {
                                if (response[3] == '0')
                                {
                                    MessageBox.Show("Nie można połączyć się z serwerem.\nWybrany login jest już zajęty.", "Błąd");
                                }
                                else
                                {
                                    MessageBox.Show("Nie można połączyć się z serwerem.\nSerwer jest pełny.", "Błąd");
                                }
                                client.Disconnect();
                                dialogResult = DialogResult.No;
                                continue;
                            }
                            else
                            {
                                assignedSeat = short.Parse(response[3].ToString());
                                playWindow = new PlayWindow(assignedSeat);
                                dialogResult = playWindow.ShowDialog();      
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Problem z połączeniem z serwerem lub adres jest niepoprawny!", "Błąd!");
                            dialogResult = DialogResult.No;
                            continue;
                        }
                    }
                }
            }
        }
    }
}
