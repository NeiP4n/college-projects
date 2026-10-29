using System;
using System.Drawing;
using System.Windows.Forms;

namespace WordAIAgent.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            LoadCurrentSettings();
            LoadModelsAsync();
        }

        private void LoadCurrentSettings()
        {
            var s = ThisAddIn.Settings;
            txtHost.Text = s.OllamaHost;

            cmbModel.Items.Clear();
            if (!string.IsNullOrWhiteSpace(s.ModelName))
            {
                cmbModel.Items.Add(s.ModelName);
                cmbModel.SelectedItem = s.ModelName;
            }

            trkTemp.Value = (int)(s.Temperature * 10);
            lblTempVal.Text = s.Temperature.ToString("0.0");
            numTokens.Value = s.MaxTokens;
            txtSystemPrompt.Text = s.SystemPrompt;
        }

        private void SelectModelOrFirst(string preferredModel)
        {
            if (!string.IsNullOrWhiteSpace(preferredModel) && cmbModel.Items.Contains(preferredModel))
            {
                cmbModel.SelectedItem = preferredModel;
            }
            else if (cmbModel.Items.Count > 0)
            {
                cmbModel.SelectedIndex = 0;
            }
        }

        private async void LoadModelsAsync()
        {
            try
            {
                var preferredModel = ThisAddIn.Settings.ModelName?.Trim();
                var models = await ThisAddIn.OllamaService.GetModelsAsync();

                cmbModel.Items.Clear();
                foreach (var model in models)
                    cmbModel.Items.Add(model);

                if (!string.IsNullOrWhiteSpace(preferredModel) && !cmbModel.Items.Contains(preferredModel))
                    cmbModel.Items.Add(preferredModel);

                SelectModelOrFirst(preferredModel);
            }
            catch
            {
                // Keep current value if models fetch fails.
            }
        }

        private async void BtnRefreshModels_Click(object sender, EventArgs e)
        {
            btnRefreshModels.Enabled = false;
            try
            {
                ThisAddIn.Settings.OllamaHost = txtHost.Text.Trim();
                ThisAddIn.OllamaService.UpdateSettings(ThisAddIn.Settings);

                var preferredModel = ThisAddIn.Settings.ModelName?.Trim();
                var models = await ThisAddIn.OllamaService.GetModelsAsync();

                cmbModel.Items.Clear();
                foreach (var model in models)
                    cmbModel.Items.Add(model);

                if (!string.IsNullOrWhiteSpace(preferredModel) && !cmbModel.Items.Contains(preferredModel))
                    cmbModel.Items.Add(preferredModel);

                SelectModelOrFirst(preferredModel);

                if (cmbModel.Items.Count > 0)
                {
                    lblConnStatus.Text = $"Найдено моделей: {cmbModel.Items.Count}";
                    lblConnStatus.ForeColor = Color.LimeGreen;
                }
                else
                {
                    lblConnStatus.Text = "Модели не найдены";
                    lblConnStatus.ForeColor = Color.Orange;
                }
            }
            catch (Exception ex)
            {
                lblConnStatus.Text = ex.Message;
                lblConnStatus.ForeColor = Color.OrangeRed;
            }
            finally
            {
                btnRefreshModels.Enabled = true;
            }
        }

        private async void BtnTestConn_Click(object sender, EventArgs e)
        {
            btnTestConn.Enabled = false;
            lblConnStatus.Text = "Проверяю...";
            lblConnStatus.ForeColor = Color.Yellow;

            try
            {
                ThisAddIn.Settings.OllamaHost = txtHost.Text.Trim();
                ThisAddIn.OllamaService.UpdateSettings(ThisAddIn.Settings);

                var connected = await ThisAddIn.OllamaService.TestConnectionAsync();
                if (connected)
                {
                    lblConnStatus.Text = "Подключено успешно";
                    lblConnStatus.ForeColor = Color.LimeGreen;
                }
                else
                {
                    lblConnStatus.Text = "Не удалось подключиться";
                    lblConnStatus.ForeColor = Color.OrangeRed;
                }
            }
            catch (Exception ex)
            {
                lblConnStatus.Text = ex.Message;
                lblConnStatus.ForeColor = Color.OrangeRed;
            }
            finally
            {
                btnTestConn.Enabled = true;
            }
        }

        private void TrkTemp_Scroll(object sender, EventArgs e)
        {
            lblTempVal.Text = (trkTemp.Value / 10.0).ToString("0.0");
        }

        private void BtnResetPrompt_Click(object sender, EventArgs e)
        {
            txtSystemPrompt.Text = new Models.AppSettings().SystemPrompt;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var s = ThisAddIn.Settings;
            s.OllamaHost = txtHost.Text.Trim();
            s.ModelName = (cmbModel.SelectedItem?.ToString() ?? cmbModel.Text).Trim();
            s.Temperature = trkTemp.Value / 10.0;
            s.MaxTokens = (int)numTokens.Value;
            s.SystemPrompt = txtSystemPrompt.Text;
            ThisAddIn.SaveSettings();

            MessageBox.Show("Настройки сохранены", "Сохранено",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
