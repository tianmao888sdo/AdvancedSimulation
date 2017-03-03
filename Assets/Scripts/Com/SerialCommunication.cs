using UnityEngine;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System;

public class SerialCommunication 
{
    private SerialPort m_serialPort;//串口工具;
    private string m_ports = "Com3";
    private int m_bitRate = 115200;//波特率

    private Thread m_thread;//工作线程
    private List<int> m_receivedBytes;//收到的文本

    public bool isRunning = false;

    public string ProtocolHeader;//协议头
    public string ProtocolTail;

    public SerialCommunication()
    {
        try
        {
            // 新建串口
            m_serialPort = new SerialPort(m_ports, m_bitRate);
            m_serialPort.Parity = Parity.None;
            m_serialPort.DataBits = 8;
            m_serialPort.StopBits = StopBits.One;
            m_serialPort.Handshake = Handshake.None;
            m_serialPort.ReadBufferSize = 8192;
            m_serialPort.Open();
            m_receivedBytes = new List<int>();
            m_thread = new Thread(new ThreadStart(ReadSerialPortData));
        }
        catch
        {
        }
    }

    public void Run()
    {
        m_thread.Start();
        isRunning = true;
    }

    public void Send(string str)
    {
        m_serialPort.WriteLine(str);
    }

    public List<int> GetBytes()
    {
        return m_receivedBytes;
    }

    public void Clear()
    {
        m_receivedBytes.Clear();
    }

    public bool IsRunning { get { return isRunning; } }

    //读取串口的数据
    private void ReadSerialPortData()
    {
        while (isRunning&&m_serialPort.IsOpen)
        {
            try
            {
                //接收串口数据并进行处理;
                m_receivedBytes.Add(m_serialPort.ReadByte());
                Debug.Log(m_receivedBytes.Count);
            }
            catch (Exception ex)
            {
            }
        }
    }

    /// <summary>
    /// 串口是否已经打开; 
    /// <returns></returns>
    public bool SerialPortIsOpen()
    {
        if (m_serialPort == null)
        {
            return false;
        }
        else
        {
            return m_serialPort.IsOpen;
        }
    }

    /// <summary>
    /// 释放资源;
    /// </summary>
    public void deleteMe()
    {
        isRunning = false;

        if (m_serialPort != null && m_serialPort.IsOpen)
        {
            try
            {
                m_serialPort.Close();
            }
            catch { }
        }

        if (m_thread != null && m_thread.IsAlive)
        {
            m_thread.Abort();
            m_thread.Join();
        }

        m_receivedBytes.Clear();
        m_receivedBytes = null;
    }
}
