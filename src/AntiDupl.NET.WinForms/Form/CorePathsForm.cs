/*
* AntiDupl.NET Program (http://ermig1979.github.io/AntiDupl).
*
* Copyright (c) 2002-2018 Yermalayeu Ihar, 2014 Borisov Dmitry.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections.Specialized;

using AntiDupl.NET.Core;
using AntiDupl.NET.Core.Original;

namespace AntiDupl.NET.WinForms
{
    public class CorePathsForm : Form
    {
        static public int FORM_WIDTH = 450;
        static public int FORM_HEIGHT = 350;

        private CoreLib m_core;
        private Options m_options;
        private CoreOptions m_oldCoreOptions; //опции до изменения
        private CoreOptions m_newCoreOptions; //опции после изменения

        private Button m_okButton;
        private Button m_cancelButton;
        private Button m_addFilesButton;
        private Button m_addFolderButton;
        private Button m_changeButton;
        private Button m_removeButton;

        private TabControl m_tabControl;
        private TabPage m_searchTabPage;
        private CheckedListBox m_searchCheckedList;
        private TabPage m_ignoreTabPage;
        private ListBox m_ignoreListBox;
        private Label m_ignoreFilterLabel;
        private TextBox m_ignoreFilterTextBox;
        private TabPage m_validTabPage;
        private ListBox m_validListBox;
        private TabPage m_deleteTabPage;
        private ListBox m_deleteListBox;
        private ToolTip m_toolTip;

        public CorePathsForm(CoreLib core, Options options, CoreOptions coreOptions)
        {
            m_core = core;
            m_options = options;
            m_oldCoreOptions = coreOptions;
            m_newCoreOptions = m_oldCoreOptions.Clone();
            InitializeComponent();
            ThemeColors.Apply(this);
            UpdateStrings();
            UpdatePath();
            m_ignoreFilterTextBox.Text = m_newCoreOptions.ignoreFilenameFilter ?? "";
            UpdateButtonEnabling();
        }

        void InitializeComponent()
        {
            ClientSize = new System.Drawing.Size(FORM_WIDTH, FORM_HEIGHT);
            FormBorderStyle = FormBorderStyle.Sizable;
            Icon = Resources.Icons.Get(Icon.Size);
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            MaximizeBox = false;
            MinimizeBox = false;
            KeyPreview = true;

            Resources.Help.Bind(this, Resources.Help.Paths);

            TableLayoutPanel mainTableLayoutPanel = InitFactory.Layout.Create(1, 2, 5);
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 85F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            Controls.Add(mainTableLayoutPanel);

            TableLayoutPanel pathTableLayoutPanel = InitFactory.Layout.Create(1, 2, 0);
            pathTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 85F));
            pathTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            pathTableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            mainTableLayoutPanel.Controls.Add(pathTableLayoutPanel, 0, 0);

            m_tabControl = new TabControl();
            m_tabControl.Dock = DockStyle.Fill;
            m_tabControl.Location = new System.Drawing.Point(0, 0);
            m_tabControl.Margin = new Padding(0);
            m_tabControl.Selected += new TabControlEventHandler(OnTabControlSelected);
            pathTableLayoutPanel.Controls.Add(m_tabControl, 0, 0);

            m_searchTabPage = new TabPage();
            m_searchTabPage.Tag = CoreDll.PathType.Search;
            m_tabControl.Controls.Add(m_searchTabPage);

            m_toolTip = new ToolTip();
            m_toolTip.ShowAlways = true;

            m_searchCheckedList = InitFactory.CheckedListBox.Create(OnSelectedIndexChanged, OnListBoxDoubleClick, OnItemCheck);
            m_searchCheckedList.HorizontalScrollbar = true;
            m_searchTabPage.Controls.Add(ThemeColors.Framed(m_searchCheckedList));

            m_ignoreTabPage = new TabPage();
            m_ignoreTabPage.Tag = CoreDll.PathType.Ignore;
            m_tabControl.Controls.Add(m_ignoreTabPage);

            TableLayoutPanel ignoreLayout = InitFactory.Layout.Create(1, 2, 5);
            ignoreLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            ignoreLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            m_ignoreTabPage.Controls.Add(ignoreLayout);

            m_ignoreListBox = InitFactory.ListBox.Create(OnSelectedIndexChanged, OnListBoxDoubleClick);
            m_ignoreListBox.Dock = DockStyle.Fill;
            m_ignoreListBox.HorizontalScrollbar = true;
            ignoreLayout.Controls.Add(ThemeColors.Framed(m_ignoreListBox), 0, 0);

            TableLayoutPanel filterLayout = InitFactory.Layout.Create(2, 1);
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            filterLayout.Dock = DockStyle.Fill;
            ignoreLayout.Controls.Add(filterLayout, 0, 1);

            m_ignoreFilterLabel = new Label();
            m_ignoreFilterLabel.AutoSize = true;
            m_ignoreFilterLabel.Anchor = AnchorStyles.Left;
            m_ignoreFilterLabel.Margin = new Padding(3);
            filterLayout.Controls.Add(m_ignoreFilterLabel, 0, 0);

            m_ignoreFilterTextBox = new TextBox();
            m_ignoreFilterTextBox.Dock = DockStyle.Fill;
            m_ignoreFilterTextBox.Margin = new Padding(3);
            m_ignoreFilterTextBox.TextChanged += new EventHandler(OnIgnoreFilterTextChanged);
            filterLayout.Controls.Add(ThemeColors.Framed(m_ignoreFilterTextBox), 1, 0);

            m_validTabPage = new TabPage();
            m_validTabPage.Tag = CoreDll.PathType.Valid;
            m_tabControl.Controls.Add(m_validTabPage);

            m_validListBox = InitFactory.ListBox.Create(OnSelectedIndexChanged, OnListBoxDoubleClick);
            m_validListBox.HorizontalScrollbar = true;
            m_validTabPage.Controls.Add(ThemeColors.Framed(m_validListBox));

            m_deleteTabPage = new TabPage();
            m_deleteTabPage.Tag = CoreDll.PathType.Delete;
            m_tabControl.Controls.Add(m_deleteTabPage);

            m_deleteListBox = InitFactory.ListBox.Create(OnSelectedIndexChanged, OnListBoxDoubleClick);
            m_deleteListBox.HorizontalScrollbar = true;
            m_deleteTabPage.Controls.Add(ThemeColors.Framed(m_deleteListBox));

            TableLayoutPanel pathButtonsTableLayoutPanel = InitFactory.Layout.Create(4, 1);
            pathButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            pathButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            pathButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            pathButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            pathTableLayoutPanel.Controls.Add(pathButtonsTableLayoutPanel, 0, 1);

            m_addFilesButton = new Button();
            m_addFilesButton.AutoSize = true;
            m_addFilesButton.Click += new EventHandler(OnAddFilesButtonClick);
            pathButtonsTableLayoutPanel.Controls.Add(m_addFilesButton, 0, 0);

            m_addFolderButton = new Button();
            m_addFolderButton.AutoSize = true;
            m_addFolderButton.Click += new EventHandler(OnAddFolderButtonClick);
            pathButtonsTableLayoutPanel.Controls.Add(m_addFolderButton, 1, 0);

            m_changeButton = new Button();
            m_changeButton.AutoSize = true;
            m_changeButton.Click += new EventHandler(OnChangeButtonClick);
            pathButtonsTableLayoutPanel.Controls.Add(m_changeButton, 2, 0);

            m_removeButton = new Button();
            m_removeButton.AutoSize = true;
            m_removeButton.Click += new EventHandler(OnRemoveButtonClick);
            pathButtonsTableLayoutPanel.Controls.Add(m_removeButton, 3, 0);

            TableLayoutPanel mainButtonsTableLayoutPanel = InitFactory.Layout.Create(3, 1);
            mainButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            mainButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            mainTableLayoutPanel.Controls.Add(mainButtonsTableLayoutPanel, 0, 1);

            m_okButton = new Button();
            m_okButton.Click += new EventHandler(OnOkButtonClick);
            mainButtonsTableLayoutPanel.Controls.Add(m_okButton, 1, 0);

            m_cancelButton = new Button();
            m_cancelButton.Click += new EventHandler(OnCancelButtonClick);
            mainButtonsTableLayoutPanel.Controls.Add(m_cancelButton, 2, 0);
            CancelButton = m_cancelButton;

            AllowDrop = true;
            DragDrop += new DragEventHandler(OnDragDrop);
            DragEnter += new DragEventHandler(OnDragEnter);
            KeyDown += new KeyEventHandler(OnKeyDown);
        }

        private void UpdateStrings()
        {
            Strings s = Resources.Strings.Current;

            Text = s.CorePathsForm_Text;

            m_toolTip.SetToolTip(m_searchCheckedList, s.CorePathsForm_SearchCheckedListBox_ToolTip_Text);

            m_searchTabPage.Text = s.CorePathsForm_SearchTabPage_Text;
            m_ignoreTabPage.Text = s.CorePathsForm_IgnoreTabPage_Text;
            m_validTabPage.Text = s.CorePathsForm_ValidTabPage_Text;
            m_deleteTabPage.Text = s.CorePathsForm_DeleteTabPage_Text;

            m_okButton.Text = s.OkButton_Text;
            m_cancelButton.Text = s.CancelButton_Text;
            m_addFilesButton.Text = s.CorePathsForm_AddFilesButton_Text;
            m_addFolderButton.Text = s.CorePathsForm_AddFolderButton_Text;
            m_changeButton.Text = s.CorePathsForm_ChangeButton_Text;
            m_removeButton.Text = s.CorePathsForm_RemoveButton_Text;
            m_ignoreFilterLabel.Text = s.CorePathsForm_IgnoreFilenameFilter_Text;
        }

        /// <summary>
        /// Set new CoreOptions to passed path.
        /// </summary>
        private CorePathWithSubFolder[] GetCurrentPath()
        {
            switch ((CoreDll.PathType)m_tabControl.SelectedTab.Tag)
            {
                case CoreDll.PathType.Search:
                    return m_newCoreOptions.searchPath;
                case CoreDll.PathType.Ignore:
                    return m_newCoreOptions.ignorePath;
                case CoreDll.PathType.Valid:
                    return m_newCoreOptions.validPath;
                case CoreDll.PathType.Delete:
                    return m_newCoreOptions.deletePath;
                default:
                    return null;
            }
        }

        private ListBox GetCurrentListBox()
        {
            switch ((CoreDll.PathType)m_tabControl.SelectedTab.Tag)
            {
                case CoreDll.PathType.Search:
                    return m_searchCheckedList;
                case CoreDll.PathType.Ignore:
                    return m_ignoreListBox;
                case CoreDll.PathType.Valid:
                    return m_validListBox;
                case CoreDll.PathType.Delete:
                    return m_deleteListBox;
                default:
                    return null;
            }
        }

        private CheckedListBox GetCurrentCheckedListBox()
        {
            switch ((CoreDll.PathType)m_tabControl.SelectedTab.Tag)
            {
                case CoreDll.PathType.Search:
                    return m_searchCheckedList;
                default:
                    return null;
            }
        }

        private void SetCurrentPath(CorePathWithSubFolder[] path)
        {
            switch ((CoreDll.PathType)m_tabControl.SelectedTab.Tag)
            {
                case CoreDll.PathType.Search:
                    m_newCoreOptions.searchPath = path;
                    break;
                case CoreDll.PathType.Ignore:
                    m_newCoreOptions.ignorePath = path;
                    break;
                case CoreDll.PathType.Valid:
                    m_newCoreOptions.validPath = path;
                    break;
                case CoreDll.PathType.Delete:
                    m_newCoreOptions.deletePath = path;
                    break;
            }
        }

        private static string GetInitialPath(CorePathWithSubFolder[] paths)
        {
            if (paths != null)
            {
                for (int i = paths.Length - 1; i >= 0; --i)
                {
                    if (paths[i] != null && Directory.Exists(paths[i].path))
                        return paths[i].path;
                }
            }

            return Application.StartupPath;
        }

        private string GetFilter()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Image files: ");
            if (m_newCoreOptions.searchOptions.JPEG)
                builder.Append("JPEG; ");
            if (m_newCoreOptions.searchOptions.GIF)
                builder.Append("GIF; ");
            if (m_newCoreOptions.searchOptions.PNG)
                builder.Append("PNG; ");
            if (m_newCoreOptions.searchOptions.BMP)
                builder.Append("BMP; ");
            if (m_newCoreOptions.searchOptions.TIFF)
                builder.Append("TIFF; ");
            if (m_newCoreOptions.searchOptions.EMF)
                builder.Append("EMF; ");
            if (m_newCoreOptions.searchOptions.WMF)
                builder.Append("WMF; ");
            if (m_newCoreOptions.searchOptions.EXIF)
                builder.Append("EXIF; ");
            if (m_newCoreOptions.searchOptions.ICON)
                builder.Append("ICON; ");
            if (m_newCoreOptions.searchOptions.JP2)
                builder.Append("JP2; ");
            if (m_newCoreOptions.searchOptions.PSD)
                builder.Append("PSD; ");
            if (m_newCoreOptions.searchOptions.JXL)
                builder.Append("JXL; ");
            if (m_newCoreOptions.searchOptions.HEIF)
                builder.Append("HEIF; ");
            if (m_newCoreOptions.searchOptions.DDS)
                builder.Append("DDS; ");
            if (m_newCoreOptions.searchOptions.AVIF)
                builder.Append("AVIF; ");
            builder.Append("|");
            if (m_newCoreOptions.searchOptions.JPEG)
                builder.Append("*.jpeg;*.jfif;*.jpg;*.jpe;*.jiff;*.jif;*.j;*.jng;*.jff;");
            if (m_newCoreOptions.searchOptions.GIF)
                builder.Append("*.gif;");
            if (m_newCoreOptions.searchOptions.PNG)
                builder.Append("*.png;");
            if (m_newCoreOptions.searchOptions.BMP)
                builder.Append("*.bmp;*.dib;*.rle;");
            if (m_newCoreOptions.searchOptions.TIFF)
                builder.Append("*.tif;*.tiff;");
            if (m_newCoreOptions.searchOptions.EMF)
                builder.Append("*.emf;*.emz;");
            if (m_newCoreOptions.searchOptions.WMF)
                builder.Append("*.wmf");
            if (m_newCoreOptions.searchOptions.EXIF)
                builder.Append("*.exif;");
            if (m_newCoreOptions.searchOptions.ICON)
                builder.Append("*.icon;*.ico;*.icn;");
            if (m_newCoreOptions.searchOptions.JP2)
                builder.Append("*.jp2;*.j2k;*.j2c;*.jpc;*.jpf;*.jpx;");
            if (m_newCoreOptions.searchOptions.PSD)
                builder.Append("*.psd;");
            if (m_newCoreOptions.searchOptions.WEBP)
                builder.Append("*.webp;");
            if (m_newCoreOptions.searchOptions.JXL)
                builder.Append("*.jxl;");
            if (m_newCoreOptions.searchOptions.HEIF)
                builder.Append("*.heif;*.heic;");
            if (m_newCoreOptions.searchOptions.TGA)
                builder.Append("*.tga;");
            if (m_newCoreOptions.searchOptions.DDS)
                builder.Append("*.dds;");
            if (m_newCoreOptions.searchOptions.AVIF)
                builder.Append("*.avif;");
            return builder.ToString();
        }

        /// <summary>
        /// Updates the contents of a ListBox.
        /// </summary>
        private static void UpdatePath(string[] path, ListBox box)
        {
            int selectedIndex = box.SelectedIndex;
            box.Items.Clear();
            for (int i = 0; i < path.Length; ++i)
                box.Items.Add(path[i]);
            if (selectedIndex >= 0 && selectedIndex < path.Length)
                box.SelectedIndex = selectedIndex;
        }

        private static void UpdatePath(CorePathWithSubFolder[] paths, CheckedListBox box)
        {
            int selectedIndex = box.SelectedIndex;
            box.Items.Clear();
            for (int i = 0; i < paths.Length; ++i)
                box.Items.Add(paths[i].path, paths[i].enableSubFolder);
            if (selectedIndex >= 0 && selectedIndex < paths.Length)
                box.SelectedIndex = selectedIndex;
        }

        private static void UpdatePath(CorePathWithSubFolder[] paths, ListBox box)
        {
            int selectedIndex = box.SelectedIndex;
            box.Items.Clear();
            for (int i = 0; i < paths.Length; ++i)
                box.Items.Add(paths[i].path);
            if (selectedIndex >= 0 && selectedIndex < paths.Length)
                box.SelectedIndex = selectedIndex;
        }

        private void UpdatePath()
        {
            UpdatePath(m_newCoreOptions.searchPath, m_searchCheckedList);
            UpdatePath(m_newCoreOptions.ignorePath, m_ignoreListBox);
            UpdatePath(m_newCoreOptions.validPath, m_validListBox);
            UpdatePath(m_newCoreOptions.deletePath, m_deleteListBox);
            AdjustWidthToPaths();
        }

        private void AdjustWidthToPaths()
        {
            int longestPathWidth = 0;
            CorePathWithSubFolder[][] pathGroups =
            {
                m_newCoreOptions.searchPath,
                m_newCoreOptions.ignorePath,
                m_newCoreOptions.validPath,
                m_newCoreOptions.deletePath
            };

            foreach (CorePathWithSubFolder[] paths in pathGroups)
            {
                if (paths == null)
                    continue;

                foreach (CorePathWithSubFolder item in paths)
                {
                    if (item == null || string.IsNullOrEmpty(item.path))
                        continue;

                    int width = TextRenderer.MeasureText(item.path, Font).Width;
                    longestPathWidth = Math.Max(longestPathWidth, width);
                }
            }

            var workingArea = Screen.FromControl(this).WorkingArea;
            int maxClientWidth = Math.Max(FORM_WIDTH, workingArea.Width - 80);
            int targetWidth = Math.Min(
                Math.Max(FORM_WIDTH, longestPathWidth + 100),
                maxClientWidth);

            if (ClientSize.Width == targetWidth)
                return;

            ClientSize = new System.Drawing.Size(targetWidth, ClientSize.Height);

            if (Visible)
            {
                int maxLeft = workingArea.Right - Width;
                if (Left > maxLeft)
                    Left = maxLeft;
                if (Left < workingArea.Left)
                    Left = workingArea.Left;
            }
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            m_newCoreOptions.CopyTo(ref m_oldCoreOptions);
            Close();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        private void OnAddFolderButtonClick(object sender, EventArgs e)
        {
            CorePathWithSubFolder[] path = GetCurrentPath();
            if (path == null)
                return;

            var dialog = new FolderPicker
            {
                Multiselect = true,
                InputPath = GetInitialPath(path)
            };

            if (dialog.ShowDialog(IntPtr.Zero) != true)
                return;

            string[] folders = dialog.ResultPaths;
            if (folders == null || folders.Length == 0)
                return;

            List<string> validFolders = new List<string>(folders.Length);
            foreach (string folder in folders)
            {
                if (!string.IsNullOrWhiteSpace(folder))
                    validFolders.Add(Path.TrimEndingDirectorySeparator(folder));
            }

            if (validFolders.Count == 0)
                return;

            int startIndex = path.Length;
            Array.Resize(ref path, startIndex + validFolders.Count);

            for (int i = 0; i < validFolders.Count; ++i)
            {
                path[startIndex + i] = new CorePathWithSubFolder
                {
                    path = validFolders[i],
                    enableSubFolder = true
                };
            }

            SetCurrentPath(path);
            m_newCoreOptions.Validate(m_core, m_options.onePath);
            UpdatePath();
            UpdateButtonEnabling();
        }

        private void OnAddFilesButtonClick(object sender, EventArgs e)
        {
            CorePathWithSubFolder[] path = GetCurrentPath();
            if (path == null)
                return;

            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = true,
                RestoreDirectory = true,
                InitialDirectory = GetInitialPath(path),
                Filter = GetFilter()
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            string[] files = dialog.FileNames;
            int startIndex = path.Length;
            Array.Resize(ref path, startIndex + files.Length);

            for (int i = 0; i < files.Length; ++i)
            {
                path[startIndex + i] = new CorePathWithSubFolder
                {
                    path = files[i],
                    enableSubFolder = false
                };
            }

            SetCurrentPath(path);
            m_newCoreOptions.Validate(m_core, m_options.onePath);
            UpdatePath();
            UpdateButtonEnabling();
        }

        private void OnRemoveButtonClick(object sender, EventArgs e)
        {
            RemovePath(false);
        }

        private void OnTabControlSelected(Object sender, TabControlEventArgs e)
        {
            UpdateButtonEnabling();
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonEnabling();
        }

        private void OnItemCheck(object sender, ItemCheckEventArgs e)
        {
            UpdateSubFolderOptions(e);
        }

        private void OnIgnoreFilterTextChanged(object sender, EventArgs e)
        {
            m_newCoreOptions.ignoreFilenameFilter = m_ignoreFilterTextBox.Text ?? "";
        }

        private void UpdateSubFolderOptions(ItemCheckEventArgs e)
        {
            CheckedListBox checkedListBox = GetCurrentCheckedListBox();
            CorePathWithSubFolder[] path = GetCurrentPath();

            if (checkedListBox == null || path == null || e.Index < 0 || e.Index >= path.Length)
                return;

            path[e.Index].enableSubFolder = e.NewValue != CheckState.Unchecked;
        }

        private void OnListBoxDoubleClick(object sender, EventArgs e)
        {
            ChangePath();
        }

        private void OnChangeButtonClick(object sender, EventArgs e)
        {
            ChangePath();
        }

        private void ChangePath()
        {
            ListBox listBox = GetCurrentListBox();
            CorePathWithSubFolder[] path = GetCurrentPath();
            if (listBox == null || path == null)
                return;
            if (listBox.SelectedIndex < 0 || listBox.SelectedIndex >= path.Length)
                return;

            int selectedIndex = listBox.SelectedIndex;
            if (Directory.Exists(path[selectedIndex].path))
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog
                {
                    ShowNewFolderButton = false,
                    SelectedPath = path[selectedIndex].path
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    path[selectedIndex].path = Path.TrimEndingDirectorySeparator(dialog.SelectedPath);
                    SetCurrentPath(path);
                    m_newCoreOptions.Validate(m_core, m_options.onePath);
                    UpdatePath();
                    UpdateButtonEnabling();
                }
            }
            else
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    RestoreDirectory = true,
                    FileName = path[selectedIndex].path,
                    Filter = GetFilter()
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    path[selectedIndex].path = dialog.FileName;
                    SetCurrentPath(path);
                    m_newCoreOptions.Validate(m_core, m_options.onePath);
                    UpdatePath();
                    UpdateButtonEnabling();
                }
            }
        }

        private void UpdateButtonEnabling()
        {
            if (m_tabControl.SelectedTab == null)
                m_tabControl.SelectedIndex = 0;

            ListBox listBox = GetCurrentListBox();
            CorePathWithSubFolder[] path = GetCurrentPath();

            if (listBox == null || path == null ||
                listBox.SelectedIndex < 0 || listBox.SelectedIndex >= path.Length)
            {
                m_changeButton.Enabled = false;
                m_removeButton.Enabled = false;
            }
            else
            {
                m_changeButton.Enabled = true;
                m_removeButton.Enabled = !(listBox == m_searchCheckedList && path.Length == 1);
            }

            m_okButton.Enabled = !m_oldCoreOptions.Equals(m_newCoreOptions);
        }

        private CorePathWithSubFolder[] GetActualPath(string[] paths)
        {
            List<CorePathWithSubFolder> actualPath = new List<CorePathWithSubFolder>();
            if (paths == null || paths.Length == 0)
                return actualPath.ToArray();

            string[] actualExtensions = m_oldCoreOptions.searchOptions.GetActualExtensions();
            foreach (string candidate in paths)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                if (Directory.Exists(candidate))
                {
                    actualPath.Add(new CorePathWithSubFolder
                    {
                        path = candidate,
                        enableSubFolder = true
                    });
                    continue;
                }

                FileInfo fileInfo = new FileInfo(candidate);
                string extension = fileInfo.Extension;
                if (extension.Length <= 1)
                    continue;

                extension = extension.Substring(1).ToUpperInvariant();
                for (int i = 0; i < actualExtensions.Length; ++i)
                {
                    if (extension != actualExtensions[i])
                        continue;

                    actualPath.Add(new CorePathWithSubFolder
                    {
                        path = candidate,
                        enableSubFolder = false
                    });
                    break;
                }
            }

            return actualPath.ToArray();
        }

        private void AddPath(string[] additional)
        {
            CorePathWithSubFolder[] current = GetCurrentPath();
            if (current == null || additional == null || additional.Length == 0)
                return;

            CorePathWithSubFolder[] actual = GetActualPath(additional);
            if (actual.Length == 0)
                return;

            List<CorePathWithSubFolder> updatedPath = new List<CorePathWithSubFolder>(
                current.Length + actual.Length);
            updatedPath.AddRange(current);
            updatedPath.AddRange(actual);

            SetCurrentPath(updatedPath.ToArray());
            m_newCoreOptions.Validate(m_core, m_options.onePath);
            UpdatePath();
            UpdateButtonEnabling();
        }

        private void RemovePath(bool copyToClipboard)
        {
            ListBox listBox = GetCurrentListBox();
            CorePathWithSubFolder[] path = GetCurrentPath();

            if (listBox == null || path == null)
                return;
            if (listBox.SelectedIndex < 0 || listBox.SelectedIndex >= path.Length)
                return;
            if (listBox == m_searchCheckedList && path.Length == 1)
                return;

            if (copyToClipboard)
                Clipboard.SetDataObject(path[listBox.SelectedIndex]);

            path[listBox.SelectedIndex].path = "";
            SetCurrentPath(path);
            m_newCoreOptions.Validate(m_core, m_options.onePath);
            UpdatePath();
            UpdateButtonEnabling();
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            string[] paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            AddPath(paths);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop, false))
                return;

            string[] paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (GetActualPath(paths).Length > 0)
                e.Effect = DragDropEffects.All;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.C | Keys.Control))
            {
                ListBox listBox = GetCurrentListBox();
                CorePathWithSubFolder[] path = GetCurrentPath();
                if (listBox != null && path != null &&
                    listBox.SelectedIndex >= 0 && listBox.SelectedIndex < path.Length)
                {
                    Clipboard.SetDataObject(path[listBox.SelectedIndex]);
                }
            }
            else if (e.KeyData == (Keys.V | Keys.Control))
            {
                if (GetCurrentListBox() == null)
                    return;

                if (Clipboard.ContainsFileDropList())
                {
                    StringCollection fileDropList = Clipboard.GetFileDropList();
                    string[] paths = new string[fileDropList.Count];
                    fileDropList.CopyTo(paths, 0);
                    AddPath(paths);
                }
                else if (Clipboard.ContainsText())
                {
                    AddPath(new[] { Clipboard.GetText() });
                }
            }
            else if (e.KeyData == (Keys.X | Keys.Control))
            {
                RemovePath(true);
            }
            else if (e.KeyData == Keys.Delete)
            {
                RemovePath(false);
            }
        }
    }
}
