using EasyModbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using FRODOG03.Printer;
using System.Printing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Web;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using Newtonsoft.Json.Linq;


enum PREAMBLEs
{
    PRE_LNX_BOARD = 0x24,   //Artlite_SOM_G03 ve Artlite_BB_FC_G07
    PRE_FTC_Client = 0x42   //Test Cihazı
}

enum Receiving_Events
{
    ACK_100 = 0x00,        // Receiving message is success
    ACK_200 = 0x01,        // Executing Command is success
    NACK_111 = 0x02,       // Receiving message is fail. CRC error or undefined message
    NACK_200 = 0x03,       // Executing Command is fail
    INFO_SUCCESS = 0x04,   // Step is success (STEP_NO is in the frame, “DATA” byte)
    INFO_FAIL = 0x05,      // Step is fail (STEP_NO is in the frame, “DATA” byte)
    INFO_STARTED = 0x06,   // Step is started (STEP_NO is in the frame, “DATA” byte)
    INFO_FINISHED = 0x07,  // Mode is completed (STEP_NO is in the frame, “DATA” byte)
    INFO_CERTID = 0x08,    // Represent nByte certificate ID data
    INFO_UNIQID = 0x09,    // Represent nByte MPU unique ID data
    INFO_SWVERS = 0x0A,    // Represent nByte SW versions data
    INFO_HWVERS = 0x0B,    // Represent nByte HW versions data
    INFO_APP_READY = 0x0C, // Application is start and ready for executing commands of FTC and Secure Production. (NA)
    INFO_STEPDATA = 0x0D   // Receiving message
}

enum Transmiting_Events
{
    CMD_EnterFTCMode = 0x00,
    CMD_EnterCertInstallingMode = 0x01,
    CMD_StartFTC = 0x02,
    CMD_StartCertDownloading = 0x03,
    CMD_StartCertInstalling = 0x04,
    CMD_ReadCertID = 0x05,
    CMD_ReadUniqueID = 0x06,
    CMD_ReadSWVers = 0x07,
    CMD_ReadHWVers = 0x08
}


enum FTC_STEPS
{
    FTC_STEP_1 = 0x01,     // Self Test / Memory Benchmark
    FTC_STEP_2 = 0x02,     // Self Test / Memory Error Check
    FTC_STEP_3 = 0x03,     // Self Test / CPU Benchmark
    FTC_STEP_4 = 0x04,     // Self Test / CPU Stress
    FTC_STEP_5 = 0x05,     // Self Test / CPU FPU
    FTC_STEP_6 = 0x06,     // Self Test / Flash Benchmark
    FTC_STEP_8 = 0x08,     // Self Test / Wifi Test
    FTC_STEP_9 = 0x09,     // Self Test / Bluetooth Test
    FTC_STEP_11 = 0x0B,     // HSM
    FTC_STEP_12 = 0x0C,     // USB1 Power On
    FTC_STEP_13 = 0x0D,     // USB1 Power Off
    FTC_STEP_14 = 0x0E,     // USB1 Speed
    FTC_STEP_15 = 0x0F,     // USB2 Power On Test
    FTC_STEP_16 = 0x10,     // USB2 Power Off Test
    FTC_STEP_17 = 0x11,     // USB2 Speed Test
    FTC_STEP_18 = 0x12,     // TOF Power On
    FTC_STEP_19 = 0x13,     // TOF Power Off
    FTC_STEP_20 = 0x14,     // TOF SHUT On
    FTC_STEP_21 = 0x15,     // TOF SHUT Off
    FTC_STEP_22 = 0x16,     // TOF Ölçümüou
    FTC_STEP_23 = 0x17      // SPI

}

enum Data_Empty
{
    DATA_EMPTY = 0X45,      //Represent no data available
}

namespace FRODOG03
{
    public partial class Main : Form
    {
        private Thread saniyeThread = null;
        public AyarForm AyarFrm;
        public Sifre SifreFrm;
        public ProgAyarForm ProgAyarFrm;
        public ProcessForm ProcessFrm;
        public ProgramlamaForm ProgramlamaFrm;

        private IntPtr ShellHwnd;
        private DateTime lastDateTime = DateTime.Now;
        private ModbusClient modbusClientPLC = null;

        const int M0 = 2048;
        const int M1 = 2049;
        const int M2 = 2050;
        const int M3 = 2051;
        const int M4 = 2052;
        const int M5 = 2053;
        const int M6 = 2054;
        const int M7 = 2055;
        const int M8 = 2056;
        const int M9 = 2057;
        const int M10 = 2058;
        const int M11 = 2059;
        const int M12 = 2060;
        const int M13 = 2061;
        const int M14 = 2062;
        const int M15 = 2063;
        const int M16 = 2064;
        const int M17 = 2065;
        const int M18 = 2066;
        const int M19 = 2067;
        const int M20 = 2068;
        const int M21 = 2069;
        const int M22 = 2070;
        const int M23 = 2071;
        const int M24 = 2072;
        const int M25 = 2073;
        const int X0 = 1024;
        const int X1 = 1025;
        const int X2 = 1026;
        const int X3 = 1027;
        const int X4 = 1028;
        const int X5 = 1029;
        const int X6 = 1030;
        const int X7 = 1031;
        const int X10 = 1032;
        const int X11 = 1033;
        const int X12 = 1034;
        const int X13 = 1035;
        const int X14 = 1036;
        const int X15 = 1037;
        const int X16 = 1038;
        const int X21 = 1041;
        const int X23 = 1043;
        const int X25 = 1045;
        const int X27 = 1047;
        const int X31 = 1049;
        const int X33 = 1051;
        const int X20 = 1040;
        const int X22 = 1042;
        const int X24 = 1044;
        const int X26 = 1046;
        const int X30 = 1048;
        const int X32 = 1050;

        const int D4 = 4100;
        const int D6 = 4102;
        const int D8 = 4104;
        const int D10 = 4106;
        const int D12 = 4108;
        const int D14 = 4110;
        const int D16 = 4112;
        const int D18 = 4114;
        const int D20 = 4116;
        const int D30 = 4126;
        const int D40 = 4136;
        const int D400 = 4496;
        const int D405 = 4501;
        const int D410 = 4506;
        const int D415 = 4511;
        const int D440 = 4536;
        const int D445 = 4541;
        const int D450 = 4546;
        const int D455 = 4551;
        const int D460 = 4556;
        const int D465 = 4561;

        const int Y1 = 1281;
        const int Y2 = 1282;
        const int Y3 = 1283;
        const int Y4 = 1284;
        const int Y5 = 1285;
        const int Y6 = 1286;
        const int Y7 = 1287;
        const int Y10 = 1288;
        const int Y11 = 1289;
        const int Y12 = 1290;
        const int Y13 = 1291;
        const int Y20 = 1296;
        const int Y21 = 1297;
        const int Y22 = 1298;
        const int Y23 = 1299;
        const int Y24 = 1300;
        const int Y25 = 1301;
        const int Y40 = 1312;
        const int Y41 = 1313;
        const int Y42 = 1314;
        const int Y43 = 1315;
        const int Y44 = 1316;
        const int Y45 = 1317;
        const int Y46 = 1318;
        const int Y47 = 1319;
        const int Y50 = 1320;
        const int Y51 = 1321;
        const int Y52 = 1322;
        const int Y53 = 1323;
        const int Y35 = 1309;
        const int Y36 = 1310;
        const int Y37 = 1311;
        const int FCT_CARD_NUMBER = 1;    //DEĞİŞTİR ONUR
        const int FCT_STEP_MAX = 11;      //DEĞİŞTİR ONUR

        //Sıfırlanmamalı
        int totalCard = 0;
        int errorCard = 0;
        public string customMessageBoxTitle = "";
        string logDosyaPath = "";
        byte[] byteArray = new byte[9];
        int byteLenght = 0;
        int DATA1 = 0;
        int DATA2 = 0;
        int DATA3 = 0;
        int LSB = 0;
        int MSB = 0;
        long PTC1 = 0;
        string FCT_SOFT_VER = "";
        string SOFT_VER = "";

        //Sıfırlanmalı
        int stepState = 0;
        int stepOut = 0;
        int feedbackType = 0;
        int fctsonuc = 0;
        int adminTimerCounter = 0;
        int timeoutTimerCounter = 0;
        int saniyeTimerCounter = 0;
        int fctSaniye = 0;
        byte[] arrayRx = new byte[256];
        int counterRxByte = 0;
        int relay_counter = 1;
        private bool isStartByteReceived = false; // Başlangıç bayrağının alınıp alınmadığını belirten bir bayrak
        private const int EXPECTED_DATA_LENGTH = 6; // Beklenen veri uzunluğu
        private const byte EXPECTED_START_BYTE = 0x24;
        private bool isDataReceived = false;

        //Sıfırlanmalı
        public int yetki = 0;
        public int barcodeCounter = 0;
        string[] filePathTxt = new string[FCT_CARD_NUMBER + 1];
        public string[] barcode50 = new string[FCT_CARD_NUMBER + 1];
        bool[] cardResult = new bool[FCT_CARD_NUMBER + 1];
        public string[] sap_no = new string[FCT_CARD_NUMBER + 1];


        SqlConnection SQLConnection;
        bool sqlConnection = false;
        string urun_id = "";
        string urun_barkod = "";
        string son_istasyon_id = "";
        string giris_zamani = "";
        string son_istasyon_zamani = "";
        string urun_durum_no = "";
        string ariza_kodu = "";
        string tamir_edildi = "";
        string son_islem_tamamlandi = "";
        string firma_no = "";
        string urun_kodu = "";
        string panacim_kodu = "";
        string parti_no = "";
        string alan_5 = "";
        string alan_6 = "";
        string alan_7 = "";
        string pcb_barkod = "";

        const string POTA_STATION = "1";
        const string PAKETLEME_STATION = "5";
        const string ICT_STATION_ISTANBUL = "15";
        const string ICT_STATION_BOLU_1 = "19";
        const string ICT_STATION_BOLU_2 = "22";
        const string ICT_STATION_BOLU_3 = "32";
        const string ALPPLAS_STATION_BEAST_GAS_TIMER = "42";      //DEĞİŞTİR ONUR  // BAKILACAK

        const string URUN_DURUM_HURDA = "2";
        const string URUN_DURUM_BEKLETILIYOR = "3";
        const string URUN_DURUM_TAMIR_EDILECEK = "4";
        const string URUN_DURUM_PROCESS = "5";
        const string URUN_DURUM_TAMIR_EDILDI = "6";
        const string URUN_DURUM_HAZIR = "7";
        const string URUN_DURUM_SEVK_EDILECEK = "8";

        const string ARIZA_YOK = "0";
        const string CHECKSUM_HATA = "1";
        const string ARTOUCH_FCT_HATA = "5";
        const string READ_SOFTWARE = "23";
        const string DUO_TESTI = "32";

        public bool traceabilityStatus = false;

