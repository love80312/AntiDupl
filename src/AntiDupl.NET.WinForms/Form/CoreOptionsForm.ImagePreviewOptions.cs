using System;
using System.Windows.Forms;

namespace AntiDupl.NET.WinForms
{
    public partial class CoreOptionsForm
    {
        private TabPage m_imagePreviewSettingsTabPage;
        private CheckBox m_hoverZoomEnabledCheckBox;
        private Label m_hoverZoomPercentLabel;
        private ComboBox m_hoverZoomPercentComboBox;
        private Label m_hoverZoomWidthLabel;
        private NumericUpDown m_hoverZoomWidthNumericUpDown;
        private Label m_hoverZoomHeightLabel;
        private NumericUpDown m_hoverZoomHeightNumericUpDown;
        private Label m_hoverZoomDelayLabel;
        private ComboBox m_hoverZoomDelayComboBox;

        private static readonly int[] HOVER_ZOOM_PERCENT_VALUES = new int[] { 150, 200, 300, 400, 600 };
        private static readonly int[] HOVER_ZOOM_DELAY_VALUES = new int[] { 0, 100, 200, 350, 500 };

        private void InitilizeImagePreviewOptionsTabPage()
        {
            m_imagePreviewSettingsTabPage = new TabPage();
            m_mainTabControl.Controls.Add(m_imagePreviewSettingsTabPage);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.AutoScroll = true;
            layout.Padding = new Padding(12);
            layout.ColumnCount = 2;
            layout.RowCount = 5;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56F));
            for (int i = 0; i < 5; ++i)
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            m_imagePreviewSettingsTabPage.Controls.Add(layout);

            m_hoverZoomEnabledCheckBox = new CheckBox();
            m_hoverZoomEnabledCheckBox.AutoSize = true;
            m_hoverZoomEnabledCheckBox.Checked = m_options.resultsOptions.HoverZoomEnabled;
            m_hoverZoomEnabledCheckBox.CheckedChanged += OnImagePreviewOptionChanged;
            layout.Controls.Add(m_hoverZoomEnabledCheckBox, 0, 0);
            layout.SetColumnSpan(m_hoverZoomEnabledCheckBox, 2);

            m_hoverZoomPercentLabel = CreateImagePreviewOptionLabel();
            layout.Controls.Add(m_hoverZoomPercentLabel, 0, 1);
            m_hoverZoomPercentComboBox = new ComboBox();
            m_hoverZoomPercentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            m_hoverZoomPercentComboBox.Width = 150;
            foreach (int value in HOVER_ZOOM_PERCENT_VALUES)
                m_hoverZoomPercentComboBox.Items.Add(value.ToString() + "%");
            m_hoverZoomPercentComboBox.SelectedIndex = FindImagePreviewOptionIndex(
                HOVER_ZOOM_PERCENT_VALUES, m_options.resultsOptions.HoverZoomPercent, 1);
            m_hoverZoomPercentComboBox.SelectedIndexChanged += OnImagePreviewOptionChanged;
            layout.Controls.Add(m_hoverZoomPercentComboBox, 1, 1);

            m_hoverZoomWidthLabel = CreateImagePreviewOptionLabel();
            layout.Controls.Add(m_hoverZoomWidthLabel, 0, 2);
            m_hoverZoomWidthNumericUpDown = new NumericUpDown();
            m_hoverZoomWidthNumericUpDown.Minimum = 240;
            m_hoverZoomWidthNumericUpDown.Maximum = 1200;
            m_hoverZoomWidthNumericUpDown.Increment = 40;
            m_hoverZoomWidthNumericUpDown.Width = 150;
            m_hoverZoomWidthNumericUpDown.Value = m_options.resultsOptions.HoverZoomWidth;
            m_hoverZoomWidthNumericUpDown.ValueChanged += OnImagePreviewOptionChanged;
            layout.Controls.Add(m_hoverZoomWidthNumericUpDown, 1, 2);

            m_hoverZoomHeightLabel = CreateImagePreviewOptionLabel();
            layout.Controls.Add(m_hoverZoomHeightLabel, 0, 3);
            m_hoverZoomHeightNumericUpDown = new NumericUpDown();
            m_hoverZoomHeightNumericUpDown.Minimum = 180;
            m_hoverZoomHeightNumericUpDown.Maximum = 900;
            m_hoverZoomHeightNumericUpDown.Increment = 30;
            m_hoverZoomHeightNumericUpDown.Width = 150;
            m_hoverZoomHeightNumericUpDown.Value = m_options.resultsOptions.HoverZoomHeight;
            m_hoverZoomHeightNumericUpDown.ValueChanged += OnImagePreviewOptionChanged;
            layout.Controls.Add(m_hoverZoomHeightNumericUpDown, 1, 3);

