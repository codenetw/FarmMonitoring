namespace Models.common
{
    public class VideoCard
    {
        public string Name { get; set; }
        public Status Status { get; set; }
        public string DriverVersion { get; set; }
        public string DeviceId { get; set; }
    }

    public enum Status
    {
        Ok,
        Disabled,
        Error
    }
}
