using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AntiDupl.NET.Core.Enums;
using AntiDupl.NET.WPF.Helper;
using AntiDupl.NET.WPF.ObjectModel;
using AntiDupl.NET.WPF.Resources;
using AntiDupl.NET.WPF.ViewModel;

namespace AntiDupl.NET.WPF.Model
{
    public class ConfigurationModel : PropertyChangedBase, IConfigurationModel
    {
        [NonSerialized]
        private const string _fileName = @"configuration.xml";

        public ConfigurationModel()
        {
            ThumbnailWidth = 150;
            UndoRedoStackSize = 10;
            MaxCachedThumbnail = 400;
            GoodColor = Colors.LightGreen;
            BadColor = Colors.LightCoral;
            AlternateColor = Colors.WhiteSmoke;
            FolderAreDiffrentColor = Colors.AliceBlue;
            Language = new SerializableCultureInfo("en-US");
        }

        int _thumbnailWidth;
        public int ThumbnailWidth
        {
            get { return _thumbnailWidth; }
            set { _thumbnailWidth = value; RaisePropertyChangedEvent(); }
        }

        ObservableCollection<Criterion> _criterions;
        public ObservableCollection<Criterion> Criterions
        {
            get { return _criterions; }
            set
            {
                _criterions = value;
                _criterions.CollectionChanged += criterions_CollectionChanged;
                RaisePropertyChangedEvent();
            }
        }

        void criterions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= criterion_PropertyChanged;
            if (e.NewItems != null)
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += criterion_PropertyChanged;
        }

