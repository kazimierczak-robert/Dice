using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiceClient
{
    public partial class PlayWindow : Form
    {
        short playerID = -1;
        Random random = new Random();
        public List<Button> diceButtons = new List<Button>();
        public List<Button> seatButtons = new List<Button>();
        public List<ArrayList> playerButtons = new List<ArrayList>();
        bool isReady = false;
        bool isMyTurn = true;
        int throwCounter = 0;
        volatile bool stopThread;
        public bool isEnd = false;
        public bool isStarted = false;
        bool serverErrorExit = false;
        public int chosenPlayer = -1;
        public PlayWindow(short assignedSeat)
        {
            InitializeComponent();
            this.Text += (" - " + Program.playerName);
            playerID = assignedSeat;
            Button button;
            Label label;
            stopThread = false;

            for (int j = 1; j < 15; j++)
            {
                if (j != 7)
                {
                    label = new Label();
                    label.BorderStyle = BorderStyle.FixedSingle;
                    label.Size = new Size(54, 24);
                    label.Location = new Point(7, 7 + 25 * j);
                    if (j < 7)
                    {
                        label.Text = j.ToString();
                    }
                    else if (j == 8)
                    {
                        label.Text = "3x";
                    }
                    else if (j == 9)
                    {
                        label.Text = "4x";
                    }
                    else if (j == 10)
                    {
                        label.Text = "3+2x";
                    }
                    else if (j == 11)
                    {
                        label.Text = "Mały strit";
                    }
                    else if (j == 12)
                    {
                        label.Text = "Duży strit";
                    }
                    else if (j == 13)
                    {
                        label.Text = "Generał";
                    }
                    else if (j == 14)
                    {
                        label.Text = "Szansa";
                    }
                    
                    label.TextAlign = ContentAlignment.MiddleLeft;
                    label.BackColor = Color.LightGray;
                    this.Controls.Add(label);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                playerButtons.Add(new ArrayList());
                for (int j = 0; j < 16; j++)
                {
                    if (j == 0 || j == 7 || j == 15)
                    {
                        label = new Label();
                        label.BorderStyle = BorderStyle.FixedSingle;
                        label.Size = new Size(49, 24);
                        label.Location = new Point(62 + 50 * i, 7 + 25 * j);
                        if (j == 0)
                        {
                            label.Text = "#" + (i + 1).ToString();
                        }
                        else
                        {
                            label.Text = "0";
                        }

                        label.TextAlign = ContentAlignment.MiddleCenter;
                        label.BackColor = Color.LightGray;
                        label.Name = i.ToString() + j.ToString();

                        this.Controls.Add(label);
                        playerButtons[i].Add(label);
                    }
                    else
                    {
                        button = new Button();
                        button.Size = new Size(51, 26);
                        button.Location = new Point(61 + 50 * i, 6 + 25 * j);
                        button.Text = "0";
                        if(i == playerID)
                        {
                            button.Enabled = true;
                        }
                        else
                        {
                            button.Enabled = false;
                        }
                        button.TabStop = false;
                        button.FlatAppearance.BorderColor = System.Drawing.Color.Blue;
                        button.ForeColor = Color.FromArgb(85, 85, 85);
                        button.BackColor = Color.FromArgb(255, 255, 255);
                        button.Name = i.ToString() + j.ToString();
                        button.Click += new System.EventHandler(this.Choice_Click);
                        this.Controls.Add(button);
                        playerButtons[i].Add(button);
                    }
                }
            }

            for (int i = 0; i < 5; i++)
            {
                button = new Button();
                button.Name = "BDice" + (i+1).ToString();
                button.Size = new Size(66, 66);
                button.Location = new Point(268, 13 + 80 * i);
                button.TabStop = false;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.Blue;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
                button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
                button.BackgroundImage = Properties.Resources.dice1;
                button.BackgroundImageLayout = ImageLayout.Center;
                button.Click += new System.EventHandler(this.Dice_Click);
                button.Enabled = false;
                this.Controls.Add(button);
                diceButtons.Add(button);
            }

            GroupBox groupbox;
            for (int i = 0; i < 4; i++)
            {
                groupbox = new GroupBox();
                groupbox.Size = new Size(200, 70);
                groupbox.Location = new Point(340, 47 + 80 * i);
                groupbox.Text = "#" + (i + 1).ToString();
                groupbox.Name = "#" + (i + 1).ToString();
                this.Controls.Add(groupbox);
                button = new Button();
                button.Size = new Size(180, 50);
                button.Location = new Point(10, 15);
                button.TabStop = false;
                button.FlatStyle = FlatStyle.Flat;
                button.ForeColor = Color.Black;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseDownBackColor = Color.FromArgb(239, 239, 239);
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 239, 239);
                if (i == playerID)
                {
                    button.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                    button.Text = Program.playerName;
                }
                else
                {
                    button.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Regular);
                    button.Text = "Miejsce #" + (i + 1).ToString();
                }

                button.Name = "BSeat" + i.ToString();
                groupbox.Controls.Add(button);
                seatButtons.Add(button);
            }

            button = new Button();
            button.Size = new Size(180, 25);
            button.Location = new Point(350, 370);
            button.Text = "Jestem gotowy";
            button.Name = "BReady";
            button.TabStop = false;
            button.Click += new System.EventHandler(this.readyButton_Click);
            this.Controls.Add(button);

            if(Program.isServer==false)
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    /* run your code here */
                    while (!stopThread)
                    {
                        try
                        {
                            string response = Program.client.Receive();
                            response = Encoding.UTF8.GetString(Convert.FromBase64String(response.Substring(0, response.Length - 6)));
                            if (response.Substring(0, 2) == "01")
                            {
                                response = response.Substring(3);
                                var result = response.Split(' ');
                                foreach (var item in result)
                                {
                                    var oneSeat = item.Split(':');
                                    int index = Int32.Parse(oneSeat[0].ToString());

                                    Color color;
                                    if (oneSeat.Count() > 2 && oneSeat[2] == "1")
                                    {
                                        color = Color.Green;
                                    }
                                    else
                                    {
                                        color = Color.Black;
                                    }

                                    if (oneSeat[1] != "")
                                    {
                                        MethodInvoker inv = delegate
                                        {
                                            seatButtons[index].Text = oneSeat[1];
                                            seatButtons[index].ForeColor = color;
                                        };
                                        this.Invoke(inv);
                                    }
                                    else
                                    {
                                        MethodInvoker inv = delegate
                                        {
                                            seatButtons[index].Text = "Miejsce #" + (index + 1).ToString();
                                            seatButtons[index].ForeColor = color;
                                        };
                                        this.Invoke(inv);
                                    }
                                }
                            }
                            else if (response.Substring(0, 2) == "04")
                            {
                                response = response.Substring(3);
                                var result = response.Split(' ');
                                foreach (var item in result)
                                {
                                    var oneSeat = item.Split(':');
                                    int index = Int32.Parse(oneSeat[0].ToString());
                                    if (oneSeat[1] == "1")
                                    {
                                        MethodInvoker inv = delegate
                                        {
                                            seatButtons[index].ForeColor = System.Drawing.Color.Green;
                                        };
                                        this.Invoke(inv);
                                    }
                                    else
                                    {
                                        MethodInvoker inv = delegate
                                        {
                                            seatButtons[index].ForeColor = System.Drawing.Color.Black;
                                        };
                                        this.Invoke(inv);
                                    }
                                }
                            }
                            else if (response.Substring(0, 2) == "05")
                            {
                                MethodInvoker inv = delegate
                                {
                                    Button b = (Button)this.Controls.Find("BReady", false).First();
                                    b.Text = "Losuj";
                                    b.Enabled = false;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        seatButtons[i].ForeColor = System.Drawing.Color.Black;
                                    }
                                };
                                this.Invoke(inv);
                                isStarted = true;
                            }
                            else if (response.Substring(0, 2) == "06")
                            {
                                chosenPlayer = Int32.Parse(response[3].ToString());
                                MethodInvoker inv = delegate
                                {
                                    foreach (var dice in diceButtons)
                                    {
                                        dice.FlatAppearance.BorderSize = 0;
                                    }
                                    if (chosenPlayer == playerID)
                                    {
                                        this.Controls.Find("BReady", false).First().Enabled = true;
                                        foreach (var dice in diceButtons)
                                        {
                                            dice.Enabled = true;
                                        }
                                    }
                                    else
                                    {
                                        this.Controls.Find("BReady", false).First().Enabled = false;
                                        foreach (var dice in diceButtons)
                                        {
                                            dice.Enabled = false;
                                        }
                                    }
                                    this.Controls.Find("BReady", false).First().Focus();
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (i == chosenPlayer)
                                        {
                                            this.Controls.Find("#" + (i + 1).ToString(), false).First().Font = new System.Drawing.Font(Font.FontFamily, Font.Size, System.Drawing.FontStyle.Bold);
                                            ((Label)playerButtons[i][0]).Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                                        }
                                        else
                                        {
                                            this.Controls.Find("#" + (i + 1).ToString(), false).First().Font = new System.Drawing.Font(Font.FontFamily, Font.Size, System.Drawing.FontStyle.Regular);
                                            ((Label)playerButtons[i][0]).Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Regular);
                                        }
                                    }
                                };
                                this.Invoke(inv);
                            }
                            else if (response.Substring(0, 2) == "07")
                            {
                                var splittedResponse = response.Split(' ');

                                var randedDices = splittedResponse[1].Split(':');
                                var lockedDices = splittedResponse[2].Split(':');
                                var options = splittedResponse[3].Split(':');

                                MethodInvoker inv = delegate
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        Button btn = diceButtons[i];
                                        if (randedDices[i] == "1")
                                        {
                                            btn.BackgroundImage = Properties.Resources.dice1;
                                        }
                                        else if (randedDices[i] == "2")
                                        {
                                            btn.BackgroundImage = Properties.Resources.dice2;
                                        }
                                        else if (randedDices[i] == "3")
                                        {
                                            btn.BackgroundImage = Properties.Resources.dice3;
                                        }
                                        else if (randedDices[i] == "4")
                                        {
                                            btn.BackgroundImage = Properties.Resources.dice4;
                                        }
                                        else if (randedDices[i] == "5")
                                        {
                                            btn.BackgroundImage = Properties.Resources.dice5;
                                        }
                                        else if (randedDices[i] == "6")
                                        {
                                            btn.BackgroundImage = Properties.Resources.dice6;
                                        }

                                        if (lockedDices[i] == "1")
                                        {
                                            btn.FlatAppearance.BorderSize = 2;
                                        }
                                        else
                                        {
                                            btn.FlatAppearance.BorderSize = 0;
                                        }
                                    }

                                //Uzupełnij wartości jeśli jest enabled i na szaro tekst
                                for (int i = 0; i < 6; i++)
                                    {
                                        Button btn = (Button)playerButtons[chosenPlayer][i + 1];
                                        if (btn.BackColor != Color.FromArgb(239, 239, 239))
                                        {
                                            btn.Text = options[i].ToString();
                                            if (chosenPlayer == playerID)
                                            {
                                                btn.Enabled = true;
                                                btn.ForeColor = Color.Black;
                                            }
                                        }
                                    }

                                    for (int i = 6; i < 13; i++)
                                    {
                                        Button btn = (Button)playerButtons[chosenPlayer][i + 2];
                                        if (btn.BackColor != Color.FromArgb(239, 239, 239))
                                        {
                                            btn.Text = options[i].ToString();
                                            if (chosenPlayer == playerID)
                                            {
                                                btn.Enabled = true;
                                                btn.ForeColor = Color.Black;
                                            }
                                        }
                                    }
                                };
                                this.Invoke(inv);
                            }
                            else if (response.Substring(0, 2) == "10")
                            {
                                int clickedChoice = Int32.Parse(response.Split(' ')[1].Split(':')[1]);
                                if (clickedChoice < 6)
                                {
                                    clickedChoice += 1;
                                }
                                else
                                {
                                    clickedChoice += 2;
                                }

                                MethodInvoker inv = delegate
                                {
                                    Button sender = ((Button)playerButtons[chosenPlayer][clickedChoice]);
                                    if (clickedChoice < 7)
                                    {
                                        string scoreFirstStr = ((Label)playerButtons[chosenPlayer][7]).Text;
                                        int scoreFirst = Int32.Parse(scoreFirstStr.Split('+')[0]) + Int32.Parse(sender.Text);
                                        string scoreFinalStr = ((Label)playerButtons[chosenPlayer][15]).Text;
                                        int scoreFinal = Int32.Parse(scoreFinalStr) + Int32.Parse(sender.Text);
                                        if (scoreFirst > 62)
                                        {
                                            if (scoreFirstStr.Last() != '+')
                                            {
                                                scoreFirst += 35;
                                                scoreFinal += 35;
                                            }
                                            ((Label)playerButtons[chosenPlayer][7]).Text = scoreFirst.ToString() + "+";
                                        }
                                        else
                                        {
                                            ((Label)playerButtons[chosenPlayer][7]).Text = scoreFirst.ToString();
                                        }

                                        //sprawdź czy dżoker
                                        bool isJoker = true;
                                        for (int i = 1; i < 5; i++)
                                        {
                                            if (diceButtons[0].BackgroundImage == diceButtons[i].BackgroundImage)
                                            {
                                                isJoker = false;
                                                break;
                                            }
                                        }
                                        if (isJoker)
                                        {
                                            int generalDiceValue = -1;
                                            if (diceButtons[0].BackgroundImage == Properties.Resources.dice1) { generalDiceValue = 1; }
                                            else if (diceButtons[0].BackgroundImage == Properties.Resources.dice2) { generalDiceValue = 2; }
                                            else if (diceButtons[0].BackgroundImage == Properties.Resources.dice3) { generalDiceValue = 3; }
                                            else if (diceButtons[0].BackgroundImage == Properties.Resources.dice4) { generalDiceValue = 4; }
                                            else if (diceButtons[0].BackgroundImage == Properties.Resources.dice5) { generalDiceValue = 5; }
                                            else { generalDiceValue = 6; }

                                            if (((Button)playerButtons[chosenPlayer][13]).BackColor == Color.FromArgb(239, 239, 239) && ((Button)playerButtons[chosenPlayer][13]).Text != "0" && generalDiceValue == clickedChoice)
                                            {
                                                scoreFinal += 100;
                                                int general = Int32.Parse(((Button)playerButtons[chosenPlayer][13]).Text);
                                                ((Button)playerButtons[chosenPlayer][13]).Text = (general + 100).ToString();
                                            }
                                        }
                                        ((Label)playerButtons[chosenPlayer][15]).Text = scoreFinal.ToString();
                                    }
                                    else
                                    {
                                        string scoreFinalStr = ((Label)playerButtons[chosenPlayer][15]).Text;
                                        int scoreFinal = Int32.Parse(scoreFinalStr) + Int32.Parse(sender.Text);

                                        //sprawdź czy dżoker
                                        bool isJoker = true;
                                        for (int i = 1; i < 5; i++)
                                        {
                                            if (diceButtons[0].BackgroundImage == diceButtons[i].BackgroundImage)
                                            {
                                                isJoker = false;
                                                break;
                                            }
                                        }
                                        if (isJoker)
                                        {
                                            int generalDiceValue = -1;
                                            if (diceButtons[0].BackgroundImage == Properties.Resources.dice1) { generalDiceValue = 1; }
                                            else if (diceButtons[0].BackgroundImage == Properties.Resources.dice2) { generalDiceValue = 2; }
                                            else if (diceButtons[0].BackgroundImage == Properties.Resources.dice3) { generalDiceValue = 3; }
                                            else if (diceButtons[0].BackgroundImage == Properties.Resources.dice4) { generalDiceValue = 4; }
                                            else if (diceButtons[0].BackgroundImage == Properties.Resources.dice5) { generalDiceValue = 5; }
                                            else { generalDiceValue = 6; }

                                            if (((Button)playerButtons[chosenPlayer][13]).BackColor == Color.FromArgb(239, 239, 239) && ((Button)playerButtons[chosenPlayer][13]).Text != "0" && ((Button)playerButtons[chosenPlayer][generalDiceValue]).BackColor == Color.FromArgb(239, 239, 239))
                                            {
                                                scoreFinal += 100;
                                                int general = Int32.Parse(((Button)playerButtons[chosenPlayer][13]).Text);
                                                ((Button)playerButtons[chosenPlayer][13]).Text = (general + 100).ToString();
                                            }
                                        }

                                        ((Label)playerButtons[chosenPlayer][15]).Text = scoreFinal.ToString();
                                    }

                                    for (int i = 0; i < 6; i++)
                                    {
                                        if (i != clickedChoice - 1 && ((Button)playerButtons[chosenPlayer][i + 1]).BackColor != Color.FromArgb(239, 239, 239))
                                        {
                                            ((Button)playerButtons[chosenPlayer][i + 1]).Text = "0";
                                        }
                                        else
                                        {
                                            ((Button)playerButtons[chosenPlayer][i + 1]).BackColor = Color.FromArgb(239, 239, 239);
                                            ((Button)playerButtons[chosenPlayer][i + 1]).ForeColor = Color.FromArgb(29, 29, 29);
                                        }
                                    }
                                    for (int i = 6; i < 13; i++)
                                    {
                                        if (i != clickedChoice - 2 && ((Button)playerButtons[chosenPlayer][i + 2]).BackColor != Color.FromArgb(239, 239, 239))
                                        {
                                            ((Button)playerButtons[chosenPlayer][i + 2]).Text = "0";
                                        }
                                        else
                                        {
                                            ((Button)playerButtons[chosenPlayer][i + 2]).BackColor = Color.FromArgb(239, 239, 239);
                                            ((Button)playerButtons[chosenPlayer][i + 2]).ForeColor = Color.FromArgb(29, 29, 29);
                                        }
                                    }
                                };
                                this.Invoke(inv);
                            }
                            else if (response.Substring(0, 2) == "12")
                            {
                                MessageBox.Show("Gracz " + (Int32.Parse(response.Split(' ')[1])+1).ToString() + " opuścił grę!\nGra będzie kontynuowana bez niego.", "Komunikat!");
                            }
                            else if (response.Substring(0, 2) == "13")
                            {
                                stopThread = true;
                                serverErrorExit = true;
                                MessageBox.Show("Hostujący gracz opuścił grę!\nGra zostanie przerwana.", "Błąd!");
                                this.Close();
                            }
                            else if (response.Substring(0, 2) == "14")
                            {
                                stopThread = true;
                                MethodInvoker inv = delegate
                                {
                                    string winnersID = response.Split(' ')[1];
                                    if (winnersID.Split(':').Count() == 1)
                                    {
                                        MessageBox.Show("Wygrał gracz " + winnersID + "!", "Koniec gry!");
                                    }
                                    else
                                    {
                                        winnersID.Replace(":", ", ");
                                        MessageBox.Show("Wygrali gracze " + winnersID + "!", "Koniec gry!");
                                    }
                                };
                                this.Invoke(inv);
                                isEnd = true;
                            }
                            else
                            {
                                stopThread = true;
                                MessageBox.Show("Problem z połączeniem z serwerem!", "Błąd!");
                                this.Close();
                            }
                        }
                        catch(Exception)
                        {
                            stopThread = true;
                            if (this.IsDisposed)
                            {
                                MethodInvoker inv = delegate
                              {
                                  this.Close();
                              };
                                this.Invoke(inv);
                            }
                            
                        }
                    }
                }).Start();
            }
        }

        private void readyButton_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text != "Losuj")
            {
                if (isReady == false)
                {
                    isReady = true;
                    ((Button)sender).Text = "Nie jestem gotowy";
                    seatButtons[playerID].ForeColor = Color.Green;
                    if (Program.isServer == false)
                    {
                        string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("03 1")) + " <EOF>";
                        Program.client.Send(message);
                    }
                    else
                    {
                        string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("04 0:1")) + " <EOF>";
                        Program.server.SendToAll(message);

                        Program.server.startGame = true;
                        foreach (var onePair in Program.server.connectedSockets)
                        {
                            if (seatButtons[onePair.Key].ForeColor == System.Drawing.Color.Black)
                            {
                                Program.server.startGame = false;
                                break;
                            }
                        }
                        if(Program.server.connectedSockets.Count == 0)
                        {
                            Program.server.startGame = false;
                        }

                        if (Program.server.startGame == true)
                        {
                            message = Convert.ToBase64String(Encoding.UTF8.GetBytes("05")) + " <EOF>";
                            Program.server.SendToAll(message);
                            Thread.Sleep(300);


                            this.Controls.Find("BReady", false).First().Text = "Losuj";
                            for (int i = 0; i < 4; i++)
                            {
                                seatButtons[i].ForeColor = System.Drawing.Color.Black;
                            }

                            Program.server.roundCounter = 0;
                            Program.server.chosenPlayer = 0;
                            chosenPlayer = 0;
                            Program.server.nextTurn();
                        }
                    }
                }
                else
                {
                    isReady = false;
                    ((Button)sender).Text = "Jestem gotowy";
                    seatButtons[playerID].ForeColor = Color.Black;
                    if (Program.isServer == false)
                    {
                        string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("03 0")) + " <EOF>";
                        Program.client.Send(message);
                    }
                    else
                    {
                        string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("04 0:0")) + " <EOF>";
                        Program.server.SendToAll(message);
                    }
                }
            }
            else
            {
                throwCounter++;
                if (Program.isServer == false)
                {
                    var lockedDices = "";
                    for (int i = 0; i < 5; i++)
                    {
                        if (diceButtons[i].FlatAppearance.BorderSize == 2)
                        {
                            lockedDices += ":1";
                        }
                        else
                        {
                            lockedDices += ":0";
                        }
                    }
                    lockedDices = lockedDices.Substring(1);
                    string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("08 " + lockedDices)) + " <EOF>";
                    Program.client.Send(message);
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (diceButtons[i].FlatAppearance.BorderSize == 2)
                        {
                            Program.server.lockedDices[i] = true;
                        }
                        else
                        {
                            Program.server.lockedDices[i] = false;
                        }
                    }
                    Program.server.newThrow();
                }
                if (throwCounter==2)
                {
                    ((Button)sender).Enabled = false;
                }
            }
        }

        private void PlayWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.No;
            stopThread = true;

            if (isEnd == false)
            {
                if (Program.isServer == true)
                {
                    string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("13")) + " <EOF>";
                    Program.server.SendToAll(message);
                    Thread.Sleep(200);
                    foreach (var socket in Program.server.connectedSockets.Values)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    Program.server.RequestStop();
                    Program.workerThread.Join();
                }
                else
                {
                    if (serverErrorExit == false)
                    {
                        if (isStarted == false)
                        {
                            string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("02")) + " <EOF>";
                            Program.client.Send(message);
                            stopThread = true;
                            Thread.Sleep(200);
                            Program.client.Disconnect();
                        }
                        else if(isEnd==false)
                        {
                            string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("11")) + " <EOF>";
                            Program.client.Send(message);
                            stopThread = true;
                            Thread.Sleep(200);
                            Program.client.Disconnect();
                        }
                    }
                }
            }
        }

        private void Dice_Click(object sender, EventArgs e)
        {
            if (isMyTurn)
            {
                if (((Button)sender).FlatAppearance.BorderSize == 0)
                {
                    ((Button)sender).FlatAppearance.BorderSize = 2;
                }
                else
                {
                    ((Button)sender).FlatAppearance.BorderSize = 0;
                }
            }
            this.Controls.Find("BReady", false).First().Focus();
        }

        private void Choice_Click(object sender, EventArgs e)
        {
            if (playerID == chosenPlayer)
            {
                throwCounter = 0;
                ((Button)sender).ForeColor = Color.FromArgb(29, 29, 29);
                ((Button)sender).Click -= Choice_Click;
                this.Controls.Find("BReady", false).First().Focus();
                int clickedChoice = -1;
                for (int i = 0; i < 6; i++)
                {
                    Button btn = (Button)playerButtons[playerID][i + 1];
                    if (btn.ForeColor == Color.Black)
                    {
                        btn.Text = "0";
                        btn.ForeColor = Color.FromArgb(85, 85, 85);
                    }
                    if (btn == (Button)sender)
                    {
                        clickedChoice = i;
                    }
                }

                for (int i = 6; i < 13; i++)
                {
                    Button btn = (Button)playerButtons[playerID][i + 2];
                    if (btn.ForeColor == Color.Black)
                    {
                        btn.Text = "0";
                        btn.ForeColor = Color.FromArgb(85, 85, 85);
                    }
                    if (btn == (Button)sender)
                    {
                        clickedChoice = i;
                    }
                }

                if (clickedChoice < 6)
                {
                    string scoreFirstStr = ((Label)playerButtons[playerID][7]).Text;
                    int scoreFirst = Int32.Parse(scoreFirstStr.Split('+')[0]) + Int32.Parse(((Button)sender).Text);
                    string scoreFinalStr = ((Label)playerButtons[playerID][15]).Text;
                    int scoreFinal = Int32.Parse(scoreFinalStr) + Int32.Parse(((Button)sender).Text);
                    if (scoreFirst > 62)
                    {
                        if (scoreFirstStr.Last() != '+')
                        {
                            scoreFirst += 35;
                            scoreFinal += 35;
                        }
                        ((Label)playerButtons[playerID][7]).Text = scoreFirst.ToString() + "+";
                    }
                    else
                    {
                        ((Label)playerButtons[playerID][7]).Text = scoreFirst.ToString();
                    }

                    //sprawdź czy dżoker
                    bool isJoker = true;
                    for (int i = 1; i < 5; i++)
                    {
                        if (diceButtons[0].BackgroundImage == diceButtons[i].BackgroundImage)
                        {
                            isJoker = false;
                            break;
                        }
                    }
                    if (isJoker)
                    {
                        int generalDiceValue = -1;
                        if (diceButtons[0].BackgroundImage == Properties.Resources.dice1) { generalDiceValue = 1; }
                        else if (diceButtons[0].BackgroundImage == Properties.Resources.dice2) { generalDiceValue = 2; }
                        else if (diceButtons[0].BackgroundImage == Properties.Resources.dice3) { generalDiceValue = 3; }
                        else if (diceButtons[0].BackgroundImage == Properties.Resources.dice4) { generalDiceValue = 4; }
                        else if (diceButtons[0].BackgroundImage == Properties.Resources.dice5) { generalDiceValue = 5; }
                        else { generalDiceValue = 6; }

                        if (((Button)playerButtons[chosenPlayer][13]).BackColor == Color.FromArgb(239, 239, 239) && ((Button)playerButtons[chosenPlayer][13]).Text != "0" && generalDiceValue == clickedChoice)
                        {
                            scoreFinal += 100;
                            int general = Int32.Parse(((Button)playerButtons[chosenPlayer][13]).Text);
                            ((Button)playerButtons[chosenPlayer][13]).Text = (general + 100).ToString();
                        }
                    }

                    ((Label)playerButtons[playerID][15]).Text = scoreFinal.ToString();
                }
                else
                {
                    string scoreFinalStr = ((Label)playerButtons[playerID][15]).Text;
                    int scoreFinal = Int32.Parse(scoreFinalStr) + Int32.Parse(((Button)sender).Text);

                    //sprawdź czy dżoker
                    bool isJoker = true;
                    for (int i = 1; i < 5; i++)
                    {
                        if(diceButtons[0].BackgroundImage == diceButtons[i].BackgroundImage)
                        {
                            isJoker = false;
                            break;
                        }
                    }
                    if (isJoker)
                    {
                        int generalDiceValue = -1;
                        if (diceButtons[0].BackgroundImage == Properties.Resources.dice1) { generalDiceValue = 1; }
                        else if (diceButtons[0].BackgroundImage == Properties.Resources.dice2) { generalDiceValue = 2; }
                        else if (diceButtons[0].BackgroundImage == Properties.Resources.dice3) { generalDiceValue = 3; }
                        else if (diceButtons[0].BackgroundImage == Properties.Resources.dice4) { generalDiceValue = 4; }
                        else if (diceButtons[0].BackgroundImage == Properties.Resources.dice5) { generalDiceValue = 5; }
                        else { generalDiceValue = 6; }

                        if (((Button)playerButtons[chosenPlayer][13]).BackColor == Color.FromArgb(239, 239, 239) && ((Button)playerButtons[chosenPlayer][13]).Text != "0" && ((Button)playerButtons[chosenPlayer][generalDiceValue]).BackColor == Color.FromArgb(239, 239, 239))
                        {
                            scoreFinal += 100;
                            int general = Int32.Parse(((Button)playerButtons[chosenPlayer][13]).Text);
                            ((Button)playerButtons[chosenPlayer][13]).Text = (general + 100).ToString();
                        }
                    }

                    ((Label)playerButtons[playerID][15]).Text = scoreFinal.ToString();
                }

                ((Button)sender).BackColor = Color.FromArgb(239, 239, 239);

                if (Program.isServer == true)
                {
                    string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("10 0:" + clickedChoice.ToString())) + " <EOF>";
                    Program.server.SendToAll(message);
                    bool selected = false;
                    while (!selected)
                    {
                        Program.server.chosenPlayer += 1;
                        Program.server.chosenPlayer %= 4;
                        if (Program.server.chosenPlayer == 0)
                        {
                            selected = true;
                            Program.server.roundCounter++;
                            break;
                        }

                        if (Program.server.connectedSockets.TryGetValue(Program.server.chosenPlayer, out Socket tmp))
                        {
                            selected = true;
                            break;
                        }
                    }
                    Thread.Sleep(300);
                    Program.server.nextTurn();
                }
                else
                {
                    string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("09 " + clickedChoice.ToString())) + " <EOF>";
                    Program.client.Send(message);
                }
            }
        }
    }
}
