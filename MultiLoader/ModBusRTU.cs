using System;

/// <summary>
/// Сводное описание для Class1
/// </summary>
public class ModBusRTU
{
    //public ModBusRTU()
    //{

    //}
    public static byte ModBusAddress = 0x00;
    public static byte mode = 0x01;//mode=1 означает, что идет программирование по RS, mode=0 означает режим поиска/модификации устройств, mode=2 означает, что сейчас работает режим смены адреса устройств
    static ushort crcBase = 0xff;
    public static byte[] DevicesAddressArray = { 0x00 };
    public static byte[] DevicesTypeArray = { 0x00 };
    public static byte[] DevicesProgramBuildArray = { 0x00 };
    public static byte[] DevicesStatusArray = { 0x00 };
    public static byte[] FileOfBytes = {0x00};
    public static byte DevicesTemporaryCounter = 0;
    public static ushort tmp_crc=0xffff;
public static int CRC16 (byte[] Data)
    {
    ushort[] wCRCTable = 
        {
        0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241,
        0XC601, 0X06C0, 0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440,
        0XCC01, 0X0CC0, 0X0D80, 0XCD41, 0X0F00, 0XCFC1, 0XCE81, 0X0E40,
        0X0A00, 0XCAC1, 0XCB81, 0X0B40, 0XC901, 0X09C0, 0X0880, 0XC841,
        0XD801, 0X18C0, 0X1980, 0XD941, 0X1B00, 0XDBC1, 0XDA81, 0X1A40,
        0X1E00, 0XDEC1, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41,
        0X1400, 0XD4C1, 0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641,
        0XD201, 0X12C0, 0X1380, 0XD341, 0X1100, 0XD1C1, 0XD081, 0X1040,
        0XF001, 0X30C0, 0X3180, 0XF141, 0X3300, 0XF3C1, 0XF281, 0X3240,
        0X3600, 0XF6C1, 0XF781, 0X3740, 0XF501, 0X35C0, 0X3480, 0XF441,
        0X3C00, 0XFCC1, 0XFD81, 0X3D40, 0XFF01, 0X3FC0, 0X3E80, 0XFE41,
        0XFA01, 0X3AC0, 0X3B80, 0XFB41, 0X3900, 0XF9C1, 0XF881, 0X3840,
        0X2800, 0XE8C1, 0XE981, 0X2940, 0XEB01, 0X2BC0, 0X2A80, 0XEA41,
        0XEE01, 0X2EC0, 0X2F80, 0XEF41, 0X2D00, 0XEDC1, 0XEC81, 0X2C40,
        0XE401, 0X24C0, 0X2580, 0XE541, 0X2700, 0XE7C1, 0XE681, 0X2640,
        0X2200, 0XE2C1, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041,
        0XA001, 0X60C0, 0X6180, 0XA141, 0X6300, 0XA3C1, 0XA281, 0X6240,
        0X6600, 0XA6C1, 0XA781, 0X6740, 0XA501, 0X65C0, 0X6480, 0XA441,
        0X6C00, 0XACC1, 0XAD81, 0X6D40, 0XAF01, 0X6FC0, 0X6E80, 0XAE41,
        0XAA01, 0X6AC0, 0X6B80, 0XAB41, 0X6900, 0XA9C1, 0XA881, 0X6840,
        0X7800, 0XB8C1, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41,
        0XBE01, 0X7EC0, 0X7F80, 0XBF41, 0X7D00, 0XBDC1, 0XBC81, 0X7C40,
        0XB401, 0X74C0, 0X7580, 0XB541, 0X7700, 0XB7C1, 0XB681, 0X7640,
        0X7200, 0XB2C1, 0XB381, 0X7340, 0XB101, 0X71C0, 0X7080, 0XB041,
        0X5000, 0X90C1, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
        0X9601, 0X56C0, 0X5780, 0X9741, 0X5500, 0X95C1, 0X9481, 0X5440,
        0X9C01, 0X5CC0, 0X5D80, 0X9D41, 0X5F00, 0X9FC1, 0X9E81, 0X5E40,
        0X5A00, 0X9AC1, 0X9B81, 0X5B40, 0X9901, 0X59C0, 0X5880, 0X9841,
        0X8801, 0X48C0, 0X4980, 0X8941, 0X4B00, 0X8BC1, 0X8A81, 0X4A40,
        0X4E00, 0X8EC1, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
        0X4400, 0X84C1, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641,
        0X8201, 0X42C0, 0X4380, 0X8341, 0X4100, 0X81C1, 0X8081, 0X4040 
        };

//byte nTemp;
int crc = 0xFFFF;
int wLength;
wLength = Data.Length + 2;

   for(int i=0; i<Data.Length; i++)
   {
       crc = (crc >> 8) ^ wCRCTable[(crc & crcBase) ^ Convert.ToInt32(Data[i])];
   }

   return ((crc<<8)|(crc>>8))&0x0000FFFF;

   }
public static byte[] GenerateModBusWriteMessage(byte[] InData)
{
    int CRC;
    byte[] OutData = new byte[263];
    OutData[0]=ModBusAddress;
    OutData[1]=0x10;
    OutData[2]=0x00;
    OutData[3]=0xAA;
    OutData[4]=0x00;
    OutData[5]=128;
    OutData[6]=255;
    if (FileOfBytes.Length == 1) Array.Resize(ref FileOfBytes, 256);
    else Array.Resize(ref FileOfBytes,FileOfBytes.Length+256);
    for (int i = 7; i < 7+InData.Length; i++)
        {
            OutData[i] = InData[i - 7];
            FileOfBytes[FileOfBytes.Length - 256 + i - 7] = InData[i - 7];
        }
    //tmp_crc = Convert.ToUInt16(ModBusRTU.CRC16_total(OutData));
    CRC = ModBusRTU.CRC16(OutData);
    Array.Resize(ref OutData, OutData.Length + 2);
    OutData[263] = Convert.ToByte((CRC >> 8) & 0x000000ff);
    OutData[264] = Convert.ToByte(CRC & 0x000000ff);
    return OutData;
}
/*
public static int CRC16_total(byte[] Data)
{
    ushort[] wCRCTable = 
        {
        0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241,
        0XC601, 0X06C0, 0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440,
        0XCC01, 0X0CC0, 0X0D80, 0XCD41, 0X0F00, 0XCFC1, 0XCE81, 0X0E40,
        0X0A00, 0XCAC1, 0XCB81, 0X0B40, 0XC901, 0X09C0, 0X0880, 0XC841,
        0XD801, 0X18C0, 0X1980, 0XD941, 0X1B00, 0XDBC1, 0XDA81, 0X1A40,
        0X1E00, 0XDEC1, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41,
        0X1400, 0XD4C1, 0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641,
        0XD201, 0X12C0, 0X1380, 0XD341, 0X1100, 0XD1C1, 0XD081, 0X1040,
        0XF001, 0X30C0, 0X3180, 0XF141, 0X3300, 0XF3C1, 0XF281, 0X3240,
        0X3600, 0XF6C1, 0XF781, 0X3740, 0XF501, 0X35C0, 0X3480, 0XF441,
        0X3C00, 0XFCC1, 0XFD81, 0X3D40, 0XFF01, 0X3FC0, 0X3E80, 0XFE41,
        0XFA01, 0X3AC0, 0X3B80, 0XFB41, 0X3900, 0XF9C1, 0XF881, 0X3840,
        0X2800, 0XE8C1, 0XE981, 0X2940, 0XEB01, 0X2BC0, 0X2A80, 0XEA41,
        0XEE01, 0X2EC0, 0X2F80, 0XEF41, 0X2D00, 0XEDC1, 0XEC81, 0X2C40,
        0XE401, 0X24C0, 0X2580, 0XE541, 0X2700, 0XE7C1, 0XE681, 0X2640,
        0X2200, 0XE2C1, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041,
        0XA001, 0X60C0, 0X6180, 0XA141, 0X6300, 0XA3C1, 0XA281, 0X6240,
        0X6600, 0XA6C1, 0XA781, 0X6740, 0XA501, 0X65C0, 0X6480, 0XA441,
        0X6C00, 0XACC1, 0XAD81, 0X6D40, 0XAF01, 0X6FC0, 0X6E80, 0XAE41,
        0XAA01, 0X6AC0, 0X6B80, 0XAB41, 0X6900, 0XA9C1, 0XA881, 0X6840,
        0X7800, 0XB8C1, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41,
        0XBE01, 0X7EC0, 0X7F80, 0XBF41, 0X7D00, 0XBDC1, 0XBC81, 0X7C40,
        0XB401, 0X74C0, 0X7580, 0XB541, 0X7700, 0XB7C1, 0XB681, 0X7640,
        0X7200, 0XB2C1, 0XB381, 0X7340, 0XB101, 0X71C0, 0X7080, 0XB041,
        0X5000, 0X90C1, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
        0X9601, 0X56C0, 0X5780, 0X9741, 0X5500, 0X95C1, 0X9481, 0X5440,
        0X9C01, 0X5CC0, 0X5D80, 0X9D41, 0X5F00, 0X9FC1, 0X9E81, 0X5E40,
        0X5A00, 0X9AC1, 0X9B81, 0X5B40, 0X9901, 0X59C0, 0X5880, 0X9841,
        0X8801, 0X48C0, 0X4980, 0X8941, 0X4B00, 0X8BC1, 0X8A81, 0X4A40,
        0X4E00, 0X8EC1, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
        0X4400, 0X84C1, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641,
        0X8201, 0X42C0, 0X4380, 0X8341, 0X4100, 0X81C1, 0X8081, 0X4040 
        };

    //byte nTemp;
    int crc = 0xf;
    int wLength;
    wLength = Data.Length + 2;

    for (int i = 0; i < Data.Length; i++)
    {
        crc = (crc >> 8) ^ wCRCTable[(crc & crcBase) ^ Convert.ToInt32(Data[i])];
    }

    return ((crc << 8) | (crc >> 8)) & 0x0000FFFF;

}
  */
public static byte[] GenerateEraseFlashMessage()
{
    int CRC;
    byte[] OutData = new byte[6];
    OutData[0] = ModBusAddress;
    OutData[1] = 0x06;
    OutData[2] = 0x00;
    OutData[3] = 0xAC;
    OutData[4] = 0x00;
    OutData[5] = 0x01;
    CRC = ModBusRTU.CRC16(OutData);
    Array.Resize(ref OutData, OutData.Length + 2);
    OutData[6] = Convert.ToByte((CRC >> 8) & 0x000000ff);
    OutData[7] = Convert.ToByte(CRC & 0x000000ff);
    return OutData;
}
public static byte[] GenerateWritingFinishedMessage(int crc)
{
    int CRC;
    byte[] OutData = new byte[6];
    OutData[0] = ModBusAddress;
    OutData[1] = 0x06;
    OutData[2] = 0x00;
    OutData[3] = 0xAB;
    OutData[4] =  Convert.ToByte(crc>>8);
    OutData[5] = Convert.ToByte(crc & 0x000000ff);
    CRC = ModBusRTU.CRC16(OutData);
    Array.Resize(ref OutData, OutData.Length + 2);
    OutData[6] = Convert.ToByte((CRC >> 8) & 0x000000ff);
    OutData[7] = Convert.ToByte(CRC & 0x000000ff);
    return OutData;
}
public static byte[] GenerateModbusAddressWriteMessage(byte NewAddress)
{
    int CRC;
    byte[] OutData = new byte[6];
    OutData[0] = ModBusAddress;
    OutData[1] = 0x06;
    OutData[2] = 0x00;
    OutData[3] = 0x93;
    OutData[4] = 0x00;
    OutData[5] = NewAddress;
    CRC = ModBusRTU.CRC16(OutData);
    Array.Resize(ref OutData, OutData.Length + 2);
    OutData[6] = Convert.ToByte((CRC >> 8) & 0x000000ff);
    OutData[7] = Convert.ToByte(CRC & 0x000000ff);
    //ModBusAddress = NewAddress;
    return OutData;
}

public static byte[] GenerateModbusReadSingleRegisterMessage(byte Register)
{
    int CRC;
    byte[] OutData = new byte[6];
    OutData[0] = ModBusAddress;
    OutData[1] = 0x03;//тип запроса (0х03 - чтение одного регистра; 0х06 - запись одного регистра; 0х10 - запись нескольких регистров)
    OutData[2] = 0x00;//адрес регистра, ст. байт
    OutData[3] = Register;//адрес регистра, мл. байт
    OutData[4] = 0x00;//количество байт в запросе ст.байт
    OutData[5] = 0x01;//количество байт в запросе мл.байт
    CRC = ModBusRTU.CRC16(OutData);
    Array.Resize(ref OutData, OutData.Length + 2);
    OutData[6] = Convert.ToByte((CRC >> 8) & 0x000000ff);//CRC High
    OutData[7] = Convert.ToByte(CRC & 0x000000ff);//CRC Low
    return OutData;
}
public static int checkCRC16(byte[] Data)
{
    byte[] Data_=Data;
    Array.Resize(ref Data_, Data_.Length -2);
    int CRC = ModBusRTU.CRC16(Data_);

    if ((Data[Data.Length - 2] == Convert.ToByte((CRC >> 8) & 0x000000ff)) & (Data[Data.Length - 1] == Convert.ToByte(CRC & 0x000000ff))) return 1;
    else return 0;

}

}