            m_hoverZoomDelayLabel = CreateImagePreviewOptionLabel();
            layout.Controls.Add(m_hoverZoomDelayLabel, 0, 4);
            m_hoverZoomDelayComboBox = new ComboBox();
            m_hoverZoomDelayComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            m_hoverZoomDelayComboBox.Width = 150;
            string[] delayLabels = new string[] { "0 ms", "100 ms", "200 ms", "350 ms", "500 ms" };
            foreach (string label in delayLabels)
                m_hoverZoomDelayComboBox.Items.Add(label);
            m_hoverZoomDelayComboBox.SelectedIndex = FindImagePreviewOptionIndex(
                HOVER_ZOOM_DELAY_VALUES, m_options.resultsOptions.HoverZoomDelay, 3);
            m_hoverZoomDelayComboBox.SelectedIndexChanged += OnImagePreviewOptionChanged;
            layout.Controls.Add(m_hoverZoomDelayComboBox, 1, 4);

            UpdateHoverZoomSettingsEnabled();
        }

        private static Label CreateImagePreviewOptionLabel()
        {
            Label label = new Label();
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            return label;
        }

        private static int FindImagePreviewOptionIndex(int[] values, int value, int fallbackIndex)
        {
            int index = Array.IndexOf(values, value);
            return index >= 0 ? index : fallbackIndex;
        }

        private void UpdateImagePreviewOptionStrings(Strings strings)
        {
            m_imagePreviewSettingsTabPage.Text = strings.CoreOptionsForm_ImagePreviewTabPage_Text;
            m_hoverZoomEnabledCheckBox.Text = strings.CoreOptionsForm_HoverZoomEnabledCheckBox_Text;
            m_hoverZoomPercentLabel.Text = strings.CoreOptionsForm_HoverZoomPercent_Text;
            m_hoverZoomWidthLabel.Text = strings.CoreOptionsForm_HoverZoomWidth_Text;
            m_hoverZoomHeightLabel.Text = strings.CoreOptionsForm_HoverZoomHeight_Text;
            m_hoverZoomDelayLabel.Text = strings.CoreOptionsForm_HoverZoomDelay_Text;
        }

        private void OnImagePreviewOptionChanged(object sender, EventArgs e)
        {
            if (sender == m_hoverZoomEnabledCheckBox)
                UpdateHoverZoomSettingsEnabled();

            if (m_inited && m_okButton != null)
                m_okButton.Enabled = true;

            if (m_setDefaultButton != null)
                m_setDefaultButton.Enabled = !m_defaultCoreOptions.Equals(m_newCoreOptions) ||
                    !AreImagePreviewOptionsDefault();
        }

        private void UpdateHoverZoomSettingsEnabled()
        {
            bool enabled = m_hoverZoomEnabledCheckBox.Checked;
            m_hoverZoomPercentLabel.Enabled = enabled;
            m_hoverZoomPercentComboBox.Enabled = enabled;
            m_hoverZoomWidthLabel.Enabled = enabled;
            m_hoverZoomWidthNumericUpDown.Enabled = enabled;
            m_hoverZoomHeightLabel.Enabled = enabled;
            m_hoverZoomHeightNumericUpDown.Enabled = enabled;
            m_hoverZoomDelayLabel.Enabled = enabled;
            m_hoverZoomDelayComboBox.Enabled = enabled;
        }

        private bool AreImagePreviewOptionsDefault()
        {
            return m_hoverZoomEnabledCheckBox.Checked &&
                m_hoverZoomPercentComboBox.SelectedIndex == 1 &&
                m_hoverZoomWidthNumericUpDown.Value == 480 &&
                m_hoverZoomHeightNumericUpDown.Value == 360 &&
                m_hoverZoomDelayComboBox.SelectedIndex == 3;
        }

        private void ApplyImagePreviewOptions()
        {
            ResultsOptions options = m_options.resultsOptions;
            options.HoverZoomEnabled = m_hoverZoomEnabledCheckBox.Checked;

            int zoomIndex = m_hoverZoomPercentComboBox.SelectedIndex;
            if (zoomIndex < 0 || zoomIndex >= HOVER_ZOOM_PERCENT_VALUES.Length)
                zoomIndex = 1;
            options.HoverZoomPercent = HOVER_ZOOM_PERCENT_VALUES[zoomIndex];

            options.HoverZoomWidth = Decimal.ToInt32(m_hoverZoomWidthNumericUpDown.Value);
            options.HoverZoomHeight = Decimal.ToInt32(m_hoverZoomHeightNumericUpDown.Value);

            int delayIndex = m_hoverZoomDelayComboBox.SelectedIndex;
            if (delayIndex < 0 || delayIndex >= HOVER_ZOOM_DELAY_VALUES.Length)
                delayIndex = 3;
            options.HoverZoomDelay = HOVER_ZOOM_DELAY_VALUES[delayIndex];
        }

        private void ResetImagePreviewOptions()
        {
            m_hoverZoomEnabledCheckBox.Checked = true;
            m_hoverZoomPercentComboBox.SelectedIndex = 1;
            m_hoverZoomWidthNumericUpDown.Value = 480;
            m_hoverZoomHeightNumericUpDown.Value = 360;
            m_hoverZoomDelayComboBox.SelectedIndex = 3;
            UpdateHoverZoomSettingsEnabled();
        }
    }
}
