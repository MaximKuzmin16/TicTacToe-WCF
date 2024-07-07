using System;
using System.Collections.Generic;
using System.Linq;

namespace GameService
{
    public class Game : IGame //класс игры реализующий интерфейс iGame
    {
        static Dictionary<Guid, GameState> games = new Dictionary<Guid, GameState>(); //создаём словарь где ключ - 128 битный идентификатор, значение-состояние игры
        public GameState GetData(Guid guid)
        {
            return games[guid]; //Узнаём состояние конкретной игры
        }

        public int GetGames()
        {
            return games.Count; //узнаём количество игр
        }

        public void DoTurn(Guid guid, int position)
        {
            GetData(guid).Position = position;
        }

        public Guid SendName(string name)
        {
            Guid guid; //создаём иденитфикатор
            if (games.Count == 0) //если игр ещё не было
            {
                games.Add(guid = Guid.NewGuid(), new GameState());//создаём игру
                if (name == "Player")//если первый польззователь при входе не задал имя
                {
                    games.ElementAt(0).Value.Name1 = "Player1";//то он игрок 1
                }
                else
                {
                    games.ElementAt(0).Value.Name1 = name;//если же задал, то используемое заданное имя
                }
                games.Last().Value.CrossNext = true;
                return guid;//возвращаем идентификатор
            }
            else
            {
                var firstVacant = games.FirstOrDefault(g => g.Value.Name2 == "");//проходимся по всем играм, смотрим есть ли в какой то игре уже 2 игрока, если есть то создаём новую сессию для новых игроков
                if (!firstVacant.Equals(default(KeyValuePair<Guid, GameState>)))//если игра не новая
	            {
                    if (name == "Player")
                    {
                        firstVacant.Value.Name2 = "Player2";
                    }
                    else
                    {
                        firstVacant.Value.Name2 = name;
                    }
                    firstVacant.Value.CrossNext = false;
                    return firstVacant.Key;
	            }
                else
                {
                    games.Add(guid = Guid.NewGuid(), new GameState());//если новая то делаем всё то же что и в первом условии
                    if (name == "Player")
                    {
                        games.First(g => g.Key == guid).Value.Name1 = "Player1";
                    }
                    else
                    {
                        games.First(g => g.Key == guid).Value.Name1 = name;
                    }
                    games.First(g => g.Key == guid).Value.CrossNext = true;
                    return guid;
                }
            }
        }

        public void Reset(Guid guid)
        {
            GetData(guid).Position = 0; //обновляем доску
        }

        public void EndGame(Guid guid)
        {
            games.Remove(guid); //удаляем игру из словаря если она кончилась
        }

        public void SetOff(Guid guid)
        {
            games[guid].On = false; //переставляем значение переменной идёт ли игра на данный момент на false
        }

        public void SetMessage(Guid guid, string[] message)
        {
            GetData(guid).Message = message; 
        }

        public void SendReady(Guid guid, bool ready, string name)
        {
            if (GetData(guid).Name1 == name)
            {
                GetData(guid).Ready1 = ready;
            }
            else
            {
                GetData(guid).Ready2 = ready;
            }
        }
    }
}