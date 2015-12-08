using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using iTextSharp.text ;

namespace TestHarnessCSharp
{
    class testPageObjects:IDisposable
    {

        bool m_isDisposed;

        private List<iTextSharp.text.pdf.TextField> m_thisFieldList;

        private List<csharppdf.AnnotationObjects > m_thisAnnotationList;

        private List<iTextSharp.text.pdf.PdfLine> m_thisLineList;

        private List<iTextSharp.text.pdf.PdfRectangle> m_thisRectList;

        /// <summary>
        /// Initialize the object collection with xml.
        /// </summary>
        /// <param name="inXML">input XML to parse into field/page objects</param>
        public void Initialize(string inXML,iTextSharp.text.pdf.PdfWriter inWriter,iTextSharp.text.pdf.PdfStamper inStamper)
        {
            XDocument xdoc = XDocument.Parse(inXML);

            XElement templatefields = xdoc.Element("templatefields");
            XElement annotationobjects = xdoc.Element("annotationobjects");
            XElement lineobjects = xdoc.Element("lineobjects");
            XElement rectobjects = xdoc.Element("rectobjects");

            //foreach templatefield

            var str = XElement.Parse(inXML);

            var fields = str.Elements("templatefields").ToList();

            foreach (XElement thisXMLField in fields)
            {
                
                float thisR;
                float thisG;
                float thisB;
                bool thisFlatten;
                string thisFieldName;
                Rectangle thisBBox;
                float thisBBoxLLX;
                float thisBBoxLLY;
                float thisBBoxURX;
                float thisBBoxURY;

                thisFieldName = thisXMLField.Element("fieldname").Value.ToString();
                float.TryParse(thisXMLField.Element("boundingcolorR").Value, out  thisR);
                float.TryParse(thisXMLField.Element("boundingcolorG").Value, out  thisG);
                float.TryParse(thisXMLField.Element("boundingcolorB").Value, out  thisB);
                bool.TryParse (thisXMLField.Element ("isFlatten").Value ,out thisFlatten );
                float.TryParse(thisXMLField.Element("BBoxLLX").Value, out thisBBoxLLX);
                float.TryParse(thisXMLField.Element("BBoxLLY").Value, out thisBBoxLLY);
                float.TryParse(thisXMLField.Element("BBoxURX").Value, out thisBBoxURX);
                float.TryParse(thisXMLField.Element("BBoxURY").Value, out thisBBoxURY);
                //et cetera

                thisBBox = new Rectangle(thisBBoxLLX, thisBBoxLLY, thisBBoxURX, thisBBoxURY);

                iTextSharp.text.pdf.TextField tempField = new iTextSharp.text.pdf.TextField(inWriter, thisBBox , thisFieldName);

                BaseColor thisBaseColor = new BaseColor(thisR  , thisG,thisB );
                tempField.BorderColor = thisBaseColor;
                if (thisFlatten )
                {
                    inStamper.PartialFormFlattening(thisFieldName);
                }

                m_thisFieldList.Add(tempField);
            }

            //Console.WriteLine(result);

            //read for annotations etc
            var annotations = str.Elements("annotationobjects").ToList();
            foreach (XElement thisXMLField in annotations)
            {
                csharppdf.AnnotationObjects thisAnno = new csharppdf.AnnotationObjects();

                iTextSharp.text.pdf.PdfAppearance highlight_ap = iTextSharp.text.pdf.PdfAppearance.CreateAppearance(inWriter, 100, 25);
                //add color from xml
                highlight_ap.SetColorFill(iTextSharp.text.BaseColor.RED);
                //add font and size from xml
                highlight_ap.SetFontAndSize(thisBaseFont, 11);

                string rectPointsString = thisXMLField.Element("RectPoints").Value.ToString();
                string linePointsString = thisXMLField.Element("LinePoints").Value.ToString();

                string[] rectPointsArray = rectPointsString.Split(',');
                string[] linePointsArray = linePointsString.Split(',');

                iTextSharp.text.pdf.PdfAnnotation aCallOut = iTextSharp.text.pdf.PdfAnnotation.CreateFreeText(inWriter , new iTextSharp.text.Rectangle(float.Parse(rectPointsArray[0]), float.Parse(rectPointsArray[1]), float.Parse(rectPointsArray[2]), float.Parse(rectPointsArray[3])), "Target Property", highlight_ap);
                int[] CalloutPoints = { int.Parse(linePointsArray[0]), int.Parse(linePointsArray[1]), int.Parse(linePointsArray[2]), int.Parse(linePointsArray[3]), int.Parse(linePointsArray[4]), int.Parse(linePointsArray[5]) };
                thisAnno.InitializeCallout(CalloutPoints);
                aCallOut = thisAnno.GenerateCallOutBox(aCallOut);

                m_thisAnnotationList.Add(thisAnno);
                
            }

            //same for lines

            //same for rects



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
