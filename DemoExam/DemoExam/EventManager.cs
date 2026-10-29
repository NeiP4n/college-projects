using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoExam
{
    internal static class EventManager
    {
        // Событие отображения
        public static event Action OnVisibled;


        public static void ExecuteOnVisibled()
        {
            OnVisibled?.Invoke();
        }
    }
}
