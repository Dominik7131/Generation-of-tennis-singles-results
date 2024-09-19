using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace NewsGenerator
{
    static class UIComponents
    {
        public static bool isInputChanged { get; set; } = false;
        public static int appendCount = 1;

        public static int clientSizeWidth { get; private set; }
        public static int clientSizeHeight { get; private set; }

        public static int buttonWidth { get; } = 120;
        public static int buttonHeight { get; } = 40;
        public static int textBoxWidth { get; } = 140;

        private static int fontSize { get; } = 14;
        private static int lowerFontSize { get; } = 7;
        private static string fontStyle { get; } = "Arial";
        private static Font inputFont { get; } = new Font(fontStyle, fontSize);
        private static Font inputFontBold { get; } = new Font(fontStyle, fontSize, FontStyle.Bold);
        private static Font lowerInputFont { get; } = new Font(fontStyle, lowerFontSize);

        private static int topOffset { get; set; } = 40;
        private static int topOffsetIncrement { get; } = topOffset;
        private static int leftOffset { get; } = 120;
        private static int labelWidth { get; } = 140;


        private static int articleTopOffset { get; set; } = 40;
        private static int articleTopOffsetIncrement { get; } = articleTopOffset * 6;
        private static int articleTextTopOffsetIncrement { get; } = 60;
        private static int articleLeftOffset { get; set; }
        private static int articleWidth { get; set; }
        private static int articleLabelWidth { get; } = 160;
        private static int articleLabelHeight { get; } = 40;
        private static int articleHeight { get; } = 160;

        private static int horizontalLineHeight { get; } = 3;

        // Control panel
        public static Button generateButton { get; set; }
        public static Button loadFromFileButton { get; set; }
        public static Button loadRandomInputButton { get; set; }

        // Input panel
        // Mandatory elements
        public static ErrorProvider errorProvider {get; set;}

        public static Label winnerNameLabel { get; set; }
        public static TextBox winnerNameTextBox { get; set; }

        public static Label loserNameLabel { get; set; }
        public static TextBox loserNameTextBox { get; set; }

        public static Label areMaleLabel { get; set; }
        public static CheckBox areMaleCheckBox { get; set; }

        public static Label tournamentNameLabel { get; set; }
        public static TextBox tournamentNameTextBox { get; set; }

        public static Label roundLabel { get; set; }
        public static ComboBox roundComboBox { get; set; }

        public static Label scoreLabel { get; set; }
        public static TextBox scoreTextBox { get; set; }

        public static Label lengthLabel { get; set; }
        public static TextBox lengthTextBox { get; set; }

        // Optional elements
        public static Label tournamentPlaceLabel { get; set; }
        public static TextBox tournamentPlaceTextBox { get; set; }

        public static Label tournamentTypeLabel { get; set; }
        public static TextBox tournamentTypeTextBox { get; set; }

        public static Label tournamentSurfaceLabel { get; set; }
        public static ComboBox tournamentSurfaceComboBox { get; set; }

        // Settings elements
        public static Label templatesVisibilityLabel { get; set; }
        public static CheckBox templatesVisibilityCheckBox { get; set; }

        public static Label resetCategoriesLabel { get; set; }
        public static CheckBox resetCategoriesCheckBox { get; set; }

        // Article elements
        public static Button saveToFileButton { get; set; }

        public static Label titleLabel { get; set; }
        public static Label titleText { get; set; }
        public static Label resultLabel { get; set; }
        public static Label resultText { get; set; }
        public static Label setLabel { get; set; }
        public static Label setText { get; set; }

        public static Label horizontalLine { get; set; }
        public static Label horizontalLine2 { get; set; }


        public static void Initialize(int formClientSizeWidth, int formClientSizeHeight)
        {
            clientSizeWidth = formClientSizeWidth;
            clientSizeHeight = formClientSizeHeight;
            articleLeftOffset = clientSizeWidth / 4;
            articleWidth = clientSizeWidth - (articleLeftOffset * 2);
        }

        public static void CreateInputPanelElements()
        {
            winnerNameLabel = new Label()
            {
                Text = "Vítěz*: ",
                Font = inputFont,
                Top = topOffset,
                Width = labelWidth
            };
            winnerNameTextBox = new TextBox()
            {
                Text = "Tomáš Berdych",
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset,
                Width = textBoxWidth
            };

            topOffset += topOffsetIncrement;

            loserNameLabel = new Label()
            {
                Font = inputFont,
                Text = "Poražený*: ",
                Top = topOffset,
                Width = labelWidth
            };
            loserNameTextBox = new TextBox()
            {
                Text = "Roger Federer",
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset,
                Width = textBoxWidth
            };

            topOffset += topOffsetIncrement;

            areMaleLabel = new Label()
            {
                Font = inputFont,
                Text = "Muži*: ",
                Top = topOffset,
                Width = labelWidth
            };
            areMaleCheckBox = new CheckBox()
            {
                Checked = true,
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset
            };

            topOffset += topOffsetIncrement;

            tournamentNameLabel = new Label()
            {
                Font = inputFont,
                Text = "Turnaj*: ",
                Top = topOffset,
                Width = labelWidth
            };
            tournamentNameTextBox = new TextBox()
            {
                Text = "Wimbledon",
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset,
                Width = textBoxWidth
            };

            topOffset += topOffsetIncrement;

            roundLabel = new Label()
            {
                Font = inputFont,
                Text = "Kolo*: ",
                Top = topOffset,
                Width = labelWidth
            };
            roundComboBox = new ComboBox()
            {
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset,
                Width = textBoxWidth,
                DropDownStyle = ComboBoxStyle.DropDownList // Do not allow user to type into combo box
            };
            foreach (string round in InputList.rounds)
            {
                roundComboBox.Items.Add(round);
            }
            roundComboBox.SelectedItem = "1. kolo";

            topOffset += topOffsetIncrement;

            scoreLabel = new Label()
            {
                Font = inputFont,
                Text = "Skóre*: ",
                Top = topOffset,
                Width = labelWidth
            };
            scoreTextBox = new TextBox()
            {
                Text = "6:4,6:3",
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset,
                Width = textBoxWidth
            };

            topOffset += topOffsetIncrement;

            lengthLabel = new Label()
            {
                Font = inputFont,
                Text = "Délka*: ",
                Top = topOffset,
                Width = labelWidth
            };
            lengthTextBox = new TextBox()
            {
                Text = "1:30",
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset,
                Width = textBoxWidth
            };

            topOffset += topOffsetIncrement * 2;

            // Optional elements
            tournamentPlaceLabel = new Label()
            {
                Font = inputFont,
                Text = "Místo: ",
                Top = topOffset,
                Width = labelWidth
            };
            tournamentPlaceTextBox = new TextBox()
            {
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset,
                Width = textBoxWidth
            };

            topOffset += topOffsetIncrement;


            tournamentTypeLabel = new Label()
            {
                Font = inputFont,
                Text = "Typ: ",
                Top = topOffset,
                Width = labelWidth
            };
            tournamentTypeTextBox = new TextBox()
            {
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset,
                Width = textBoxWidth
            };

            topOffset += topOffsetIncrement;

            tournamentSurfaceLabel = new Label()
            {
                Font = inputFont,
                Text = "Povrch: ",
                Top = topOffset,
                Width = labelWidth
            };
            tournamentSurfaceComboBox = new ComboBox()
            {
                Font = inputFont,
                Left = leftOffset,
                Top = topOffset,
                Width = textBoxWidth,
                DropDownStyle = ComboBoxStyle.DropDownList // Do not allow user to type into combo box
            };
            tournamentSurfaceComboBox.Items.Add("");
            foreach (string[] surface in InputList.surfaces)
            {
                tournamentSurfaceComboBox.Items.Add(string.Join(' ', surface));
            }
            tournamentSurfaceComboBox.SelectedItem = "";

            errorProvider = new ErrorProvider();
        }

        public static void CreateControlPanelElements()
        {
            loadFromFileButton = new Button()
            {
                Text = "Načíst vstup ze souboru",
                Width = buttonWidth,
                Height = buttonHeight
            };

            loadRandomInputButton = new Button()
            {
                Text = "Načíst náhodný vstup",
                Width = buttonWidth,
                Height = buttonHeight
            };

            generateButton = new Button()
            {
                Text = "Generovat",
                Width = buttonWidth,
                Height = buttonHeight,
            };

            saveToFileButton = new Button()
            {
                Text = "Uložit do souboru",
                Width = buttonWidth,
                Height = buttonHeight
            };
        }

        public static void CreateSettingsPanelElements()
        {
            int settingsTopOffset = topOffset + topOffsetIncrement * 2;
            int height = 60;

            templatesVisibilityLabel = new Label()
            {
                Font = inputFont,
                Text = "Zobrazit šablony",
                Top = settingsTopOffset,
                Height = height
            };
            templatesVisibilityCheckBox = new CheckBox()
            {
                Checked = false,
                Font = inputFont,
                Left = leftOffset,
                Top = settingsTopOffset
            };

            settingsTopOffset += topOffsetIncrement + (topOffsetIncrement / 2);

            resetCategoriesLabel = new Label()
            {
                Font = inputFont,
                Text = "Resetovat kategorie",
                Top = settingsTopOffset,
                Height = height
            };
            resetCategoriesCheckBox = new CheckBox()
            {
                Checked = false,
                Font = inputFont,
                Left = leftOffset,
                Top = settingsTopOffset
            };
        }

        public static void CreateTextPanelElements()
        {
            titleLabel = new Label()
            {
                Text = "Titulek",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = inputFontBold,
                Width = articleLabelWidth,
                Height = articleLabelHeight,
                Top = articleTopOffset,
                
            };
 
            titleLabel.Left = ((clientSizeWidth / 2) - (titleLabel.Width / 2));

            titleText = new Label()
            {
                //Font = new Font("Arial", 14, FontStyle.Bold),
                Font = inputFont,
                TextAlign = ContentAlignment.TopCenter,
                Left = articleLeftOffset,
                Width = articleWidth,
                Height = articleHeight,
                Top = articleTopOffset + articleTextTopOffsetIncrement
            };

            horizontalLine = new Label()
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = horizontalLineHeight
            };

            articleTopOffset += articleTopOffsetIncrement;

            resultLabel = new Label()
            {
                Text = "Výsledek",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = inputFontBold,
                Width = articleLabelWidth,
                Height = articleLabelHeight,
                Top = articleTopOffset
            };
            resultLabel.Left = ((clientSizeWidth / 2) - (resultLabel.Width / 2));

            resultText = new Label()
            {
                //Font = new Font("Helvetica Neue", 14),
                Font = inputFont,
                TextAlign = ContentAlignment.TopCenter,
                Left = articleLeftOffset,
                Width = articleWidth,
                Height = articleHeight,
                Top = articleTopOffset + articleTextTopOffsetIncrement
            };

            horizontalLine2 = new Label()
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = horizontalLineHeight
            };

            articleTopOffset += articleTopOffsetIncrement;

            setLabel = new Label()
            {
                Text = "Průběh setů",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = inputFontBold,
                Width = articleLabelWidth,
                Height = articleLabelHeight,
                Top = articleTopOffset
            };
            setLabel.Left = ((clientSizeWidth / 2) - (setLabel.Width / 2));

            setText = new Label
            {
                //Font = new Font("Helvetica Neue", 14),
                Font = inputFont,
                TextAlign = ContentAlignment.TopCenter,
                Left = articleLeftOffset,
                Width = articleWidth,
                Height = articleHeight * 2,
                Top = articleTopOffset + articleTextTopOffsetIncrement
            };
        }

        public static void HideTextElements()
        {
            titleText.Hide();
            resultText.Hide();
            setText.Hide();
            horizontalLine.Hide();
            horizontalLine2.Hide();
        }

        public static void ShowTextElements()
        {
            titleText.Show();
            resultText.Show();
            setText.Show();
            horizontalLine.Show();
            horizontalLine2.Show();
        }

        public static MatchInput LoadInput()
        {
            MatchInput matchInput = new MatchInput();

            matchInput.nameWinner = winnerNameTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            matchInput.nameLoser = loserNameTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            matchInput.areMen = areMaleCheckBox.Checked ? "true" : "false";
            matchInput.tournamentName = tournamentNameTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            matchInput.round = roundComboBox.Text;
            matchInput.score = scoreTextBox.Text;
            matchInput.length = lengthTextBox.Text;

            matchInput.tournamentPlace = tournamentPlaceTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            matchInput.tournamentType = tournamentTypeTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries); ;
            matchInput.tournamentSurface = tournamentSurfaceComboBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return matchInput;
        }

        public static void SetInput()
        {
            MatchParameters matchParameters = InputManager.matchParameters;
            MatchInput matchInput = matchParameters.matchInput;

            winnerNameTextBox.Text = string.Join(' ', matchInput.nameWinner);
            loserNameTextBox.Text = string.Join(' ', matchInput.nameLoser);
            areMaleCheckBox.Checked = matchInput.areMen == "true";
            tournamentNameTextBox.Text = string.Join(' ', matchInput.tournamentName);
            roundComboBox.SelectedItem = matchInput.round;
            scoreTextBox.Text = matchInput.score;
            lengthTextBox.Text = matchInput.length;
        }

        public static void SetOptionalInput()
        {
            MatchParameters matchParameters = InputManager.matchParameters;
            MatchInput matchInput = matchParameters.matchInput;

            if (matchInput.tournamentPlace != null)
            {
                tournamentPlaceTextBox.Text = string.Join(' ', matchParameters.tournament.city);
            }
            else
            {
                tournamentPlaceTextBox.Text = "";
            }
            if (matchInput.tournamentType != null)
            {
                tournamentTypeTextBox.Text = string.Join(' ', matchParameters.tournament.category);
            }
            else
            {
                tournamentTypeTextBox.Text = "";
            }
            if (matchInput.tournamentSurface != null)
            {
                tournamentSurfaceComboBox.SelectedItem = string.Join(' ', matchParameters.tournament.surface);
            }
            else
            {
                tournamentSurfaceComboBox.SelectedItem = "";
            }
        }

        public static void SetTextElements(string titleText, string resultText, string matchText)
        {
            UIComponents.titleText.Text = titleText;
            UIComponents.resultText.Text = resultText;
            UIComponents.setText.Text = matchText;

            // Decreases the sets text size if it does not fit on the screen
            if (templatesVisibilityCheckBox.Checked && InputManager.matchParameters.setsAggregation.Count > 3)
            {
                setText.Font = lowerInputFont;
            }
            else
            {
                setText.Font = inputFont;
            }
        }

        public static void OnBoxValidated(object sender, EventArgs e)
        {
            isInputChanged = true;
        }

        public static Tournament LoadTournamentFromOptionalFields(string[] tournamentName)
        {
            Tournament tournament = new Tournament
            {
                name = tournamentName,
                city = tournamentPlaceTextBox.Text != "" ? tournamentPlaceTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries) : null,
                category = tournamentTypeTextBox.Text != "" ? tournamentTypeTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries) : null,
                surface = tournamentSurfaceComboBox.Text != "" ? tournamentSurfaceComboBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries) : null
            };
            return tournament;
        }

        public static void AppendToFile()
        {
            bool areMale = areMaleCheckBox.Checked;
            string male = areMale ? "M" : "F";

            string outputFileName = "out.txt";
            using StreamWriter writer = File.AppendText(outputFileName);

            if (NewsGenerator.isEvaluation)
            {
                writer.WriteLine($"{appendCount++} {male}");
                writer.WriteLine($"Winner: {winnerNameTextBox.Text}");
                writer.WriteLine($"Loser: {loserNameTextBox.Text}");
                writer.WriteLine($"Tournament: {tournamentNameTextBox.Text}");
                writer.WriteLine($"Round: {roundComboBox.Text}");
                writer.WriteLine($"Length: {lengthTextBox.Text}");
                writer.WriteLine($"Score: {scoreTextBox.Text}");
                writer.WriteLine();
            }

            writer.WriteLine(titleText.Text);
            writer.WriteLine();
            writer.WriteLine(resultText.Text);
            writer.WriteLine();
            writer.WriteLine(setText.Text);
            writer.WriteLine();
            writer.WriteLine("-------------------------------------------------------------------------------------------");
            writer.WriteLine();
        }
    }
}