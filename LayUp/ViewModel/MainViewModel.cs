using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using LayUp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LayUp.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// 
        private FrmPLC _frmPLC;
       public FrmPLC FrmPLC { 
            get 
            { return _frmPLC; } 
            set
            { Set(ref _frmPLC, value); }
        }

        private Fx3GA _layupPLC=new Fx3GA();

        public Fx3GA LayupPLC
        {
            get { return _layupPLC; }
            set { Set(ref _layupPLC,value); }
        }

        private ObservableCollection<string> _returnCode ;
        public ObservableCollection<string> ReturnCode
        {
            get { return _returnCode;}
            set{Set(ref _returnCode,value);
            } }

        private string _currentTime;

        public string CurrentTime
        {
            get { return _currentTime; }
            set { Set(ref _currentTime, value); }
        }
        //�����״̬
        private string _output;
        public string Output
        {
            get { return _output; }
            set { Set(ref _output, value); }
        }

        //��ʱ��
        private readonly DispatcherTimer DispatcherTimer1;

        public ICommand ConnectCommand { get; set; }
        public ICommand SwitchOutputCommand{ get; set; }
        public ICommand StopMonitorCommand { get; set; }
       

        public ICommand ShowView2Command { private set; get; }
        public ICommand SwitchLangugeCommand { get; set; }
        public ICommand WriteDataCommand { get; set; }

        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            ///
            
            ConnectCommand = new RelayCommand<bool>(Connect,CanConnectExcute);
            SwitchOutputCommand = new RelayCommand<string>(SwitchOutput);
            StopMonitorCommand = new RelayCommand<bool>(StopMonitor, CanStopMonitorExcute);

            ShowView2Command = new RelayCommand(ShowView2CommandExecute);
            SwitchLangugeCommand = new RelayCommand<string>(SwitchLanguage);
            WriteDataCommand = new RelayCommand<object>(WriteData);
            _frmPLC = new FrmPLC();
            _layupPLC = new Fx3GA();

            ReturnCode=new ObservableCollection<string>();
            //��ʼ����ʱ������ʱ��ȡPLC״̬
            DispatcherTimer1 = new DispatcherTimer();
            DispatcherTimer1.Interval = new System.TimeSpan(500);
            DispatcherTimer1.Tick += GetOutputStatus;
            DispatcherTimer1.Tick += GetInputStatus;
            DispatcherTimer1.Tick += GetDataStatus;
            DispatcherTimer1.Tick += GetMStatus;




        }
        /// <summary>
        /// ������Զ�����״̬�����Command
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private bool CanSwitchOutput(string arg)
        {
            if (_layupPLC.IsInManualMode == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ����Ѿ����ӵ�PLC�����Command
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private bool CanConnectExcute(bool arg)
        {
            if (_layupPLC.IsDisConnected ==true)
            {
                return true;
            }
            else
            {
                return false;
            }
           
        }

        private void GetCurrentTime(object sender, EventArgs e)
        {
            //
          _currentTime=  System.DateTime.Now.ToString();
        }


        //����PLC
        private void Connect(bool isConnected)
        {
            //����վ��
            _frmPLC.axActUtlType1.ActLogicalStationNumber = (int) LayupPLC.StationNumber;
            
            if (_layupPLC.IsConnected==true)
            {

            
                DispatcherTimer1.Stop();
                _frmPLC.axActUtlType1.Close();

                LayupPLC.IsConnected = false;
               

            }
            else
            {
                int b = _frmPLC.axActUtlType1.Open();
                LayupPLC.RetrunCode = b.ToString();
                if (b == 0)
                {
                    GetPLCInfo();
                    DispatcherTimer1.Start();
                    LayupPLC.IsConnected = true;
                }
                else
                {
                    DispatcherTimer1.Stop();
                    LayupPLC.IsConnected = false;

                }
            }
            
           
        }
        private bool CanStopMonitorExcute(bool arg)
        {
            if (_layupPLC.IsConnected == true)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        //ֹͣ���
        private void StopMonitor(bool isConnected)
        {
            DispatcherTimer1.Stop();
            _frmPLC.axActUtlType1.Close();
            _layupPLC.IsConnected = false;
        }
        //��/�ر����
        private void SwitchOutput(string str)
        {
            int a;
            
            int b = _frmPLC.axActUtlType1.GetDevice(str, out a);
            if (a==0)
            {
                _frmPLC.axActUtlType1.SetDevice(str, 1);
            }
            else
            {
                _frmPLC.axActUtlType1.SetDevice(str, 0);

            }
          
            //_frmPLC.axActUtlType1.GetDevice(str, out a);
            //_layupPLC.StationNumber = b;
            //_layupPLC.Output000 = a;

        }
        //��ȡ���״̬
        private void GetOutputStatus(object sender, System.EventArgs e)
        {
            int[] a=new int[2];
            int b = _frmPLC.axActUtlType1.ReadDeviceBlock("y000", 1, out a[0]);
            if (b!=0)
            {
                StopMonitor(true);
                return;
            }
            string str = Convert.ToString(a[0], 2).PadLeft(16, '0');
            int[] array1 = new int[16];
            for (int i = 0; i < 16; i++)
            {
                array1[i] = int.Parse(str.Substring(i, 1));
            }

           // _returnCode.Add(String.Format("0x{0:x8} [HEX]", b));
            _layupPLC.Output000 = array1[15];
            _layupPLC.Output001 = array1[14];
            _layupPLC.Output002 = array1[13];
            _layupPLC.Output003 = array1[12];
            _layupPLC.Output004 = array1[11];
            _layupPLC.Output005 = array1[10];
            _layupPLC.Output006 = array1[9];
            _layupPLC.Output007 = array1[8];
            _layupPLC.Output010 = array1[7];
            _layupPLC.Output011 = array1[6];
            _layupPLC.Output012 = array1[5];
            _layupPLC.Output013 = array1[4];
            _layupPLC.Output014 = array1[3];
            _layupPLC.Output015 = array1[2];
            _layupPLC.Output016 = array1[1];
            _layupPLC.Output017 = array1[0];

        }
        //��ȡ����״̬

        private void GetInputStatus(object sender, System.EventArgs e)
        {
            int[] a = new int[2];
            int b = _frmPLC.axActUtlType1.ReadDeviceBlock("X000", 2, out a[0]);
            string str = Convert.ToString(a[0], 2).PadLeft(16, '0');

            int[] array1 = new int[16];
            int[] array2 = new int[16];
            for (int i = 0; i < 16; i++)
            {
                array1[i] = int.Parse(str.Substring(i,1));
            }
            str = Convert.ToString(a[1], 2).PadLeft(16, '0');
            for (int i = 0; i < 16; i++)
            {
                array2[i] = int.Parse(str.Substring(i, 1));
            }
            _layupPLC.Input000 = array1[15];
            _layupPLC.Input001 = array1[14];
            _layupPLC.Input002 = array1[13];
            _layupPLC.Input003 = array1[12];
            _layupPLC.Input004 = array1[11];
            _layupPLC.Input005 = array1[10];
            _layupPLC.Input006 = array1[9];
            _layupPLC.Input007 = array1[8];
            _layupPLC.Input010 = array1[7];
            _layupPLC.Input011 = array1[6];
            _layupPLC.Input012 = array1[5];
            _layupPLC.Input013 = array1[4];
            _layupPLC.Input014 = array1[3];
            _layupPLC.Input015 = array1[2];
            _layupPLC.Input016 = array1[1];
            _layupPLC.Input017 = array1[0];
            _layupPLC.Input020 = array2[15];
            _layupPLC.Input021 = array2[14];
            _layupPLC.Input022 = array2[13];
            _layupPLC.Input023 = array2[12];
            _layupPLC.Input024 = array2[11];
            _layupPLC.Input025 = array2[10];
            _layupPLC.Input026 = array2[9];
            _layupPLC.Input027 = array2[8];
            // _layupPLC.Input004 = array1[0];
            //Console.WriteLine(str);
         //   BitConverter
            
        }
        //��ȡD�Ĵ���״̬
        private void GetDataStatus(object sender, System.EventArgs e)
        {
            int[] a = new int[16];
            int b = _frmPLC.axActUtlType1.ReadDeviceBlock("D200", 16, out a[0]);
            foreach (var item in a)
            {
                
                Console.WriteLine(item.ToString());
            }
            _layupPLC.Data200 = a[0];
            _layupPLC.Data201 = a[1];
            _layupPLC.Data202 = a[2];
            _layupPLC.Data210 = a[10];
            _layupPLC.Data211 = a[11];
            _layupPLC.Data212 = a[12];
        }
        //��ȡM�Ĵ���״̬
        private void GetMStatus(object sender, System.EventArgs e)
        {

            int[] a = new int[2];
            int b = _frmPLC.axActUtlType1.ReadDeviceBlock("M224", 1, out a[0]);
            //if (b != 0)
            //{
            //    StopMonitor(true);
            //    return;
            //}
            string str = Convert.ToString(a[0], 2).PadLeft(16, '0');
            int[] array1 = new int[16];
            for (int i = 0; i < 16; i++)
            {
                array1[i] = int.Parse(str.Substring(i, 1));
            }

            // _returnCode.Add(String.Format("0x{0:x8} [HEX]", b));
            _layupPLC.M231 = array1[8];
            _layupPLC.M232 = array1[7];
            _layupPLC.M233 = array1[6];
            _layupPLC.M234 = array1[5];
            _layupPLC.M235 = array1[4];
        }
        //��ȡPLC������Ϣ
        private void GetPLCInfo()
        {
            int cpuType;
            string cpuName;
            int returnCode = _frmPLC.axActUtlType1.GetCpuType(out cpuName, out cpuType);
            _layupPLC.CPUName = cpuName;
        }

        //д��D�Ĵ���

        private void WriteData(object parameter)
        {
            int[] a = new int[3];
            var values = (object[])parameter;
               int temp= Convert.ToInt32(values[0]);
               int b= _frmPLC.axActUtlType1.SetDevice("D200",temp );
                b= _frmPLC.axActUtlType1.SetDevice("D201", Convert.ToInt32(values[1]));
                b= _frmPLC.axActUtlType1.SetDevice("D202", Convert.ToInt32(values[2]));
                b= _frmPLC.axActUtlType1.SetDevice("D210", Convert.ToInt32(values[3]));
                b= _frmPLC.axActUtlType1.SetDevice("D211", Convert.ToInt32(values[4]));
                b= _frmPLC.axActUtlType1.SetDevice("D212", Convert.ToInt32(values[5]));
            
           // int b = _frmPLC.axActUtlType1.WriteDeviceBlock("D200", 3, ref a[0]);
        }
        //�л��ֶ��Զ�
        private void Switch()
        {
            int tempReturnCode =_frmPLC.axActUtlType1.SetDevice("m0081", 0);

        }
        public void ShowView2CommandExecute()
        {
            Messenger.Default.Send(new NotificationMessage("ShowView2"));
        }
        //�л���������
        public void SwitchLanguage(string str)
        {
            // TODO: �л�ϵͳ��Դ�ļ�
            ResourceDictionary dict = new ResourceDictionary();
            try
            {
                switch (str)
                {
                    case "zh-cn":
                        dict.Source = new Uri(@"Languages\zh-cn.xaml", UriKind.Relative);
                        break;
                    case "en":
                        dict.Source = new Uri(@"Languages\en.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri(@"Languages\zh-cn.xaml", UriKind.Relative);
                        break;
                }
                Application.Current.Resources.MergedDictionaries[0] = dict;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message+"δ�ҵ������ļ�");

            }
           

            

        }
    }
}