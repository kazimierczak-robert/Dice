using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;

//https://msdn.microsoft.com/pl-pl/library/fx6588te(v=vs.110).aspx
namespace Dice
{
    class AsynchronousSocketListener
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        IPAddress ipAddress;
        public Dictionary<short, Socket> connectedSockets;

        short[] randedDices = new short[5];
        public bool[] lockedDices = new bool[5];
        Random random = new Random();
        public short roundCounter = -1;
        public short chosenPlayer = 0;
        public bool startGame = false;

        private volatile bool _shouldStop;

        public AsynchronousSocketListener(IPAddress ipAddress)
        {
            this.ipAddress = ipAddress;
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        public void StartListening()
        {
            _shouldStop = false;
            startGame = false;
            connectedSockets = new Dictionary<short, Socket>();

            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11050);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (!_shouldStop)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.                
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne(1000);  
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Program.allowToRun = false;
            }
           
            listener.Close();
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            try
            {
                Socket handler = listener.EndAccept(ar);

                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception)
            {
                
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = 0;

            try
            {
                // Read data from the client socket. 
                bytesRead = handler.EndReceive(ar);
            }
            catch(Exception)
            {
                bytesRead = 0;
            }

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    content = Encoding.UTF8.GetString(Convert.FromBase64String(content.Substring(0, content.Length - 6)));
                    if (content.Substring(0, 2) == "00" && !startGame)
                    {
                        if (connectedSockets.Count < 3)
                        {
                            content = content.Substring(3);

                            foreach (var onePair in connectedSockets)
                            {
                                if(Program.playWindow.seatButtons[onePair.Key].Text == content)
                                {
                                    Send(handler, "NO 0 <EOF>");
                                    handler.Shutdown(SocketShutdown.Both);
                                    handler.Close();
                                    return;
                                }
                            }

                            short assignedSeat = 1;
                            if (connectedSockets.TryGetValue(assignedSeat, out Socket tmp)) //seat1 is unavailable
                            {
                                assignedSeat++;
                                if (connectedSockets.TryGetValue(assignedSeat, out tmp)) //seat2 is unavailable == seat3 is available
                                {
                                    assignedSeat++;
                                }
                                else  //seat2 is available
                                { }
                            }

                            connectedSockets.Add(assignedSeat, handler);
                            Send(handler, "OK " + assignedSeat.ToString() + " <EOF>");

                            Thread.Sleep(500);
                            Send(handler, GetActualSeats());


                            MethodInvoker inv = delegate
                            {
                                Program.playWindow.seatButtons[assignedSeat].Text = content;
                            };
                            Program.playWindow.Invoke(inv);

                            string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("01 " + assignedSeat.ToString() + ":" + content)) + " <EOF>";
                            SendToAllExceptOne(message, handler);

                            state.buffer = new byte[1024];
                            state.sb = new StringBuilder();
                            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                        }
                        else
                        {
                            Send(handler, "NO 1 <EOF>");
                        }
                    }
                    else if(content.Substring(0, 2) == "02" && !startGame)
                    {
                        short freeSeat = connectedSockets.FirstOrDefault(x => x.Value == handler).Key;
                        connectedSockets.Remove(freeSeat);
                        MethodInvoker inv = delegate
                        {
                            Program.playWindow.seatButtons[freeSeat].Text = "Miejsce #" + (freeSeat + 1).ToString();
                        };
                        Program.playWindow.Invoke(inv);

                        string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("01 " + freeSeat.ToString() + ":")) + " <EOF>";
                        SendToAll(message);

                        startGame = true;
                        foreach (var onePair in connectedSockets)
                        {
                            if (Program.playWindow.seatButtons[onePair.Key].ForeColor == System.Drawing.Color.Black)
                            {
                                startGame = false;
                                break;
                            }
                        }

