using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AntiDupl.NET.WPF.Helper;

namespace AntiDupl.NET.WPF.View
{
    public partial class ConfigurationWindow
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
            Loaded += ConfigurationWindow_Loaded;

            // TabControl only creates the selected tab's visual tree. Reapply when the
            // Deep tab is selected so the CPU-only controls are handled when created.
            if (Content is TabControl tabs)
                tabs.SelectionChanged += (_, __) => Dispatcher.BeginInvoke(new Action(ApplyCpuOnlySettings));
        }

        private void ConfigurationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyCpuOnlySettings();
        }

        private void ApplyCpuOnlySettings()
        {
            // CUDA/cuDNN controls are retained in XAML for source compatibility, but this
            // build uses CPU-only ONNX Runtime and must not present GPU setup as an option.
            string cudaGroupHeader = LocalizationHelper.S("configurationWindow_deepGroupCuda");
            foreach (GroupBox group in FindVisualChildren<GroupBox>(this))
            {
                if (Equals(group.Header, cudaGroupHeader))
                {
                    group.Visibility = Visibility.Collapsed;
                    group.IsEnabled = false;
                    break;
                }
            }

            // These values are fixed by the CPU profile (WGC=1, prefetch=0).
            // Keep them visible for transparency, but prevent edits that cannot take effect.
            foreach (TextBox textBox in FindVisualChildren<TextBox>(this))
            {
                string path = textBox.GetBindingExpression(TextBox.TextProperty)?.ParentBinding?.Path?.Path;
                if (path == "DeepWgcParallelismText" || path == "DeepPrefetchCountText")
                    textBox.IsReadOnly = true;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
            where T : DependencyObject
        {
            if (parent == null)
                yield break;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match)
                    yield return match;

                foreach (T descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }
    }
}
