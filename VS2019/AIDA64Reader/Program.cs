using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AIDA64Reader
{
    class Program
    {
        public struct AIDA64Item
        {
            internal string id;
            internal string value;
        };
        static void Main(string[] args)
        {
            for (; ; )
            { 
                try
                {
                    string tempString = string.Empty;
                    tempString += "<AIDA64>";

                    MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("AIDA64_SensorValues");
                    MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
                    tempString = tempString + "";
                    for (int i = 0; i < accessor.Capacity; i++)
                    {
                        tempString = tempString + (char)accessor.ReadByte(i);
                    }
                    tempString = tempString.Replace("\0", "");
                    tempString = tempString + "";
                    accessor.Dispose();
                    mmf.Dispose();

                    tempString += "</AIDA64>";
                    XDocument aidaXML = XDocument.Parse(tempString);
                    var sysElements = aidaXML.Element("AIDA64").Elements("sys");
                    var tempElements = aidaXML.Element("AIDA64").Elements("temp");
                    var fanElements = aidaXML.Element("AIDA64").Elements("fan");
                    var dutyElements = aidaXML.Element("AIDA64").Elements("duty");
                    var voltElements = aidaXML.Element("AIDA64").Elements("volt");
                    var pwrElements = aidaXML.Element("AIDA64").Elements("pwr");
                    SerialPort serialPort1 = new SerialPort("COM7", 1500000, Parity.None, 8, StopBits.One); //先到设备管理器里找CH340G对应的端口
                    serialPort1.Open();
                    List<AIDA64Item> items = new List<AIDA64Item>();
                    foreach (var i in sysElements)
                    {
                        Console.WriteLine(i.Element("id").Value + "\t" + i.Element("label").Value + "\t" + i.Element("value").Value);
                        AIDA64Item item = new AIDA64Item();
                        switch (i.Element("id").Value)
                        {
                            case "SCPUUTI":  //CPU使用率
                                item.id = i.Element("id").Value;
                                item.value = i.Element("value").Value;
                                break;
                            case "SCPUCLK":  //CPU时钟频率
                                item.id = i.Element("id").Value;
                                item.value = i.Element("value").Value;
                                break;
                        }
                        if (item.id != null)
                        {
                            items.Add(item);
                        }
                    }
                    foreach (var i in tempElements)
                    {
                        Console.WriteLine(i.Element("id").Value + "\t" + i.Element("label").Value + "\t" + i.Element("value").Value);
                        AIDA64Item item = new AIDA64Item();
                        switch (i.Element("id").Value)
                        {
                            case "TCPU":   //CPU温度
                                item.id = i.Element("id").Value;
                                item.value = i.Element("value").Value;
                                break;
                        }
                        if (item.id != null)
                        {
                            items.Add(item);
                        }
                    }
                    foreach (var i in voltElements)
                    {
                        Console.WriteLine(i.Element("id").Value + "\t" + i.Element("label").Value + "\t" + i.Element("value").Value);
                        AIDA64Item item = new AIDA64Item();
                        switch (i.Element("id").Value)
                        {
                            case "VCPU":  //CPU核心电压
                                item.id = i.Element("id").Value;
                                item.value = i.Element("value").Value;
                                break;
                        }
                        if (item.id != null)
                        {
                            items.Add(item);
                        }
                    }
                    foreach (var i in items)
                    {
                        serialPort1.Write("?" + i.id + "=" + i.value + "!");
                    }
                    Thread.Sleep(1000);
                    serialPort1.Close();
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
