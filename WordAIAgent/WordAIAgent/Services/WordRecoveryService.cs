using System;
using System.Runtime.InteropServices;

namespace WordAIAgent.Services
{
    public class WordRecoveryService
    {
        private readonly WordService _ws;

        public WordRecoveryService(WordService ws) => _ws = ws;

        public bool TryRecover(Exception ex, out string message)
        {
            message = null;
            if (!(ex is COMException)) return false;

            try
            {
                _ws.EnsureDocumentReady();
                _ws.SelectAll();
                _ws.TypeParagraph();
                message = "Word восстановлен (best-effort).";
                return true;
            }
            catch (Exception e)
            {
                message = "Восстановление не удалось: " + e.Message;
                return false;
            }
        }
    }
}
