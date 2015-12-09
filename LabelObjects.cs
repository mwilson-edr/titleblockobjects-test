using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csharppdf
{
    public class LabelObjects:IDisposable 
    {

        bool m_isDisposed = false;

        string m_labelText;
        int m_labelX;
        int m_labelY;
        int m_labelFontSize;
        //font?
        //color?

        public string LabelText
        {
            get { return m_labelText; }
            set { m_labelText = value; }
        }

        public int LabelX
        {
            get { return m_labelX; }
            set { m_labelX = value; }
        }

        public int LabelY
        {
            get { return m_labelY; }
            set { m_labelY = value; }
        }

        public int LabelFontSize
        {
            get { return m_labelFontSize; }
            set { m_labelFontSize = value; }
        }


        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!m_isDisposed)
            {
                if (disposing)
                {
                    //if (_resource != null)
                    //   _resource.Dispose();
                    //Console.WriteLine("Object disposed.");
                }

                // Indicate that the instance has been disposed.
                //_resource = null;
                m_isDisposed = true;
            }
        }
    }
}
