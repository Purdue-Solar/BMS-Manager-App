using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Input;
using Windows.Security.Cryptography.Certificates;
using Windows.Media.Devices;
using Windows.UI.Core;
using Windows.System;
using Microsoft.UI.Xaml.Automation.Peers;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;
using System.ComponentModel;
using System.Threading;
using Microsoft.Extensions.Logging;
using CsvHelper;

//Setup Logger

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BMSManagerRebuilt
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //Initializing Variable
        private string value; //
        private string[] portsNames = SerialPort.GetPortNames(); //List of Available COM Ports
        private string selectedPortName;
        private SerialPort serialPort;
        private int tries = 22;

        //Initializing Logging
        static ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Debug));
        ILogger logger = factory.CreateLogger<MainWindow>();

        public MainWindow()
        {
            this.InitializeComponent();
            
        }
            
        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            
            if ((string)myButton.Content == "Edit")
            {
                logger.LogDebug("Button clicked");
                string temp = value;
                value = processTextBox();
                logger.LogDebug("{temp} is changed into {value}", temp, value);
                WritePort(value, tries);
                myButton_Switch();       
            }
        }
        private void myButton_Switch()
        {
            myButton.Content = "Accepted";
        }
        private string processTextBox()
        {
            return textBoxTest.Text;
        }
        private void TextBoxChange(object sender, TextChangedEventArgs e)
        {
            myButton.Content = "Edit";
        }
        private void TextBoxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == (VirtualKey)13)
            {
                logger.LogDebug("Enter Key is hit");
                string temp = value;
                value = processTextBox();
                logger.LogDebug("{temp} is changed into {value}", temp, value);
                WritePort(value, tries);
                myButton_Switch();
            }
        }
        private void PortDetect(object sender, RoutedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            PortsBox.ItemsSource = null;
            PortsBox.Items.Clear();
            portsNames = SerialPort.GetPortNames();
            PortsBox.ItemsSource = portsNames;
        }
        private void PortSelect(object sender, SelectionChangedEventArgs e)
        {
            int tries = 22;
            if (portsNames.Length > 0)
            {
                if (e.AddedItems[0].ToString() != "")
                { 
                    selectedPortName = e.AddedItems[0].ToString();
                    serialPort = new SerialPort(selectedPortName, 9600, Parity.None, 8, StopBits.One); 
                }
            }
            while (tries > 0)
            {
                try
                {
                    serialPort.Handshake = Handshake.XOnXOff;
                    if (!serialPort.IsOpen)
                    {
                        serialPort.Open();
                        Thread.Sleep(4000);
                    }
                    //serialPort.Write(text);
                    //Console.WriteLine("Port write");
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    tries--;
                    Thread.Sleep(1);
                }
            }
            logger.LogDebug("{port} is selected and opened", selectedPortName);
        }
        //private void AppClose(object sender, CancelEventArgs e)
        //{
        //    if (serialPort.IsOpen)
        //    {
        //        serialPort.Close();
        //    }
        //}
        private void WritePort(string text, int tries)
        {   

        }
        private void ReadFromPort(object sender, SerialDataReceivedEventArgs e)
        {

        }

        //End Line
    }  
}
