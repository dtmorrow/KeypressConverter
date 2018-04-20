using System.Windows.Forms;

namespace KeypressConverter
{
    class ConvertedKey
    {
        public Keys Key;
        public int Delay;

        public ConvertedKey() : this(Keys.None, 0) {}

        public ConvertedKey(Keys key, int delay)
        {
            Key = key;
            Delay = delay;
        }

        public override string ToString()
        {
            string returnString = Key.ToString();

            if (Delay > 0)
            {
                returnString += ", KeyUp delayed for " + Delay + " ms";
            }

            return returnString;
        }
    }
}