        public Main()
        {
            this.AyarFrm = new AyarForm();
            this.AyarFrm.MainFrm = this;
            this.SifreFrm = new Sifre();
            this.SifreFrm.MainFrm = this;
            this.ProgAyarFrm = new ProgAyarForm();
            this.ProgAyarFrm.MainFrm = this;
            this.ProcessFrm = new ProcessForm();
            this.ProcessFrm.MainFrm = this;
            this.ProgramlamaFrm = new ProgramlamaForm();
            this.ProgramlamaFrm.MainFrm = this;
            InitializeComponent();
        }

        public class INIKaydet
        {
            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

            public INIKaydet(string dosyaYolu)
            {
                DOSYAYOLU = dosyaYolu;
            }
            private string DOSYAYOLU = String.Empty;
            public string Varsayilan { get; set; }
            public string Oku(string bolum, string ayaradi)
            {
                Varsayilan = Varsayilan ?? string.Empty;
                StringBuilder StrBuild = new StringBuilder(256);
                GetPrivateProfileString(bolum, ayaradi, Varsayilan, StrBuild, 255, DOSYAYOLU);
                return StrBuild.ToString();
            }
            public long Yaz(string bolum, string ayaradi, string deger)
            {
                return WritePrivateProfileString(bolum, ayaradi, deger, DOSYAYOLU);
            }
        }

