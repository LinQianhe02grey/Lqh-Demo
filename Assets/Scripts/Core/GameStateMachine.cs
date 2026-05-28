using System;
using System.Collections.Generic;

namespace Cardwin.Core
{
    public class GameStateMachine
    {
        private readonly Dictionary<Type, IGameStateHandler> _handlers = new();

        public GameState CurrentState { get; private set; }

        public void RegisterHandler<T>(IGameStateHandler handler) where T : IGameStateHandler
        {
            _handlers[typeof(T)] = handler;
        }

        public void TransitionTo(GameState newState)
        {
            CurrentState = newState;

            foreach (var handler in _handlers.Values)
                handler.OnStateChanged(newState);
        }
    }

    public interface IGameStateHandler
    {
        void OnStateChanged(GameState newState);
    }
}
