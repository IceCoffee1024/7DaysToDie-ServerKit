namespace SdtdServerKit.Models
{
    public class SystemInfo
    {
        public string DeviceModel { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string DeviceUniqueIdentifier { get; set; }
        public string OperatingSystem { get; set; }
        public string OperatingSystemFamily { get; set; }
        public int ProcessorCount { get; set; }
        public int ProcessorFrequency { get; set; }
        public string ProcessorType { get; set; }
        public int SystemMemorySize { get; set; }
    }
}
