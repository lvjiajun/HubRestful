

namespace HubRestful.Configure
{
    public class S7Setting:IS7setting
    {
        public string Url { get; set; }
        
        public uint Rack { get; set; }
        
        public uint Slot { get; set; }
    }

    public interface IS7setting
    {
        string Url { get; set; }
        
        uint Rack { get; set; }
        
        uint Slot { get; set; }
    }
}