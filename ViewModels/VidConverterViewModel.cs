using AbFfmpeg.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;

namespace AbFfmpeg
{
    [Serializable]
    public enum VideoFormat : int
    {
        AVI,
        MPEG,
        MP3
    }

    public class VidConverterViewModel : INotifyPropertyChanged
    {
        private VidConverter _vidConverter;
        private string _currVidPath;
        private string _currOutputDir;
        private VideoFormat _currVidFormat;

        #region Properties
        public VidConverter VidConverter
        {
            get { return _vidConverter; }
            set { _vidConverter = value; }
        }

        public string CurrVidPath
        {
            get { return _currVidPath; }
            set 
            { 
                _currVidPath = value;
                OnPropertyChanged("CurrVidPath");
            }
        }

        public string CurrOutputDir
        {
            get { return _currOutputDir; }
            set
            {
                _currOutputDir = value;
                OnPropertyChanged("CurrOutputDir");
            }
        }
        
        public int Progress
        {
            get { return _vidConverter.Progress; }
            set { }
        }

        public VideoFormat CurrVidFormat
        {
            get { return _currVidFormat; }
            set
            {
                _currVidFormat = value;
                OnPropertyChanged("CurrVidFormat");
            }
        }

        public IEnumerable<VideoFormat> VideoFormatValues
        {
            get
            {
                return Enum.GetValues(typeof(VideoFormat))
                    .Cast<VideoFormat>();
            }
        }
        #endregion

        #region Commands
        public ConvertCommand ConvertCommand
        {
            get;
            set;
        }

        public BrowseFilesCommand BrowseFilesCommand
        {
            get;
            set;
        }

        public BrowseDirectoriesCommand BrowseDirectoriesCommand
        {
            get;
            set;
        }
        #endregion

        public VidConverterViewModel()
        {
            ConvertCommand = new ConvertCommand(this);
            BrowseFilesCommand = new BrowseFilesCommand(this);
            BrowseDirectoriesCommand = new BrowseDirectoriesCommand(this);
            CurrVidPath = "Select a video file you'd like to convert...";
            CurrOutputDir = "Select output folder (will be same as video file's if unselected)";
            VidConverter = new VidConverter();
            VidConverter.PropertyChanged += VidConverter_PropertyChanged;
        }

        public void ConvertVideo()
        {
            if (!String.Equals(CurrOutputDir, "Select output folder (will be same as video file's if unselected)"))
                VidConverter.BeginConversion(CurrVidPath, CurrOutputDir, CurrVidFormat);
            else
                VidConverter.BeginConversion(CurrVidPath, CurrVidFormat);
        }

        public void OpenFileBrowser()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "AVI Files (*.avi)|*.avi|FLV Files (*.flv)|*.flv|WAV Files (*.wav)|*.wav|MP4 Files (*.mp4)|*.mp4";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string filename = dlg.FileName;
                CurrVidPath = filename;
            }
        }

        public void OpenDirectoryBrowser()
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string directoryName = dlg.SelectedPath;
                CurrOutputDir = directoryName;
            }
        }

        #region implements INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private void VidConverter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Progress")
            {
                OnPropertyChanged("");
            }
        }
        #endregion
    }
}
