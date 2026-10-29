// !! IMPORTANT: Add this file to the project as "Class" (not "User Control").
// If Visual Studio already created ChatPaneHost.Designer.cs — DELETE that file.
// This file is the ONLY declaration of ChatPaneHost. It must NOT be partial.

using System.Windows.Forms;

namespace WordAIAgent.Forms
{
    /// <summary>
    /// Plain UserControl wrapper required by CustomTaskPanes.Add().
    /// AIAgentForm is a Form — VSTO cannot accept Form directly.
    /// </summary>
    internal sealed class ChatPaneHost : System.Windows.Forms.UserControl
    {
        private AIAgentForm _agentForm;

        public ChatPaneHost()
        {
            Dock = DockStyle.Fill;

            _agentForm = new AIAgentForm();
            _agentForm.TopLevel      = false;
            _agentForm.FormBorderStyle = FormBorderStyle.None;
            _agentForm.Dock          = DockStyle.Fill;
            _agentForm.Visible       = true;

            Controls.Add(_agentForm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _agentForm?.Dispose(); _agentForm = null; }
            base.Dispose(disposing);
        }
    }
}

