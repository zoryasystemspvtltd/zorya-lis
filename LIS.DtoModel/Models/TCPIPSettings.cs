﻿namespace LIS.DtoModel
{
    public class TCPIPSettings
    {
       
        public string IPAddress { get; set; }
        public int PortNo { get; set; }
        public bool AutoConnect { get; set; }
        public string TestName { get; set; }
        public bool RunOnStartup { get; set; }
    }
}
