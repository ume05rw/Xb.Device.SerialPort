using System;
using System.Collections.Generic;

namespace Xb.Device
{
    /// <summary>
    /// シリアルポート管理クラス
    /// </summary>
    /// <remarks></remarks>
    public class SerialPort : IDisposable
    {
        /// <summary>
        /// ボーレート
        /// </summary>
        /// <remarks></remarks>
        public enum BaudRate
        {
            Br115200 = 15200,
            Br57600 = 57600,
            Br56000 = 56000,
            Br38400 = 38400,
            Br19200 = 19200,
            Br9600 = 9600,
            Br4800 = 4800,
            Br2400 = 2400
        }


        /// <summary>
        /// ビット
        /// </summary>
        /// <remarks></remarks>
        public enum Bit
        {
            Bit8 = 8,
            Bit7 = 7
        }


        /// <summary>
        /// パリティ
        /// </summary>
        /// <remarks></remarks>
        public enum Parity
        {
            None = System.IO.Ports.Parity.None,
            Even = System.IO.Ports.Parity.Even,
            Mark = System.IO.Ports.Parity.Mark,
            Odd = System.IO.Ports.Parity.Odd,
            Space = System.IO.Ports.Parity.Space
        }


        /// <summary>
        /// ストップビット
        /// </summary>
        /// <remarks></remarks>
        public enum StopBit
        {
            One = System.IO.Ports.StopBits.One,
            OnePointFive = System.IO.Ports.StopBits.OnePointFive,
            Two = System.IO.Ports.StopBits.Two,
            None = System.IO.Ports.StopBits.None
        }


        /// <summary>
        /// データ受信イベントの引数クラス
        /// </summary>
        /// <remarks></remarks>
        public class RecieveEventArgs : EventArgs
        {


            public byte[] Result;
            public RecieveEventArgs(byte[] data)
            {
                this.Result = data;
            }

        }


        public delegate void RecieveEventHandler(System.Object sender, RecieveEventArgs e);
        public event RecieveEventHandler Recieved;

        private System.IO.Ports.SerialPort withEventsField__port;
        private System.IO.Ports.SerialPort _port
        {
            get { return withEventsField__port; }
            set
            {
                if (withEventsField__port != null)
                {
                    withEventsField__port.DataReceived -= Recieve;
                }
                withEventsField__port = value;
                if (withEventsField__port != null)
                {
                    withEventsField__port.DataReceived += Recieve;
                }
            }
        }

        private List<string> _localPorts;

        /// <summary>
        /// シリアルポートオブジェクト
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public System.IO.Ports.SerialPort Port
        {
            get { return this._port; }
        }


        /// <summary>
        /// ローカルのポート名配列
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> LocalPortNames
        {
            get { return this._localPorts; }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks></remarks>

        public SerialPort()
        {
            this._port = new System.IO.Ports.SerialPort();

            this.RefreshPorts();
        }


        /// <summary>
        /// ローカル環境のポート名を更新する。
        /// </summary>
        /// <remarks></remarks>
        public void RefreshPorts()
        {
            this._localPorts = new List<string>();
            foreach (string prt in System.IO.Ports.SerialPort.GetPortNames())
            {
                this._localPorts.Add(prt);
            }
        }


        /// <summary>
        /// UIスレッドで渡し値イベントをレイズする。
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="args"></param>
        /// <remarks></remarks>
        private void FireEvent(object eventType, object args)
        {
            switch (eventType.ToString())
            {
                case "Recieved":
                    Recieved?.Invoke(this, new RecieveEventArgs((byte[])args));

                    break;
                default:
                    break;
                    //何もしない。
            }
        }


        /// <summary>
        /// ポートを開く
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="bit"></param>
        /// <param name="parity"></param>
        /// <param name="stopBit"></param>
        /// <remarks></remarks>

        public void Open(string portName = null, 
                         BaudRate baudRate = Device.SerialPort.BaudRate.Br115200, 
                         Bit bit = Device.SerialPort.Bit.Bit8, 
                         Parity parity = Device.SerialPort.Parity.None, 
                         StopBit stopBit = Device.SerialPort.StopBit.One)
        {
            this.RefreshPorts();
            if (this._localPorts.Count <= 0)
            {
                Xb.Util.Out("Xb.Device.SerialPort.Open: シリアルポートが検出できません。");
                throw new ArgumentException("Xb.Device.SerialPort.Open: シリアルポートが検出できません。");
            }

            if (portName == null)
                portName = this._localPorts[0];

            if (!this._localPorts.Contains(portName))
            {
                Xb.Util.Out("Xb.Device.SerialPort.Open: 引数のポート名が不正です。");
                throw new ArgumentException("Xb.Device.SerialPort.Open: 引数のポート名が不正です。");
            }

            this._port.PortName = portName;
            this._port.BaudRate = (int)baudRate;
            this._port.Parity = (System.IO.Ports.Parity)parity;
            this._port.DataBits = (int)bit;
            this._port.StopBits = (System.IO.Ports.StopBits)stopBit;

            // シリアルポートのオープン
            this._port.Open();
            this._port.DtrEnable = true;
            this._port.RtsEnable = true;
        }


        /// <summary>
        /// データを送付する
        /// </summary>
        /// <param name="data"></param>
        /// <remarks></remarks>
        public void Send(byte[] data)
        {
            if (data == null)
            {
                Xb.Util.Out("Xb.Device.SerialPort.Send: 渡し値データが不正です。");
                throw new ArgumentException("Xb.Device.SerialPort.Send: 渡し値データが不正です。");
            }
            if (this._port == null || !this._port.IsOpen)
            {
                Xb.Util.Out("Xb.Device.SerialPort.Send: ポートがオープンしていません。");
                throw new ApplicationException("Xb.Device.SerialPort.Send: ポートがオープンしていません。");
            }

            this._port.Write(data, 0, data.GetLength(0));
            this._port.DiscardOutBuffer();
        }


        /// <summary>
        /// データ受信時のイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>

        private void Recieve(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // シリアルポートからデータ受信
            byte[] data = new byte[_port.BytesToRead];
            this._port.Read(data, 0, data.GetLength(0));
            this._port.DiscardInBuffer();

            this.FireEvent("Recieved", data);
        }


        /// <summary>
        /// ポートを閉じる。
        /// </summary>
        /// <remarks></remarks>

        public void Close()
        {
            if (this._port == null || !this._port.IsOpen)
            {
                Xb.Util.Out("Xb.Device.SerialPort.Close: ポートがオープンしていません。");
                throw new ApplicationException("Xb.Device.SerialPort.Close: ポートがオープンしていません。");
            }
            this._port.Close();
        }


        // 重複する呼び出しを検出するには
        private bool _disposedValue = false;

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    if (((this._port != null) && this._port.IsOpen))
                    {
                        this._port.Close();
                    }
                    this._port = null;
                }
            }
            this._disposedValue = true;
        }

        #region " IDisposable Support "
        // このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(ByVal disposing As Boolean) に記述します。
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

}
