using System;
using System.Windows;
using System.Windows.Controls;

namespace GameClient
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class WindowName : Window
    {
        bool gotName;
        public WindowName()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox.Text != string.Empty)
            {
                ((MainWindow)Owner).gameGuid = ((MainWindow)Owner).channel.SendName(TextBox.Text);
            }
            else
            {
                ((MainWindow)Owner).gameGuid = ((MainWindow)Owner).channel.SendName("Player");                
            }
            gotName = true;
            Close();    
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!gotName)
            {
                ((MainWindow)Owner).gameGuid = ((MainWindow)Owner).channel.SendName("Player");                
            }
        }
    }
}