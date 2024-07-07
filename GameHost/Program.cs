using System;
using System.ServiceModel;
using GameService;

namespace GameHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost sh = new ServiceHost(typeof(Game));//Создаём хост
            sh.AddServiceEndpoint(typeof(IGame), new NetTcpBinding(), "net.tcp://localhost/Game/ep1");//добавляем конечную точку службы
            sh.Open();//открываем хост
            Console.WriteLine("Хост запущен.");
            Console.ReadLine();//ждём пока не нажмут enter
            sh.Close();//закрываем хост
        }
    }
}