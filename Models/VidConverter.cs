using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AbFfmpeg
{


    public class VidConverter : INotifyPropertyChanged 
    {
        BackgroundWorker _vidWorker;

        private const string ToAVICmdLine = "-y -i \"{0}\" \"{1}.avi\"";
        private const string ToMPEGCmdLine = "-y -i \"{0}\" \"{1}.mpeg\"";
        private const string ToMP3CmdLine = "-y -i \"{0}\" -vn \"{1}.mp3\"";

        //Either place the ffmpeg executable into your program directory or specify a different path
        private string _ffmpegPath = Directory.GetCurrentDirectory() + "\\ffmpeg.exe";
        private string _vidPath;
        private string _tempDirectory = Directory.GetCurrentDirectory();
        private string _outputDirectory;
        private string _tempVidPath;
        private VideoFormat _vidFormat;

        private double _vidDuration;
        private double _vidTime;

        public int Progress
        {
            get
            {
                return (int)Math.Round((_vidTime / _vidDuration) * 100.0);
            }
            set {}
        }

        public VidConverter()
        {
            _vidWorker = new BackgroundWorker();
            _vidWorker.WorkerSupportsCancellation = true;
            _vidWorker.WorkerReportsProgress = true;
            _vidWorker.DoWork 
                += new DoWorkEventHandler(vidWorker_DoWork);
            _vidWorker.ProgressChanged 
                += new ProgressChangedEventHandler(vidWorker_ProgressChanged);
            _vidWorker.RunWorkerCompleted 
                += new RunWorkerCompletedEventHandler(vidWorker_RunWorkerCompleted);
        }

        public void BeginConversion(string vidPath, VideoFormat vidFormat)
        {
            BeginConversion(vidPath, Path.GetDirectoryName(vidPath), vidFormat);
        }

        //Manually specify output directory
        public void BeginConversion(string vidPath, string outputDirectory, VideoFormat vidFormat)
        {
            if (_vidWorker.IsBusy)
            {
                MessageBox.Show("Please wait for the current conversion to finish", "Conversion in Progress",
                   MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);

            } 
            else if (!File.Exists(vidPath))
            {
                MessageBox.Show("Please specify a valid video file", "File Not Found",
                   MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }
            else if (!Directory.Exists(outputDirectory))
            {
                MessageBox.Show("The output directory you selected can't be found", "No Output Directory",
                  MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }
            else
            {
                _outputDirectory = outputDirectory;
                _vidPath = vidPath;
                _vidFormat = vidFormat;
                //Creates a temporary copy of the specified video file in the program's directory, overwritting files with the same name
                _tempVidPath = _tempDirectory + "\\" + "Temp_Copy_" + Path.GetFileName(vidPath);
                File.Copy(vidPath, _tempVidPath, true);

                _vidWorker.RunWorkerAsync();
            }
        }

        
        private void vidWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string cmd = String.Empty;
            if (_vidFormat == VideoFormat.AVI) cmd = ToAVICmdLine;
            if (_vidFormat == VideoFormat.MPEG) cmd = ToMPEGCmdLine;
            if (_vidFormat == VideoFormat.MP3) cmd = ToMP3CmdLine;

            try
            {
                //Creates the output file path
                string outputFile = (_outputDirectory + "\\" +  Path.GetFileNameWithoutExtension(_vidPath) + "_OUTPUT"); 
                //Formats the initial ffmpeg command, adding the temporary source file and the new output path
                cmd = String.Format(cmd, _tempVidPath, outputFile);
                
                ProcessStartInfo psi = new ProcessStartInfo(_ffmpegPath, cmd);
                
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                using (Process p = Process.Start(psi))
                {
                    p.StartInfo = psi;
                    p.ErrorDataReceived += CmdError;
                    p.OutputDataReceived += CmdError;
                    p.EnableRaisingEvents = true;

                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.Start();

                    p.WaitForExit();
                }
            }
            finally
            { }
        }

        private void vidWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnPropertyChanged("Progress");
        }

        private void vidWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                MessageBox.Show("Coversion Unexpectedly Canceled", "FFmpeg",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }

            else if (!(e.Error == null))
            {
                MessageBox.Show("Conversion Error: " + e.Error.ToString(), "FFmpeg",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }

            else
            {
                MessageBox.Show("Video Conversion Complete!", "FFmpeg",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }
        }

        //This is where FFmpeg's cmd output is received (not sure why it's an error)
        void CmdError(object sender, DataReceivedEventArgs e)
        {
            String currOutput = "";

            if (e.Data != null)
                currOutput = e.Data;

            ExtractConversionProg((Process)sender, currOutput);
        }

        private void ExtractConversionProg(Process conversionProcess, String processOutput)
        {
            if (processOutput.Contains("Duration"))
            {
                String rawDuration = processOutput.Substring(12, 11);
                _vidDuration = GetTimeSecs(rawDuration);
                _vidTime = 0.0;
            } 
            else if (processOutput.Contains("time="))
            {
                int timeStart = processOutput.IndexOf("time=");
                String rawTime = processOutput.Substring(timeStart + 5, 11);
                _vidTime = GetTimeSecs(rawTime);

                _vidWorker.ReportProgress(Progress);
                //Resets progress when conversion finishes
                if (Progress >= 100)
                {
                    _vidDuration = 0.0;
                    _vidTime = 0.0;
                    _vidWorker.ReportProgress(Progress);
                    conversionProcess.Kill();
                }
            }
        }

        static double GetTimeSecs(String rawTime)
        {
            //Example of raw ffmpeg duration format: 00:03:52.25
            char[] colonPeriod = { ':', '.' };

            String[] hourMinSec = rawTime.Split(colonPeriod);
            double timeSecs = (Double.Parse(hourMinSec[0]) * 3600) + (Double.Parse(hourMinSec[1]) * 60)
                + Double.Parse(hourMinSec[2]) + (Double.Parse(hourMinSec[3]) * .01);

            return timeSecs;
        }

        #region implements INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
