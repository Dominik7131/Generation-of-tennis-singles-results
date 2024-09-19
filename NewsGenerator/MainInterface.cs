using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static NewsGenerator.UIComponents;

namespace NewsGenerator
{
    public partial class NewsGenerator : Form
    {
        public static bool isEvaluation = false;
        public const int EVALUATION_NEWS_COUNT = 50;

        public NewsGenerator()
        {
            InitializeComponent();
        }

        private void NewsGenerator_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            UIComponents.Initialize(ClientSize.Width, ClientSize.Height);
            CreateInputPanelElements();
            CreateControlPanelElements();
            CreateSettingsPanelElements();
            CreateTextPanelElements();
            bool areTemplatesLoaded = Templates.Init();
            if (!areTemplatesLoaded)
            {
                Application.Exit();
            }
            else
            {
                Utility.MorphoditaGenerator.Init();
                Utility.MorphoditaTagger.Init();
            }
        }

        private void CreateInputPanelElements()
        {
            UIComponents.CreateInputPanelElements();

            // Add controls
            Controls.Add(winnerNameTextBox);
            Controls.Add(winnerNameLabel);
            Controls.Add(loserNameTextBox);
            Controls.Add(loserNameLabel);
            Controls.Add(areMaleCheckBox);
            Controls.Add(areMaleLabel);
            Controls.Add(tournamentNameTextBox);
            Controls.Add(tournamentNameLabel);
            Controls.Add(roundComboBox);
            Controls.Add(roundLabel);
            Controls.Add(scoreTextBox);
            Controls.Add(scoreLabel);
            Controls.Add(lengthTextBox);
            Controls.Add(lengthLabel);
            Controls.Add(tournamentPlaceTextBox);
            Controls.Add(tournamentPlaceLabel);
            Controls.Add(tournamentTypeTextBox);
            Controls.Add(tournamentTypeLabel);
            Controls.Add(tournamentSurfaceComboBox);
            Controls.Add(tournamentSurfaceLabel);

            // Event handlers
            winnerNameTextBox.Validating += OnTextBoxWinnerValidating;
            loserNameTextBox.Validating += OnTextBoxLoserValidating;
            tournamentNameTextBox.Validating += OnTextBoxTournamentNameValidating;
            scoreTextBox.Validating += OnTextBoxScoreValidating;
            lengthTextBox.Validating += OnTextBoxLengthValidating;

            winnerNameTextBox.Validated += OnBoxValidated;
            loserNameTextBox.Validated += OnBoxValidated;
            tournamentNameTextBox.Validated += OnBoxValidated;
            scoreTextBox.Validated += OnBoxValidated;
            lengthTextBox.Validated += OnBoxValidated;
            roundComboBox.Validated += OnBoxValidated;

            tournamentPlaceTextBox.Validated += OnBoxValidated;
            tournamentTypeTextBox.Validated += OnBoxValidated;
            tournamentSurfaceComboBox.Validated += OnBoxValidated;
        }

        private void CreateControlPanelElements()
        {
            UIComponents.CreateControlPanelElements();

            // Position
            generateButton.Left = ((ClientSize.Width - buttonWidth) / 2);
            generateButton.Top = ClientSize.Height - buttonHeight;

            loadFromFileButton.Left = 0;
            loadFromFileButton.Top = ClientSize.Height - buttonHeight;

            loadRandomInputButton.Left = buttonWidth;
            loadRandomInputButton.Top = ClientSize.Height - buttonHeight;

            saveToFileButton.Top = ClientSize.Height - buttonHeight;
            saveToFileButton.Left = ClientSize.Width - buttonWidth;

            // Event handlers
            generateButton.Click += OnClickGenerateButton;
            loadFromFileButton.Click += OnClickLoadFromFileButton;
            loadRandomInputButton.Click += OnClickLoadRandomInputButton;
            saveToFileButton.Click += OnClickSaveToFileButton;

            // Add controls
            Controls.Add(generateButton);
            Controls.Add(loadFromFileButton);
            Controls.Add(loadRandomInputButton);
        }

