﻿using Splendor.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Splendor.Core.AI
{
    public class AiGameRunner
    {
        private readonly ISpendorAi[] _playerAi;
        private readonly IGame _game;

        private readonly Action<string> m_Log;

        public AiGameRunner(IEnumerable<ISpendorAi> players, Action<string> log)
        {
            _playerAi = players?.ToArray() ?? throw new ArgumentNullException(nameof(players));
            var state = new DefaultGameInitialiser(new DefaultCards()).Create(players: _playerAi.Length);
            _game = new Game(state);
            m_Log = log ?? new Action<string>(s => { });
        }

        public Dictionary<ISpendorAi, int> Run()
        {
            int playersPassed = 0;
            while (!_game.IsGameFinished && playersPassed < _game.State.Players.Length)
            {
                var index = Array.IndexOf(_game.State.Players, _game.State.CurrentPlayer);
                var ai = _playerAi[index];
                var action = ai.ChooseAction(_game.State);
                if (action is NoAction) playersPassed++; else playersPassed = 0;
                _game.CommitTurn(action);
                var thisPlayer = _game.State.Players[index];
                m_Log($"{ai.Name} (Bank:{thisPlayer.Purse.Values.Sum()}), {action}");
            }

            m_Log($"**** Game over after {_game.RoundsCompleted} rounds.");

            var results = new Dictionary<ISpendorAi, int>();
            for (int i = 0; i < _game.State.Players.Length; i++)
            {
                Player player = _game.State.Players[i];
                var score = player.VictoryPoints();
                var s = score == 1 ? "" : "s";
                var nobles = player.Nobles.Count();
                var ns = nobles == 1 ? "" : "s";
                var nobleNames = nobles > 0 ? ": " + string.Join(", ", player.Nobles.Select(n => n.Name)) : "";
                var bonuses = string.Join(", ", player.GetDiscount().Where(kvp => kvp.Key != CoinColour.Gold)
                                                                    .Select(kvp=> $"{kvp.Value} {kvp.Key}"));
                m_Log($"{_playerAi[i].Name} — {score} point{s} ({nobles} noble{ns}{nobleNames}) (Bonuses {bonuses})");
                results.Add(_playerAi[i], score);
            }
            return results;
        }
    }
}
