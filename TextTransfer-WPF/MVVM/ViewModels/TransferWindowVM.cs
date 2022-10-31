using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TextTransfer_WPF.Enums;
using TextTransfer_WPF.MVVM.Views;
using TextTransfer_WPF.Commands;
using TextTransfer_WPF.MVVM.Views;

namespace TextTransfer_WPF.MVVM.ViewModels
{
    public class TransferWindowVM
    {

        #region PropertyChangedEventHandler

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Members

        private string openPath;

        private string savePath;


        public int MaximumLenght = new int();

        public Thread TransferThread { get; set; }

        public string OpenPath
        {
            get { return openPath; }
            set { openPath = value; }
        }

        public string SavePath
        {
            get { return savePath; }
            set { savePath = value; ; }
        }

        public Views.TransferWindow Window { get; set; }

        #endregion

        #region Commands

        public RelayCommand OpenCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }

        public RelayCommand StartCommand { get; set; }

        public RelayCommand ResumeCommand { get; set; }

        public RelayCommand SuspendCommand { get; set; }

        #endregion

        #region Methods

        public void RefreshPathTexbox()
        {
            Window.From.Text = OpenPath;
            Window.To.Text = SavePath;
        }

        public void PrepareAction(bool state)
        {
            Window.Suspend.IsEnabled = state;
            Window.Resume.IsEnabled = state;
            Window.Start.IsEnabled = !state;
        }

        public void OpenRun(object param)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text File (* txt)| *.txt";
            if (!(bool)ofd.ShowDialog())
                return;

            OpenPath = ofd.FileName;

            RefreshPathTexbox();
        }

        public void SaveRun(object param)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File (* txt)| *.txt";
            if (!(bool)sfd.ShowDialog())
                return;

            if (OpenPath == sfd.FileName)
            {
                CMessageBox.Show("It is probably not a good idea to copy the file where it is.", CMessageTitle.Warning, CMessageButton.Ok, CMessageButton.None);
                return;
            }

            SavePath = sfd.FileName;

            RefreshPathTexbox();
        }

        public void StartRun(object param)
        {
            PrepareAction(true);

            TransferData();
        }

        [Obsolete]
        public void SuspendRun(object param)
        {
            try
            {
                TransferThread.Suspend();

            }
            catch (Exception) { }   
        }

        //public bool SuspendCanRun(object param) => Window.Suspend.IsEnabled;

        public void ResumeRun(object param)
        {
            try
            {
                TransferThread.Resume();
            }
            catch (Exception) { }
        }

        //public bool ResumeCanRun(object param) => Window.Suspend.IsEnabled;

        public void StartTransfer()
        {
            // Transfer Data

            FileStream readStream = new FileStream(OpenPath, FileMode.Open);
            FileStream writeStream = new FileStream(SavePath, FileMode.Append);
            try
            {
                int index = 0;
                while (true)
                {
                    index = readStream.ReadByte();
                    if (index < 0)
                        break;
                    writeStream.WriteByte((byte)index);
                    Window.Dispatcher.Invoke(() => Window.Progress.Value++);
                    Thread.Sleep(50);
                }
            }
            finally
            {
                readStream.Close();
                writeStream.Close();
            }
        }

        public void TransferData()
        {
            MaximumLenght = File.ReadAllBytes(OpenPath).Length;
            Window.Progress.Maximum = MaximumLenght;
            TransferThread = new Thread(() =>
            {
                try
                {
                    StartTransfer();
                }
                finally
                {

                    Window.Dispatcher.Invoke(() =>
                    {
                        CMessageBox.Show("Finish Transfer data !", CMessageTitle.Info, CMessageButton.Ok, CMessageButton.None);
                        Window.Progress.Maximum = 100;
                        Window.Progress.Value = 0;
                        OpenPath = String.Empty;
                        SavePath = String.Empty;
                        PrepareAction(false);
                    });
                }
            });
            TransferThread.Start();
        }

        public bool StartCanRun(object param) => !string.IsNullOrWhiteSpace(OpenPath) && !string.IsNullOrWhiteSpace(SavePath);

        #endregion

        public TransferWindowVM()
        {
            OpenCommand = new RelayCommand(OpenRun);
            SaveCommand = new RelayCommand(SaveRun);
            StartCommand = new RelayCommand(StartRun, StartCanRun);
            //ResumeCommand = new(ResumeRun, ResumeCanRun);
            //SuspendCommand = new(SuspendRun, SuspendCanRun);
            ResumeCommand = new RelayCommand(ResumeRun);
            SuspendCommand = new RelayCommand(SuspendRun);
        }

    }
}