        [DllImport("user32.dll")]
        public static extern byte ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string ClassName, string WindowName);

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (traceabilityStatus)
            {
                if (sqlConnection)
                {
                    sqlConnection = false;
                    SQLConnection.Close();
                }
            }
            if (saniyeThread != null)
            {
                saniyeThread.Abort();
            }
            this.serialPort1.Close();
            modbusClientPLC.Disconnect();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //clientSocket.Connect("10.5.5.10", 9876); // KAMERA TCP/IP ILE CONNCET
            //this.ShellHwnd = Main.FindWindow("Shell TrayWnd", (string)null);
            //IntPtr shellHwnd = this.ShellHwnd;
            //int num1 = (int)Main.ShowWindow(this.ShellHwnd, 0);
            traceabilityStatus = Ayarlar.Default.chBoxIzlenebilirlik;
            sqlCommonConnection();
            if (sqlConnection)
            {
                settingGetInit();
                FCT_Clear();
                this.ProgramlamaFrm.programlamaInit();
                this.yetkidegistir();
                saniyeThread = new Thread(saniyeThreadFunc);
                saniyeThread.Start();
            }
        }

        private void settingGetInit()
        {
            this.customMessageBoxTitle = Ayarlar.Default.projectName;
            this.projectNameTxt.Text = customMessageBoxTitle;
            this.Text = customMessageBoxTitle;
            this.cardPicture.ImageLocation = Ayarlar.Default.PNGdosyayolu;
            this.logDosyaPath = Ayarlar.Default.txtLogDosya;

            foreach (string portName in SerialPort.GetPortNames())
            {
                this.AyarFrm.SerialPort1Com.Items.Add((object)portName);
            }

            this.serialPort1.PortName = Ayarlar.Default.SerialPort1Com;
            this.serialPort1.BaudRate = Ayarlar.Default.SerialPort1Baud;
            this.serialPort1.DataBits = Ayarlar.Default.SerialPort1dataBits;
            this.serialPort1.StopBits = Ayarlar.Default.SerialPort1stopBit;
            this.serialPort1.Parity = Ayarlar.Default.SerialPort1Parity;
            this.serialPort1.ReceivedBytesThreshold = 1;

            modbusClientPLC = new ModbusClient(Ayarlar.Default.SerialPort2Com);
            modbusClientPLC.UnitIdentifier = 1; //Not necessary since default slaveID = 1;
            modbusClientPLC.Baudrate = Ayarlar.Default.SerialPort2Baud;   // Not necessary since default baudrate = 9600
            modbusClientPLC.Parity = Ayarlar.Default.SerialPort2Parity;
            modbusClientPLC.StopBits = Ayarlar.Default.SerialPort2stopBit;
            modbusClientPLC.ConnectionTimeout = 2000;

            this.timerAdmin.Interval = Ayarlar.Default.timerAdmin;
            this.serialRxTimeout.Interval = Ayarlar.Default.serialRxTimeout;

            if (Ayarlar.Default.chBoxSerial1)  //UDAQ1
            {
                try
                {
                    this.serialPort1.DtrEnable = true;
                    this.serialPort1.Open();
                    lblStatusCom1.Text = "ON";
                    lblStatusCom1.BackColor = Color.Green;
                }
                catch (Exception ex)
                {
                    int num2 = (int)MessageBox.Show("UDAQ1 Port Hatası: " + ex.ToString());
                    lblStatusCom1.Text = "OFF";
                    lblStatusCom1.BackColor = Color.Red;
                }
            }
            if (Ayarlar.Default.chBoxSerial2)  //PLC
            {
                try
                {
                    modbusClientPLC.Connect();
                    lblStatusCom2.Text = "ON";
                    lblStatusCom2.BackColor = Color.Green;
                }
                catch (Exception ex)
                {
                    int num2 = (int)MessageBox.Show("PLC Port Hatası: " + ex.ToString());
                    lblStatusCom2.Text = "OFF";
                    lblStatusCom2.BackColor = Color.Red;
                }
            }
        }

        /****************************************** SQL *************************************************/
        public void sqlCommonConnection()
        {
            if (traceabilityStatus)
            {
                if (sqlConnection == false)
                {
                    try
                    {
                        string connetionString = @"Data Source=192.168.0.8\MEYER;Initial Catalog=Alpplas_Uretim_Takip;User ID=Alpplas_user;Password=Alp-User-21*";
                        SQLConnection = new SqlConnection(connetionString);
                        SQLConnection.Open();
                        ConsoleAppendLine("SQL Baglantısı Açıldı", Color.Green);
                        sqlConnection = true;
                        lblStatusSQL.Text = "ON";
                        lblStatusSQL.BackColor = Color.Green;
                    }
                    catch (Exception ex)
                    {
                        sqlConnection = false;
                        lblStatusSQL.Text = "OFF";
                        lblStatusSQL.BackColor = Color.Red;
                        ConsoleAppendLine("sqlCommonConnection Error: " + ex.Message, Color.Red);
                    }
                }
            }
            else
            {
                lblStatusSQL.Text = "OFF";
                lblStatusSQL.BackColor = Color.Red;
                sqlConnection = true;
            }
        }

        public void sqlWriteError()
        {
            sqlConnection = false;
            lblStatusSQL.Text = "OFF";
            lblStatusSQL.BackColor = Color.Red;
            ConsoleAppendLine("sqlWriteError()", Color.Red);
        }

        public bool urunlerRead(string fullproductCode)
        {
            if (traceabilityStatus)
            {
                sqlCommonConnection();
                if (sqlConnection)
                {
                    try
                    {
                        string sql1 = "SELECT URUN_ID, URUN_BARKOD, SON_ISTASYON_ID, GIRIS_ZAMANI, SON_ISTASYON_ZAMANI, URUN_DURUM_NO, ARIZA_KODU, TAMIR_EDILDI, SON_ISLEM_TAMAMLANDI, FIRMA_NO, URUN_KODU, PANACIM_KODU, PARTI_NO, ALAN_5, ALAN_6, ALAN_7, PCB_BARKOD FROM URUNLER WHERE URUN_BARKOD='" + fullproductCode + "'";
                        SqlCommand command1 = new SqlCommand(sql1, SQLConnection);
                        SqlDataReader dataReader1 = command1.ExecuteReader(CommandBehavior.CloseConnection);
                        bool findState = false;
                        dataReader1.Read();
                        findState = dataReader1.HasRows;
                        if (findState)
                        {
                            urun_id = Convert.ToString(dataReader1.GetValue(0));
                            urun_barkod = Convert.ToString(dataReader1.GetValue(1));
                            son_istasyon_id = Convert.ToString(dataReader1.GetValue(2));
                            giris_zamani = Convert.ToString(dataReader1.GetValue(3));
                            son_istasyon_zamani = Convert.ToString(dataReader1.GetValue(4));
                            urun_durum_no = Convert.ToString(dataReader1.GetValue(5));
                            ariza_kodu = Convert.ToString(dataReader1.GetValue(6));
                            tamir_edildi = Convert.ToString(dataReader1.GetValue(7));
                            son_islem_tamamlandi = Convert.ToString(dataReader1.GetValue(8));
                            firma_no = Convert.ToString(dataReader1.GetValue(9));
                            urun_kodu = Convert.ToString(dataReader1.GetValue(10));
                            panacim_kodu = Convert.ToString(dataReader1.GetValue(11));
                            parti_no = Convert.ToString(dataReader1.GetValue(12));
                            alan_5 = Convert.ToString(dataReader1.GetValue(13));
                            alan_6 = Convert.ToString(dataReader1.GetValue(14));
                            alan_7 = Convert.ToString(dataReader1.GetValue(15));
                            pcb_barkod = Convert.ToString(dataReader1.GetValue(16));
                            ConsoleAppendLine("Ürün Id: " + urun_id, Color.Black);
                            ConsoleAppendLine("Son İstasyon Id: " + son_istasyon_id, Color.Black);
                            ConsoleAppendLine("İlk Giriş Zamanı: " + giris_zamani, Color.Black);
                            ConsoleAppendLine("Son İstasyon Zamanı: " + son_istasyon_zamani, Color.Black);
                            ConsoleAppendLine("Ürün Durum No: " + urun_durum_no, Color.Black);
                            ConsoleAppendLine("Arıza Kodu: " + ariza_kodu, Color.Black);
                            ConsoleAppendLine("Tamir Edildi: " + tamir_edildi, Color.Black);
                            ConsoleAppendLine("Son İşlem Tamamlandı: " + son_islem_tamamlandi, Color.Black);
                            ConsoleNewLine();
                            urunDurum();
                            sonIstasyonDurum();
                            arizaDurum();

                            dataReader1.Close();
                            if (sqlConnection)
                            {
                                sqlConnection = false;
                                SQLConnection.Close();
                            }
                        }
                        else
                        {
                            dataReader1.Close();
                            if (sqlConnection)
                            {
                                sqlConnection = false;
                                SQLConnection.Close();
                            }
                            ConsoleNewLine();
                            ConsoleNewLine();
                            ConsoleAppendLine("YANLIŞ BARKOD YA DA ÜRÜN SİSTEM'DE KAYITLI DEĞİL!", Color.Red);
                            return false;
                        }

                        ConsoleNewLine();
                        ConsoleNewLine();
                        if (son_istasyon_id == POTA_STATION && urun_durum_no == URUN_DURUM_HAZIR && son_islem_tamamlandi == "True")
                        {
                            ConsoleAppendLine("ÜRÜN POTADAN'DAN GEÇMİŞ ICT'YE GİRMELİ", Color.Green);
                            return false;
                        }
                        else if (son_istasyon_id == POTA_STATION && urun_durum_no == URUN_DURUM_TAMIR_EDILDI && son_islem_tamamlandi == "True" && tamir_edildi == "True")
                        {
                            ConsoleAppendLine("ÜRÜN TAMİR'DEN GEÇMİŞ FCT'YE GİREBİLİR", Color.Green);
                            return true;
                        }
                        else if ((son_istasyon_id == ICT_STATION_BOLU_1 || son_istasyon_id == ICT_STATION_BOLU_2 || son_istasyon_id == ICT_STATION_BOLU_3 || son_istasyon_id == ICT_STATION_ISTANBUL) && urun_durum_no == URUN_DURUM_HAZIR && son_islem_tamamlandi == "True")
                        {
                            ConsoleAppendLine("ÜRÜN ICT'DEN GEÇMİŞ FCT'YE GİREBİLİR", Color.Green);
                            return true;
                        }
                        else if ((son_istasyon_id == ICT_STATION_BOLU_1 || son_istasyon_id == ICT_STATION_BOLU_2 || son_istasyon_id == ICT_STATION_BOLU_3 || son_istasyon_id == ICT_STATION_ISTANBUL) && (urun_durum_no == URUN_DURUM_PROCESS && urun_durum_no == URUN_DURUM_TAMIR_EDILECEK) && son_islem_tamamlandi == "False")
                        {
                            ConsoleAppendLine("ÜRÜN ICT'DEN KALMIŞ FCT'YE GİREMEZ", Color.Red);
                            return false;
                        }
                        else if ((son_istasyon_id == ALPPLAS_STATION_BEAST_GAS_TIMER) && urun_durum_no == URUN_DURUM_TAMIR_EDILECEK && son_islem_tamamlandi == "True")
                        {
                            ConsoleAppendLine("KART TAMİRE GİRMELİ YA DA TEKRAR FCT'YE GİREBİLİR", Color.Green);
                            return true;
                        }
                        else if ((son_istasyon_id == ALPPLAS_STATION_BEAST_GAS_TIMER) && urun_durum_no == URUN_DURUM_HAZIR && son_islem_tamamlandi == "True")
                        {
                            ConsoleAppendLine("ÜRÜN FCT'DEN DAHA ÖNCE GEÇTİ FCT'YE GİREBİLİR", Color.Green);
                            return true;
                        }
                        else if (son_istasyon_id == PAKETLEME_STATION)
                        {
                            ConsoleAppendLine("KART PAKETLEMEDEN GEÇMİŞ FCT-ICT'YE SOKMAYIN", Color.Orange);
                            return false;
                        }
                        else
                        {
                            ConsoleAppendLine("KART BİR ÖNCEKİ İSTASYONA GİRMELİ", Color.Red);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        sqlWriteError();
                        ConsoleAppendLine("urunlerRead Error: " + ex.Message, Color.Red);
                        return false;
                    }
                }
                else
                {
                    ConsoleAppendLine("SQL BAĞLANTI KAPALI", Color.Red);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void sonIstasyonDurum()
        {
            if (son_istasyon_id == POTA_STATION)
            {
                if (urun_durum_no == URUN_DURUM_HAZIR)
                {
                    ConsoleAppendLine("SON GİRDİĞİ İSTASYON: POTA", Color.Green);
                }
                else if (urun_durum_no == URUN_DURUM_TAMIR_EDILDI)
                {
                    ConsoleAppendLine("SON GİRDİĞİ İSTASYON: TAMİR", Color.Green);
                }
            }
            else if (son_istasyon_id == PAKETLEME_STATION)
            {
                ConsoleAppendLine("SON GİRDİĞİ İSTASYON: PAKETLEME", Color.Green);
            }
            else if (son_istasyon_id == ICT_STATION_BOLU_1)
            {
                ConsoleAppendLine("SON GİRDİĞİ İSTASYON: ICT-1", Color.Green);
            }
            else if (son_istasyon_id == ICT_STATION_BOLU_2)
            {
                ConsoleAppendLine("SON GİRDİĞİ İSTASYON: ICT-2", Color.Green);
            }
            else if (son_istasyon_id == ICT_STATION_BOLU_3)
            {
                ConsoleAppendLine("SON GİRDİĞİ İSTASYON: ICT-3", Color.Green);
            }
            else if (son_istasyon_id == ICT_STATION_ISTANBUL)
            {
                ConsoleAppendLine("SON GİRDİĞİ İSTASYON: ICT-İSTANBUL", Color.Green);
            }
            else if (son_istasyon_id == ALPPLAS_STATION_BEAST_GAS_TIMER)
            {
                ConsoleAppendLine("SON GİRDİĞİ İSTASYON: ALPPLAS_STATION_NXE7S_G05 FCT", Color.Green);
            }
        }

        private void urunDurum()
        {
            if (son_istasyon_id == PAKETLEME_STATION)
            {
                ConsoleAppendLine("ÜRÜN DURUM: ÜRÜN SEVKİYATA HAZIR", Color.Green);
            }
            else
            {
                if (urun_durum_no == URUN_DURUM_HURDA)
                {
                    ConsoleAppendLine("ÜRÜN DURUM: ÜRÜN HURDA", Color.Red);
                }
                else if (urun_durum_no == URUN_DURUM_BEKLETILIYOR)
                {
                    ConsoleAppendLine("ÜRÜN DURUM: ÜRÜN BEKLETİLİYOR", Color.Red);
                }
                else if (urun_durum_no == URUN_DURUM_TAMIR_EDILECEK)
                {
                    ConsoleAppendLine("ÜRÜN DURUM: ÜRÜN TAMİR EDİLECEK", Color.Red);
                }
                else if (urun_durum_no == URUN_DURUM_PROCESS)
                {
                    ConsoleAppendLine("ÜRÜN DURUM: ÜRÜN TEST EDİLİYOR VEYA İŞLEM ALTINDA", Color.Green);
                }
                else if (urun_durum_no == URUN_DURUM_TAMIR_EDILDI)
                {
                    ConsoleAppendLine("ÜRÜN DURUM: ÜRÜN TAMİR EDİLDİ", Color.Green);
                }
                else if (urun_durum_no == URUN_DURUM_HAZIR)
                {
                    ConsoleAppendLine("ÜRÜN DURUM: ÜRÜN BİR SONRAKİ TESTE HAZIR", Color.Green);
                }
                else if (urun_durum_no == URUN_DURUM_SEVK_EDILECEK)
                {
                    ConsoleAppendLine("ÜRÜN DURUM: ÜRÜN SEVKİYATA HAZIR", Color.Green);
                }
            }
        }

        private void arizaDurum()
        {
            if (ariza_kodu == ARIZA_YOK)
            {
                ConsoleAppendLine("ARIZA DURUM: ARIZA_YOK", Color.Green);
            }
            else if (ariza_kodu == CHECKSUM_HATA)
            {
                ConsoleAppendLine("ARIZA DURUM: CHECKSUM_HATA", Color.Red);
            }
            else if (ariza_kodu == READ_SOFTWARE)
            {
                ConsoleAppendLine("ARIZA DURUM: YAZILIM_TESTİ_HATA", Color.Red);
            }
            else if (ariza_kodu == DUO_TESTI)
            {
                ConsoleAppendLine("ARIZA DURUM: HABERLEŞME_HATA", Color.Red);
            }
        }

        public string barcodeUnıqTestIdRead(string urun_id)
        {
            string urun_test_id = "";
            sqlCommonConnection();
            if (sqlConnection)
            {
                try
                {
                    string sql5 = "SELECT URUN_TEST_ID, URUN_ID, MAKINA_NO, TEST_BASLANGIC_ZAMANI, TEST_BITIS_ZAMANI FROM URUN_TESTLER WHERE URUN_ID=" + Convert.ToInt32(urun_id);
                    SqlCommand command5 = new SqlCommand(sql5, SQLConnection);
                    SqlDataReader dataReader5 = command5.ExecuteReader(CommandBehavior.CloseConnection);

                    bool findState = false;
                    dataReader5.Read();
                    findState = dataReader5.HasRows;
                    if (findState)
                    {
                        while (dataReader5.Read())
                        {
                            urun_test_id = Convert.ToString(dataReader5["URUN_TEST_ID"].ToString());
                        }
                        dataReader5.Close();
                        if (sqlConnection)
                        {
                            sqlConnection = false;
                            SQLConnection.Close();
                        }
                        return urun_test_id;
                    }
                    else
                    {
                        dataReader5.Close();
                        if (sqlConnection)
                        {
                            sqlConnection = false;
                            SQLConnection.Close();
                        }
                        ConsoleNewLine();
                        ConsoleNewLine();
                        ConsoleAppendLine("YANLIŞ BARKOD YA DA ÜRÜN SİSTEM'DE KAYITLI DEĞİL !", Color.Red);
                        ConsoleAppendLine("LÜTFEN KARTI POTADAN GEÇİRİN !", Color.Red);
                        return urun_test_id;
                    }
                }
                catch (Exception ex)
                {
                    sqlWriteError();  //ID READ
                    ConsoleAppendLine("barcodeUnıqTestIdRead Error: " + ex.Message, Color.Red);
                    return urun_test_id;
                }
            }
            else
            {
                ConsoleAppendLine("SQL BAĞLANTI KAPALI", Color.Red);
                return urun_test_id;
            }
        }

        private bool urunTestlerInsert()
        {
            if (traceabilityStatus)
            {
                sqlCommonConnection();
                if (sqlConnection)
                {
                    try
                    {
                        DateTime dt = DateTime.Now;
                        string nowYear = Convert.ToString(dt.Year);
                        string nowMonth = Convert.ToString(dt.Month);
                        string nowDay = Convert.ToString(dt.Day);
                        string nowHour = Convert.ToString(dt.Hour);
                        string nowMinute = Convert.ToString(dt.Minute);
                        string nowSecond = Convert.ToString(dt.Second);
                        string mnowSecond = Convert.ToString(dt.Millisecond);
                        string firstTime = nowYear + "-" + nowMonth + "-" + nowDay + " " + nowHour + ":" + nowMinute + ":" + nowSecond + "." + mnowSecond;
                        string sql2 = "INSERT INTO URUN_TESTLER (URUN_ID,MAKINA_NO,TEST_BASLANGIC_ZAMANI,TEST_BITIS_ZAMANI) VALUES('"
                            + urun_id + "'," + "'" + ALPPLAS_STATION_BEAST_GAS_TIMER + "'," + "'" + firstTime + "'," + "NULL" + ")";

                        SqlCommand command2 = new SqlCommand(sql2, SQLConnection);
                        SqlDataReader dataReader2 = command2.ExecuteReader();
                        while (dataReader2.Read())
                        {
                            if (command2.ExecuteNonQuery() == 1)
                            {
                                ConsoleAppendLine("SQL Success 1", Color.Green);
                            }
                            else
                            {
                                ConsoleAppendLine("SQL Success 2", Color.Green);
                            }
                        }
                        ConsoleAppendLine("SQL Firt Insert", Color.Green);
                        dataReader2.Close();
                        if (sqlConnection)
                        {
                            sqlConnection = false;
                            SQLConnection.Close();
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        sqlWriteError();
                        ConsoleAppendLine("urunTestlerInsert Error: " + ex.Message, Color.Red);
                        return false;
                    }
                }
                else
                {
                    ConsoleAppendLine("SQL BAĞLANTI KAPALI", Color.Red);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool urunlerUpdate()
        {
            if (traceabilityStatus)
            {
                sqlCommonConnection();
                if (sqlConnection)
                {
                    try
                    {
                        DateTime dt = DateTime.Now;
                        string nowYear = Convert.ToString(dt.Year);
                        string nowMonth = Convert.ToString(dt.Month);
                        string nowDay = Convert.ToString(dt.Day);
                        string nowHour = Convert.ToString(dt.Hour);
                        string nowMinute = Convert.ToString(dt.Minute);
                        string nowSecond = Convert.ToString(dt.Second);
                        string mnowSecond = Convert.ToString(dt.Millisecond);
                        string lastTime = nowYear + "-" + nowMonth + "-" + nowDay + " " + nowHour + ":" + nowMinute + ":" + nowSecond + "." + mnowSecond;
                        //  string lastTime = "2021-05-03 14:41:10.587";
                        string sql3 = "UPDATE URUNLER SET SON_ISTASYON_ID='" + ALPPLAS_STATION_BEAST_GAS_TIMER + "'" + ",URUN_DURUM_NO='" + urun_durum_no + "'"
                            + ",SON_ISLEM_TAMAMLANDI='" + "1" + "'" + ",ARIZA_KODU='" + ariza_kodu + "'"
                            + ",SON_ISTASYON_ZAMANI='" + lastTime + "'" + "WHERE URUN_ID='" + urun_id + "'";
                        SqlCommand command3 = new SqlCommand(sql3, SQLConnection);
                        SqlDataReader dataReader3 = command3.ExecuteReader();
                        while (dataReader3.Read())
                        {
                            if (command3.ExecuteNonQuery() == 1)
                            {
                                ConsoleAppendLine("SQL Success 1", Color.Green);
                            }
                            else
                            {
                                ConsoleAppendLine("SQL Success 2", Color.Green);
                            }
                        }
                        ConsoleAppendLine("SQL Last Update", Color.Green);
                        dataReader3.Close();
                        if (sqlConnection)
                        {
                            sqlConnection = false;
                            SQLConnection.Close();
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        sqlWriteError();
                        ConsoleAppendLine("urunlerUpdate Error: " + ex.Message, Color.Red);
                        return false;
                    }
                }
                else
                {
                    ConsoleAppendLine("SQL BAĞLANTI KAPALI", Color.Red);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public bool fonksiyonTestInsert(string adimNo, string testTipi, string birim, string altLimit, string ustLimit, string olculen, string sonuc, string bos)
        {
            if (traceabilityStatus)
            {
                string urun_test_id = "";
                urun_test_id = barcodeUnıqTestIdRead(urun_id);
                sqlCommonConnection();
                if (sqlConnection)
                {
                    try
                    {
                        DateTime dt = DateTime.Now;
                        string nowYear = Convert.ToString(dt.Year);
                        string nowMonth = Convert.ToString(dt.Month);
                        string nowDay = Convert.ToString(dt.Day);
                        string nowHour = Convert.ToString(dt.Hour);
                        string nowMinute = Convert.ToString(dt.Minute);
                        string nowSecond = Convert.ToString(dt.Second);
                        string mnowSecond = Convert.ToString(dt.Millisecond);
                        string firstTime = nowYear + "-" + nowMonth + "-" + nowDay + " " + nowHour + ":" + nowMinute + ":" + nowSecond + "." + mnowSecond;
                        string sql4 = "INSERT INTO FONKSIYON_TEST (URUN_TEST_ID,ADIM_NO,TEST_TIPI,BIRIM,ALT_LIMIT,UST_LIMIT,OLCULEN,SONUC,CREATE_DATE) VALUES('"
                          + urun_test_id + "'," + "'" + adimNo + "'," + "'" + testTipi + "'," + "'" + birim + "'," + "'" + altLimit + "'," + "'" + ustLimit + "'," + "'" + olculen + "'," + "'" + sonuc + "'," + "'" + firstTime + "'" + ")";
                        SqlCommand command4 = new SqlCommand(sql4, SQLConnection);
                        SqlDataReader dataReader4 = command4.ExecuteReader();
                        while (dataReader4.Read())
                        {
                            if (command4.ExecuteNonQuery() == 1)
                            {
                                ConsoleAppendLine("SQL Success 1", Color.Green);
                            }
                            else
                            {
                                ConsoleAppendLine("SQL Success 2", Color.Green);
                            }
                        }
                        ConsoleAppendLine("SQL fonksiyonTestInsert Insert", Color.Green);
                        dataReader4.Close();
                        if (sqlConnection)
                        {
                            sqlConnection = false;
                            SQLConnection.Close();
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        sqlWriteError();
                        ConsoleAppendLine("fonksiyonTestInsert Error: " + ex.Message, Color.Red);
                        return false;
                    }
                }
                else
                {
                    ConsoleAppendLine("SQL BAĞLANTI KAPALI", Color.Red);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public bool fctGenelInsert(string sonuc)
        {
            if (traceabilityStatus)
            {
                string urun_test_id = "";
                urun_test_id = barcodeUnıqTestIdRead(urun_id);
                sqlCommonConnection();
                if (sqlConnection)
                {
                    try
                    {
                        DateTime dt = DateTime.Now;
                        string nowYear = Convert.ToString(dt.Year);
                        string nowMonth = Convert.ToString(dt.Month);
                        string nowDay = Convert.ToString(dt.Day);
                        string nowHour = Convert.ToString(dt.Hour);
                        string nowMinute = Convert.ToString(dt.Minute);
                        string nowSecond = Convert.ToString(dt.Second);
                        string mnowSecond = Convert.ToString(dt.Millisecond);
                        string firstTime = nowYear + "-" + nowMonth + "-" + nowDay + " " + nowHour + ":" + nowMinute + ":" + nowSecond + "." + mnowSecond;
                        string sql6 = "INSERT INTO FCT_GENEL (URUN_ID,URUN_TEST_ID,SONUC,CREATE_DATE,SON_ISTASYON_ID) VALUES('"
                          + urun_id + "'," + "'" + urun_test_id + "'," + "'" + sonuc + "'," + "'" + firstTime + "'," + "'" + ALPPLAS_STATION_BEAST_GAS_TIMER + "'" + ")";
                        SqlCommand command6 = new SqlCommand(sql6, SQLConnection);
                        SqlDataReader dataReader6 = command6.ExecuteReader();
                        while (dataReader6.Read())
                        {
                            if (command6.ExecuteNonQuery() == 1)
                            {
                                ConsoleAppendLine("SQL Success 1", Color.Green);
                            }
                            else
                            {
                                ConsoleAppendLine("SQL Success 2", Color.Green);
                            }
                        }
                        ConsoleAppendLine("SQL fctGenelInsert Insert", Color.Green);
                        dataReader6.Close();
                        if (sqlConnection)
                        {
                            sqlConnection = false;
                            SQLConnection.Close();
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        sqlWriteError();
                        ConsoleAppendLine("fctGenelInsert Error: " + ex.Message, Color.Red);
                        return false;
                    }
                }
                else
                {
                    ConsoleAppendLine("SQL BAĞLANTI KAPALI", Color.Red);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /****************************************** SERIAL *************************************************/
        private List<byte> receivedData = new List<byte>(); // Alınan verileri tutmak için bir liste tanımlanır
        byte[] byteArrayENT = new byte[6];


        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialPort1.BytesToRead > 0)
            {
                byte currentByte = Convert.ToByte(serialPort1.ReadByte());

                if (!isStartByteReceived)
                {
                    if (currentByte == 0x24) // Başlangıç bayrağı 0x24 (hexadecimal)
                    {
                        isStartByteReceived = true; // Başlangıç bayrağı alındı
                    }
                    else
                    {
                        // Başlangıç bayrağı beklenen değerden farklı, beklenen değeri alana kadar devam et
                        continue;
                    }
                }

                // Gelen veriyi listeye ekle
                receivedData.Add(currentByte);

                // Eğer son alınan byte 0xAF ise, veri işleme aşamasına geç
                if (currentByte == 0xAF) // Bitiş bayrağı 0xAF (hexadecimal)
                {
                    // Gelen verileri işlemek için metodu çağır
                    this.Invoke(new EventHandler(ShowData1));

                    // Alınan veri ile ilgili diğer işlemler yapılabilir
                    receivedData.Clear(); // Alınan verileri temizle, bir sonraki paket için hazır hale getirilir
                    isStartByteReceived = false; // Başlangıç bayrağı alınmadı olarak işaretlenir, bir sonraki paket için hazır hale getirilir
                }
            }
        }
        private void ShowData1(object sender, EventArgs e)
        {
            // Alınan verileri konsola yazdır
            Console.WriteLine("Received Data: ");
            foreach (byte data in receivedData)
            {
                Console.Write(data.ToString("X2") + " "); // Her byte'ı hexadecimal olarak yazdır
            }
            Console.WriteLine(); // Yeni satıra geç

            // justFeedbackCheck metodunu çağır ve receivedData listesini parametre olarak aktar
            justFeedbackCheck1(receivedData);
        }

        private void justFeedbackCheck1(List<byte> receivedData)
        {
            // receivedData listesindeki verileri kullanarak geri bildirim işlemini gerçekleştir
            // Örnek:
            if (stepState == 0)
            {
                if (receivedData.Count >= 6 && receivedData[0] == 0x24 && receivedData[5] == 0xAF)
                {
                    // İşlem yapılabilir, örneğin:
                    byte[] dataBytes = receivedData.ToArray();
                    nextTimer.Start();
                }
                else
                {
                    // Geçersiz veri, işlem yapılamaz
                    // Gerekirse bir hata mesajı gösterilebilir
                }
            }

            else if (stepState == 1)
            {
                logTut1(" ", "", "");
                logTut1("FTC GİRİŞ", "", "");
                logTut1(" ", "", "");

                if (receivedData.Count >= 6)
                {
                    if (receivedData[0] == 0x24 && receivedData[4] == 0x45 && receivedData[5] == 0xAF)
                    {
                        if ((receivedData[1] == 0x00 || receivedData[1] == 0x01) && receivedData[2] == 0x01 && receivedData[3] == 0x00)
                        {
                            if (receivedData[1] == 0x00)
                            {
                                Console.WriteLine("EnterFTC check-1");
                            }
                            else if (receivedData[1] == 0x01)
                            {
                                Console.WriteLine("EnterFTC check-2");

                                logTut1("FCT Giriş Testi ", ":Passed ", "OK" + " Dönüş");
                                ProcessFrm.ProcessSuccess(stepState);
                                nextTimer.Start();
                            }
                            else
                            {
                                Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                                logTut1("FCT GİRİŞ TESTİ ", ":Failed ", "NOK" + " Dönüş");
                                ProcessFrm.ProcessFailed(stepState);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                            logTut1("FCT GİRİŞ TESTİ ", ":Failed ", "NOK" + " Dönüş");
                            ProcessFrm.ProcessFailed(stepState);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                        logTut1("FCT GİRİŞ TESTİ ", ":Failed ", "NOK" + " Dönüş");
                        ProcessFrm.ProcessFailed(stepState);
                    }
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("FCT GİRİŞ TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 2)
            {
                logTut1(" ", "", "");
                logTut1("MEMORY BENCHMARK TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("MemoryBenchmark check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("MemoryBenchmark check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x0D)
                {
                    List<byte> jsonBytes = receivedData.GetRange(2, receivedData.Count - 3);

                    // Son bayrağı kaldır
                    byte[] jsonBytesArray = jsonBytes.ToArray();

                    // receivedBytes dizisini string bir ifadeye dönüştür
                    string json = Encoding.ASCII.GetString(jsonBytesArray);

                    string jsonData = json.Trim().Replace("\r", "").Replace("\0", "").Replace("$", "").TrimStart('"').TrimEnd('?');

                    Console.WriteLine("MemoryBenchmark: " + jsonData);
                }
                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("MemoryBenchmark check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("MemoryBenchmark check-5");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("MemoryBenchmark check-6");

                    logTut1("MEMORY BENCHMARK TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("MEMORY BENCHMARK TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 3)
            {
                logTut1(" ", "", "");
                logTut1("MEMORY ERROR CHECK TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("MemoryErrorCheck check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("MemoryErrorCheck check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("MemoryErrorCheck check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("MemoryErrorCheck check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("MemoryErrorCheck check-5");
                    logTut1("MEMORY ERROR CHECK TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);
                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("MEMORY ERROR CHECK TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 4)
            {
                logTut1(" ", "", "");
                logTut1("CPU BENCHMARK TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("CPUBenchmark check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("CPUBenchmark check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x0D)
                {
                    List<byte> jsonBytes = receivedData.GetRange(2, receivedData.Count - 3);
                    byte[] jsonBytesArray = jsonBytes.ToArray();
                    string json = Encoding.ASCII.GetString(jsonBytesArray);

                    string jsonData = json.Trim().Replace("\r", "").Replace("\0", "").Replace("#", "").TrimStart('"').Replace(".", "");

                    jsonData = json.TrimStart('/');

                    Console.WriteLine("CPUBenchmark: " + jsonData);

                }
                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("CPUBenchmark check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("CPUBenchmark check-5");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("CPUBenchmark check-6");
                    logTut1("CPU BENCHMARK TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);
                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("CPU BENCHMARK TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 5)
            {
                logTut1(" ", "", "");
                logTut1("MEMORY STRESS TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("CPUStress check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("CPUStress check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x0D)
                {
                    // Son bayrağı kaldır
                    receivedData.RemoveAt(receivedData.Count - 1);

                    byte[] receivedBytes = receivedData.ToArray();

                    // receivedBytes dizisini string bir ifadeye dönüştür
                    string json = Encoding.ASCII.GetString(receivedBytes);

                    string cleanedData = Regex.Replace(json, @"[\u0000\u001b\r\n\0]", "");

                    int jsonStartIndex = cleanedData.IndexOf('{');
                    int jsonEndIndex = cleanedData.LastIndexOf('}');
                    string jsonData1 = cleanedData.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);

                    Console.WriteLine("CPUStress: " + jsonData1);

                }
                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("CPUStress check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("CPUStress check-5");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("CPUStress check-6");
                    logTut1("CPU STRESS TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);
                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("CPU STRESS TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 6)
            {
                logTut1(" ", "", "");
                logTut1("CPU FPU TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("CPU FPU check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("CPU FPU check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("CPU FPU check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("CPU FPU check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("CPU FPU check-5");
                    logTut1("CPU FPU TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);
                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("CPU FPU TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 7)
            {
                logTut1(" ", "", "");
                logTut1("FLASH BENCHMARK TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("FlashBenchmark check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("FlashBenchmark check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x0D)
                {
                    // Son bayrağı kaldır
                    receivedData.RemoveAt(receivedData.Count - 1);

                    byte[] receivedBytes = receivedData.ToArray();

                    // receivedBytes dizisini string bir ifadeye dönüştür
                    string json = Encoding.ASCII.GetString(receivedBytes);

                    string cleanedData = Regex.Replace(json, @"[\u0000\u001b\r\n\0]", "");

                    int jsonStartIndex = cleanedData.IndexOf('{');
                    int jsonEndIndex = cleanedData.LastIndexOf('}');
                    string jsonData1 = cleanedData.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);

                    Console.WriteLine("FlashBenchmark: " + jsonData1);

                }
                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("FlashBenchmark check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("FlashBenchmark check-5");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("FlashBenchmark check-6");
                    logTut1("FLASH BENCHMARK TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);
                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("FLASH BENCHMARK TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 8)
            {
                logTut1(" ", "", "");
                logTut1("WİFİ TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("WiFi Test check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("WiFi Test check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x0D)
                {
                    // Son bayrağı kaldır
                    receivedData.RemoveAt(receivedData.Count - 1);

                    byte[] receivedBytes = receivedData.ToArray();

                    // receivedBytes dizisini string bir ifadeye dönüştür
                    string json = Encoding.ASCII.GetString(receivedBytes);

                    string cleanedData = Regex.Replace(json, @"[\u0000\u001b\r\n\0]", "");
                    cleanedData = json.Trim().Replace("\r", "").Replace("\0", "").Replace("?", "").Replace("\n", "").Replace("@", "").TrimStart('"');

                    int jsonStartIndex = cleanedData.IndexOf('{');
                    int jsonEndIndex = cleanedData.LastIndexOf('}');
                    string jsonData1 = cleanedData.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);

                    Console.WriteLine("WiFi Test: " + jsonData1);
                    JObject jsonObject = JObject.Parse(jsonData1);

                    string rssi_dbm = jsonObject["rssi_dbm"].ToString();
                    string avg_rtt_ms = jsonObject["avg_rtt_ms"].ToString();
                    string tcp_rcv_bitrate_kb = jsonObject["tcp_rcv_bitrate_kb"].ToString();
                    string udp_loss_ms = jsonObject["udp_loss_ms"].ToString();

                    Console.WriteLine("rssi_dbm: " + rssi_dbm); 
                    Console.WriteLine("avg_rtt_ms: " + avg_rtt_ms);
                    Console.WriteLine("tcp_rcv_bitrate_kb: " + tcp_rcv_bitrate_kb); 
                    Console.WriteLine("udp_loss_ms: " + udp_loss_ms);


                }
                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("WiFi Test check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("WiFi Test check-5");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("WiFi Test check-6");
                    logTut1("WİFİ TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);
                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("WİFİ TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 9)
            {
                logTut1(" ", "", "");
                logTut1("BLUETOOTH TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("Bluetooth Test check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("Bluetooth Test check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("Bluetooth Test check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("Bluetooth Test check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("Bluetooth Test check-5");
                    logTut1("BLUETOOTH TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);
                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("BLUETOOTH TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 10)
            {
                logTut1(" ", "", "");
                logTut1("HSM TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("HSM check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("HSM check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("HSM check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("HSM check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("HSM check -5");
                    logTut1("HSM TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);
                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("HSM TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 11)
            {
                logTut1(" ", "", "");
                logTut1("USB1 POWER ON TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("USB1PowOn check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("USB1PowOn check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("USB1PowOn check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("USB1PowOn check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("USB1PowOn check -5");
                    logTut1("USB1 POWER ON TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);
                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("USB1 POWER ON TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 12)
            {
                logTut1(" ", "", "");
                logTut1("USB1 POWER OFF TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("USB1PowOff check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("USB1PowOff check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("USB1PowOff check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("USB1PowOff check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("USB1PowOff check -5");
                    logTut1("USB POWER OFF TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("USB1 POWER OFF TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 13)
            {
                logTut1(" ", "", "");
                logTut1("USB1 SPEED TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("USB1 Speed Test check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("USB1 Speed Test check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x0D)
                {
                    // Son bayrağı kaldır
                    receivedData.RemoveAt(receivedData.Count - 1);

                    byte[] receivedBytes = receivedData.ToArray();

                    // receivedBytes dizisini string bir ifadeye dönüştür
                    string json = Encoding.ASCII.GetString(receivedBytes);

                    string cleanedData = Regex.Replace(json, @"[\u0000\u001b\r\n\0]", "");
                    cleanedData = json.Trim().Replace("\r", "").Replace("\0", "").Replace("?", "").Replace("\n", "").Replace("@", "").TrimStart('"');

                    int jsonStartIndex = cleanedData.IndexOf('{');
                    int jsonEndIndex = cleanedData.LastIndexOf('}');
                    string jsonData1 = cleanedData.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);

                    Console.WriteLine("WiFi Test: " + jsonData1);
                    JObject jsonObject = JObject.Parse(jsonData1);

                    string tcp_upload_bitrate_mbps = jsonObject["tcp_upload_bitrate_mbps"].ToString();
                    string tcp_download_bitrate_mbps = jsonObject["tcp_download_bitrate_mbps"].ToString();
                    string udp_packet_loss = jsonObject["udp_packet_loss"].ToString();

                    Console.WriteLine("tcp_upload_bitrate_mbps: " + tcp_upload_bitrate_mbps);
                    Console.WriteLine("tcp_download_bitrate_mbps: " + tcp_download_bitrate_mbps);
                    Console.WriteLine("udp_packet_loss: " + udp_packet_loss);

                }
                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("USB1 Speed Test check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("USB1 Speed Test check-5");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("USB1 Speed Test check-6");
                    logTut1("USB1 SPEED TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("USB1 SPEED TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }
            
            else if (stepState == 14)
            {
                logTut1(" ", "", "");
                logTut1("USB1 POWER ON TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("USB2PowOn check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("USB2PowOn check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("USB2PowOn check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("USB2PowOn check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("USB2PowOn check -5");

                    logTut1("USB2 POWER ON TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("USB2 POWER ON TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 15)
            {
                logTut1(" ", "", "");
                logTut1("USB2 POWER OFF TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("USB2PowOff check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("USB2PowOff check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("USB2PowOff check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("USB2PowOff check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("USB2PowOff check -5");

                    logTut1("USB2 POWER OFF TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("USB2 POWER OFF TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 16)
            {
                logTut1(" ", "", "");
                logTut1("USB2 SPEED TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("USB2 Speed Test check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("USB2 Speed Test check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x0D)
                {
                    // Son bayrağı kaldır
                    receivedData.RemoveAt(receivedData.Count - 1);

                    byte[] receivedBytes = receivedData.ToArray();

                    // receivedBytes dizisini string bir ifadeye dönüştür
                    string json = Encoding.ASCII.GetString(receivedBytes);

                    string cleanedData = Regex.Replace(json, @"[\u0000\u001b\r\n\0]", "");
                    cleanedData = json.Trim().Replace("\r", "").Replace("\0", "").Replace("?", "").Replace("\n", "").Replace("@", "").TrimStart('"');

                    int jsonStartIndex = cleanedData.IndexOf('{');
                    int jsonEndIndex = cleanedData.LastIndexOf('}');
                    string jsonData1 = cleanedData.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);

                    Console.WriteLine("WiFi Test: " + jsonData1);
                    JObject jsonObject = JObject.Parse(jsonData1);

                    string tcp_upload_bitrate_mbps = jsonObject["tcp_upload_bitrate_mbps"].ToString();
                    string tcp_download_bitrate_mbps = jsonObject["tcp_download_bitrate_mbps"].ToString();
                    string udp_packet_loss = jsonObject["udp_packet_loss"].ToString();

                    Console.WriteLine("tcp_upload_bitrate_mbps: " + tcp_upload_bitrate_mbps);
                    Console.WriteLine("tcp_download_bitrate_mbps: " + tcp_download_bitrate_mbps);
                    Console.WriteLine("udp_packet_loss: " + udp_packet_loss);
                }
                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("USB2 Speed Test check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("USB2 Speed Test check-5");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("USB2 Speed Test check-6");

                    logTut1("USB2 SPEED TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("USB2 SPEED TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 17)
            {
                logTut1(" ", "", "");
                logTut1("TOF POWER ON TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("TOFPowOn check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("TOFPowOn check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("TOFPowOn check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("TOFPowOn check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("TOFPowOn check -5");

                    logTut1("TOF POWER ON TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("TOF POWER ON TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 18)
            {
                logTut1(" ", "", "");
                logTut1("TOF POWER OFF TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("TOFPowOff check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("TOFPowOff check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("TOFPowOff check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("TOFPowOff check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("TOFPowOff check -5");

                    logTut1("TOF POWER OFF TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("TOF POWER OFF TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 19)
            {
                logTut1(" ", "", "");
                logTut1("TOF SHUT ON TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("TOFXShutOn check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("TOFXShutOn check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("TOFXShutOn check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("TOFXShutOn check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("TOFXShutOn check -5");

                    logTut1("TOF SHUT ON TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("TOF SHUT ON TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 20)
            {
                logTut1(" ", "", "");
                logTut1("TOF SHUT OFF TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("TOFXShutOff check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("TOFXShutOff check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("TOFXShutOff check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("TOFXShutOff check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("TOFXShutOff check -5");

                    logTut1("TOF SHUT OFF TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("TOF SHUT OFF TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

            else if (stepState == 21)
            {
                logTut1(" ", "", "");
                logTut1("TOF ÖLÇÜM TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData.Count >= 6 && receivedData[0] == 0x24)
                {

                    if (receivedData[1] == 0x00)
                    {
                        Console.WriteLine("TOFOlcumTest check-1");
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else if (receivedData[1] == 0x06)
                    {
                        Console.WriteLine("TOFOlcumTest check-2");
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else if (receivedData[1] == 0x0D)
                    {
                        // Son bayrağı kaldır
                        receivedData.RemoveAt(receivedData.Count - 1);

                        byte[] receivedBytes = receivedData.ToArray();

                        // receivedBytes dizisini string bir ifadeye dönüştür
                        string json = Encoding.ASCII.GetString(receivedBytes);

                        string cleanedData = Regex.Replace(json, @"[\u0012\u001b\r\n\0]", "");

                        int jsonStartIndex = cleanedData.IndexOf('{');
                        int jsonEndIndex = cleanedData.LastIndexOf('}');
                        string jsonData1 = cleanedData.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);

                        Console.WriteLine("Distance: " + jsonData1);
                    }
                    else if (receivedData[1] == 0x04)
                    {
                        Console.WriteLine("TOFOlcumTest check-4");
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else if (receivedData[1] == 0x01)
                    {
                        Console.WriteLine("TOFOlcumTest check-5");
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else if (receivedData[1] == 0x07)
                    {
                        Console.WriteLine("TOFOlcumTest check-6");
                        ModBusWriteSingleCoils(M4, true);

                        logTut1("TOF ÖLÇÜM TESTİ ", ":Passed ", "OK" + " Dönüş");
                        ProcessFrm.ProcessSuccess(stepState);

                        nextTimer.Start();
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else
                    {
                        Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                        logTut1("TOF ÖLÇÜM TESTİ ", ":Failed ", "NOK" + " Dönüş");
                        ProcessFrm.ProcessFailed(stepState);
                    }
                }
            }
            else if (stepState == 22)
            {
                logTut1(" ", "", "");
                logTut1("TOF ÖLÇÜM TESTİ-2", "", "");
                logTut1(" ", "", "");

                if (receivedData.Count >= 6 && receivedData[0] == 0x24)
                {

                    if (receivedData[1] == 0x00)
                    {
                        Console.WriteLine("TOFOlcumTest2 check-1");
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else if (receivedData[1] == 0x06)
                    {
                        Console.WriteLine("TOFOlcumTest2 check-2");
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else if (receivedData[1] == 0x0D)
                    {
                        // Son bayrağı kaldır
                        receivedData.RemoveAt(receivedData.Count - 1);

                        byte[] receivedBytes = receivedData.ToArray();

                        // receivedBytes dizisini string bir ifadeye dönüştür
                        string json = Encoding.ASCII.GetString(receivedBytes);

                        string cleanedData = Regex.Replace(json, @"[\u0012\u001b\r\n\0]", "");

                        int jsonStartIndex = cleanedData.IndexOf('{');
                        int jsonEndIndex = cleanedData.LastIndexOf('}');
                        string jsonData1 = cleanedData.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);

                        Console.WriteLine("Distance: " + jsonData1);
                    }
                    else if (receivedData[1] == 0x04)
                    {
                        Console.WriteLine("TOFOlcumTest2 check-4");
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else if (receivedData[1] == 0x01)
                    {
                        Console.WriteLine("TOFOlcumTest2 check-5");
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else if (receivedData[1] == 0x07)
                    {
                        Console.WriteLine("TOFOlcumTest2 check-5");

                        logTut1("TOF ÖLÇÜM TESTİ-2 ", ":Passed ", "OK" + " Dönüş");
                        ProcessFrm.ProcessSuccess(stepState);

                        nextTimer.Start();
                        // Burada ilgili işlemleri yapabilirsiniz
                    }
                    else
                    {
                        Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                        logTut1("TOF ÖLÇÜM TESTİ-2 ", ":Failed ", "NOK" + " Dönüş");
                        ProcessFrm.ProcessFailed(stepState);
                    }
                }
            }

            else if (stepState == 23)
            {
                logTut1(" ", "", "");
                logTut1("SPI TESTİ", "", "");
                logTut1(" ", "", "");

                if (receivedData[1] == 0x00)
                {
                    Console.WriteLine("SPI check-1");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x06)
                {
                    Console.WriteLine("SPI check-2");
                    // Burada ilgili işlemleri yapabilirsiniz
                }

                else if (receivedData[1] == 0x04)
                {
                    Console.WriteLine("SPI check-3");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x01)
                {
                    Console.WriteLine("SPI check-4");
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else if (receivedData[1] == 0x07)
                {
                    Console.WriteLine("SPI check -5");

                    logTut1("SPI TESTİ ", ":Passed ", "OK" + " Dönüş");
                    ProcessFrm.ProcessSuccess(stepState);

                    nextTimer.Start();
                    // Burada ilgili işlemleri yapabilirsiniz
                }
                else
                {
                    Console.WriteLine("Hata: Hiçbir koşul sağlanmadı");

                    logTut1("SPI TESTİ ", ":Failed ", "NOK" + " Dönüş");
                    ProcessFrm.ProcessFailed(stepState);
                }
            }

        }

        private void ProcessFCT1()
        {
            if (stepState == 1)
            {
                Console.WriteLine("**************************Enter FCT State******************************");

                byteArrayENT[0] = 0x42;
                byteArrayENT[1] = 0x00;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = 0x45;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 2)
            {
                Console.WriteLine("**************************Memory Benchmark State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_1;
                byteArrayENT[5] = 0xFA;

                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 3)
            {
                Console.WriteLine("**************************Memory Error Check State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_2;
                byteArrayENT[5] = 0xFA;

                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 4)
            {
                Console.WriteLine("**************************CPU Benchmark State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_3;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 5)
            {
                Console.WriteLine("**************************CPU Stress State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_4;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 6)
            {
                Console.WriteLine("**************************CPU FPU State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_5;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 7)
            {
                Console.WriteLine("**************************Flash Benchmark State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_6;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 8)
            {
                Console.WriteLine("**************************WiFi Control State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_8;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 9)
            {
                Console.WriteLine("**************************Bluetooth Control State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_9;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 10)
            {
                Console.WriteLine("**************************HSM State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_11;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 11)
            {
                Console.WriteLine("**************************USB1 PowOn State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_12;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 12)
            {
                Console.WriteLine("**************************USB1 PowOff State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_13;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 13)
            {
                Console.WriteLine("**************************USB1 Speed State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_14;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 14)
            {
                Console.WriteLine("**************************USB2 PowOn State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_15;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 15)
            {
                Console.WriteLine("**************************USB2 PowOff State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_16;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 16)
            {
                Console.WriteLine("**************************USB2 Speed State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_17;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 17)
            {
                Console.WriteLine("**************************TOF PowOn State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_18;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 18)
            {
                Console.WriteLine("**************************TOF PowOff State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_19;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 19)
            {
                Console.WriteLine("**************************TOF ShutOn State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_20;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 20)
            {
                Console.WriteLine("**************************TOF ShutOff State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_21;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 21)
            {
                Console.WriteLine("**************************TOF Ölçüm State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_22;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 22)
            {
                Console.WriteLine("**************************TOF Ölçüm State2******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_22;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 23)
            {
                ModBusWriteSingleCoils(M3, true);  // kart enerji aktif

                Console.WriteLine("**************************SPI Test State******************************");

                byteArrayENT[0] = (byte)PREAMBLEs.PRE_FTC_Client;
                byteArrayENT[1] = (byte)Transmiting_Events.CMD_StartFTC;
                byteArrayENT[2] = 0x01;
                byteArrayENT[3] = 0x00;
                byteArrayENT[4] = (byte)FTC_STEPS.FTC_STEP_23;
                byteArrayENT[5] = 0xFA;

                // byteArrayENT dizisini seri porta gönder
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(byteArrayENT, 0, byteArrayENT.Length);
                }
            }

            if (stepState == 24)
            {
                Console.WriteLine("**************************END State******************************");

                ModBusWriteSingleCoils(M9, true);
                Console.WriteLine("Test Bitti");
            }
        }

        private void serialWriteByte1()
        {
            try
            {
                for (int i = 0; i < byteLenght; i++)
                {
                    ConsoleAppendLine("' " + Convert.ToByte(byteArray[i]) + " '", Color.Orange);
                }
                ConsoleAppendLine("UDAQ1'e gitti.", Color.Orange);
                serialPort1.Write(byteArray, 0, byteLenght);
            }
            catch (Exception ex)
            {
                ConsoleAppendLine("serialWriteByte1: " + ex.Message, Color.Red);
            }
        }

        private void serialBufferClear()
        {
            for (int i = 0; i <= counterRxByte; i++)
            {
                arrayRx[i] = 0x0;
            }
            counterRxByte = 0;

            if (Ayarlar.Default.chBoxSerial1)
            {
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();
            }
        }

        /****************************************** MODBUS *************************************************/
        private bool ModBusReadCoils(int address, int length)
        {
            if (modbusClientPLC.Connected)
            {
                try
                {
                    return modbusClientPLC.ReadCoils(address, length)[0];
                }
                catch
                {
                    ConsoleAppendLine("ModBus Read Coil Hatası." + address, Color.Red);
                    return false;
                }
            }
            else
            {
                ConsoleAppendLine("ModBus Kapalı Hatası." + address, Color.Red);
                return false;
            }
        }

        private void ModBusWriteSingleCoils(int address, bool state)
        {
            if (modbusClientPLC.Connected)
            {
                try
                {
                    modbusClientPLC.WriteSingleCoil(address, state);
                }
                catch
                {
                    ConsoleAppendLine("ModBus WriteSingle Coil Hatası." + address, Color.Red);
                }
            }
            else
            {
                ConsoleAppendLine("ModBus Kapalı Hatası." + address, Color.Red);
            }
        }

        private int ModBusReadHoldingRegisters(int address, int length)
        {
            if (modbusClientPLC.Connected)
            {
                try
                {
                    return modbusClientPLC.ReadHoldingRegisters(address, length)[0];
                }
                catch
                {
                    ConsoleAppendLine("ModBus ReadHoldingRegisters Coil Hatası." + address, Color.Red);
                    return 0;
                }
            }
            else
            {
                ConsoleAppendLine("ModBus Kapalı Hatası." + address, Color.Red);
                return 0;
            }
        }

        private bool ModBusReadDiscreteInputs(int address, int length)
        {
            if (modbusClientPLC.Connected)
            {
                try
                {
                    return modbusClientPLC.ReadDiscreteInputs(address, length)[0];
                }
                catch
                {
                    ConsoleAppendLine("ModBus ReadDiscreteInputs Hatası." + address, Color.Red);
                    return false;
                }
            }
            else
            {
                ConsoleAppendLine("ModBus Kapalı Hatası." + address, Color.Red);
                return false;
            }
        }

        /****************************************** INIT *************************************************/
        private void tbBarcodeCurrent_TextChanged(object sender, EventArgs e)
        {
            int maxLenght = 50;

            string barcode = tbBarcodeCurrent.Text;
            if (Convert.ToInt32(barcode.Length) >= maxLenght)
            {
                barcodeCounter++;
                if (ProgramlamaFrm.BarcodeControl(tbBarcodeCurrent.Text))
                {
                    barcode50[barcodeCounter] = tbBarcodeLast.Text;
                    btnFCTInit.BackColor = Color.Green;
                    btnFCTInit.Text = barcodeCounter + ".KART EKLENDİ!";
                }
                else
                {
                    barcodeCounter--;
                    btnFCTInit.BackColor = Color.Red;
                    btnFCTInit.Text = barcodeCounter + ".KART EKLENEMEDİ!";
                }

                if (barcodeCounter == FCT_CARD_NUMBER)
                {
                    btnFCTInit.BackColor = Color.Green;
                    btnFCTInit.Text = "BUTONLARA BASARAK FCT TESTİNİ BAŞLAT";
                    timerInit.Start();
                }
            }
        }

        private void timerInit_Tick(object sender, EventArgs e)
        {
            if (ModBusReadDiscreteInputs(X4, 1) && cardFailed == false)  //Güvenlik Biti ve Piston Aşağıda ise
            {
                timerInit.Stop();
                timerInit.Enabled = false;
                lblTimer1.BackColor = Color.Transparent;
                lblTimer1.Text = "OFF";
                timerEmergencyStop.Start();
                lblTimer2.BackColor = Color.Green;
                lblTimer2.Text = "ON";
                FCTInit();
            }
        }

        private void timerEmergencyStop_Tick_1(object sender, EventArgs e)
        {
            if (ModBusReadDiscreteInputs(X0, 1) && ModBusReadDiscreteInputs(X4, 1) == false)  //Acil Basıldı ve Piston Yukarıda ise
            {
                timerEmergencyStop.Stop();
                timerEmergencyStop.Enabled = false;
                lblTimer2.BackColor = Color.Transparent;
                lblTimer2.Text = "OFF";
                FCT_Fail();
            }
        }

        private void nextTimer_Tick(object sender, EventArgs e)
        {
            nextTimer.Stop();
            nextTimer.Enabled = false;
            stepState++;
            ProcessFCT1();
        }

        private void FCTInit()
        {
            if (serialPort1.IsOpen && modbusClientPLC.Connected)
            {
                saniyeState = true;
                Thread.Sleep(500);
                for (int i = 1; i <= FCT_CARD_NUMBER; i++)
                {
                    textCreate(i);
                    Thread.Sleep(200);
                }
                serialBufferClear();
                urunTestlerInsert();   //Teste Girdim
                Thread.Sleep(1000);
                btnFCTInit.BackColor = Color.Green;
                btnFCTInit.Text = "TEST BAŞLADI";
                ModBusWriteSingleCoils(M1, true);
                //nextTimer.Start();
            }
            else
            {
                CustomMessageBox.ShowMessage("Serial Bağlantısını Kontrol Ediniz!", customMessageBoxTitle, MessageBoxButtons.OK, CustomMessageBoxIcon.Information, Color.Red);
                ProcessFrm.ProcessFailed(1);
            }
        }

        /****************************************************FCT*******************************************************************/
        public void textCreate(int barcodeNum)
        {
            try
            {
                DateTime dt = DateTime.Now;
                string nowYear = Convert.ToString(dt.Year);
                string nowMonth = Convert.ToString(dt.Month);
                string nowDay = Convert.ToString(dt.Day);
                string nowHour = Convert.ToString(dt.Hour);
                string nowMinute = Convert.ToString(dt.Minute);
                string nowSecond = Convert.ToString(dt.Second);
                string name = barcode50[barcodeNum] + "-" + nowYear + "-" + nowMonth + "-" + nowDay + "-" + nowHour + "-" + nowMinute + "-" + nowSecond;
                filePathTxt[barcodeNum] = logDosyaPath + "//" + name + ".txt"; //
                StreamWriter FileWrite = new StreamWriter(filePathTxt[barcodeNum]);
                FileWrite.Close();
            }
            catch (Exception ex)
            {
                ConsoleAppendLine("textCreate: " + ex.Message, Color.Red);
            }
        }

        public bool cardFailed = false;

        private void FCT_Success()
        {

            if (urunlerUpdate())
            {
                fctGenelInsert("1");
                // printerFunction(barcode50[1]);
                cardFailed = false;
                FCT_Finish();
                ModBusWriteSingleCoils(M21, true);     // PLC Herşey RESET
                CustomMessageBox.ShowMessage("FCT Testi Sonlandı. Lütfen Tekrar Başlayın!", customMessageBoxTitle, MessageBoxButtons.OK, CustomMessageBoxIcon.Information, Color.Green);
                componentsClear();
                ProcessFrm.Process_Clear();
            }
            else
            {
                ConsoleAppendLine("SQL LAST WRITE PROBLEM", Color.Red);
                FCT_Fail();
            }
        }

        public void FCT_Fail()
        {
            fctGenelInsert("0");
            cardFailed = true;
            errorCard = errorCard + FCT_CARD_NUMBER;
            errorCardTxt.Text = Convert.ToString(errorCard);

            Thread.Sleep(500);
            byteArray[0] = 0x08; // İkinci Start Komutunu Gönderme
            byteArray[1] = 0x00;
            byteArray[2] = 0x00;
            byteArray[3] = 0x02;
            byteArray[4] = 0xF5;
            byteLenght = 5;
            serialWriteByte1();
            Thread.Sleep(500);
            logTut1(" ", "", "");
            logTut1("Flash Yazma 1.Kademe ", ":Failed ", arrayRx[0] + "-" + arrayRx[1] + " Dönüş");
            Thread.Sleep(500);
            byteArray[0] = 0x08; // İkinci Start Komutunu Gönderme
            byteArray[1] = 0x00;
            byteArray[2] = 0x01;
            byteArray[3] = 0xFD;
            byteArray[4] = 0xF9;
            byteLenght = 5;
            serialWriteByte1();
            Thread.Sleep(500);
            logTut1("Flash Yazma 2.Kademe ", ":Failed ", arrayRx[0] + "-" + arrayRx[1] + " Dönüş");
            Thread.Sleep(3000);
            FCT_Finish();
            CustomMessageBox.ShowMessage("FCT Testi Başarısız Sonlandı. Lütfen Tekrar Başlayın!", customMessageBoxTitle, MessageBoxButtons.OK, CustomMessageBoxIcon.Information, Color.Red);
            cardFailed = false;
            ModBusWriteSingleCoils(M21, true);     // PLC Herşey RESET
            componentsClear();
            ProcessFrm.Process_Clear();
        }

        public void FCT_Finish()
        {
            FCT_Clear();
            Verim();
        }

        private void FCT_Clear()
        {
            timersClear();
            variablesClear();
        }

        private void timersClear()
        {
            nextTimer.Stop();
            nextTimer.Enabled = false;
            timerAdmin.Stop();
            timerAdmin.Enabled = false;
            serialRxTimeout.Stop();
            serialRxTimeout.Enabled = false;
            timerEmergencyStop.Stop();
            timerEmergencyStop.Enabled = false;
            lblTimer2.BackColor = Color.Transparent;
            lblTimer2.Text = "OFF";
            timerInit.Start();
            lblTimer1.BackColor = Color.Green;
            lblTimer1.Text = "ON";
        }

        private void variablesClear()
        {
            saniyeState = false;
            stepState = 0;
            stepOut = 0;
            feedbackType = 0;
            adminTimerCounter = 0;
            timeoutTimerCounter = 0;
            saniyeTimerCounter = 0;
            fctSaniye = 0;

            yetki = 0;
            barcodeCounter = 0;
            for (int i = 1; i <= FCT_CARD_NUMBER; i++)
            {
                filePathTxt[i] = "";
                barcode50[i] = "";
                cardResult[i] = true;
                sap_no[i] = "";
            }
        }

        private void componentsClear()
        {
            btnFCTInit.BackColor = Color.Yellow;
            btnFCTInit.Text = "KART EKLEMEYE BAŞLAYABİLİRSİNİZ";
            progressBarFCT.Value = 0;
        }

        private void Verim()
        {
            totalCard = totalCard + FCT_CARD_NUMBER;
            totalCardTxt.Text = Convert.ToString(totalCard);
            verimTxt.Text = Convert.ToString(100 - ((float)((float)errorCard / totalCard)) * 100);
        }
        /***************************************************PRINTER***************************************************************/
        private void printerFunction(object data)  //PRINTER AKSİYON
        {
            try
            {
                string fullproductCode = (string)data;
                string company_no = string.Empty;
                string sap_no = string.Empty;
                string mamul_no = string.Empty;
                string product_date = string.Empty;
                string index_no = string.Empty;
                string index_no_new = string.Empty;
                string product_no = string.Empty;
                string card_type = string.Empty;
                string gerber_ver = string.Empty;
                string bom_ver = string.Empty;
                string hardware_ver = string.Empty;
                string hardware_rev = string.Empty;
                string software_ver = string.Empty;
                string software_rev = string.Empty;
                string lot_number = string.Empty;
                string FCTDay = string.Empty;
                string FCTMonth = string.Empty;
                string FCTYear = string.Empty;

                company_no = fullproductCode.Substring(0, 2);
                sap_no = fullproductCode.Substring(2, 10);
                mamul_no = sap_no.Substring(4, 6);  //OK
                product_date = fullproductCode.Substring(12, 4); //OK
                index_no = fullproductCode.Substring(16, 6);
                index_no_new = index_no.Substring(0, 4) + "-" + index_no.Substring(5, 1); //OK
                product_no = fullproductCode.Substring(22, 14);
                card_type = fullproductCode.Substring(36, 2);
                gerber_ver = fullproductCode.Substring(38, 2);
                bom_ver = fullproductCode.Substring(40, 2);
                hardware_ver = fullproductCode.Substring(42, 2); //OK
                hardware_rev = fullproductCode.Substring(44, 2); //OK
                software_ver = fullproductCode.Substring(46, 2); //OK
                software_rev = fullproductCode.Substring(48, 2); //OK
                lot_number = Prog_Ayarlar.Default.companyNo; ;   //OK

                DateTime dt = DateTime.Now;
                FCTYear = Convert.ToString(dt.Year);
                FCTMonth = Convert.ToString(dt.Month);
                FCTDay = Convert.ToString(dt.Day);
                if (FCTMonth.Length < 2)
                {
                    FCTMonth = "0" + FCTMonth;
                }
                if (FCTDay.Length < 2)
                {
                    FCTDay = "0" + FCTDay;
                }

                string testStr = "";
                string start = "^XA" + "^LH" + Ayarlar.Default.printerPos; // (5,10)
                string qr = "^FO150,37" + "^BQN,2,2" + "^FDQA" + " " + company_no + sap_no + product_date + index_no +
                product_no + card_type + gerber_ver + bom_ver + hardware_ver + hardware_rev + software_ver + software_rev + "^FS";
                string s1 = "^FO10,10^A0,17,17^FDALPPLAS" + "                " + "NX-E7S" + "^FS";
                string s2 = "^FO10,40^A0,16,16^FDY/N:" + software_ver + "." + software_rev + "^FS"; //
                string s3 = "^FO150,27^A0,16,16^FDY/N:" + software_ver + "." + software_rev + "^FS"; // 
                string s4 = "^FO10,60^A0,16,16^FDREV:" + "G:" + gerber_ver + " " + "B:" + bom_ver + " " + "D:01"; //
                string s5 = "^FO10,80" + "^A0,16,16" + "^FDP/N:" + sap_no + "^FS"; //
                string s6 = "^FO10,100" + "^A0,16,16" + "^FDS/N:" + index_no + "^FS"; //
                string s7 = "^FO150,110" + "^A0,14,14" + "^FDVDE:Type-" + "^FS";
                string s8 = "^FO150,125" + "^A0,14,14" + "^FDUL:" + "^FS";
                string s9 = "^FO175,138" + "^A0,12,12" + "^FDROHS" + "^FS";
                string end = "^XZ";
                testStr = start + qr + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8 + s9 + end;

                //Get local print server
                var server = new LocalPrintServer();

                //Load queue for correct printer
                PrintQueue pq = server.GetPrintQueue(Ayarlar.Default.printerName, new string[0] { });
                PrintJobInfoCollection jobs = pq.GetPrintJobInfoCollection();
                foreach (PrintSystemJobInfo job in jobs)
                {
                    job.Cancel();
                }

                if (!RawPrinterHelper.SendStringToPrinter(Ayarlar.Default.printerName, testStr))
                {
                    ConsoleAppendLine("Printer Error1: ", Color.Red);
                }
            }
            catch (Exception ex)
            {
                ConsoleAppendLine("Printer Error2: " + ex.Message, Color.Red);
            }
        }
        /****************************************************LOG*******************************************************************/
        private void logTut1(string testName, string testResult, string testState)
        {
            try
            {
                if (logDosyaPath != "")
                {
                    List<string> lines = new List<string>();
                    lines = File.ReadAllLines(filePathTxt[1]).ToList();
                    lines.Add(testName + testResult + testState);
                    ConsoleAppendLine(testName + testResult + testState + "Eklendi", Color.Green);
                    File.WriteAllLines(filePathTxt[1], lines);
                }
                else
                {
                    ConsoleAppendLine("Dosya Yolu Boş Kalamaz", Color.Red);
                }
            }
            catch (Exception ex)
            {
                ConsoleAppendLine("sqlTextYaz: " + ex.Message, Color.Red);
            }
        }

        /****************************************************CONSOLE TEXT*******************************************************************/
        private void rtbConsole_TextChanged(object sender, EventArgs e)
        {
            RichTextBox rtb = sender as RichTextBox;
            rtb.SelectionStart = rtb.Text.Length;
            rtb.ScrollToCaret();
        }

        /*Kullanıcı Arayüzüne Yazı Yazılır*/
        public void ConsoleAppendLine(string text, Color color)
        {
            if (rtbConsole.InvokeRequired)
            {
                rtbConsole.Invoke(new Action(delegate ()
                {
                    rtbConsole.Select(rtbConsole.TextLength, 0);
                    rtbConsole.SelectionColor = color;
                    rtbConsole.AppendText(text + Environment.NewLine);
                    rtbConsole.Select(rtbConsole.TextLength, 0);
                    rtbConsole.SelectionColor = Color.White;
                }));
            }
            else
            {
                rtbConsole.Select(rtbConsole.TextLength, 0);
                rtbConsole.SelectionColor = color;
                rtbConsole.AppendText(text + Environment.NewLine);
                rtbConsole.Select(rtbConsole.TextLength, 0);
                rtbConsole.SelectionColor = Color.White;
            }
        }

        /*Kullanıcı Arayüzünde Bir Satır Boşluk Bırakılır*/
        public void ConsoleNewLine()
        {
            if (rtbConsole.InvokeRequired)
            {
                rtbConsole.Invoke(new Action(delegate ()
                {
                    rtbConsole.AppendText(Environment.NewLine);
                }));
            }
            else
            {
                rtbConsole.AppendText(Environment.NewLine);
            }
        }

        public void ConsoleClean()
        {
            if (rtbConsole.InvokeRequired)
            {
                rtbConsole.Invoke(new Action(delegate ()
                {
                    rtbConsole.Text = "";
                    rtbConsole.Select(rtbConsole.TextLength, 0);
                    rtbConsole.SelectionColor = Color.White;
                }));
            }
            else
            {
                rtbConsole.Text = "";
                rtbConsole.Select(rtbConsole.TextLength, 0);
                rtbConsole.SelectionColor = Color.White;
            }
        }

        /****************************************************PAGE CHANGE*******************************************************************/
        private void btnCikis_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAyar_Click(object sender, EventArgs e)
        {
            int num = (int)this.AyarFrm.ShowDialog();
        }

        private void btnProgAyar_Click(object sender, EventArgs e)
        {
            int num = (int)this.ProgAyarFrm.ShowDialog();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.L)
                return false;
            if (this.yetki != 0)
            {
                timerAdmin.Stop();
                this.yetki = 0;
                this.yetkidegistir();
            }
            else
            {
                try { int num = (int)this.SifreFrm.ShowDialog(); }
                catch (Exception) { }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void yetkidegistir()
        {
            if (this.yetki == 0)
            {
                this.btnCikis.Enabled = false;
                this.btnAyar.Enabled = false;
                this.btnProgAyar.Enabled = false;

                this.btnCikis.BackColor = Color.Beige;
                this.btnAyar.BackColor = Color.Beige;
                this.btnProgAyar.BackColor = Color.Beige;
            }
            if (this.yetki == 1)
            {
                this.btnCikis.Enabled = true;
                this.btnAyar.Enabled = true;
                this.btnProgAyar.Enabled = true;

                this.btnCikis.BackColor = Color.Red;
                this.btnAyar.BackColor = Color.Red;
                this.btnProgAyar.BackColor = Color.Red;
                timerAdmin.Start();
            }
            if (this.yetki == 2)
            {
                this.btnCikis.Enabled = true;
                this.btnCikis.BackColor = Color.Red;
                this.btnAyar.BackColor = Color.Beige;
                this.btnProgAyar.BackColor = Color.Beige;
                timerAdmin.Start();
            }
        }

        /****************************************************TIMERS*******************************************************************/
        private void timerAdmin_Tick_1(object sender, EventArgs e)
        {
            adminTimerCounter++;
            if (adminTimerCounter == 1)
            {
                adminTimerCounter = 0;
                timerAdmin.Stop();
                this.yetki = 0;
                this.yetkidegistir();
            }
        }

        private void serialRxTimeout_Tick(object sender, EventArgs e)
        {
            timeoutTimerCounter++;
            if (timeoutTimerCounter == 1)
            {
                ConsoleAppendLine("TIMEOUT_RX", Color.Red);
                timeoutTimerCounter = 0;
                serialRxTimeout.Stop();
                serialRxTimeout.Enabled = false;
                ProcessFrm.ProcessFailed(stepState);
            }
        }

        private void saniyeTimer_Tick(object sender, EventArgs e)
        {
            saniyeTimerCounter++;
            if (saniyeTimerCounter == 1)
            {
                saniyeTimerCounter = 0;
                fctTimerTxt.Text = Convert.ToString(++fctSaniye);
            }
        }

        bool saniyeState = false;
        int second = 0;
        int oldSecond = 0;
        private void saniyeThreadFunc()
        {
            for (; ; )
            {
                if (saniyeState)
                {
                    DateTime dt = DateTime.Now;
                    second = dt.Second;
                    if (second != oldSecond)
                    {
                        oldSecond = second;
                        // fctTimerTxt.Text = Convert.ToString(++fctSaniye);   // Teslimde Aç 
                    }
                    Thread.Sleep(1);
                }
            }
        }

        private void cardPicture_Click(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }
}

