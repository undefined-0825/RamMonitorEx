using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    /// <summary>
    /// パネル名入力ダイアログ
    /// </summary>
    public class PaneNameDialog : Form
    {
        private TextBox nameTextBox;
        private Button okButton;
        private Button cancelButton;
        private Label promptLabel;
        private Label validationLabel;

        public string PaneName { get; private set; } = string.Empty;

        public PaneNameDialog(string defaultName, string prompt = "パネル名を入力してください:")
        {
            InitializeComponents(defaultName, prompt);
        }

        private void InitializeComponents(string defaultName, string prompt)
        {
            this.Text = "パネル名の入力";
            this.Size = new System.Drawing.Size(400, 180);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // プロンプトラベル
            promptLabel = new Label
            {
                Text = prompt,
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(350, 20)
            };

            // テキストボックス
            nameTextBox = new TextBox
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(350, 25),
                Text = defaultName
            };
            nameTextBox.TextChanged += NameTextBox_TextChanged;
            nameTextBox.SelectAll();

            // 検証ラベル
            validationLabel = new Label
            {
                Location = new System.Drawing.Point(20, 80),
                Size = new System.Drawing.Size(350, 20),
                ForeColor = System.Drawing.Color.Red,
                Text = string.Empty
            };

            // OKボタン
            okButton = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(210, 110),
                Size = new System.Drawing.Size(75, 25),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            // キャンセルボタン
            cancelButton = new Button
            {
                Text = "キャンセル",
                Location = new System.Drawing.Point(295, 110),
                Size = new System.Drawing.Size(75, 25),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(promptLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(validationLabel);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            ValidateName();
        }

        private void NameTextBox_TextChanged(object? sender, EventArgs e)
        {
            ValidateName();
        }

        private void ValidateName()
        {
            string name = nameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                validationLabel.Text = "パネル名を空にすることはできません。";
                okButton.Enabled = false;
                return;
            }

            if (PaneNameManager.Instance.IsNameRegistered(name))
            {
                validationLabel.Text = "このパネル名は既に使用されています。";
                okButton.Enabled = false;
                return;
            }

            validationLabel.Text = string.Empty;
            okButton.Enabled = true;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            PaneName = nameTextBox.Text.Trim();
        }
    }
}
