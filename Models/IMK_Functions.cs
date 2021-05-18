using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class IMK_Functions
    {
        [Key]
        public int Id {get;set;}

        //FRU
        public int FruStatus {get;set;}
        public int FruState {get;set;}
        public int FruSerial {get;set;}
        public int FruProdNo {get;set;}

        ////RET
        public int RetSerial { get; set; }
        public int TMA { get; set; }
        public int RetAntenna { get; set; }

        ////
        public int VSWR { get; set; }
        public int CPRI { get; set; }

        ////Transport
        public int Transport { get; set; }
        public int TransportRoutes { get; set; }
        public int TransportInterfaces { get; set; }

        ////Traffic
        public int MMEStatus { get; set; }
        public int GsmTRX { get; set; }
        public int GsmState { get; set; }
        public int SgwStatus { get; set; }
        //Rat
        public int Traffic3g { get; set; }
        public int Traffic4g { get; set; }
        public int Traffic5g { get; set; }


        ////Health
        //RSSI
        public int RSSIUMTS { get; set; }
        
        public int RSSIFDD { get; set; }//RSSI 4g
        public int RSSITDD { get; set; }//RSSI 4g
        
        public int RSSINR { get; set; }
       
        public int ExternalAlarm { get; set; }
        public int Alarm { get; set; }





    }
}