using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;

namespace RaaiVan.Modules.GlobalUtilities
{
    public static class SMSSender
    {
        private static GsmCommMain _GsmComm;

        private static bool _connect2gsm()
        {
            string[] _coms = SerialPort.GetPortNames();

            foreach (string com in _coms)
            {
                _GsmComm = new GsmCommMain(com, 9600, 150);

                try
                {
                    if (!_GsmComm.IsOpen()) _GsmComm.Open();
                    if (_GsmComm.IsOpen()) return true;
                }
                catch (Exception e) { string st = e.Message; }
            }

            return false;
        }

        private static bool _disconnect_from_gsm()
        {
            _GsmComm.Close();
            if (!_GsmComm.IsOpen()) return true;
            return false;
        }

        public static bool send(string phoneNumber, string message)
        {
            bool unicode = true;
            bool alert = false;

            if (((_GsmComm == null || !_GsmComm.IsOpen()) && !_connect2gsm()) || string.IsNullOrEmpty(message)) return false;

            try
            {
                SmsSubmitPdu pdu;

                if (!alert && !unicode)
                    pdu = new SmsSubmitPdu(message, phoneNumber, "");
                else
                {
                    byte dcs;

                    if (!alert && unicode) dcs = DataCodingScheme.NoClass_16Bit;
                    else if (alert && !unicode) dcs = DataCodingScheme.Class0_7Bit;
                    else if (alert && unicode) dcs = DataCodingScheme.Class0_16Bit;
                    else dcs = DataCodingScheme.NoClass_7Bit;

                    pdu = new SmsSubmitPdu(message, phoneNumber, "", dcs);

                    _GsmComm.SendMessage(pdu);
                }

            }
            catch (Exception ex) { return false; }

            return true;
        }
    }
}
