using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace MultiLoader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public string[] devName = { "" };
        string spRead;
       // int ReadBytesLastCycle = 0;
       // int ReadDataCRCOk;
        int ResponseBytes = 9;
        bool ResendMessageFlag = false;
        bool SendMessageFlag = true;
        bool FlashEraseFlag = false;
        bool OpenButtonPressed = false;
        bool SearchDevicesFlag = false;
        byte LastAskedRegister = 0;
        int actualBoot = 11;//17 - на данный момент//actualBoot = DeviceType*10+ProgramBuild;эту переменную мы получим из выбранной прошивки. В случае, если перменная больше, чем расчитанное аналогичным образом число для опрашиваемого устройства, предлагаем прошить (помечаем галкой) устройство.
        int devicesChecked = 0;
        int crcErrors = 0;
        int ProgrammingMode = 0;
        int timeoutErrors = 0;
        int flashCRC = 0;
        string devices_programmed;
        byte[] programmed_devices = { 0 };
        string[] lines;
        int linePointer = 0;
        int ProgrammingFinishedFlag = 0;
        byte[] example =    {
                            0x0C, 0x94, 0x46, 0x02, 0x0C, 0x94, 0x00, 0x00, 0x0C, 0x94, 0x00, 0x00, 0x0C, 0x94, 0x00, 0x00, 
                            0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00, 
                            0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x45 , 0x0B , 0x0C , 0x94 , 0x00 , 0x00, 
                            0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00, 
                            0x0C , 0x94 , 0x29 , 0x07 , 0x0C , 0x94 , 0x50 , 0x07 , 0x0C , 0x94 , 0xBD , 0x02 , 0x0C , 0x94 , 0x00 , 0x00, 
                            0x0C , 0x94 , 0xF6 , 0x06 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 ,
                            0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 ,
                            0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x22 , 0x0B , 0x0C , 0x94 , 0x79 , 0x02 , 0x0C , 0x94 , 0x00 , 0x00 ,
                            0x0C , 0x94 , 0xC4 , 0x06 , 0x0C , 0x94 , 0x00 , 0x00 , 0x0C , 0x94 , 0x00 , 0x00 , 0x10 , 0x27 , 0xE8 , 0x03 ,
                            0x64 , 0x00 , 0x0A , 0x00 , 0x01 , 0x00 , 0x00 , 0x10 , 0x00 , 0x01 , 0x10 , 0x00 , 0x01 , 0x00 , 0x00 , 0x00 ,
                            0x01 , 0x00 , 0x01 , 0x00 , 0x01 , 0x00 , 0x01 , 0x00 , 0x1E , 0x00 , 0x28 , 0x00 , 0x1E , 0x00 , 0x03 , 0x00 ,
                            0x01 , 0x00 , 0x91 , 0x00 , 0x00 , 0x00 , 0xC0 , 0xC1 , 0xC1 , 0x81 , 0x01 , 0x40 , 0xC3 , 0x01 , 0x03 , 0xC0 ,
                            0x02 , 0x80 , 0xC2 , 0x41 , 0xC6 , 0x01 , 0x06 , 0xC0 , 0x07 , 0x80 , 0xC7 , 0x41 , 0x05 , 0x00 , 0xC5 , 0xC1 ,
                            0xC4 , 0x81 , 0x04 , 0x40 , 0xCC , 0x01 , 0x0C , 0xC0 , 0x0D , 0x80 , 0xCD , 0x41 , 0x0F , 0x00 , 0xCF , 0xC1 ,
                            0xCE , 0x81 , 0x0E , 0x40 , 0x0A , 0x00 , 0xCA , 0xC1 , 0xCB , 0x81 , 0x0B , 0x40 , 0xC9 , 0x01 , 0x09 , 0xC0 ,
                            0x08 , 0x80 , 0xC8 , 0x41 , 0xD8 , 0x01 , 0x18 , 0xC0 , 0x19 , 0x80 , 0xD9 , 0x41 , 0x1B , 0x00 , 0xDB , 0xC1 ,
                            };
        byte[] flashPages;
        byte[] DevicesToProgramming;//здесь хранятся адреса устройств, которые можно прошить
        int flashPagePointer = 0;
        //byte[] flashPageOverflow = new byte[16];
        delegate void SetTextCallback(string text);
        private Thread demoThread = null;
        byte[] ArrayOfModbusAddresses = {32};
        private void button2_Click(object sender, EventArgs e)
        {
           // for (int l = 0; l < ModBusRTU.FileOfBytes.Length; l++) ModBusRTU.FileOfBytes[l] = 0;
            flashPagePointer = 0;
            Array.Resize(ref ModBusRTU.FileOfBytes, 1);
            this.openFileDialog1.ShowDialog();
            this.textBox2.Text = this.openFileDialog1.FileName;
            lines = System.IO.File.ReadAllLines(this.openFileDialog1.FileName);// считываем файл в массив строк типа string
            //byte[] tmp_arr = HexToByte(lines[11]);

            //this.openFileDialog1.OpenFile(this.textBox2.Text);
            //System.IO.StreamReader sr = new
            //System.IO.StreamReader(this.openFileDialog1.FileName);
            //byte[] HexFile = new byte[]StreamReader(this.openFileDialog1.FileName);
            flashPages = CreateFlashPages();
            while (flashPagePointer * 256 < flashPages.Length)//шлем флеш-страницы
            {
                byte[] tmp = new byte[256];
                for (int i = 0; i < 256; i++)
                tmp[i] = flashPages[flashPagePointer * 256 + i];
                byte[] tmp_mBus = ModBusRTU.GenerateModBusWriteMessage(tmp);//генерируем сообщение ModBus
                flashPagePointer++;
            }
            int j=0;
            while (ModBusRTU.FileOfBytes[j] != 0x91) j++;
            actualBoot = ModBusRTU.FileOfBytes[j];
            string tmp_str;
            if ((ModBusRTU.FileOfBytes[j+1]==0x34)&(((actualBoot & 0x80) >> 7) == 1))
            {
                tmp_str = "A.";
                tmp_str += ((actualBoot & 0x70) >> 4).ToString() + ".";
                tmp_str += (actualBoot & 0x0f).ToString();
                this.textBox7.Text = tmp_str;
                flashCRC = ModBusRTU.CRC16(ModBusRTU.FileOfBytes);
                this.textBox3.Text = flashCRC.ToString();
                this.textBox4.Text = ByteToHex(ModBusRTU.FileOfBytes).ToString();

            }
            else
            {
                string caption = "ошибка загрузки";
                //MessageBoxButtons buttons = MessageBoxButtons.OK;
                tmp_str = "ошибка при идентификации версии прошивки, файл неверный, либо поврежден";
                MessageBox.Show(tmp_str, caption);
                this.textBox2.Clear();
                textBox7.Clear();
              //  for (int l = 0; l < ModBusRTU.FileOfBytes.Length; l++) ModBusRTU.FileOfBytes[l] = 0;
                flashPagePointer = 0;
                Array.Resize(ref ModBusRTU.FileOfBytes, 1);
            }
            
        }
        #region ByteToHex
        /// <summary>
        /// method to convert a byte array into a hex string
        /// </summary>
        /// <param name="comByte">byte array to convert</param>
        /// <returns>a hex string</returns>
        private string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
        }
        #endregion
        #region HexToByte
        /// <summary>
        /// method to convert hex string into a byte array
        /// </summary>
        /// <param name="msg">string to convert</param>
        /// <returns>a byte array</returns>
        private byte[] HexToByte(string msg)
        {
            //remove any spaces from the string
            msg = msg.Replace(" ", "");
            msg = msg.Replace(":", "");
            msg = msg.Replace(".", "");
            msg = msg.Replace("\r\n", "");
            //create a byte array the length of the
            //string divided by 2
            byte[] comBuffer = new byte[msg.Length / 2];
            //loop through the length of the provided string
            for (int i = 0; i < msg.Length; i += 2)
                //convert each set of 2 characters to a byte
                //and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            //return the array
            //byte[] buf2 = HartProtocol.AppendCRC(comBuffer);
            return comBuffer;
        }
        #endregion

        
        private void SendMBusMessage()
        {
           // this.textBox3.AppendText(spError.ToString());
            progressBar1.Style = ProgressBarStyle.Continuous;
            if (SearchDevicesFlag)
            {
                if (LastAskedRegister == 0) this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " Searching available devices...\r\n");
                if (AddDeviceToList() == 1)
                {
                    //AddDeviceToList();
                    byte[] tmp_mBus = ModBusRTU.GenerateModbusReadSingleRegisterMessage(LastAskedRegister);
                    this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + ByteToHex(tmp_mBus)+"\r\n");
                    //this.serialPort1.ReceivedBytesThreshold = ResponseBytes - 1;
                    this.serialPort1.Write(tmp_mBus, 0, tmp_mBus.Length);
                    ModBusRTU.ModBusAddress++;
                    progressBar1.Value++;
                }
                else
                {
                    LastAskedRegister = 0;
                    timer1.Stop();
                    devicesChecked = 0;
                    byte tmp_val=0;
                    for (int i = 0; i < ModBusRTU.DevicesAddressArray.Length - 1; i++)
                    {
                        //tmp_val=0;
                        if (actualBoot > (ModBusRTU.DevicesTypeArray[i] * 10 + ModBusRTU.DevicesProgramBuildArray[i]))
                        {
                         
                            if ((ModBusRTU.DevicesTypeArray[i] * 10 + ModBusRTU.DevicesProgramBuildArray[i]) == 0) CreateNewDevice(i, Color.Red, false);
                            else
                            {
                                CreateNewDevice(i, Color.Black, true);
                                tmp_val = ModBusRTU.DevicesAddressArray[i];
                            }
                         
                            devicesChecked++;
                        }
                        else
                        {
                            CreateNewDevice(i, Color.Blue, false);
                            tmp_val = ModBusRTU.DevicesAddressArray[i];
                        }
                   //     Array.Resize(ref DevicesToProgramming, DevicesToProgramming.Length + 1);
                   //     DevicesToProgramming[DevicesToProgramming.Length] = tmp_val;
 
                    }
                    string message="Устройств найдено: "+(ModBusRTU.DevicesAddressArray.Length-1).ToString()+"\r\n";
                    message += "устройств с устаревшей прошивкой: " + devicesChecked.ToString();
                    string caption = "результаты поиска";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    //DialogResult result;

                    // Displays the MessageBox.

                    MessageBox.Show(message, caption, buttons);
                    this.numericUpDown1.Enabled = true;
                    this.numericUpDown2.Enabled = true;
                    if(this.checkedListBox1.Items.Count!=0)checkedListBox1.SelectedIndex = 0;
                    progressBar1.Value = 0;
                   //this.textBox4.AppendText(ModBusRTU.mode.ToString() + SearchDevicesFlag.ToString() + ResponseBytes.ToString() + FlashEraseFlag.ToString() + ProgrammingFinishedFlag.ToString() + serialPort1.ReadExisting());
                }
            }
            else
            {
                if (FlashEraseFlag == true)//если отправлено сообщение стирания флеш-памяти устройства
                {
                    this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " Erasing device's flash now\r\n");
                    ResponseBytes = 8;
                    byte[] tmp_mBus = ModBusRTU.GenerateEraseFlashMessage();//генерируем сообщение ModBus
                    this.serialPort1.ReceivedBytesThreshold = ResponseBytes - ProgrammingFinishedFlag;
                    progressBar1.Style = ProgressBarStyle.Marquee;
                    serialPort1.Write(tmp_mBus, 0, tmp_mBus.Length);
                    
                }
                else
                {
                    if (flashPagePointer * 256 < flashPages.Length)//шлем флеш-страницы
                    {
                      
                        ResponseBytes = 9;
                        //this.timer1.Start();//запуск таймера, отслеживающего ошибку по таймауту
                        //System.Threading.Thread.Sleep(200);
                        //System.Threading.Thread.SpinWait(100000000);
                        //byte[] tmp = HexToByte(lines[linePointer]);//преобразуем строку, соответствующую строчному указателю в массив байт
                        byte[] tmp = new byte[256];
                        for (int i = 0; i < 256; i++)
                            tmp[i] = flashPages[flashPagePointer * 256 + i];

                        progressBar1.Value = flashPagePointer + 1;

                        // if ((SendMessageFlag)&(!ResendMessageFlag)) linePointer++;//наращиваем указатель
                        if (((crcErrors >= 5) || (timeoutErrors >= 5)) && (ResendMessageFlag))
                        {
                            //linePointer = lines.Length;
                            flashPagePointer = flashPages.Length / 256;
                            this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " Connection error, programming aborted\r\n");
                        }
                        //this.textBox5.Text = ByteToHex(tmp);
                        //this.textBox4.Text = tmp.Length.ToString();
                        byte[] tmp_mBus = ModBusRTU.GenerateModBusWriteMessage(tmp);//генерируем сообщение ModBus
                        // serialPort1.DiscardInBuffer();
                        // this.textBox6.AppendText();
                        // this.textBox6.AppendText(DateTime.Now.ToString() + " ---> " + ByteToHex(tmp_mBus) + " \r\n");
                        this.serialPort1.ReceivedBytesThreshold = ResponseBytes - ProgrammingFinishedFlag;
                        this.textBox1.AppendText(ByteToHex(tmp_mBus));
                        serialPort1.Write(tmp_mBus, 0, tmp_mBus.Length);//this.ByteToHex(this.HexToByte(text)));//,0,(int)fileStream.Length);



                        //serialPort1.DiscardOutBuffer();
                    }
                    else
                    {

                        if (ProgrammingFinishedFlag == 1)//если программирование завершено (сообщение завершения отправлено)
                        {
                            this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " programming finished\r\n");
                            timer1.Stop();
                           // programmed_devices[programmed_devices.Length - 1] = ModBusRTU.ModBusAddress;
                            devicesChecked--;
                            devices_programmed +="\r\n"+ ModBusRTU.ModBusAddress.ToString()+"\r\n";
                            //this.checkedListBox1.SetItemChecked(this.checkedListBox1.SelectedIndex, false);
                          //  this.textBox5.AppendText(                 ModBusRTU.ModBusAddress.ToString());
                            
                            //if (ProgrammingFinishedFlag == 1)
                           // {
                            if (devicesChecked > 0)
                            {
                             //   Array.Resize(ref programmed_devices, programmed_devices.Length + 1);
                                if (this.checkedListBox1.GetItemChecked(checkedListBox1.SelectedIndex))
                                {
                                    ResponseBytes = 9;
                                    flashPagePointer = 0;
                                    ProgrammingFinishedFlag = 0;
                                    checkedListBox1.SelectedIndex++;
                                    ModBusRTU.mode = 1;
                                    ModBusRTU.ModBusAddress = ModBusRTU.DevicesAddressArray[this.checkedListBox1.SelectedIndex];
                                    this.textBox5.AppendText(this.checkedListBox1.SelectedIndex.ToString());
                                    this.timer1.Interval = 500;
                                    timer1.Start();
                                    SendMBusMessage();
                                }
                            }
                            else
                            {
                                progressBar1.Value = 0;
                                string caption = "Внимание!";
                                //MessageBoxButtons buttons = MessageBoxButtons.OK;
                                string tmp_str = "устройства со следующими адресами запрограммированы:" + devices_programmed;
                                MessageBox.Show(tmp_str, caption);
                                devices_programmed = "";
                            }
                           //// }
                           //     ProgrammingFinishedFlag = 0;
                            
                            //serialPort1.Close();
                        }
                        else//если осталось послать лишь сообщение финализации записи
                        {
                            this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " sended finishing message\r\n");
                            if (((crcErrors >= 5) || (timeoutErrors >= 5)) && (ResendMessageFlag))
                            {
                                //linePointer = lines.Length;
                                //  flashPagePointer = flashPages.Length / 256;
                                this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " Connection error, programming aborted\r\n");
                            }
                            byte[] tmp_mBus = ModBusRTU.GenerateWritingFinishedMessage(flashCRC);//генерируем завершающее сообщение ModBus
                            ProgrammingFinishedFlag = 1;
                            //  this.textBox6.AppendText(DateTime.Now.ToString() + " ---> " + ByteToHex(tmp_mBus) + " \r\n");
                            this.serialPort1.ReceivedBytesThreshold = 9 - ProgrammingFinishedFlag;
                           // this.textBox3.Text =(ModBusRTU.CRC16(ModBusRTU.FileOfBytes)).ToString();
                           // this.textBox4.Text = ByteToHex(ModBusRTU.FileOfBytes).ToString();
                            serialPort1.Write(tmp_mBus, 0, tmp_mBus.Length);
                        }


                    }
                }
            }
            //serialPort1.DiscardInBuffer();
            //timer1.Stop();
           
        }

        private byte[] CreateFlashPages()
        {
            int tmpBytesQ ;
            if (((lines.Length-1)%16)==0)tmpBytesQ=(lines.Length-1)*16;//вообще нужно плясать от размера флеш-страницы, для Atmega128 flashpage=256 байт или 128 слов, по сути же это число 16 получается как (размер страницы)/16
            //else tmpBytesQ = (lines.Length+(16-lines.Length%16)) * 16;
            else tmpBytesQ = (lines.Length -1 + (16 - ((lines.Length-1) % 16))) * 16;
            int flashPageBPtrTail = 0;
            int flashPageBPtrHead = 0;
            linePointer = 0;
            byte[] tmpArr = new byte [tmpBytesQ];
            for (int i = 0; i < tmpBytesQ; i++)tmpArr[i] = 0xff;
            for (int i = 0; i < lines.Length; i++)
            {
                byte[] tmp = HexToByte(lines[i]);
                if (tmp[3] == 0)
                {
                    flashPageBPtrTail += tmp[0];
                    if (flashPageBPtrTail > tmpBytesQ) flashPageBPtrTail = tmpBytesQ-1;
                    for (int j = flashPageBPtrHead; j < flashPageBPtrTail+1; j++) tmpArr[j] = tmp[4 + j - flashPageBPtrHead];
                    flashPageBPtrHead = flashPageBPtrTail;
                }
            }
            progressBar1.Maximum = tmpArr.Length / 256;
            return tmpArr;
        }
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    ModBusRTU.ModBusAddress = ModBusRTU.DevicesAddressArray[this.checkedListBox1.SelectedIndex];
        //    //ModBusRTU.ModBusAddress = 19;
        //    SearchDevicesFlag = false;
        //    ModBusRTU.mode = 1;
        //    RunProgramming();
        //    ResponseBytes = 9;
            
        //    //ResendMessageFlag = true;
        //    //SendMessageFlag = false;
        //}



        private int RunSearchingDevices()
        {
            int error = 0;

            {

                SearchDevicesFlag = true;
                this.timer1.Start();
                SendMBusMessage();
            }
            return error;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.comboBox1.Items.AddRange(SerialPort.GetPortNames());
            this.comboBox1.SelectedIndex = comboBox1.Items.Count-1;
           // this.button1.Enabled = false;
            this.button2.Enabled = false;
           // this.button3.Enabled = false;
            this.button5.Enabled = false;
            
            //checkedListBox1.DrawMode = DrawMode.OwnerDrawFixed;
            //this.checkedListBox1.DrawItem +=
   // new System.Windows.Forms.DrawItemEventHandler(this.lstBox_DrawItem);
            //this.serialPort1.BreakState = true;
        }   

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            this.serialPort1.PortName = this.comboBox1.SelectedItem.ToString();
            //this.serialPort1.ReadTimeout
            
        }
        

        private void button3_Click(object sender, EventArgs e)
        {

            if(linePointer==0)lines = System.IO.File.ReadAllLines(this.openFileDialog1.FileName);// считываем файл в массив строк типа string

            byte[] tmp = HexToByte(lines[linePointer]);
            //textBox3.Text = ModBusRTU.CRC16(tmp).ToString();
            byte[] tmp_mBus = ModBusRTU.GenerateModBusWriteMessage(tmp);
            //tmp_crc[0] = Convert.ToByte((ModBusRTU.CRC16(tmp) >> 8) & 0x000000FF);
            //tmp_crc[1] = Convert.ToByte(ModBusRTU.CRC16(tmp) & 0x000000FF);
            //Array.Resize(ref tmp, tmp.Length + 2);
            //tmp[tmp.Length - 2] = tmp_crc[0];
            //tmp[tmp.Length - 1] = tmp_crc[1];
         //   textBox3.Text = ByteToHex(tmp_mBus);
         //   textBox4.Text = ByteToHex(tmp);
        //    linePointer++;
        }
        
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            byte[] buffer = new byte[ResponseBytes - ProgrammingFinishedFlag];

            for (int i = 0; i < ResponseBytes - ProgrammingFinishedFlag; i++)
            {
                buffer[i] = (byte)serialPort1.ReadByte();

            }
            
            spRead = ByteToHex(buffer);

            if (ModBusRTU.mode == 1)
            {

                if ((ModBusRTU.checkCRC16(buffer) == 1))
                {
                    spRead += " ---> CRC OK!\r\n";
                    if (FlashEraseFlag)
                    {
                        spRead += DateTime.Now.ToString() + " ---> " + " flash память устройства стёрта\r\n";
                        this.timer1.Interval = 2500;
                        string caption = "Внимание!";
                        string tmp_str = "устройство с адресом "+ ModBusRTU.ModBusAddress.ToString()+" стерто";
                        
                        MessageBox.Show(tmp_str, caption);
                        
                    }
                    else
                    {
                        if (ProgrammingFinishedFlag == 1)
                        {
                            spRead += DateTime.Now.ToString() + " ---> " + " программирование завершено успешно\r\n";
                            ProgrammingFinishedFlag = 0;
                            timer1.Stop();

                        }
                        else
                        {
                            spRead += DateTime.Now.ToString() + " ---> " + (flashPagePointer + 1).ToString() + " страниц памяти запрограммировано\r\n";

                        }
                    }
                    FlashEraseFlag = false;
                    crcErrors = 0;
                    timeoutErrors = 0;
                    SendMessageFlag = true;
                    ResendMessageFlag = false;
                    flashPagePointer++;
                    //linePointer++;
                    //serialPort1.DiscardInBuffer();
                    //serialPort1.DiscardOutBuffer();

                }
                else
                {
                    spRead += " ---> CRC Wrong!\r\n";
                    spRead += DateTime.Now.ToString() + " ---> " + "recieve CRC error occured, resending " + (flashPagePointer + 1).ToString() + " block\r\n";
                    ResendMessageFlag = true;
                    SendMessageFlag = false;
                    crcErrors++;
                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardOutBuffer();
                }
            }
            if(ModBusRTU.mode == 0)
            {
                
             
                if ((ModBusRTU.checkCRC16(buffer) == 1))
                {
      
                   // if (buffer[3] == 0x96)
                   // {
                        ModBusRTU.DevicesAddressArray[ModBusRTU.DevicesTemporaryCounter] = buffer[0];
                        Array.Resize(ref ModBusRTU.DevicesAddressArray, ModBusRTU.DevicesAddressArray.Length + 1);
                        ModBusRTU.DevicesTypeArray [ModBusRTU.DevicesTemporaryCounter] = Convert.ToByte(Convert.ToInt16(buffer[4])>>4);
                        Array.Resize(ref ModBusRTU.DevicesTypeArray, ModBusRTU.DevicesTypeArray.Length + 1);
                        ModBusRTU.DevicesProgramBuildArray[ModBusRTU.DevicesTemporaryCounter] = Convert.ToByte(Convert.ToInt16(buffer[4]));
                        Array.Resize(ref ModBusRTU.DevicesProgramBuildArray, ModBusRTU.DevicesProgramBuildArray.Length + 1);
                        ModBusRTU.DevicesTemporaryCounter++;
                    //}

                }
            }
            if (ModBusRTU.mode == 2)
            {
                if ((ModBusRTU.checkCRC16(buffer) == 1))
                {
                    spRead += " ---> CRC OK!\r\n";
                    spRead += " ---> Устройству с адресом " + ModBusRTU.ModBusAddress.ToString() + " присвоен адрес " + numericUpDown3.Value.ToString();
                    ModBusRTU.mode = 0;
                    ModBusRTU.ModBusAddress = Convert.ToByte(numericUpDown3.Value);
                }
            }
                //SendMBusMessage();

                this.demoThread =
                    new Thread(new ThreadStart(this.ThreadProcSafe));

            this.demoThread.Start();

        }

        private void serialPort1_PinChanged_1(object sender, SerialPinChangedEventArgs e)
        {
            crcErrors++;

        }
        private void ThreadProcSafe()
        {
            this.SetText(spRead);

        }
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
           // System.Threading.Thread.Sleep(100);
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                // this.textBox2.Clear();
                //text += "\r\n";
                this.textBox1.AppendText(DateTime.Now.ToString() + " ---> ");
                this.textBox1.AppendText(text);//  = text;
                this.textBox1.AppendText("\r\n");
              //  this.textBox3.AppendText(crcErrors.ToString());
              //  this.textBox4.AppendText(SendMessageFlag.ToString());
              //  this.textBox5.AppendText(ResendMessageFlag.ToString());
            }
            progressBar1.Style = ProgressBarStyle.Continuous;
            
            //serialPort1.DiscardInBuffer();

            //SendMBusMessage();
            
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
           // 

            if (!SendMessageFlag)
            {
                this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + "recieve timeout error occured, resending" + (linePointer+1).ToString() + "block\r\n");
                ResendMessageFlag = true;
                timeoutErrors++;
            }
            //this.textBox1.AppendText(DateTime.Now.ToString() + " ---> ");
            //this.textBox1.AppendText(spRead);//  = text;
            //this.textBox1.AppendText("\r\n");
            SendMBusMessage();
            //this.timer1.Start();
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
                //System.Threading.Thread.Sleep(20);
                //if (this.backgroundWorker1.CancellationPending)
                //    backgroundWorker1.CancelAsync();


        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //SendMBusMessage();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.serialPort1.IsOpen)
            {
                this.OpenButtonPressed = false;
              //  this.button1.Enabled = false;
                this.button2.Enabled = false;
             //   this.button3.Enabled = false;
                this.button5.Enabled = false;
                this.serialPort1.Close();
                this.button4.Text = "открыть порт";
            }
            else
            {
                this.OpenButtonPressed = true;
              //  this.button1.Enabled = true;
                this.button2.Enabled = true;
              //  this.button3.Enabled = true;
                this.button5.Enabled = true;
                this.serialPort1.Open();
                this.button4.Text = "закрыть порт";
            }
        }

        private void serialPort1_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            ////this.textBox3.AppendText(e.EventType.ToString());
            //this.timer1.Start();//запуск таймера, отслеживающего ошибку по таймауту 
            //spRead += " ---> CRC Wrong!\r\n";
            //spRead += DateTime.Now.ToString() + " ---> " + "recieve CRC error occured, resending" + linePointer.ToString() + "block\r\n";
            ////ResendMessageFlag = true;
            ////SendMessageFlag = false;
            //crcErrors++;
            ////SendMBusMessage();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            ModBusRTU.ModBusAddress = ModBusRTU.DevicesAddressArray[this.checkedListBox1.SelectedIndex];
            ModBusRTU.mode = 1;
            ResponseBytes = 9;
            SearchDevicesFlag = false;
            this.timer1.Interval = 3000;
            progressBar1.Value = 0;
            progressBar1.Style = ProgressBarStyle.Marquee;
            FlashEraseFlag = true;
            SendMBusMessage();
            this.serialPort1.DiscardInBuffer();
            this.serialPort1.DiscardOutBuffer();
            //this.textBox4.AppendText(ModBusRTU.mode.ToString() + SearchDevicesFlag.ToString() + ResponseBytes.ToString() + FlashEraseFlag.ToString());

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        //private void button3_Click_1(object sender, EventArgs e)
        //{
        //   // this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " Установка МодБас адреса\r\n");
        //   // ResponseBytes = 8;
        //    //ProgrammingFinishedFlag = 0;
        //   // ModBusRTU.ModBusAddress = Convert.ToByte(this.numericUpDown2.Value);
        //    //this.numericUpDown2.Value = this.numericUpDown1.Value;
        //   // byte[] tmp_mBus = ModBusRTU.GenerateModbusAddressWriteMessage(Convert.ToByte(this.numericUpDown1.Value));//генерируем сообщение ModBus
        //   // this.serialPort1.ReceivedBytesThreshold = 8;
        //    //serialPort1.Write(tmp_mBus, 0, tmp_mBus.Length);
        //}

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
          //  ModBusRTU.ModBusAddress = Convert.ToByte(this.numericUpDown2.Value);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.openFileDialog2.ShowDialog();
            this.textBox3.Text = this.openFileDialog2.FileName;
        }

        private int AddDeviceToList()                            //формирователь запросов поиска устройств
        {
            int out_val=1;
            SearchDevicesFlag = true;
            if (LastAskedRegister == 0) LastAskedRegister = 0x96;
            //else
            //{
                //if (LastAskedRegister == 0x93) LastAskedRegister++;
               // if (LastAskedRegister == 0x94) LastAskedRegister++;
               // else if (LastAskedRegister == 0x95)
            //    {
                    if (ModBusRTU.ModBusAddress <= this.numericUpDown1.Value)
                    {
                        
                        LastAskedRegister = 0x96;
                    }
                    else
                    {
                        SearchDevicesFlag = false;
                        out_val=0;//в случае, если это последнее устройство и последний регистр считан
                    }
              //  }

          //  }
            return out_val;

        }
        

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            string newtext;
            newtext = System.IO.File.ReadAllText(this.openFileDialog2.FileName);
            int tmp_address_pos=0;
            int tmp_address_val;
            int i=0;
            tmp_address_pos = newtext.IndexOf("address = ");//ищем соответствующий набор символов
            while (tmp_address_pos != -1)//проверяем, существует ли интересующий нас набор символов в текущем контексте
            {
                if (i > ArrayOfModbusAddresses.Length - 1) Array.Resize(ref ArrayOfModbusAddresses, i + 1);
                tmp_address_pos = 0;
                newtext = newtext.Remove(tmp_address_pos, (newtext.IndexOf("address = ") + 10));
               // this.textBox4.Text = newtext;
                tmp_address_val = Convert.ToInt16(newtext.Substring(0, 3));
                ArrayOfModbusAddresses[i] = Convert.ToByte(tmp_address_val);
               // this.textBox5.AppendText(tmp_address_val.ToString());
                tmp_address_pos = newtext.IndexOf("address = ");
                i++;
            }
           // this.textBox6.Text = ByteToHex(ArrayOfModbusAddresses);
         
        }

        private void CreateNewDevice(int newDeviceIndex, Color itemcolor, bool check)
        {
            Array.Resize(ref devName, devName.Length + 1);
           // Array.Resize(ref ModBusRTU.DevicesAddressArray, ModBusRTU.DevicesAddressArray.Length + 1);
            //ModBusRTU.DevicesAddressArray[ModBusRTU.DevicesAddressArray.Length - 1] = Convert.ToByte(this.numericUpDown1.Text);
          //  Array.Resize(ref ModBusRTU.DevicesProgramBuildArray,  ModBusRTU.DevicesProgramBuildArray.Length + 1);
            //ModBusRTU.DevicesProgramBuildArray[ModBusRTU.DevicesProgramBuildArray.Length - 1] = Convert.ToByte(this.numericUpDown4.Text);
          //  Array.Resize(ref ModBusRTU.DevicesStatusArray, ModBusRTU.DevicesStatusArray.Length + 1);
            //ModBusRTU.DevicesStatusArray[ModBusRTU.DevicesStatusArray.Length - 1] = Convert.ToByte(this.numericUpDown3.Text);
          //  Array.Resize(ref ModBusRTU.DevicesTypeArray, ModBusRTU.DevicesTypeArray.Length + 1);
            //ModBusRTU.DevicesTypeArray[ModBusRTU.DevicesTypeArray.Length - 1] = Convert.ToByte(this.numericUpDown2.Text);
            devName[newDeviceIndex] = "адрес устройства: ";
            devName[newDeviceIndex] += ModBusRTU.DevicesAddressArray[newDeviceIndex].ToString();
            //devName[newDeviceIndex] += "  ID устройстсва: " + ModBusRTU.DevicesTypeArray[newDeviceIndex].ToString();
            devName[newDeviceIndex]+= "  тип устройства: УКЗМ-1 " + "версия ПО: ";
            //int tmp_boot;
            //tmp_boot = ModBusRTU.DevicesTypeArray[newDeviceIndex];
            int tmp_boot = ModBusRTU.DevicesProgramBuildArray[newDeviceIndex];
            if (((tmp_boot & 0x80) >> 7) == 1)
            {
                devName[newDeviceIndex] += "A.";
            }
            else devName[newDeviceIndex] += "B.";
            devName[newDeviceIndex] += ((tmp_boot & 0x70) >> 4).ToString() + ".";
            devName[newDeviceIndex] += (tmp_boot & 0x0f).ToString();
            
            // this.listBox1.Items.Add(devName[newDeviceIndex]);
           // this.checkedListBox1.Font = New Font("Arial", 8.25, FontStyle.Regular, GraphicsUnit.Point, Nothing);
           // this.checkedListBox1.Items.Add(New Info("Visual Studio", Color.Black, New Font("Arial", 8.25, FontStyle.Bold, GraphicsUnit.Point, Nothing)));
            //this.listView1.Items.Add(devName[newDeviceIndex]);
            ListViewItem Item1 = new ListViewItem(devName[newDeviceIndex], 0);
            Item1.ForeColor = itemcolor;
            Item1.Checked = check;
            //listView1.Items.Add(Item1);
            this.checkedListBox1.Items.Add(devName[newDeviceIndex]);
            if (actualBoot > tmp_boot) this.checkedListBox1.SetItemChecked(newDeviceIndex, true);
            //this.checkedListBox1.SelectedItem
        }