        private void CreateSettingsPanelElements()
        {
            UIComponents.CreateSettingsPanelElements();

            Controls.Add(templatesVisibilityLabel);
            Controls.Add(templatesVisibilityCheckBox);

            Controls.Add(resetCategoriesLabel);
            Controls.Add(resetCategoriesCheckBox);
        }

        private void CreateTextPanelElements()
        {
            UIComponents.CreateTextPanelElements();
            HideTextElements();
            // ShowTextElements();

            // Add controls
            Controls.Add(titleLabel);
            Controls.Add(titleText);
            Controls.Add(horizontalLine);
            Controls.Add(resultLabel);
            Controls.Add(resultText);
            Controls.Add(setLabel);
            Controls.Add(setText);
            Controls.Add(horizontalLine2);
            Controls.Add(saveToFileButton);
        }

        private void OnClickLoadRandomInputButton(object sender, EventArgs e)
        {
            InputManager.LoadInput(isRandomInput: true);
            UIComponents.SetInput();
        }

        private void OnTextBoxWinnerValidating(object sender, EventArgs e)
        {
            ValidateNameTextBox(winnerNameTextBox);
        }
        private void OnTextBoxLoserValidating(object sender, EventArgs e)
        {
            ValidateNameTextBox(loserNameTextBox);
        }
        private void OnTextBoxTournamentNameValidating(object sender, EventArgs e)
        {
            ValidateTextBox(tournamentNameTextBox);
        }
        private void OnTextBoxScoreValidating(object sender, EventArgs e)
        {
            ValidateTextBox(scoreTextBox);
        }
        private void OnTextBoxLengthValidating(object sender, EventArgs e)
        {
            ValidateTextBox(lengthTextBox);
        }

        private void ValidateTextBox(TextBox textBox)
        {
            bool isValid = textBox.Text != "";
            if (isValid)
            {
                errorProvider.SetError(textBox, "");
            }
            else
            {
                errorProvider.SetError(textBox, "Nesprávný formát parametru");
            }
        }

        private void ValidateNameTextBox(TextBox textBox)
        {
            bool isValid = textBox.Text != "" && textBox.Text.IndexOf(' ') != -1;
            if (isValid)
            {
                errorProvider.SetError(textBox, "");
            }
            else
            {
                errorProvider.SetError(textBox, "Nesprávný formát parametru");
            }
        }

        private void OnClickGenerateButton(object sender, EventArgs e)
        {
            bool isValid = InputManager.CheckInput();
            if (!isValid)
            {
                return;
            }

            bool isResetCategories = resetCategoriesCheckBox.Checked;
            if (isResetCategories)
            {
                Categories.Reset();
                resetCategoriesCheckBox.Checked = false;
            }
            bool isShowTemplates = templatesVisibilityCheckBox.Checked;
            GenerateNews(InputManager.matchParameters, isShowTemplates);
        }

        private void OnClickLoadFromFileButton(object sender, EventArgs e)
        {
            if (isEvaluation)
            {
                templatesVisibilityCheckBox.Checked = true;
                for (int i = 0; i < EVALUATION_NEWS_COUNT; i++)
                {
                    OnClickLoadRandomInputButton(sender, e);
                    OnClickGenerateButton(sender, e);
                    OnClickSaveToFileButton(sender, e);
                }
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Vyberte vstupní soubor v json formátu";
            dialog.Filter = "json files|*.json";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                bool isLoaded = InputManager.LoadInput(dialog.FileName);
                if (!isLoaded)
                {
                    MessageBox.Show("Chyba: Neplatný soubor");
                    return;
                }
                UIComponents.SetInput();
            }
        }

        private void OnClickSaveToFileButton(object sender, EventArgs e)
        {
            UIComponents.AppendToFile();
        }

        private void GenerateNews(MatchParameters matchParameters, bool isShowTemplates)
        {
            string titleText = matchParameters.Generate(MessageType.TITLE, isShowTemplates);
            string resultText = matchParameters.Generate(MessageType.RESULT, isShowTemplates);
            string matchText = matchParameters.Generate(MessageType.MATCHTITLE, isShowTemplates);

            UIComponents.SetTextElements(titleText, resultText, matchText);
            UIComponents.ShowTextElements();
        }
    }
}