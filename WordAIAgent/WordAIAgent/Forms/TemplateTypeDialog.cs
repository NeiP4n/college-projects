using System;
using System.Drawing;
using System.Windows.Forms;

namespace WordAIAgent.Forms
{
    /// <summary>
    /// Small dialog shown when the user adds a file/template.
    /// Lets them choose how the file will be used by the agent.
    /// </summary>
    public class TemplateTypeDialog : Form
    {
        public Services.TemplateManager.TemplateType SelectedType { get; private set; }
        public string Category { get; private set; } = "Общие";
        public string Description { get; private set; } = "";

        private RadioButton rdContext;
        private RadioButton rdInsert;
        private RadioButton rdStyle;
        private TextBox txtCategory;
        private TextBox txtDescription;
        private Button btnOk;
        private Button btnCancel;

        public TemplateTypeDialog()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "Тип файла / шаблона";
            Size = new Size(420, 340);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(28, 28, 44);
            ForeColor = Color.FromArgb(210, 210, 240);

            Color fg = Color.FromArgb(210, 210, 240);
            Font uiFont = new Font("Segoe UI", 9.5f);

            var lbl = new Label {
                Text = "Как использовать этот файл?",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 200, 255),
                Location = new Point(14, 14), AutoSize = true
            };

            rdContext = new RadioButton {
                Text = "📋 Контекст для AI (агент читает содержимое, но не вставляет в документ)",
                Font = uiFont, ForeColor = fg, Checked = true,
                Location = new Point(14, 48), Size = new Size(380, 44), AutoSize = false
            };
            rdInsert = new RadioButton {
                Text = "📥 Вставить содержимое в документ (InsertFileContent)",
                Font = uiFont, ForeColor = fg,
                Location = new Point(14, 96), Size = new Size(380, 40), AutoSize = false
            };
            rdStyle = new RadioButton {
                Text = "🎨 Применить как шаблон стилей (.docx / .dotx)",
                Font = uiFont, ForeColor = fg,
                Location = new Point(14, 138), Size = new Size(380, 40), AutoSize = false
            };

            var lblCat = new Label { Text = "Категория:", Font = uiFont, ForeColor = fg, Location = new Point(14, 188), AutoSize = true };
            txtCategory = new TextBox {
                Text = "Общие", Font = uiFont,
                BackColor = Color.FromArgb(38,38,55), ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(100, 185), Width = 280
            };

            var lblDesc = new Label { Text = "Описание:", Font = uiFont, ForeColor = fg, Location = new Point(14, 218), AutoSize = true };
            txtDescription = new TextBox {
                Font = uiFont,
                BackColor = Color.FromArgb(38,38,55), ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(100, 215), Width = 280
            };

            btnOk = new Button {
                Text = "Добавить", DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(40,100,60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = uiFont,
                Location = new Point(210, 258), Size = new Size(100, 30)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) => {
                SelectedType = rdInsert.Checked
                    ? Services.TemplateManager.TemplateType.Reference
                    : rdStyle.Checked
                        ? Services.TemplateManager.TemplateType.StyleTemplate
                        : Services.TemplateManager.TemplateType.ContextDocument;
                Category = txtCategory.Text.Trim().Length > 0 ? txtCategory.Text.Trim() : "Общие";
                Description = txtDescription.Text.Trim();
            };

            btnCancel = new Button {
                Text = "Отмена", DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(70,40,40), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = uiFont,
                Location = new Point(320, 258), Size = new Size(80, 30)
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] {
                lbl, rdContext, rdInsert, rdStyle,
                lblCat, txtCategory, lblDesc, txtDescription,
                btnOk, btnCancel
            });
        }
    }
}
