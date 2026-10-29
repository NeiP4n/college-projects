namespace OllamaAI
{
    partial class OllamaRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        private System.ComponentModel.IContainer components = null;

        public OllamaRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Designer generated code

        private void InitializeComponent()
        {
            this.tabOllama = this.Factory.CreateRibbonTab();
            this.groupConnection = this.Factory.CreateRibbonGroup();
            this.cboModel = this.Factory.CreateRibbonComboBox();
            this.btnRefresh = this.Factory.CreateRibbonButton();
            this.btnDiscoverLan = this.Factory.CreateRibbonButton();
            this.btnAutoRefresh = this.Factory.CreateRibbonToggleButton();
            this.btnSettings = this.Factory.CreateRibbonButton();
            this.lblStatus = this.Factory.CreateRibbonLabel();
            this.lblConnectionInfo = this.Factory.CreateRibbonLabel();
            this.lblEndpoint = this.Factory.CreateRibbonLabel();
            this.groupWrite = this.Factory.CreateRibbonGroup();
            this.btnImprove = this.Factory.CreateRibbonButton();
            this.btnRewriteTracked = this.Factory.CreateRibbonButton();
            this.btnTranslate = this.Factory.CreateRibbonButton();
            this.btnTone = this.Factory.CreateRibbonButton();
            this.btnGrammar = this.Factory.CreateRibbonButton();
            this.btnSummarize = this.Factory.CreateRibbonButton();
            this.sepWrite = this.Factory.CreateRibbonSeparator();
            this.btnExpand = this.Factory.CreateRibbonButton();
            this.btnShorten = this.Factory.CreateRibbonButton();
            this.groupGenerate = this.Factory.CreateRibbonGroup();
            this.btnPrompt = this.Factory.CreateRibbonButton();
            this.btnAgent = this.Factory.CreateRibbonButton();
            this.btnContinue = this.Factory.CreateRibbonButton();
            this.btnTable = this.Factory.CreateRibbonButton();
            this.btnList = this.Factory.CreateRibbonButton();
            this.btnHeadings = this.Factory.CreateRibbonButton();
            this.btnGostSpec = this.Factory.CreateRibbonButton();
            this.sepGenerate = this.Factory.CreateRibbonSeparator();
            this.btnExecutiveSummary = this.Factory.CreateRibbonButton();
            this.groupReview = this.Factory.CreateRibbonGroup();
            this.btnAnalyze = this.Factory.CreateRibbonButton();
            this.btnKeyPoints = this.Factory.CreateRibbonButton();
            this.btnErrors = this.Factory.CreateRibbonButton();
            this.btnStats = this.Factory.CreateRibbonButton();
            this.btnExplainComment = this.Factory.CreateRibbonButton();
            this.groupKnowledge = this.Factory.CreateRibbonGroup();
            this.btnKbManage = this.Factory.CreateRibbonButton();
            this.cboTemplate = this.Factory.CreateRibbonComboBox();
            this.btnKbFromSelection = this.Factory.CreateRibbonButton();
            this.btnKbApply = this.Factory.CreateRibbonButton();
            this.btnKbGenerate = this.Factory.CreateRibbonButton();
            this.lblTemplateInfo = this.Factory.CreateRibbonLabel();
            this.groupChat = this.Factory.CreateRibbonGroup();
            this.btnChat = this.Factory.CreateRibbonToggleButton();
            this.btnAskDoc = this.Factory.CreateRibbonButton();
            this.tabOllama.SuspendLayout();
            this.groupConnection.SuspendLayout();
            this.groupWrite.SuspendLayout();
            this.groupGenerate.SuspendLayout();
            this.groupReview.SuspendLayout();
            this.groupKnowledge.SuspendLayout();
            this.groupChat.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabOllama
            // 
            this.tabOllama.Groups.Add(this.groupConnection);
            this.tabOllama.Groups.Add(this.groupWrite);
            this.tabOllama.Groups.Add(this.groupGenerate);
            this.tabOllama.Groups.Add(this.groupReview);
            this.tabOllama.Groups.Add(this.groupKnowledge);
            this.tabOllama.Groups.Add(this.groupChat);
            this.tabOllama.Label = "Ollama AI";
            this.tabOllama.Name = "tabOllama";
            // 
            // groupConnection
            // 
            this.groupConnection.Items.Add(this.cboModel);
            this.groupConnection.Items.Add(this.btnRefresh);
            this.groupConnection.Items.Add(this.btnDiscoverLan);
            this.groupConnection.Items.Add(this.btnAutoRefresh);
            this.groupConnection.Items.Add(this.btnSettings);
            this.groupConnection.Items.Add(this.lblStatus);
            this.groupConnection.Items.Add(this.lblConnectionInfo);
            this.groupConnection.Items.Add(this.lblEndpoint);
            this.groupConnection.Label = "Подключение";
            this.groupConnection.Name = "groupConnection";
            // 
            // cboModel
            // 
            this.cboModel.Label = "Модель";
            this.cboModel.Name = "cboModel";
            this.cboModel.SizeString = "llama3.2:latest (2.0 GB)_______";
            this.cboModel.Text = "Нет моделей";
            this.cboModel.TextChanged += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.cboModel_TextChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeRegular;
            this.btnRefresh.OfficeImageId = "Refresh";
            this.btnRefresh.Label = "Обновить";
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.ShowImage = true;
            this.btnRefresh.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnRefresh_Click);
            // 
            // btnDiscoverLan
            // 
            this.btnDiscoverLan.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeRegular;
            this.btnDiscoverLan.OfficeImageId = "Refresh";
            this.btnDiscoverLan.Label = "Найти в LAN";
            this.btnDiscoverLan.Name = "btnDiscoverLan";
            this.btnDiscoverLan.ShowImage = true;
            this.btnDiscoverLan.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnDiscoverLan_Click);
            // 
            // btnAutoRefresh
            // 
            this.btnAutoRefresh.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeRegular;
            this.btnAutoRefresh.OfficeImageId = "RefreshAll";
            this.btnAutoRefresh.Label = "Авто";
            this.btnAutoRefresh.Name = "btnAutoRefresh";
            this.btnAutoRefresh.ShowImage = true;
            this.btnAutoRefresh.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnAutoRefresh_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeRegular;
            this.btnSettings.OfficeImageId = "OptionsDialog";
            this.btnSettings.Label = "Настройки";
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.ShowImage = true;
            this.btnSettings.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnSettings_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Label = "[OFF]";
            this.lblStatus.Name = "lblStatus";
            // 
            // lblConnectionInfo
            // 
            this.lblConnectionInfo.Label = "Проверка...";
            this.lblConnectionInfo.Name = "lblConnectionInfo";
            // 
            // lblEndpoint
            // 
            this.lblEndpoint.Label = "http://localhost:11434";
            this.lblEndpoint.Name = "lblEndpoint";
            // 
            // groupWrite
            // 
            this.groupWrite.Items.Add(this.btnImprove);
            this.groupWrite.Items.Add(this.btnRewriteTracked);
            this.groupWrite.Items.Add(this.btnTranslate);
            this.groupWrite.Items.Add(this.btnTone);
            this.groupWrite.Items.Add(this.btnGrammar);
            this.groupWrite.Items.Add(this.btnSummarize);
            this.groupWrite.Items.Add(this.sepWrite);
            this.groupWrite.Items.Add(this.btnExpand);
            this.groupWrite.Items.Add(this.btnShorten);
            this.groupWrite.Label = "Текст";
            this.groupWrite.Name = "groupWrite";
            // 
            // btnImprove
            // 
            this.btnImprove.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnImprove.OfficeImageId = "SpellingAndGrammar";
            this.btnImprove.Label = "Улучшить";
            this.btnImprove.Name = "btnImprove";
            this.btnImprove.ShowImage = true;
            this.btnImprove.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnImprove_Click);
            // 
            // btnRewriteTracked
            // 
            this.btnRewriteTracked.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnRewriteTracked.OfficeImageId = "ReviewTrackChanges";
            this.btnRewriteTracked.Label = "Переписать (правки)";
            this.btnRewriteTracked.Name = "btnRewriteTracked";
            this.btnRewriteTracked.ShowImage = true;
            this.btnRewriteTracked.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnRewriteTracked_Click);
            // 
            // btnTranslate
            // 
            this.btnTranslate.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnTranslate.OfficeImageId = "TranslateDocument";
            this.btnTranslate.Label = "Перевести";
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.ShowImage = true;
            this.btnTranslate.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnTranslate_Click);
            // 
            // btnTone
            // 
            this.btnTone.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnTone.OfficeImageId = "StylesPane";
            this.btnTone.Label = "Тон";
            this.btnTone.Name = "btnTone";
            this.btnTone.ShowImage = true;
            this.btnTone.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnTone_Click);
            // 
            // btnGrammar
            // 
            this.btnGrammar.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnGrammar.OfficeImageId = "SpellingAndGrammar";
            this.btnGrammar.Label = "Исправить";
            this.btnGrammar.Name = "btnGrammar";
            this.btnGrammar.ShowImage = true;
            this.btnGrammar.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnGrammar_Click);
            // 
            // btnSummarize
            // 
            this.btnSummarize.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnSummarize.OfficeImageId = "WordCount";
            this.btnSummarize.Label = "Суммаризация";
            this.btnSummarize.Name = "btnSummarize";
            this.btnSummarize.ShowImage = true;
            this.btnSummarize.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnSummarize_Click);
            // 
            // sepWrite
            // 
            this.sepWrite.Name = "sepWrite";
            // 
            // btnExpand
            // 
            this.btnExpand.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnExpand.OfficeImageId = "FieldNext";
            this.btnExpand.Label = "Расширить";
            this.btnExpand.Name = "btnExpand";
            this.btnExpand.ShowImage = true;
            this.btnExpand.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnExpand_Click);
            // 
            // btnShorten
            // 
            this.btnShorten.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnShorten.OfficeImageId = "FieldPrevious";
            this.btnShorten.Label = "Сократить";
            this.btnShorten.Name = "btnShorten";
            this.btnShorten.ShowImage = true;
            this.btnShorten.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnShorten_Click);
            // 
            // groupGenerate
            // 
            this.groupGenerate.Items.Add(this.btnPrompt);
            this.groupGenerate.Items.Add(this.btnAgent);
            this.groupGenerate.Items.Add(this.btnContinue);
            this.groupGenerate.Items.Add(this.btnTable);
            this.groupGenerate.Items.Add(this.btnList);
            this.groupGenerate.Items.Add(this.btnHeadings);
            this.groupGenerate.Items.Add(this.btnGostSpec);
            this.groupGenerate.Items.Add(this.sepGenerate);
            this.groupGenerate.Items.Add(this.btnExecutiveSummary);
            this.groupGenerate.Label = "Генерация";
            this.groupGenerate.Name = "groupGenerate";
            // 
            // btnPrompt
            // 
            this.btnPrompt.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnPrompt.OfficeImageId = "CreateList";
            this.btnPrompt.Label = "По запросу";
            this.btnPrompt.Name = "btnPrompt";
            this.btnPrompt.ShowImage = true;
            this.btnPrompt.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnPrompt_Click);
            // 
            // btnAgent
            // 
            this.btnAgent.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnAgent.OfficeImageId = "DiagramCreate";
            this.btnAgent.Label = "Агент";
            this.btnAgent.Name = "btnAgent";
            this.btnAgent.ShowImage = true;
            this.btnAgent.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnAgent_Click);
            // 
            // btnContinue
            // 
            this.btnContinue.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnContinue.OfficeImageId = "AutoTextInsert";
            this.btnContinue.Label = "Продолжить";
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.ShowImage = true;
            this.btnContinue.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnContinue_Click);
            // 
            // btnTable
            // 
            this.btnTable.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnTable.OfficeImageId = "TableInsert";
            this.btnTable.Label = "Таблица";
            this.btnTable.Name = "btnTable";
            this.btnTable.ShowImage = true;
            this.btnTable.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnTable_Click);
            // 
            // btnList
            // 
            this.btnList.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnList.OfficeImageId = "ListBullets";
            this.btnList.Label = "Список";
            this.btnList.Name = "btnList";
            this.btnList.ShowImage = true;
            this.btnList.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnList_Click);
            // 
            // btnHeadings
            // 
            this.btnHeadings.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnHeadings.OfficeImageId = "OutlineViewClose";
            this.btnHeadings.Label = "Заголовки";
            this.btnHeadings.Name = "btnHeadings";
            this.btnHeadings.ShowImage = true;
            this.btnHeadings.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnHeadings_Click);
            // 
            // btnGostSpec
            // 
            this.btnGostSpec.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnGostSpec.OfficeImageId = "TableStylesGallery";
            this.btnGostSpec.Label = "ТЗ по ГОСТ";
            this.btnGostSpec.Name = "btnGostSpec";
            this.btnGostSpec.ShowImage = true;
            this.btnGostSpec.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnGostSpec_Click);
            // 
            // sepGenerate
            // 
            this.sepGenerate.Name = "sepGenerate";
            // 
            // btnExecutiveSummary
            // 
            this.btnExecutiveSummary.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnExecutiveSummary.OfficeImageId = "TableStylesGallery";
            this.btnExecutiveSummary.Label = "Резюме в начало";
            this.btnExecutiveSummary.Name = "btnExecutiveSummary";
            this.btnExecutiveSummary.ShowImage = true;
            this.btnExecutiveSummary.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnExecutiveSummary_Click);
            // 
            // groupReview
            // 
            this.groupReview.Items.Add(this.btnAnalyze);
            this.groupReview.Items.Add(this.btnKeyPoints);
            this.groupReview.Items.Add(this.btnErrors);
            this.groupReview.Items.Add(this.btnStats);
            this.groupReview.Items.Add(this.btnExplainComment);
            this.groupReview.Label = "Анализ";
            this.groupReview.Name = "groupReview";
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnAnalyze.OfficeImageId = "TableStylesGallery";
            this.btnAnalyze.Label = "Анализ док.";
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.ShowImage = true;
            this.btnAnalyze.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnAnalyze_Click);
            // 
            // btnKeyPoints
            // 
            this.btnKeyPoints.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnKeyPoints.OfficeImageId = "OutlineMoveUp";
            this.btnKeyPoints.Label = "Ключевые идеи";
            this.btnKeyPoints.Name = "btnKeyPoints";
            this.btnKeyPoints.ShowImage = true;
            this.btnKeyPoints.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnKeyPoints_Click);
            // 
            // btnErrors
            // 
            this.btnErrors.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnErrors.OfficeImageId = "ReviewTrackChanges";
            this.btnErrors.Label = "Ошибки";
            this.btnErrors.Name = "btnErrors";
            this.btnErrors.ShowImage = true;
            this.btnErrors.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnErrors_Click);
            // 
            // btnStats
            // 
            this.btnStats.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnStats.OfficeImageId = "WordCount";
            this.btnStats.Label = "Статистика";
            this.btnStats.Name = "btnStats";
            this.btnStats.ShowImage = true;
            this.btnStats.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnStats_Click);
            // 
            // btnExplainComment
            // 
            this.btnExplainComment.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnExplainComment.OfficeImageId = "ReviewPane";
            this.btnExplainComment.Label = "Комментарий";
            this.btnExplainComment.Name = "btnExplainComment";
            this.btnExplainComment.ShowImage = true;
            this.btnExplainComment.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnExplainComment_Click);
            // 
            // groupKnowledge
            // 
            this.groupKnowledge.Items.Add(this.btnKbManage);
            this.groupKnowledge.Items.Add(this.cboTemplate);
            this.groupKnowledge.Items.Add(this.btnKbFromSelection);
            this.groupKnowledge.Items.Add(this.btnKbApply);
            this.groupKnowledge.Items.Add(this.btnKbGenerate);
            this.groupKnowledge.Items.Add(this.lblTemplateInfo);
            this.groupKnowledge.Label = "База знаний";
            this.groupKnowledge.Name = "groupKnowledge";
            // 
            // cboTemplate
            // 
            this.cboTemplate.Label = "Шаблон";
            this.cboTemplate.Name = "cboTemplate";
            this.cboTemplate.SizeString = "Technical Specification [Engineering]______";
            this.cboTemplate.Text = "Нет шаблонов";
            this.cboTemplate.TextChanged += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.cboTemplate_TextChanged);
            // 
            // btnKbManage
            // 
            this.btnKbManage.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnKbManage.OfficeImageId = "OptionsDialog";
            this.btnKbManage.Label = "Управление";
            this.btnKbManage.Name = "btnKbManage";
            this.btnKbManage.ShowImage = true;
            this.btnKbManage.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnKbManage_Click);
            // 
            // btnKbFromSelection
            // 
            this.btnKbFromSelection.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnKbFromSelection.OfficeImageId = "CreateList";
            this.btnKbFromSelection.Label = "Создать шаблон";
            this.btnKbFromSelection.Name = "btnKbFromSelection";
            this.btnKbFromSelection.ShowImage = true;
            this.btnKbFromSelection.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnKbFromSelection_Click);
            // 
            // btnKbApply
            // 
            this.btnKbApply.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnKbApply.OfficeImageId = "SpellingAndGrammar";
            this.btnKbApply.Label = "Применить";
            this.btnKbApply.Name = "btnKbApply";
            this.btnKbApply.ShowImage = true;
            this.btnKbApply.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnKbApply_Click);
            // 
            // btnKbGenerate
            // 
            this.btnKbGenerate.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnKbGenerate.OfficeImageId = "AutoTextInsert";
            this.btnKbGenerate.Label = "Генерировать";
            this.btnKbGenerate.Name = "btnKbGenerate";
            this.btnKbGenerate.ShowImage = true;
            this.btnKbGenerate.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnKbGenerate_Click);
            // 
            // lblTemplateInfo
            // 
            this.lblTemplateInfo.Label = "Шаблоны: 0";
            this.lblTemplateInfo.Name = "lblTemplateInfo";
            // 
            // groupChat
            // 
            this.groupChat.Items.Add(this.btnChat);
            this.groupChat.Items.Add(this.btnAskDoc);
            this.groupChat.Label = "Чат";
            this.groupChat.Name = "groupChat";
            // 
            // btnChat
            // 
            this.btnChat.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnChat.OfficeImageId = "GroupMailingAndTracking";
            this.btnChat.Label = "Чат";
            this.btnChat.Name = "btnChat";
            this.btnChat.ShowImage = true;
            this.btnChat.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnChat_Click);
            // 
            // btnAskDoc
            // 
            this.btnAskDoc.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnAskDoc.OfficeImageId = "GroupMailingAndTracking";
            this.btnAskDoc.Label = "Вопрос по док.";
            this.btnAskDoc.Name = "btnAskDoc";
            this.btnAskDoc.ShowImage = true;
            this.btnAskDoc.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnAskDoc_Click);
            // 
            // OllamaRibbon
            // 
            this.Name = "OllamaRibbon";
            this.RibbonType = "Microsoft.Word.Document";
            this.Tabs.Add(this.tabOllama);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.OllamaRibbon_Load);
            this.tabOllama.ResumeLayout(false);
            this.tabOllama.PerformLayout();
            this.groupConnection.ResumeLayout(false);
            this.groupConnection.PerformLayout();
            this.groupWrite.ResumeLayout(false);
            this.groupWrite.PerformLayout();
            this.groupGenerate.ResumeLayout(false);
            this.groupGenerate.PerformLayout();
            this.groupReview.ResumeLayout(false);
            this.groupReview.PerformLayout();
            this.groupKnowledge.ResumeLayout(false);
            this.groupKnowledge.PerformLayout();
            this.groupChat.ResumeLayout(false);
            this.groupChat.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabOllama;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupConnection;
        internal Microsoft.Office.Tools.Ribbon.RibbonComboBox cboModel;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnRefresh;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnDiscoverLan;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton btnAutoRefresh;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSettings;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel lblStatus;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel lblConnectionInfo;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel lblEndpoint;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupWrite;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnImprove;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnRewriteTracked;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnTranslate;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnTone;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnGrammar;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSummarize;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator sepWrite;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnExpand;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnShorten;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupGenerate;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnPrompt;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnAgent;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnContinue;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnTable;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnList;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnHeadings;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnGostSpec;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator sepGenerate;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnExecutiveSummary;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupReview;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnAnalyze;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnKeyPoints;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnErrors;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnStats;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnExplainComment;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupKnowledge;
        internal Microsoft.Office.Tools.Ribbon.RibbonComboBox cboTemplate;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnKbManage;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnKbFromSelection;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnKbApply;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnKbGenerate;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel lblTemplateInfo;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupChat;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton btnChat;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnAskDoc;
    }

    partial class ThisRibbonCollection
    {
        internal OllamaRibbon OllamaRibbon
        {
            get { return this.GetRibbon<OllamaRibbon>(); }
        }
    }
}