                        if (startGame == true && Program.playWindow.seatButtons[0].ForeColor == System.Drawing.Color.Green)
                        {
                            message = Convert.ToBase64String(Encoding.UTF8.GetBytes("05")) + " <EOF>";
                            SendToAll(message);
                            Thread.Sleep(300);

                            MethodInvoker inv2 = delegate
                            {
                                Program.playWindow.Controls.Find("BReady", false).First().Text = "Losuj";
                                for (int i = 0; i < 4; i++)
                                {
                                    Program.playWindow.seatButtons[i].ForeColor = System.Drawing.Color.Black;
                                }
                            };
                            Program.playWindow.Invoke(inv2);

                            roundCounter = 0;
                            chosenPlayer = 0;
                            Program.playWindow.isStarted = true;
                            Program.playWindow.chosenPlayer = 0;
                            nextTurn();
                        }
                        else
                        {
                            startGame = false;
                        }

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                    else if (content.Substring(0, 2) == "03" && !startGame)
                    {
                        short seatNumber = connectedSockets.FirstOrDefault(x => x.Value == handler).Key;

                        if (content[3] == '1')
                        {
                            MethodInvoker inv = delegate
                            {
                                Program.playWindow.seatButtons[seatNumber].ForeColor = System.Drawing.Color.Green;
                            };
                            Program.playWindow.Invoke(inv);

                            string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("04 " + seatNumber.ToString() + ":1")) + " <EOF>";
                            SendToAllExceptOne(message, handler);
 
                            startGame = true;
                            foreach (var onePair in connectedSockets)
                            {
                                if (Program.playWindow.seatButtons[onePair.Key].ForeColor == System.Drawing.Color.Black)
                                {
                                    startGame = false;
                                    break;
                                }
                            }

                            if (startGame == true && Program.playWindow.seatButtons[0].ForeColor == System.Drawing.Color.Green)
                            {
                                message = Convert.ToBase64String(Encoding.UTF8.GetBytes("05")) + " <EOF>";
                                SendToAll(message);
                                Thread.Sleep(300);

                                MethodInvoker inv2 = delegate
                                {
                                    Program.playWindow.Controls.Find("BReady", false).First().Text = "Losuj";
                                    for (int i = 0; i < 4; i++)
                                    {
                                        Program.playWindow.seatButtons[i].ForeColor = System.Drawing.Color.Black;
                                    }
                                };
                                Program.playWindow.Invoke(inv2);

                                roundCounter = 0;
                                chosenPlayer = 0;
                                Program.playWindow.isStarted = true;
                                Program.playWindow.chosenPlayer = 0;
                                nextTurn();
                            }
                            else
                            {
                                startGame = false;
                            }
                            
                            state.buffer = new byte[1024];
                            state.sb = new StringBuilder();
                            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                        }
                        else if (content[3] == '0')
                        {
                            {
                                MethodInvoker inv = delegate
                                {
                                    Program.playWindow.seatButtons[seatNumber].ForeColor = System.Drawing.Color.Black;
                                };
                                Program.playWindow.Invoke(inv);

                                string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("04 " + seatNumber.ToString() + ":0")) + " <EOF>";
                                SendToAllExceptOne(message, handler);

                                state.buffer = new byte[1024];
                                state.sb = new StringBuilder();
                                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                            }
                        }
                    }
                    else if (content.Substring(0, 2) == "08" && startGame)
                    {
                        var chosenDices = content.Split(' ')[1].Split(':');
                        MethodInvoker inv = delegate
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                Button btn = Program.playWindow.diceButtons[i];
                                if (chosenDices[i] == "1")
                                {
                                    btn.FlatAppearance.BorderSize = 2;
                                    lockedDices[i] = true;
                                }
                                else
                                {
                                    btn.FlatAppearance.BorderSize = 0;
                                    lockedDices[i] = false;
                                }
                            }
                        };
                        Program.playWindow.Invoke(inv);

                        newThrow();
                        state.buffer = new byte[1024];
                        state.sb = new StringBuilder();
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                    else if (content.Substring(0, 2) == "09" && startGame)
                    {
                        int clickedChoice = Int32.Parse(content.Split(' ')[1]);
                        string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("10 " + chosenPlayer.ToString() + ":" + clickedChoice.ToString())) + " <EOF>";
                        SendToAllExceptOne(message, handler);
                        if(clickedChoice<6)
                        {
                            clickedChoice += 1;
                        }
                        else
                        {
                            clickedChoice += 2;
                        }
                        MethodInvoker inv = delegate
                        {
                            Button sender = ((Button)Program.playWindow.playerButtons[chosenPlayer][clickedChoice]);
                            if (clickedChoice < 7)
                            {
                                string scoreFirstStr = ((Label)Program.playWindow.playerButtons[chosenPlayer][7]).Text;
                                int scoreFirst = Int32.Parse(scoreFirstStr.Split('+')[0]) + Int32.Parse(sender.Text);
                                string scoreFinalStr = ((Label)Program.playWindow.playerButtons[chosenPlayer][15]).Text;
                                int scoreFinal = Int32.Parse(scoreFinalStr) + Int32.Parse(sender.Text);
                                if (scoreFirst > 62)
                                {
                                    if (scoreFirstStr.Last() != '+')
                                    {
                                        scoreFirst += 35;
                                        scoreFinal += 35;
                                    }
                                    ((Label)Program.playWindow.playerButtons[chosenPlayer][7]).Text = scoreFirst.ToString() + "+";
                                }
                                else
                                {
                                    ((Label)Program.playWindow.playerButtons[chosenPlayer][7]).Text = scoreFirst.ToString();
                                }

                                //sprawdź czy dżoker
                                bool isJoker = true;
                                for (int i = 1; i < 5; i++)
                                {
                                    if (Program.playWindow.diceButtons[0].BackgroundImage == Program.playWindow.diceButtons[i].BackgroundImage)
                                    {
                                        isJoker = false;
                                        break;
                                    }
                                }
                                if (isJoker)
                                {
                                    int generalDiceValue = -1;
                                    if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice1) { generalDiceValue = 1; }
                                    else if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice2) { generalDiceValue = 2; }
                                    else if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice3) { generalDiceValue = 3; }
                                    else if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice4) { generalDiceValue = 4; }
                                    else if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice5) { generalDiceValue = 5; }
                                    else { generalDiceValue = 6; }

                                    if (((Button)Program.playWindow.playerButtons[chosenPlayer][13]).BackColor == Color.FromArgb(239, 239, 239) && ((Button)Program.playWindow.playerButtons[chosenPlayer][13]).Text != "0" && generalDiceValue == clickedChoice)
                                    {
                                        scoreFinal += 100;
                                        int general = Int32.Parse(((Button)Program.playWindow.playerButtons[chosenPlayer][13]).Text);
                                        ((Button)Program.playWindow.playerButtons[chosenPlayer][13]).Text = (general + 100).ToString();
                                    }
                                }

                                ((Label)Program.playWindow.playerButtons[chosenPlayer][15]).Text = scoreFinal.ToString();
                            }
                            else
                            {
                                string scoreFinalStr = ((Label)Program.playWindow.playerButtons[chosenPlayer][15]).Text;
                                int scoreFinal = Int32.Parse(scoreFinalStr) + Int32.Parse(sender.Text);

                                //sprawdź czy dżoker
                                bool isJoker = true;
                                for (int i = 1; i < 5; i++)
                                {
                                    if (Program.playWindow.diceButtons[0].BackgroundImage == Program.playWindow.diceButtons[i].BackgroundImage)
                                    {
                                        isJoker = false;
                                        break;
                                    }
                                }
                                if (isJoker)
                                {
                                    int generalDiceValue = -1;
                                    if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice1) { generalDiceValue = 1; }
                                    else if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice2) { generalDiceValue = 2; }
                                    else if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice3) { generalDiceValue = 3; }
                                    else if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice4) { generalDiceValue = 4; }
                                    else if (Program.playWindow.diceButtons[0].BackgroundImage == Properties.Resources.dice5) { generalDiceValue = 5; }
                                    else { generalDiceValue = 6; }

                                    if (((Button)Program.playWindow.playerButtons[chosenPlayer][13]).BackColor == Color.FromArgb(239, 239, 239) && ((Button)Program.playWindow.playerButtons[chosenPlayer][13]).Text != "0" && ((Button)Program.playWindow.playerButtons[chosenPlayer][generalDiceValue]).BackColor == Color.FromArgb(239, 239, 239))
                                    {
                                        scoreFinal += 100;
                                        int general = Int32.Parse(((Button)Program.playWindow.playerButtons[chosenPlayer][13]).Text);
                                        ((Button)Program.playWindow.playerButtons[chosenPlayer][13]).Text = (general + 100).ToString();
                                    }
                                }

                                ((Label)Program.playWindow.playerButtons[chosenPlayer][15]).Text = scoreFinal.ToString();
                            }
                            for (int i = 0; i < 6; i++)
                            {
                                if (i != clickedChoice - 1 && ((Button)Program.playWindow.playerButtons[chosenPlayer][i + 1]).BackColor != Color.FromArgb(239, 239, 239))
                                {
                                    ((Button)Program.playWindow.playerButtons[chosenPlayer][i + 1]).Text = "0";
                                }
                                else
                                {
                                    ((Button)Program.playWindow.playerButtons[chosenPlayer][i + 1]).BackColor = Color.FromArgb(239, 239, 239);
                                }
                            }
                            for (int i = 6; i < 13; i++)
                            {
                                if (i != clickedChoice - 2 && ((Button)Program.playWindow.playerButtons[chosenPlayer][i + 2]).BackColor != Color.FromArgb(239, 239, 239))
                                {
                                    ((Button)Program.playWindow.playerButtons[chosenPlayer][i + 2]).Text = "0";
                                }
                                else
                                {
                                    ((Button)Program.playWindow.playerButtons[chosenPlayer][i + 2]).BackColor = Color.FromArgb(239, 239, 239);
                                }
                            }

                        };
                        Program.playWindow.Invoke(inv);
                        bool selected = false;
                        while (!selected)
                        {
                            chosenPlayer += 1;
                            chosenPlayer %= 4;
                            if (chosenPlayer==0)
                            {
                                selected = true;
                                roundCounter++;
                                break;
                            }

                            if (connectedSockets.TryGetValue(chosenPlayer, out Socket tmp))
                            {
                                selected = true;
                                break;
                            }
                        }
                        Thread.Sleep(300);

                        if (roundCounter < 13)
                        {
                            nextTurn();
                            state.buffer = new byte[1024];
                            state.sb = new StringBuilder();
                            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                        }
                        else
                        {
                            FinishGame();
                        }       
                    }
                    else if (content.Substring(0, 2) == "11" && startGame)
                    {
                        short freeSeat = connectedSockets.FirstOrDefault(x => x.Value == handler).Key;

                        MessageBox.Show("Gracz " + (freeSeat+1).ToString() + " opuścił grę!\nGra będzie kontynuowana bez niego.", "Komunikat!");
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        connectedSockets.Remove(freeSeat);

                        if(chosenPlayer==freeSeat)
                        {
                            bool selected = false;
                            while (!selected)
                            {
                                chosenPlayer += 1;
                                chosenPlayer %= 4;
                                if (chosenPlayer == 0)
                                {
                                    selected = true;
                                    roundCounter++;
                                    break;
                                }

                                if (connectedSockets.TryGetValue(chosenPlayer, out Socket tmp))
                                {
                                    selected = true;
                                    break;
                                }
                            }
                            Thread.Sleep(300);

                            if (roundCounter < 13 && connectedSockets.Count>0)
                            {
                                nextTurn();
                            }
                            else
                            {
                                FinishGame();
                            }
                        }
                        string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("12 " + freeSeat.ToString())) + " <EOF>";
                        SendToAll(message);
                        if (connectedSockets.Count == 0)
                        {
                            FinishGame();
                        }
                    }
                    else
                    {
                        Send(handler, "NO 1 <EOF>");
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        return;
                    }
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private void FinishGame()
        {
            int maxScore = -1;

            for (int i = 0; i < 4; i++)
            {
                int score = Int32.Parse(((Label)Program.playWindow.playerButtons[i][15]).Text);
                if (score > maxScore)
                {
                    if (i==0 || connectedSockets.ContainsKey((short)i))
                    {
                        maxScore = score;
                    }
                }
            }
            List<int> winnersID = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                int score = Int32.Parse(((Label)Program.playWindow.playerButtons[i][15]).Text);
                if (score == maxScore)
                {
                    if (i == 0 || connectedSockets.ContainsKey((short)i))
                    {
                        winnersID.Add(i+1);
                    }
                }
            }

            string winners = "";
            foreach (var winnerID in winnersID)
            {
                winners += ":" + winnerID.ToString();
            }
            winners = winners.Substring(1);

            string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("14 " + winners)) + " <EOF>";
            SendToAll(message);

            foreach (var socket in connectedSockets.Values)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            connectedSockets.Clear();

            RequestStop();

            if (winners.Split(':').Count() == 1)
            {
                MessageBox.Show("Wygrał gracz " + winners + "!", "Koniec gry!");
            }
            else
            {
                winners.Replace(":", ", ");
                MessageBox.Show("Wygrali gracze " + winners + "!", "Koniec gry!");
            }
            Program.playWindow.isEnd = true;
        }
        public void nextTurn()
        {
            string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("06 "+ chosenPlayer.ToString())) + " <EOF>";
            SendToAll(message);
            Thread.Sleep(300);
            lockedDices = new bool[5];
            for (int i = 0; i < 5; i++)
            {
                lockedDices[i] = false;
            }
            randedDices = new short[5];
            MethodInvoker inv = delegate
            {
                foreach (var dice in Program.playWindow.diceButtons)
                {
                    dice.FlatAppearance.BorderSize = 0;
                }
                if (chosenPlayer == 0)
                {
                    Program.playWindow.Controls.Find("BReady", false).First().Enabled = true;
                    foreach (var dice in Program.playWindow.diceButtons)
                    {
                        dice.Enabled = true;
                    }
                }
                else
                {
                    Program.playWindow.Controls.Find("BReady", false).First().Enabled = false;
                    foreach (var dice in Program.playWindow.diceButtons)
                    {
                        dice.Enabled = false;
                    }
                }
                Program.playWindow.Controls.Find("BReady", false).First().Focus();
                for (int i = 0; i < 4; i++)
                {
                    if (i == chosenPlayer)
                    {
                        Program.playWindow.Controls.Find("#" + (i + 1).ToString(), false).First().Font = new System.Drawing.Font(Program.playWindow.Font.FontFamily, Program.playWindow.Font.Size, System.Drawing.FontStyle.Bold);
                        ((Label)Program.playWindow.playerButtons[i][0]).Font = new System.Drawing.Font(Program.playWindow.Font.FontFamily, Program.playWindow.Font.Size, FontStyle.Bold);
                    }
                    else
                    {
                        Program.playWindow.Controls.Find("#" + (i + 1).ToString(), false).First().Font = new System.Drawing.Font(Program.playWindow.Font.FontFamily, Program.playWindow.Font.Size, System.Drawing.FontStyle.Regular);
                        ((Label)Program.playWindow.playerButtons[i][0]).Font = new System.Drawing.Font(Program.playWindow.Font.FontFamily, Program.playWindow.Font.Size, FontStyle.Regular);
                    }
                }
            };
            Program.playWindow.Invoke(inv);
            newThrow();
        }

        public void newThrow()
        {
            RandDice();
            string messageDices = "";
            for (int i = 0; i < 5; i++)
            {
                messageDices += ":" + randedDices[i].ToString();
            }
            messageDices = messageDices.Substring(1);

            string messageLockedDices = "";
            for (int i = 0; i < 5; i++)
            {
                if (lockedDices[i])
                {
                    messageLockedDices += ":1";
                }
                else
                {
                    messageLockedDices += ":0";
                }
            }
            messageLockedDices = messageLockedDices.Substring(1);

            short[] calculatedOptionsResults = CalculateOptionsResults();
            string messageResults = "";
            for (int i = 0; i < 13; i++)
            {
                messageResults += ":" + calculatedOptionsResults[i].ToString();
            }
            messageResults = messageResults.Substring(1);

            MethodInvoker inv = delegate
            {
                for (int i = 0; i < 5; i++)
                {
                    Button btn = Program.playWindow.diceButtons[i];
                    if(randedDices[i] == 1)
                    {
                        btn.BackgroundImage = Properties.Resources.dice1;
                    }
                    else if (randedDices[i] == 2)
                    {
                        btn.BackgroundImage = Properties.Resources.dice2;
                    }
                    else if (randedDices[i] == 3)
                    {
                        btn.BackgroundImage = Properties.Resources.dice3;
                    }
                    else if (randedDices[i] == 4)
                    {
                        btn.BackgroundImage = Properties.Resources.dice4;
                    }
                    else if (randedDices[i] == 5)
                    {
                        btn.BackgroundImage = Properties.Resources.dice5;
                    }
                    else if (randedDices[i] == 6)
                    {
                        btn.BackgroundImage = Properties.Resources.dice6;
                    }

                    if(lockedDices[i])
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
                    Button btn = (Button)Program.playWindow.playerButtons[chosenPlayer][i + 1];
                    if (btn.BackColor != Color.FromArgb(239, 239, 239))
                    {
                        btn.Text = calculatedOptionsResults[i].ToString();
                        if(chosenPlayer==0)
                        {
                            btn.Enabled = true;
                            btn.ForeColor = Color.Black;
                        }
                    }
                }

                for (int i = 6; i < 13; i++)
                {
                    Button btn = (Button)Program.playWindow.playerButtons[chosenPlayer][i + 2];
                    if (btn.BackColor != Color.FromArgb(239, 239, 239))
                    {
                        btn.Text = calculatedOptionsResults[i].ToString();
                        if (chosenPlayer == 0)
                        {
                            btn.Enabled = true;
                            btn.ForeColor = Color.Black;
                        }
                    }
                }

            };
            Program.playWindow.Invoke(inv);

            string message = Convert.ToBase64String(Encoding.UTF8.GetBytes("07 " + messageDices + " " + messageLockedDices + " " + messageResults)) + " <EOF>";
            SendToAll(message);


        }

        private void RandDice()
        {
            for (int i = 0; i < 5; i++)
            {
                if(lockedDices[i]==false)
                {
                    randedDices[i] = short.Parse(random.Next(1, 7).ToString());
                }
            }
        }

        private short[] CalculateOptionsResults()
        {
            short[] calculatedOptionsResults = new short[13];
            for (int i = 0; i < 13; i++)
            {
                calculatedOptionsResults[i] = 0;
            }

            //1-6 and sum
            short sum = 0;
            for (int i = 0; i < 5; i++)
            {
                calculatedOptionsResults[randedDices[i] - 1]++;
                sum += randedDices[i];
            }

            //trójki
            for (int i = 0; i < 6; i++)
            {
                if (calculatedOptionsResults[i]>2)
                {
                    calculatedOptionsResults[6] = sum;
                    break;
                }
            }

            //czwórki
            for (int i = 0; i < 6; i++)
            {
                if (calculatedOptionsResults[i] > 3)
                {
                    calculatedOptionsResults[7] = sum;
                    break;
                }
            }

            //full 3+2
            for (int i = 0; i < 6 && calculatedOptionsResults[8] == 0; i++)
            {
                if (calculatedOptionsResults[i] == 5)
                {
                    calculatedOptionsResults[8] = 25;
                    break;
                }
                else if (calculatedOptionsResults[i] == 3)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (calculatedOptionsResults[j] == 2)
                        {
                            calculatedOptionsResults[8] = 25;
                            break;
                        }
                    }
                }
            }

            //mały strit
            for (int i = 0; i < 3; i++)
            {
                if (calculatedOptionsResults[i] > 0 && calculatedOptionsResults[i+1] > 0 && calculatedOptionsResults[i+2] > 0 && calculatedOptionsResults[i+3] > 0)
                {
                    calculatedOptionsResults[9] = 30;
                    break;
                }
            }

            //duży strit
            for (int i = 0; i < 2; i++)
            {
                if (calculatedOptionsResults[i] > 0 && calculatedOptionsResults[i + 1] > 0 && calculatedOptionsResults[i + 2] > 0 && calculatedOptionsResults[i + 3] > 0 && calculatedOptionsResults[i + 4] > 0)
                {
                    calculatedOptionsResults[10] = 40;
                    break;
                }
            }

            //generał
            int generalDiceValue = -1;
            for (int i = 0; i < 6; i++)
            {
                if (calculatedOptionsResults[i] == 5)
                {
                    calculatedOptionsResults[11] = 50;
                    generalDiceValue = i +1 ;
                    break;
                }
            }

            for (short i = 0; i < 6; i++)
            {
                calculatedOptionsResults[i] = ((short)(calculatedOptionsResults[i] * (i + 1)));
            }

            //sprawdź czy dżoker
            if(calculatedOptionsResults[11] == 50)
            {
                if(((Button)Program.playWindow.playerButtons[chosenPlayer][13]).BackColor == Color.FromArgb(239, 239, 239) && ((Button)Program.playWindow.playerButtons[chosenPlayer][13]).Text != "0" && ((Button)Program.playWindow.playerButtons[chosenPlayer][generalDiceValue]).BackColor == Color.FromArgb(239, 239, 239))
                {
                    calculatedOptionsResults[8] = 25;
                    calculatedOptionsResults[9] = 30;
                    calculatedOptionsResults[10] = 40;
                }
            }

            //szansa
            calculatedOptionsResults[12] = sum;
            return calculatedOptionsResults;
        }

        public string GetActualSeats()
        {
            string content = "";
            for (int i = 0; i < 4; i++)
            {
                if (Program.playWindow.seatButtons[i].Text != "Miejsce #" + (i + 1).ToString())
                {
                    content += " " + i.ToString() + ":" + Program.playWindow.seatButtons[i].Text + ":";
                    if(Program.playWindow.seatButtons[i].ForeColor == System.Drawing.Color.Green)
                    {
                        content += "1";
                    }
                    else
                    {
                        content += "0";
                    }
                }
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes("01" + content)) + " <EOF>";

        }

        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);    
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }

        public void SendToAll(string message)
        {
            foreach (var onePair in connectedSockets)
            {
                Send(onePair.Value, message);
            }
        }

        public void SendToAllExceptOne(string message, Socket exceptSocket)
        {
            foreach (var onePair in connectedSockets)
            {
                if (onePair.Value != exceptSocket)
                {
                    Send(onePair.Value, message);
                }
            }
        }
    }
}
