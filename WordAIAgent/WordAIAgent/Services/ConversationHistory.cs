using System.Collections.Generic;
using WordAIAgent.Models;

namespace WordAIAgent.Services
{
    public class ConversationHistory
    {
        private readonly List<ChatMessage> _items = new List<ChatMessage>();
        private readonly int _maxCount;

        public ConversationHistory(int maxCount = 40)
        {
            _maxCount = maxCount < 1 ? 1 : maxCount;
        }

        public IReadOnlyList<ChatMessage> Items => _items;

        public void AddUser(string text)
        {
            Add(new ChatMessage("user", text ?? string.Empty));
        }

        public void AddAssistant(string text)
        {
            Add(new ChatMessage("assistant", text ?? string.Empty));
        }

        private void Add(ChatMessage message)
        {
            _items.Add(message);
            while (_items.Count > _maxCount)
                _items.RemoveAt(0);
        }

        public void Clear() => _items.Clear();
    }
}
