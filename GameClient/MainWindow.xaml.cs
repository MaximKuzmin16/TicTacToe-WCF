using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameService;

namespace GameClient
{

    public partial class MainWindow : Window
    {
        readonly BitmapSource blank = BitmapSource.Create(1, 1, 0, 0, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.Transparent }), new byte[] { 0, 0, 0, 0 }, 1);
        readonly BitmapImage cross = new BitmapImage(new Uri("pack://application:,,,/pics/cross.png"));
        readonly BitmapImage zero = new BitmapImage(new Uri("pack://application:,,,/pics/zero.png"));
        public bool crossNext;
        bool myTurn;
        int steps;
        string name1;
        string name2;
        byte score1;
        byte score2;
        string beginner;
        readonly ChannelFactory<IGame> factory = new ChannelFactory<IGame>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost/Game/ep1"));
        public IGame channel;
        public Guid gameGuid;
        bool bothReady;
        
        public MainWindow()
        {
            InitializeComponent();
            channel = factory.CreateChannel();
        }

        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            WindowName win = new WindowName
            {
                Owner = this
            };
            win.ShowDialog();
            LabelName1.Content = name1 = channel.GetData(gameGuid).Name1;
            LabelName2.Content = name2 = channel.GetData(gameGuid).Name2;
            StartNewGame();
            crossNext = channel.GetData(gameGuid).CrossNext;
            MainWindow1.Title = crossNext ? name1 : name2;
            if (name2 == "")
            {
                LabelBottom.Content = "Ожидание соперника...";
            }

            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }

        private void MainWindow1_Closed(object sender, EventArgs e)
        {
            if (name2 != "")
            {
                channel.SetOff(gameGuid);
            }
            else
            {
                channel.EndGame(gameGuid);
            }
            factory.Close();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            var position = int.Parse(image.Name.Last().ToString());
            if (image.Source == blank && myTurn == true)
            {
                if (crossNext)
                {
                    image.Source = cross;
                    channel.DoTurn(gameGuid, position);
                }
                else
                {
                    image.Source = zero;
                    channel.DoTurn(gameGuid, position);
                }
                steps++;
                if (steps >= 5 && CheckForGameEnd())
                {
                    return;
                }
                NextPlayersTurn();
                Block();
                bothReady = false;
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (name2 == "")
            {
                LabelName2.Content = name2 = channel.GetData(gameGuid).Name2;
            }
            if (name2 != "" && LabelBottom.Content.ToString() == "Ожидание соперника...")
            {
                LabelBottom.Content = "Ваш ход...";
                Unblock();
            }
            if (!bothReady && (Title == name1 && channel.GetData(gameGuid).Ready1 && channel.GetData(gameGuid).Ready2 ||
                Title == name2 && channel.GetData(gameGuid).Ready1 && channel.GetData(gameGuid).Ready2))
            {
                if (beginner == name1 && Title == name1 || beginner == name2 && Title == name2)
                {
                    LabelBottom.Content = "Ваш ход...";
                    Unblock();
                }
                else
                {
                    LabelBottom.Content = "Ход соперника...";
                    Block();
                }
                channel.SendReady(gameGuid, false, name1);
                channel.SendReady(gameGuid, false, name2);
            }

            var position = channel.GetData(gameGuid).Position;
            ProcessPosition(position);

            var message = channel.GetData(gameGuid).Message;
            ProcessMessage(message);

            if (!channel.GetData(gameGuid).On)
            {
                MessageBox.Show(this, "Соперник отключился.", "Отключение");
                channel.EndGame(gameGuid);
                Close();
            }
        }

        private void StartNewGame()
        {
            Image1.Source = blank;
            Image2.Source = blank;
            Image3.Source = blank;
            Image4.Source = blank;
            Image5.Source = blank;
            Image6.Source = blank;
            Image7.Source = blank;
            Image8.Source = blank;
            Image9.Source = blank;
            steps = 0;
            crossNext = !crossNext;
            beginner = beginner == name1 ? name2 : name1;
            if (beginner == name1 && Title == name1 || beginner == name2 && Title == name2)
            {
                LabelBottom.Content = "Ваш ход...";
                Unblock();
            }
            else
            {
                LabelBottom.Content = "Ход соперника...";
                Block();
            }
            channel.Reset(gameGuid);

            if (Title == name1)
            {
                channel.SendReady(gameGuid, true, name1);
            }
            else
            {
                channel.SendReady(gameGuid, true, name2);
            }
            if (Title == name1 && !channel.GetData(gameGuid).Ready2 || Title == name2 && !channel.GetData(gameGuid).Ready1)
            {
                Block();
                LabelBottom.Content = "Ожидание готовности соперника...";
            }
            else
            {
                bothReady = true;
            }
        }

        private bool CheckForGameEnd()
        {
            var check = CheckIfCrossWins();
            if (check == true)
            {
                CrossWins();
                return true;
            }
            if (check == false)
            {
                ZeroWins();
                return true;
            }
            if (check == null && steps == 9)
            {
                Draw();
                return true;
            }
            return false;
        }

        private bool? CheckIfCrossWins()
        {
            if (Image1.Source == cross && Image2.Source == cross && Image3.Source == cross ||
                Image4.Source == cross && Image5.Source == cross && Image6.Source == cross ||
                Image7.Source == cross && Image8.Source == cross && Image9.Source == cross ||
                Image1.Source == cross && Image4.Source == cross && Image7.Source == cross ||
                Image2.Source == cross && Image5.Source == cross && Image8.Source == cross ||
                Image3.Source == cross && Image6.Source == cross && Image9.Source == cross ||
                Image1.Source == cross && Image5.Source == cross && Image9.Source == cross ||
                Image3.Source == cross && Image5.Source == cross && Image7.Source == cross)
            {
                return true; // cross wins
            }
            else if (Image1.Source == zero && Image2.Source == zero && Image3.Source == zero ||
                Image4.Source == zero && Image5.Source == zero && Image6.Source == zero ||
                Image7.Source == zero && Image8.Source == zero && Image9.Source == zero ||
                Image1.Source == zero && Image4.Source == zero && Image7.Source == zero ||
                Image2.Source == zero && Image5.Source == zero && Image8.Source == zero ||
                Image3.Source == zero && Image6.Source == zero && Image9.Source == zero ||
                Image1.Source == zero && Image5.Source == zero && Image9.Source == zero ||
                Image3.Source == zero && Image5.Source == zero && Image7.Source == zero)
            {
                return false; // zero wins
            }
            else
            {
                return null; // draw
            }
        }

        private void Draw()
        {
            channel.SetMessage(gameGuid, new string[3] { "Ничья.", "Ничья!", Title == name1 ? name2 : name1 });
            MessageBox.Show("Ничья.", "Ничья!");
            StartNewGame();
        }

        private void ZeroWins()
        {
            channel.SetMessage(gameGuid, new string[3] { string.Format("Крестики проиграли.\nСоперник игрока {0} получает очко.", Title == name1 ? name2 : name1), "Проигрыш!", Title == name1 ? name2 : name1 });
            MessageBox.Show(string.Format("Нолики выиграли.\n{0} получает очко.", Title), "Победа!");
            SetScore();
            StartNewGame();
        }

        private void CrossWins()
        {
            channel.SetMessage(gameGuid, new string[3] { string.Format("Нолики проиграли.\nСоперник игрока {0} получает очко.", Title == name1 ? name2 : name1), "Проигрыш!", Title == name1 ? name2 : name1 });
            MessageBox.Show(string.Format("Крестики выиграли.\n{0} получает очко.", Title), "Победа!");
            SetScore();
            StartNewGame();
        }

        private void SetScore()
        {
            if (Title == name1)
            {
                score1++;
            }
            if (Title == name2)
            {
                score2++;
            }
            LabelScore.Content = string.Format("{0}:{1}", score1, score2);
        }

        private void ProcessPosition(int position)
        {
            switch (position)
            {
                case 1:
                    MakeMove(Image1);
                    break;
                case 2:
                    MakeMove(Image2);
                    break;
                case 3:
                    MakeMove(Image3);
                    break;
                case 4:
                    MakeMove(Image4);
                    break;
                case 5:
                    MakeMove(Image5);
                    break;
                case 6:
                    MakeMove(Image6);
                    break;
                case 7:
                    MakeMove(Image7);
                    break;
                case 8:
                    MakeMove(Image8);
                    break;
                case 9:
                    MakeMove(Image9);
                    break;
                default:
                    break;
            }
        }

        private void ProcessMessage(string[] message)
        {
            if (message[0] != null && message[2] == Title)
            {
                MessageBox.Show(this, message[0], message[1]);
                if (Title == name1)
                {
                    if (message[1] == "Победа!")
                    {
                        score1++;
                    }
                    else if (message[1] == "Проигрыш!")
                    {
                        score2++;
                    }
                }
                else if (Title == name2)
                {
                    if (message[1] == "Победа!")
                    {
                        score2++;
                    }
                    else if (message[1] == "Проигрыш!")
                    {
                        score1++;
                    }
                }
                LabelScore.Content = string.Format("{0}:{1}", score1, score2);
                channel.SetMessage(gameGuid, new string[3]);
                StartNewGame();
            }
        }

        private void MakeMove(Image image)
        {
            if (image.Source == blank)
            {
                image.Source = crossNext ? zero : cross;
                steps++;
                NextPlayersTurn();
                Unblock();
            }
        }

        private void NextPlayersTurn()
        {
            if (LabelBottom.Content.ToString() == "Ваш ход...")
            {
                LabelBottom.Content = "Ход соперника...";
            }
            else
	        {
                LabelBottom.Content = "Ваш ход...";
	        }
        }

        private void Block()
        {
            myTurn = false;
        }

        private void Unblock()
        {
            myTurn = true;
        }
    }
}