using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OllamaAI
{
    public class TemplateEditForm : Form
    {
        private readonly TextBox _name;
        private readonly TextBox _category;
        private readonly TextBox _instruction;
        private readonly TextBox _format;
        private readonly TextBox _example;
        private readonly CheckBox _pinned;

        public KnowledgeTemplate Template { get; private set; }

        public TemplateEditForm(KnowledgeTemplate initial = null)
        {
            Text = initial == null ? "Новый шаблон" : "Редактирование шаблона";
            Size = new Size(760, 640);
            MinimumSize = new Size(680, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Pal.Bg0;
            AutoScaleMode = AutoScaleMode.Dpi;

            _name = CreateTextBox(initial != null ? initial.Name : "", false);
            _category = CreateTextBox(initial != null ? initial.Category : "", false);
            _instruction = CreateTextBox(initial != null ? initial.Instruction : "", true);
            _format = CreateTextBox(initial != null ? initial.OutputFormat : "", true);
            _example = CreateTextBox(initial != null ? initial.Example : "", true);
            _pinned = new CheckBox
            {
                Text = "Закрепить шаблон",
                Checked = initial != null && initial.IsPinned,
                ForeColor = Pal.Txt,
                BackColor = Pal.Bg0,
                Font = Pal.F
            };

            var saveButton = Pal.Btn("Сохранить", Pal.Grn);
            saveButton.Image = Pal.ISave;
            saveButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            saveButton.ForeColor = Pal.Bg0;
            saveButton.Click += delegate
            {
                var name = (_name.Text ?? "").Trim();
                var instruction = (_instruction.Text ?? "").Trim();
                if (name.Length == 0 || instruction.Length == 0)
                {
                    MessageBox.Show("Поля «Название шаблона» и «Инструкция» обязательны.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var t = initial != null ? initial.Clone() : KnowledgeBaseStore.Create("", "", "", "", "");
                t.Name = name;
                t.Category = (_category.Text ?? "").Trim();
                t.Instruction = instruction;
                t.OutputFormat = (_format.Text ?? "").Trim();
                t.Example = (_example.Text ?? "").Trim();
                t.IsPinned = _pinned.Checked;
                t.UpdatedUtc = DateTime.UtcNow;
                if (t.CreatedUtc == default(DateTime))
                {
                    t.CreatedUtc = DateTime.UtcNow;
                }

                Template = t;
                DialogResult = DialogResult.OK;
                Close();
            };

            var cancelButton = Pal.Btn("Отмена", Pal.Bg2);
            cancelButton.Image = Pal.ICancel;
            cancelButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            cancelButton.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Pal.Bg0
            };
            buttons.Controls.Add(saveButton);
            buttons.Controls.Add(cancelButton);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                ColumnCount = 1,
                RowCount = 8
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 38f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 28f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 28f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(Field("Название шаблона", _name), 0, 0);
            layout.Controls.Add(Field("Категория", _category), 0, 1);
            layout.Controls.Add(Field("Инструкция (что должен делать ИИ)", _instruction), 0, 2);
            layout.Controls.Add(Field("Формат вывода (строгая структура)", _format), 0, 3);
            layout.Controls.Add(Field("Пример (необязательно)", _example), 0, 4);
            layout.Controls.Add(_pinned, 0, 5);
            layout.Controls.Add(buttons, 0, 7);

            Controls.Add(layout);
        }

        private static TextBox CreateTextBox(string text, bool multiline)
        {
            return new TextBox
            {
                Text = text ?? "",
                Multiline = multiline,
                ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
                Height = multiline ? 120 : 26,
                BackColor = Pal.Bg2,
                ForeColor = Pal.Txt,
                Font = multiline ? Pal.FS : Pal.F,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill
            };
        }

        private static Panel Field(string label, Control content)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Pal.Bg0, Padding = new Padding(0, 4, 0, 6) };
            var lbl = Pal.Lbl(label, true);
            lbl.Dock = DockStyle.Top;
            var inner = new Panel { Dock = DockStyle.Fill, BackColor = Pal.Bg2, Padding = new Padding(8) };
            inner.Controls.Add(content);
            p.Controls.Add(inner);
            p.Controls.Add(lbl);
            return p;
        }
    }

    public class KnowledgeBaseManagerForm : Form
    {
        private List<KnowledgeTemplate> _templates;
        private readonly ListBox _list;
        private readonly RichTextBox _preview;

        public List<KnowledgeTemplate> Templates => _templates.Select(x => x.Clone()).ToList();

        public KnowledgeBaseManagerForm(List<KnowledgeTemplate> templates)
        {
            _templates = templates != null ? templates.Select(x => x.Clone()).ToList() : new List<KnowledgeTemplate>();

            Text = "Менеджер базы знаний";
            Size = new Size(980, 620);
            MinimumSize = new Size(860, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Pal.Bg0;
            AutoScaleMode = AutoScaleMode.Dpi;

            _list = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Pal.Bg2,
                ForeColor = Pal.Txt,
                Font = Pal.F,
                BorderStyle = BorderStyle.None
            };
            _list.SelectedIndexChanged += delegate { UpdatePreview(); };

            _preview = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Pal.Bg2,
                ForeColor = Pal.Txt,
                Font = Pal.FC,
                BorderStyle = BorderStyle.None
            };

            var addBtn = Pal.Btn("Добавить");
            addBtn.Image = Pal.IAdd;
            addBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            addBtn.Click += delegate
            {
                using (var f = new TemplateEditForm())
                {
                    if (f.ShowDialog() != DialogResult.OK || f.Template == null)
                    {
                        return;
                    }

                    _templates.Add(f.Template.Clone());
                    Sort();
                    Rebind();
                    SelectById(f.Template.Id);
                }
            };

            var editBtn = Pal.Btn("Изменить");
            editBtn.Image = Pal.IEdit;
            editBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            editBtn.Click += delegate
            {
                var selected = Current();
                if (selected == null)
                {
                    return;
                }

                using (var f = new TemplateEditForm(selected))
                {
                    if (f.ShowDialog() != DialogResult.OK || f.Template == null)
                    {
                        return;
                    }

                    var idx = _templates.FindIndex(x => x.Id == selected.Id);
                    if (idx >= 0)
                    {
                        _templates[idx] = f.Template.Clone();
                        Sort();
                        Rebind();
                        SelectById(f.Template.Id);
                    }
                }
            };

            var deleteBtn = Pal.Btn("Удалить", Color.FromArgb(180, 70, 70));
            deleteBtn.Image = Pal.IDelete;
            deleteBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            deleteBtn.Click += delegate
            {
                var selected = Current();
                if (selected == null)
                {
                    return;
                }

                var ok = MessageBox.Show(
                    "Удалить шаблон '" + selected.Name + "'?",
                    "Удаление",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (ok != DialogResult.Yes)
                {
                    return;
                }

                _templates.RemoveAll(x => x.Id == selected.Id);
                Rebind();
            };

            var pinBtn = Pal.Btn("Закрепить/Открепить", Pal.Bg2);
            pinBtn.Image = Pal.IPin;
            pinBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            pinBtn.Click += delegate
            {
                var selected = Current();
                if (selected == null)
                {
                    return;
                }

                selected.IsPinned = !selected.IsPinned;
                selected.UpdatedUtc = DateTime.UtcNow;
                Sort();
                Rebind();
                SelectById(selected.Id);
            };

            var closeBtn = Pal.Btn("Сохранить и закрыть", Pal.Grn);
            closeBtn.Image = Pal.ISave;
            closeBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            closeBtn.ForeColor = Pal.Bg0;
            closeBtn.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            var leftButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, BackColor = Pal.Bg0, Padding = new Padding(0, 6, 0, 6), WrapContents = false, AutoScroll = true };
            leftButtons.Controls.Add(addBtn);
            leftButtons.Controls.Add(editBtn);
            leftButtons.Controls.Add(pinBtn);
            leftButtons.Controls.Add(deleteBtn);

            var left = new Panel { Dock = DockStyle.Fill, BackColor = Pal.Bg0 };
            left.Controls.Add(_list);
            left.Controls.Add(leftButtons);

            var rightHeader = Pal.Lbl("Предпросмотр шаблона", true);
            rightHeader.Dock = DockStyle.Top;
            rightHeader.Padding = new Padding(4, 4, 4, 8);

            var right = new Panel { Dock = DockStyle.Fill, BackColor = Pal.Bg0, Padding = new Padding(8, 0, 0, 0) };
            right.Controls.Add(_preview);
            right.Controls.Add(rightHeader);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BackColor = Pal.Bg0,
                SplitterDistance = 360
            };
            split.Panel1.BackColor = Pal.Bg0;
            split.Panel2.BackColor = Pal.Bg0;
            split.Panel1.Controls.Add(left);
            split.Panel2.Controls.Add(right);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Pal.Bg0,
                Padding = new Padding(0, 10, 0, 0),
                WrapContents = false
            };
            footer.Controls.Add(closeBtn);

            Controls.Add(split);
            Controls.Add(footer);

            Sort();
            Rebind();
        }

        private void Sort()
        {
            _templates = _templates
                .OrderByDescending(x => x.IsPinned)
                .ThenBy(x => x.Category ?? "", StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Name ?? "", StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void Rebind()
        {
            _list.Items.Clear();
            foreach (var t in _templates)
            {
                var pin = t.IsPinned ? "★ " : "";
                _list.Items.Add(pin + t.ToString());
            }

            if (_list.Items.Count > 0)
            {
                _list.SelectedIndex = 0;
            }
            else
            {
                _preview.Text = "База знаний пуста.";
            }
        }

        private KnowledgeTemplate Current()
        {
            if (_list.SelectedIndex < 0 || _list.SelectedIndex >= _templates.Count)
            {
                return null;
            }

            return _templates[_list.SelectedIndex];
        }

        private void UpdatePreview()
        {
            var t = Current();
            if (t == null)
            {
                _preview.Text = "Шаблон не выбран.";
                return;
            }

            _preview.Text =
                "Название: " + t.Name + "\n" +
                "Категория: " + t.Category + "\n" +
                "Закреплен: " + (t.IsPinned ? "Да" : "Нет") + "\n" +
                "Обновлен: " + t.UpdatedUtc.ToLocalTime().ToString("g") + "\n\n" +
                "Инструкция:\n" + t.Instruction + "\n\n" +
                "Формат вывода:\n" + t.OutputFormat + "\n\n" +
                "Пример:\n" + t.Example;
        }

        private void SelectById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            var idx = _templates.FindIndex(x => x.Id == id);
            if (idx >= 0)
            {
                _list.SelectedIndex = idx;
            }
        }
    }
}
