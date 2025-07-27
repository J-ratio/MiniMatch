using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniMatch.Core.Events
{
    public class GameEventBus : MonoBehaviour
    {
        private static GameEventBus _instance;
        public static GameEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("GameEventBus");
                    _instance = go.AddComponent<GameEventBus>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();
        
        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);
            if (!_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType] = new List<Delegate>();
                
            _eventHandlers[eventType].Add(handler);
        }
        
        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType].Remove(handler);
                if (_eventHandlers[eventType].Count == 0)
                    _eventHandlers.Remove(eventType);
            }
        }
        
        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in _eventHandlers[eventType])
                {
                    try
                    {
                        ((Action<T>)handler)?.Invoke(gameEvent);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error handling event {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }
        
        public void Clear()
        {
            _eventHandlers.Clear();
        }
        
        private void OnDestroy()
        {
            Clear();
        }
    }
}