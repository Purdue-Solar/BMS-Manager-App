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
using System.Runtime.InteropServices;
using PSR.BMS.Configuration;
using BMSManagerRebuilt.GUIOperation;
using ScottPlot.WinUI;
using ScottPlot;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Defaults;

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

        /// <summary>
        /// Serial Port Operation | Uncomment this when serial ports are integrated
        /// </summary>
        //private string[] portsNames = SerialPort.GetPortNames(); //List of Available COM Ports
        //private string selectedPortName;
        //private SerialPort serialPort;
        //private int tries = 22;
        //private bool portConnected = false;
        //private byte[] portBuffer = new byte[16];
        double[] xs = { 1, 2, 3, 4, 5 };
        double[] ys = { 1, 4, 9, 16, 25 };

        private byte[] incomingData = new byte[ConfigurationWriter.MaxFileSize];
        private byte[] outcomingData = new byte[ConfigurationWriter.MaxFileSize];

        /// <summary>
        /// Debug Logger
        /// </summary>
        static ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Debug));
        ILogger logger = factory.CreateLogger<MainWindow>();

        public MainWindow()
        {
            this.InitializeComponent();
            WinUIPlot1.Plot.Add.Scatter(xs, ys);
            WinUIPlot1.Refresh();

            WinUIPlot2.Plot.Add.Scatter(xs, ys);
            WinUIPlot2.Refresh();

            WinUIPlot3.Plot.Add.Scatter(xs, ys);
            WinUIPlot3.Refresh();
        }

        private void Window_SizeChanged(object sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width <= 1000)
            {
                MyScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            else
            {
                MyScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            if (e.Size.Height <= 1000)
            {
                MyScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            else
            {
                MyScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        //Wrapper for ButtonOperation
        public void ConfigureButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonOperation.ConfigureButton_Click(sender, "Hello");
        }



        //private void changeButtonText(Button myButton, string text)
        //{
        //    myButton.Content = text;
        //}

        //private string processTextBox()
        //{
        //    return textBoxTest.Text;
        //}

        //private void TextBoxChange(object sender, TextChangedEventArgs e)
        //{
        //    changeButtonText(myButton, "Edit");
        //}

        //private void TextBoxKeyDown(object sender, KeyRoutedEventArgs e)
        //{
        //    if (e.Key == (VirtualKey)13)
        //    {
        //        logger.LogDebug("Enter Key is hit");
        //        string temp = value;
        //        value = processTextBox();
        //        logger.LogDebug("{temp} is changed into {value}", temp, value);
        //        //WriteToPort(value); //For Serial Port Operation
        //        changeButtonText(myButton, "Edited");
        //    }
        //}



        //Port operation
        /// <summary>
        /// Detect New Port Names
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void PortDetect(object sender, RoutedEventArgs e)
        //{
        //    logger.LogDebug("Run Port Detection");
        //    portsNames = SerialPort.GetPortNames();
        //    PortsBox.ItemsSource = portsNames;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void PortSelect(object sender, SelectionChangedEventArgs e)
        //{
        //    if (portsNames.Length > 0 & !portConnected)
        //    {
        //        changeButtonText(PortDisconnectButton, "Disconnect");
        //        if (e.AddedItems[0] != null)
        //        { 
        //            selectedPortName = e.AddedItems[0].ToString();
        //            serialPort = new SerialPort(selectedPortName, 9600, Parity.None, 8, StopBits.One);
        //            serialPort.RtsEnable = true;
        //            serialPort.DataReceived += new SerialDataReceivedEventHandler(ReadFromPort);
        //            //Attempting to connect
        //            PortConnect(serialPort);
        //            portConnected = true;
        //        }
        //    }
        //}

        //private void PortConnect(SerialPort serialPort)
        //{
        //    logger.LogDebug("Connecting to {port}", serialPort.PortName);
        //    int tries = 22;
        //    while (tries > 0)
        //    {
        //        try
        //        {
        //            serialPort.Handshake = Handshake.XOnXOff;
        //            if (!serialPort.IsOpen)
        //            {
        //                serialPort.Open();
        //                Thread.Sleep(1);
        //            }
        //            break;
        //        }
        //        catch (UnauthorizedAccessException)
        //        {
        //            tries--;
        //            Thread.Sleep(1);  
        //        }
        //    }
        //    if (serialPort.IsOpen)
        //    {
        //        PortStatusText.Text = "Port Status:    Connected"; //Spacing to match "Port Status: Disconnected"
        //        logger.LogDebug("{port} is selected and opened!", serialPort.PortName);
        //    }
        //}

        //private void DisconnectPort(object sender, RoutedEventArgs e)
        //{
        //    if (serialPort != null)
        //    {
        //        if (serialPort.IsOpen)
        //        {
        //            logger.LogDebug("Disconnecting from {port}", serialPort.PortName);
        //            serialPort.Close();
        //            changeButtonText(PortDisconnectButton, "Disconnected");
        //            PortStatusText.Text = "Port Status: Disconnected";
        //            errorWindows DisconnectWindow = new errorWindows();
        //            DisconnectWindow.Activate();
        //            logger.LogDebug("Disconnected. If you want to connect back to the same port, MUST unplug and plug it in again");
        //        }
        //    }
        //}

        //private void WriteToPort(string text)
        //{
        //    logger.LogDebug("Writing Data: {text}", text);
        //     if (serialPort.IsOpen)
        //    {
        //        serialPort.WriteLine(text);
        //        logger.LogDebug("Data Written: {text}", text);
        //    }
        //}

        ////Read 1 byte from Port
        //private void ReadFromPort(object sender, SerialDataReceivedEventArgs e)
        //{
        //    logger.LogDebug("Intercepting Data");
        //    if (serialPort.IsOpen)
        //    {
        //        serialPort.Read(portBuffer, 0, 1);
        //        logger.LogDebug("Data read: {data}", portBuffer[0]);
        //        logger.LogDebug("Data Intercepted");
        //    }
        //}
        ////End Line
    }  
}
