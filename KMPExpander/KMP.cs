namespace KMPExpander
{
    public class KMP
    {
        public byte[] Data { get; set; }
        public KMP(byte[] Data)
        {
            this.Data = Data;
        }

        public byte this[int i]
        {
            get
            {
                return Data[i];
            }
            set
            {
                Data[i] = value;
            }    
        }
    }
}
