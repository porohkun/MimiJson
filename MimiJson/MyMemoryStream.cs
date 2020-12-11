using System.IO;

namespace MimiJson
{
#if !NET45
    public class MyMemoryStream : MemoryStream
    {
        public override void Close() { }

        public void ReallyClose()
        {
            base.Close();
        }
    }
#endif
}
