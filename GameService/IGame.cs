using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GameService
{
    [ServiceContract]
    public interface IGame //интерфейс самой игры
    {
        [OperationContract]
        GameState GetData(Guid guid); //получить информацию о играх

        [OperationContract]
        void DoTurn(Guid clientId, int position); //сделать шаг

        [OperationContract]
        Guid SendName(string name); //отправка имени

        [OperationContract]
        void Reset(Guid guid); //обновить доску

        [OperationContract]
        void EndGame(Guid guid); //закончить игру

        [OperationContract]
        void SetOff(Guid guid);

        [OperationContract]
        int GetGames(); //получить количество игр

        [OperationContract]
        void SetMessage(Guid guid, string[] message);//отправка сообщению пользователю

        [OperationContract]
        void SendReady(Guid guid, bool ready, string name);//готовонсть пользователей
    }

    [DataContract]
    public class GameState //класс состояний игры
    {
        [DataMember]
        public bool Ready1 { get; set; }//готовонсть первого игрока

        [DataMember]
        public bool Ready2 { get; set; }//готовонсть второго игрока

        [DataMember]
        public string[] Message { get; set; } = new string[3]; //сообщение

        [DataMember]
        public bool On { get; set; } = true; //твой ли ход

        [DataMember]
        public int Position { get; set; } //позиция

        [DataMember]
        public bool CrossNext { get; set; } = true; //ходят ли крестики следующими

        [DataMember]
        public string Name1 { get; set; } = ""; //имя первого игрока

        [DataMember]
        public string Name2 { get; set; } = ""; //имя второго игрока
    }
}