        bool calculateInProgress = false;
        void criterion_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsUse")
            {
                Criterion cr = sender as Criterion;
                if (cr.Type == CriterionType.LargerFileSize)
                {
                    var founded = _criterions.First(c => c.Type == CriterionType.SmallerFileSize);
                    founded.PropertyChanged -= criterion_PropertyChanged;
                    founded.IsUse = false;
                    founded.PropertyChanged += criterion_PropertyChanged;
                }
                else if (cr.Type == CriterionType.SmallerFileSize)
                {
                    var founded = _criterions.First(c => c.Type == CriterionType.LargerFileSize);
                    founded.PropertyChanged -= criterion_PropertyChanged;
                    founded.IsUse = false;
                    founded.PropertyChanged += criterion_PropertyChanged;
                }
            }
            else if (e.PropertyName == "Weight" && !calculateInProgress)
            {
                Criterion cr = sender as Criterion;
                if (cr.Weight > 1)
                    cr.Weight = 0.99M;
                calculateInProgress = true;
                SaatiHelper.RecalculateIndex(_criterions, cr);
                calculateInProgress = false;
            }
        }

        public SerializableCultureInfo Language { get; set; }

        // ==== Deep (DINOv2) search configuration ====
        bool _verifyByLightGlue = true;
        public bool VerifyByLightGlue
        {
            get { return _verifyByLightGlue; }
            set { _verifyByLightGlue = value; RaisePropertyChangedEvent(); }
        }

        string _lightGlueModelPath = "";
        public string LightGlueModelPath
        {
            get { return _lightGlueModelPath; }
            set { _lightGlueModelPath = value; RaisePropertyChangedEvent(); }
        }

        string _dinov2ModelPath = "";
        public string Dinov2ModelPath
        {
            get { return _dinov2ModelPath; }
            set { _dinov2ModelPath = value; RaisePropertyChangedEvent(); }
        }

        string _dinov2ModelUrl = "";
        public string Dinov2ModelUrl
        {
            get { return _dinov2ModelUrl; }
            set { _dinov2ModelUrl = value; RaisePropertyChangedEvent(); }
        }

        // 舊版 GPU 設定保留供 XmlSerializer 讀取既有 configuration.xml；CPU-only 建置不使用。
        string _cudaPath = "";
        public string CudaPath
        {
            get { return _cudaPath; }
            set { _cudaPath = value; RaisePropertyChangedEvent(); }
        }

        string _cudnnPath = "";
        public string CudnnPath
        {
            get { return _cudnnPath; }
            set { _cudnnPath = value; RaisePropertyChangedEvent(); }
        }

        double _deepMinSimilarityForPair = 0.5;
        public double DeepMinSimilarityForPair
        {
            get { return _deepMinSimilarityForPair; }
            set { _deepMinSimilarityForPair = value; RaisePropertyChangedEvent(); }
        }

        int _deepMaxClusterSize = 50;
        public int DeepMaxClusterSize
        {
            get { return _deepMaxClusterSize; }
            set { _deepMaxClusterSize = value; RaisePropertyChangedEvent(); }
        }

        double _deepTransitivityRatio = 0.7;
        public double DeepTransitivityRatio
        {
            get { return _deepTransitivityRatio; }
            set { _deepTransitivityRatio = value; RaisePropertyChangedEvent(); }
        }

        // 雙核心 CPU：LightGlue 工作階段一次只執行一組影像。
        public int DeepWgcParallelism
        {
            get => 1;
            set { _ = value; }
        }

        double _deepMinMatchScore = 0.6;
        public double DeepMinMatchScore
        {
            get { return _deepMinMatchScore; }
            set { _deepMinMatchScore = value; RaisePropertyChangedEvent(); }
        }

        // BatchSize 最多為 2；舊設定檔中的 32 等高值會在反序列化時自動限幅。
        int _deepBatchSize = 2;
        public int DeepBatchSize
        {
            get { return _deepBatchSize; }
            set { _deepBatchSize = Math.Clamp(value, 1, 2); RaisePropertyChangedEvent(); }
        }

        // CPU-only 低負載設定關閉預載佇列，並兼容舊設定檔中的欄位。
        public int DeepPrefetchCount
        {
            get => 0;
            set { _ = value; }
        }

        public string LastResultFile { get; set; }
        public string IgnoreFilenameFilter { get; set; }
        public ViewMode ResultsViewMode { get; set; } = ViewMode.Pair;
        public HotKeyOptions HotKeys { get; set; }
        public ResultsColumnsOptions ResultsColumns { get; set; }
        public AlgorithmComparing? CompareAlgorithm { get; set; }
        public uint UndoRedoStackSize { get; set; }
        public uint MaxCachedThumbnail { get; set; }
        public bool CheckResultsAtLoading { get; set; } = true;
        public bool CheckMistakesAtLoading { get; set; } = true;
        public string ImageDiffExecutablePath { get; set; }
        public string ImageDiffExecutableArguments { get; set; }
        public bool UseSinglePath { get; set; }

        Color _goodColor;
        public Color GoodColor
        {
            get { return _goodColor; }
            set { _goodColor = value; RaisePropertyChangedEvent(); }
        }

        Color _badColor;
        public Color BadColor
        {
            get { return _badColor; }
            set { _badColor = value; RaisePropertyChangedEvent(); }
        }

        Color _alternateColor;
        public Color AlternateColor
        {
            get { return _alternateColor; }
            set
            {
                _alternateColor = value;
                RaisePropertyChangedEvent("BadColor");
            }
        }

        Color _folderAreDiffrentColor;
        public Color FolderAreDiffrentColor
        {
            get { return _folderAreDiffrentColor; }
            set { _folderAreDiffrentColor = value; RaisePropertyChangedEvent(); }
        }

        bool _loadResultOnOpen = true;
        public bool LoadResultOnOpen
        {
            get { return _loadResultOnOpen; }
            set { _loadResultOnOpen = value; RaisePropertyChangedEvent(); }
        }

        bool _saveResultOnClose = true;
        public bool SaveResultOnClose
        {
            get { return _saveResultOnClose; }
            set { _saveResultOnClose = value; RaisePropertyChangedEvent(); }
        }

        public Brush AlternateBrush => new SolidColorBrush(_alternateColor);

        public static ConfigurationModel Load()
        {
            ConfigurationModel model = SerializeHelper<ConfigurationModel>.Load(GetFilePath());
            if (model == null)
                model = new ConfigurationModel();

            if (model.Criterions == null || model.Criterions.Count != 5)
                model.Criterions = model.GetDefaultCriterions();
            if (model.HotKeys == null)
                model.HotKeys = new HotKeyOptions();

            // 保險處理舊版 configuration.xml，確保實際使用值與介面顯示值一致。
            model.DeepBatchSize = model.DeepBatchSize;
            model.DeepPrefetchCount = 0;
            model.DeepWgcParallelism = 1;
            return model;
        }

        public ObservableCollection<Criterion> GetDefaultCriterions()
        {
            var criterions = new ObservableCollection<Criterion>();
            criterions.Add(new Criterion() { Type = CriterionType.Resolution, Weight = 0.4M, IsUse = true });
            criterions.Add(new Criterion() { Type = CriterionType.SmallerFileSize, Weight = 0.00001M, IsUse = true });
            criterions.Add(new Criterion() { Type = CriterionType.LargerFileSize, IsUse = false });
            criterions.Add(new Criterion() { Type = CriterionType.Blockiness, Weight = 0.3001M, IsUse = true });
            criterions.Add(new Criterion() { Type = CriterionType.Bluring, Weight = 0.29989M, IsUse = true });
            return criterions;
        }

        public void Save() => SerializeHelper<ConfigurationModel>.Save(this, GetFilePath());

        static string GetFilePath() => Path.Combine(AntiDupl.NET.WPF.Resources.Resources.UserPath, _fileName);
    }
}
