using LDRCNetClassification;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Win32;
using Pentacube.CAx.DFx.LDRC.Views;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace LDRCNetClassificationUI
{
    public class MainWindowViewModel : BindableBase
    {
        private string _regexFilePath;
        private string _netFilePath;

        private NetNameMapping NetNameMappingProcess { get; set; }

        private ObservableCollection<GridRowData> _gridItems;
        public ObservableCollection<GridRowData> GridItems    // DataGrid binding Collection
        {
            get => _gridItems;
            set => SetProperty(ref _gridItems, value);
        }

        private string _statics;
        public string Statics
        {
            get => _statics;
            set => SetProperty(ref _statics, value);
        }

        public Dictionary<string, int> CountDic = new Dictionary<string, int>();

        /// <summary>
        /// Regex file name Control Text
        /// </summary>
        private string _regexFileNameText = " Regex file : none";
        public string RegexFileNameText
        {
            get => _regexFileNameText;
            set => SetProperty(ref _regexFileNameText, value);
        }

        /// <summary>
        /// Net file name Control Text
        /// </summary>
        private string _netFileNameText = " Net name file : none";
        public string NetFileNameText
        {
            get => _netFileNameText;
            set => SetProperty(ref _netFileNameText, value);
        }

        #region Commands
        public DelegateCommand RegexButtonCommand { get; }
        public DelegateCommand NetNameButtonCommand { get; }
        public DelegateCommand ExecuteButtonCommand { get; }
        public DelegateCommand SaveButtonCommand { get; }
        public DelegateCommand ClearButtonCommand { get; }
        #endregion Commands

        public MainWindowViewModel()
        {
            GridItems = new ObservableCollection<GridRowData>();

            RegexButtonCommand = new DelegateCommand(RegexButton);
            NetNameButtonCommand = new DelegateCommand(NetNameButton);
            ExecuteButtonCommand = new DelegateCommand(ExecuteButton);
            SaveButtonCommand = new DelegateCommand(SaveButton);
            ClearButtonCommand = new DelegateCommand(ClearButton);
        }

        private void RegexButton()
        {
            if(SetFileNameFromDialog("csv, json, xml Files | *.csv;*.json;*.xml", out var rPath))
            {
                _regexFilePath = rPath;
                RegexFileNameText = " Regex file : " + Path.GetFileName(_regexFilePath); // UI에 갱신 값 적용
            }
        }

        private void NetNameButton()
        {
            if(SetFileNameFromDialog("edf Files | *.edf;", out var nPath))  // 도면파일 경로갱신
            {
                _netFilePath = nPath;
                NetFileNameText = " Net name file : " + Path.GetFileName(_netFilePath);
            }
        }

        private bool SetFileNameFromDialog(string fileType, out string path)
        {
            var ofdlg = new OpenFileDialog { Filter = fileType };

            if (ofdlg.ShowDialog() != true)
            {
                path = null;
                return false;
            }

            path = ofdlg.FileName;
            MessageBox.Show("Success");

            return true;
        }

        private void ExecuteButton()
        {
            Reset();

            if (RegexFileNameText.Contains("none") || NetFileNameText.Contains("none"))
            {
                MessageBox.Show("Please Register File First", "Error");
                return;
            }

            ExecuteClassfication();
        }

        private void Reset()
        {
            CountDic.Clear();
            GridItems.Clear();

            NetNameMappingProcess = new NetNameMapping();

            typeof(DRCConfig).GetProperties()
                .Select(p => p.Name.Substring(0, p.Name.Length - 1))
                .ForEach(n => CountDic[n] = 0);
        }

        /// <summary>
        /// LDRCNetClassification 프로젝트로 부터 mapping결과를 받아오는 함수
        /// </summary>
        private void ExecuteClassfication()
        {
            // 1. load the regex file
            if (!NetNameMappingProcess.TryLoadRegexConfig(_regexFilePath))
                throw new FileNotFoundException();

            // 2. match net name to net classification
            if (!NetNameMappingProcess.AddToNetGroups(_netFilePath))
                throw new FileNotFoundException();

            // 3. fill the DataGrid's rows
            NetNameMappingProcess.NetConfigInfoList
                .Select((config, idx) => new GridRowData(idx + 1, config.Net.Name, config.SymbolName))
                .ForEach(GridItems.Add);

            // 4. show statistics
            SumCount();
            SetStatics();
        }

        private void SetStatics()
        {
            Statics = string.Join(
                "\n\n",
                CountDic
                .OrderBy(kv => kv.Key)
                .Select(cd => Format(cd.Key, cd.Value, Calculate(cd.Value))));

            double Calculate(int cnt) => (double)cnt / GridItems.Count * 100D;
            string Format(string prefix, int cnt, double per) => $"{prefix} : {cnt} ({per:0.##}%)";
        }

        private void SumCount()
        {
            // 분류 당 개수 추가
            NetNameMappingProcess.NetConfigInfoList
                .Select(mp => mp.GetSymbolName())
                .ForEach(key => ++CountDic[key]);
        }

        private void SaveButton()
        {
            // 실행이 안되었거나 결과가 존재하지 않는 경우
            if (GridItems.Count == 0)
            {
                var result = MessageBox.Show(
                    "결과가 없거나 실행되지 않았습니다", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            var saveAsfile = new SaveFileDialog
            {
                Title = "다른 이름으로 저장",     //saveFileDialog 창 이름 설정
                Filter = "텍스트 파일(*.txt)|*.txt",
            };

            if (saveAsfile.ShowDialog() != true) // Nullable<bool> : true, false, null
                return;

            // 확인 후 저장 진행
            var printItems = NetNameMappingProcess.NetConfigInfoList
                .Select(config => $"{config.Net.Name},{config.SymbolName}");
            var printContent = string.Join("\n", printItems);

            try
            {
                // path가 존재하지 않을때에는 file생성후 write
                File.WriteAllText(saveAsfile.FileName, printContent);
            }
            catch
            {
                // ignore
            }

            var isOk = MessageBox.Show(
                "저장한 파일 열기", "", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (isOk.HasFlag(MessageBoxResult.Yes))
                Process.Start(saveAsfile.FileName);
        }

        private void ClearButton()
        {
            if (GridItems.Count > 0)
                GridItems.Clear();

            _regexFilePath = string.Empty;
            _netFilePath = string.Empty;

            RegexFileNameText = " Regex file : none";
            NetFileNameText = " Net name file : none";

            //Statics = "None :\n\nPowerNetGroup :\n\nCANGroup :\n\nLINGroup :\n\nSideOutGroup :\n\n" +
            //    "ActiveInputGroup :\n\nIllGroup :\n\nAnalogNetGroup :";
        }
    }
}