/*
        private void lstBox_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
    {
    //
    // Draw the background of the ListBox control for each item.
    // Create a new Brush and initialize to a Black colored brush
    // by default.
    //
    e.DrawBackground();
    Brush myBrush = Brushes.Black;
    //
    // Determine the color of the brush to draw each item based on 
    // the index of the item to draw.
    //
    switch (e.Index)
    {
        case 0:
            myBrush = Brushes.Red;
            break;
        case 1:
            myBrush = Brushes.Orange;
            break;
        case 2:
            myBrush = Brushes.Purple;
            break;
    }
    //
    // Draw the current item text based on the current 
    // Font and the custom brush settings.
    //
    e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), 
        e.Font, myBrush,e.Bounds,StringFormat.GenericDefault);
    //
    // If the ListBox has focus, draw a focus rectangle 
    // around the selected item.
    //
    e.DrawFocusRectangle();
    }*/
        private void ChangeSelectedDevice(int SelectedDevIndex)
        {
            //this.listBox1.SelectedItem = devName[SelectedDevIndex];
          //  this.checkedListBox1.Items.RemoveAt(SelectedDevIndex - 1);
            // this.listBox1.Items.Remove(devName[SelectedDevIndex]);// = devName[SelectedDevIndex];
            devName[SelectedDevIndex] = "адрес устройства: ";
            devName[SelectedDevIndex] += ModBusRTU.DevicesAddressArray[SelectedDevIndex].ToString();
            devName[SelectedDevIndex] += "  тип устройстсва: " + ModBusRTU.DevicesTypeArray[SelectedDevIndex].ToString();
            devName[SelectedDevIndex] += "  версия ПО устройства: " + ModBusRTU.DevicesProgramBuildArray[SelectedDevIndex].ToString();
         //   this.checkedListBox1.Items.Insert(SelectedDevIndex - 1, devName[SelectedDevIndex]);
         //   this.checkedListBox1.SelectedIndex = 0;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            progressBar1.Style = ProgressBarStyle.Continuous;
            linePointer = 0;
            //this.serialPort1.Close();
            //this.serialPort1.Open();
            this.serialPort1.DiscardInBuffer();
            LastAskedRegister = 0;
            progressBar1.Value = 0;
            progressBar1.Maximum = Convert.ToInt16(this.numericUpDown1.Value - numericUpDown2.Value + 1);

            //ModBusRTU.mode = 2;
            //ResponseBytes = 8;
            this.serialPort1.DiscardOutBuffer();
            this.serialPort1.RtsEnable = false;
            this.serialPort1.DtrEnable = false;
            ProgrammingFinishedFlag = 0;
            this.timer1.Interval = 60;
            ModBusRTU.ModBusAddress = Convert.ToByte(this.numericUpDown2.Value);
            this.numericUpDown1.Enabled = false;
            this.numericUpDown2.Enabled = false;
            this.checkedListBox1.Items.Clear();
            Array.Resize(ref ModBusRTU.DevicesAddressArray,1);
            ModBusRTU.DevicesAddressArray[0] = 0;
            Array.Resize(ref ModBusRTU.DevicesTypeArray, 1);
            ModBusRTU.DevicesTypeArray[0] = 0;
            Array.Resize(ref ModBusRTU.DevicesProgramBuildArray, 1);
            ModBusRTU.DevicesProgramBuildArray[0] = 0;
            Array.Resize(ref ModBusRTU.DevicesStatusArray, 1);
            ModBusRTU.DevicesStatusArray[0] = 0;
            ModBusRTU.DevicesTemporaryCounter = 0;
            ModBusRTU.mode = 0;
            ResponseBytes = 7;
            serialPort1.ReceivedBytesThreshold = 7;
            RunSearchingDevices();
           // this.serialPort1.DiscardInBuffer();
          //  this.serialPort1.DiscardOutBuffer();
           
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string message = "Устройств найдено: " + (ModBusRTU.DevicesAddressArray.Length - 1).ToString() + "\r\n";
            string caption = "результаты поиска";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            //DialogResult result;

            // Displays the MessageBox.

            MessageBox.Show(message, caption, buttons);
        }

        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " Установка МодБас адреса\r\n");
            ModBusRTU.mode = 2;
            ResponseBytes = 8;
            ProgrammingFinishedFlag = 0;
            ModBusRTU.ModBusAddress = ModBusRTU.DevicesAddressArray[checkedListBox1.SelectedIndex];
            //this.textBox4.AppendText(checkedListBox1.SelectedIndex.ToString());
            //this.numericUpDown2.Value = this.numericUpDown1.Value;
            byte[] tmp_mBus = ModBusRTU.GenerateModbusAddressWriteMessage(Convert.ToByte(this.numericUpDown3.Value));//генерируем сообщение ModBus
            //this.serialPort1.ReceivedBytesThreshold = 9;
            serialPort1.Write(tmp_mBus, 0, tmp_mBus.Length);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
         //   ModBusRTU.ModBusAddress = ModBusRTU.DevicesAddressArray[Convert.ToInt16(listView1.SelectedIndices)];
           
        }
        
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                for (int i = 0; i < ModBusRTU.DevicesAddressArray.Length - 1; i++)
                {
 
                    this.checkedListBox1.SelectedIndex = i;
                    this.checkedListBox1.SetItemChecked(i, true);

                }
            }
            else
                for (int i = 0; i < ModBusRTU.DevicesAddressArray.Length - 1; i++)
                {
                    this.checkedListBox1.SelectedIndex = i;
                    this.checkedListBox1.SetItemChecked(i, false);
                }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            //ModBusRTU.ModBusAddress = ModBusRTU.DevicesAddressArray[this.checkedListBox1.SelectedIndex];
            ////ModBusRTU.ModBusAddress = 19;
            //SearchDevicesFlag = false;
            //ModBusRTU.mode = 1;
            //RunProgramming();
            //ResponseBytes = 9;
            ModBusRTU.mode = 1;
            ModBusRTU.ModBusAddress = ModBusRTU.DevicesAddressArray[this.checkedListBox1.SelectedIndex];
            checkedListBox1.SelectedIndex = 0;
            progressBar1.Style = ProgressBarStyle.Continuous;
            ProgrammingMode = 1;
            ProgrammingFinishedFlag = 0;
            SearchDevicesFlag = false;
            RunProgramming();
            //devicesChecked--;
            //this.checkedListBox1.SetItemChecked(this.checkedListBox1.SelectedIndex, false);
            //for (int i = 0; i < 1; i++)
            //{
            //    ModBusRTU.ModBusAddress = Convert.ToByte(i * 13);
                
            //    ResponseBytes = 9;
            //    ModBusRTU.mode = 1;
            //    RunProgramming();
            //}
            /*
            while (devicesChecked>0)
            {
                if(this.checkedListBox1.GetItemChecked(checkedListBox1.SelectedIndex))
                {
                    ResponseBytes = 9;
                    ModBusRTU.mode=1;
                    ModBusRTU.ModBusAddress = ModBusRTU.DevicesAddressArray[this.checkedListBox1.SelectedIndex];
                    this.textBox5.AppendText(ModBusRTU.ModBusAddress.ToString());
                    devicesChecked--;
                   // this.timer1.Interval = 500;
                    this.checkedListBox1.SetItemChecked(this.checkedListBox1.SelectedIndex, false);
                    this.textBox5.AppendText(this.checkedListBox1.SelectedIndex.ToString());
                    RunProgramming();
                }
                if(this.checkedListBox1.SelectedIndex<this.checkedListBox1.Items.Count-1)this.checkedListBox1.SelectedIndex++;
            }*/
        //    this.serialPort1.DiscardInBuffer();
        //    this.serialPort1.DiscardOutBuffer();
        }
        private int RunProgramming()
        {
            int error = 0;
            if (textBox7.Text == "ошибка при идентификации версии прошивки, файл неверный, либо поврежден")
            {
                this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " невозможно начать программирование, файл ошибочный, либо поврежден, попробуйте открыть другой\r\n");

            }
            else
            {
                this.textBox1.AppendText(DateTime.Now.ToString() + " ---> " + " программирование устройства с адресом "+ModBusRTU.ModBusAddress.ToString()+  " началось\r\n");

                //this.serialPort1.BreakState = false;
                //GetBootVersion(lines[11]);

                flashPagePointer = 0;
                crcErrors = 0;
                SendMessageFlag = true;
                //flashPages = CreateFlashPages();
                progressBar1.Value = 0;
                progressBar1.Maximum = flashPages.Length / 256 + 1;
                // this.textBox7.Text = ByteToHex(flashPages);
                //this.textBox3.Text = lines.Length.ToString();
                //this.textBox4.Text = flashPages.Length.ToString();
                // this.timer1.Enabled = true;
                timer1.Interval = 500;
                this.timer1.Start();
               // if (ProgrammingFinishedFlag == 1) ProgrammingFinishedFlag = 0;
                SendMBusMessage();
            }
            return error;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            serialPort1.PortName = comboBox1.SelectedItem.ToString();
        }
 
    }

}