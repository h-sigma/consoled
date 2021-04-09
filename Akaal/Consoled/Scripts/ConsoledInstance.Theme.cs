using UnityEngine;

namespace Akaal.Consoled
{
    public partial class ConsoledInstance
    {
        #region Theme

        public class ConsoledTheme
        {
            public Color ParseError  = Color.red;
            public Color Error       = Color.red;
            public Color Warning     = Color.yellow;
            public Color Important   = Color.magenta;
            public Color Information = Color.white;
            public Color Command     = Color.green;

            public ConsoledTheme ShallowCopy()
            {
                return (ConsoledTheme) MemberwiseClone();
            }
        }

        #endregion

        #region Public

        public ConsoledTheme Theme { get; private set; } = new ConsoledTheme();

        public void CopyTheme(ConsoledInstance from)
        {
            Theme = from.Theme.ShallowCopy();
        }

        #endregion
    }
}