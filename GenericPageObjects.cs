using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using iTextSharp.text;
using System.Data ;

namespace csharppdf
{
    class GenericPageObjects : IDisposable
    {

        /// <summary>
        /// Report Visibility equates to the reportvisibility flag in the xml.
        /// </summary>
        /// 
        //alignment
        //0=left
        //1=center
        //2=right
        public enum ReportVisibility
        {
            None = 0,
            All = 1,
            Aerial = 2,
            Sanborn = 4,
            Topo = 8
            //et cetera
        }

        bool m_isDisposed;

        private List<csharppdf.LabelObjects> m_thisLabelList;

        private List<iTextSharp.text.pdf.TextField> m_thisFieldList;

        private List<csharppdf.AnnotationObjects> m_thisAnnotationList;

        private List<iTextSharp.text.pdf.PdfLine> m_thisLineList;

        private List<iTextSharp.text.Rectangle> m_thisRectList;

        public List<csharppdf.LabelObjects> LabelList
        {
            get { return m_thisLabelList; }
        }

        public List<iTextSharp.text.pdf.TextField> TextFieldList
        {
            get { return m_thisFieldList; }
        }

        public List<iTextSharp.text.Rectangle> RectFieldList
        {
            get { return m_thisRectList; }
        }

        /// <summary>
        /// Initialize the object collection with xml.
        /// </summary>
        /// <param name="inXML">input XML to parse into field/page objects</param>
        public void Initialize(string inXML, iTextSharp.text.pdf.PdfStamper inStamper,ReportVisibility  inReportType,DataTable inDataTableToMerge)
        {

            XDocument xdoc = XDocument.Parse(inXML);

            iTextSharp.text.pdf.BaseFont bf = iTextSharp.text.pdf.BaseFont.CreateFont(iTextSharp.text.pdf.BaseFont.HELVETICA, iTextSharp.text.pdf.BaseFont.CP1252, iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);

            var str = XElement.Parse(inXML);

            //text fields
            var tempfields = str.Elements("templatefields").ToList();
            var fields = tempfields.Elements("templatefield").ToList();

            if (fields.Count > 0)
            {
                m_thisFieldList = new List<iTextSharp.text.pdf.TextField>();
            }

            foreach (XElement thisXMLField in fields)
            {

                bool thisFlatten;
                string thisFieldName;
                string thisFieldValue;
                Rectangle thisBBox;
                int thisAlignment = 0;
                int thisFontSize;
                int thisVisibility;

                var points = thisXMLField.Element("points");
                var rgb = thisXMLField.Element("rgb");

                string[] rectPointsArray = points.Value.ToString().Split(',');
                string[] rectRGBArray = rgb.Value.ToString().Split(',');

                thisFieldName = thisXMLField.Element("fieldname").Value.ToString();
                thisFieldValue = thisXMLField.Element("fieldvalue").Value.ToString();
                int.TryParse(thisXMLField.Element("fieldvisibility").Value, out thisVisibility);
                bool.TryParse(thisXMLField.Element("istoflatten").Value, out thisFlatten);
                int.TryParse(thisXMLField.Element("alignment").Value, out thisAlignment);
                int.TryParse(thisXMLField.Element("textboxfontsize").Value, out thisFontSize);

                //et cetera


                if (thisVisibility == 1 || thisVisibility == inReportType)
                {

                    thisBBox = new iTextSharp.text.Rectangle(float.Parse(rectPointsArray[0]), float.Parse(rectPointsArray[1]), float.Parse(rectPointsArray[2]), float.Parse(rectPointsArray[3]));
                    if (rectRGBArray.Length > 1)
                    {
                        thisBBox.BorderColor = new BaseColor(int.Parse(rectRGBArray[0]), int.Parse(rectRGBArray[1]), int.Parse(rectRGBArray[2]));
                    }


                    iTextSharp.text.pdf.TextField tempField = new iTextSharp.text.pdf.TextField(inStamper.Writer, thisBBox, thisFieldName);

                    if (thisFieldValue.Length > 0)
                    {
                        tempField.Text = thisFieldValue;
                    }

                    if (thisFlatten)
                    {
                        inStamper.PartialFormFlattening(thisFieldName);
                        tempField.Options = iTextSharp.text.pdf.TextField.READ_ONLY;
                    }

                    //alignment = default left=0
                    if (thisAlignment > 0)
                    {
                        tempField.Alignment = thisAlignment;
                    }

                    tempField.FontSize = thisFontSize;

                    m_thisFieldList.Add(tempField);
                }
            }

            //annotations
            tempfields = str.Elements("annotationobjects").ToList();
            var annotations = tempfields.Elements("annotationobject").ToList();
            if (annotations.Count() > 0)
            {
                m_thisAnnotationList = new List<csharppdf.AnnotationObjects>();
            }

            foreach (XElement thisXMLField in annotations)
            {
                //visibility

                csharppdf.AnnotationObjects thisAnno = new csharppdf.AnnotationObjects();

                iTextSharp.text.pdf.PdfAppearance highlight_ap = iTextSharp.text.pdf.PdfAppearance.CreateAppearance(inStamper.Writer, 100, 25);
                //add color from xml
                highlight_ap.SetColorFill(iTextSharp.text.BaseColor.RED);
                //add font and size from xml
                highlight_ap.SetFontAndSize(bf, 11);

                string rectPointsString = thisXMLField.Element("RectPoints").Value.ToString();
                string linePointsString = thisXMLField.Element("LinePoints").Value.ToString();

                string[] rectPointsArray = rectPointsString.Split(',');
                string[] linePointsArray = linePointsString.Split(',');

                iTextSharp.text.pdf.PdfAnnotation aCallOut = iTextSharp.text.pdf.PdfAnnotation.CreateFreeText(inStamper.Writer, new iTextSharp.text.Rectangle(float.Parse(rectPointsArray[0]), float.Parse(rectPointsArray[1]), float.Parse(rectPointsArray[2]), float.Parse(rectPointsArray[3])), "Target Property", highlight_ap);
                int[] CalloutPoints = { int.Parse(linePointsArray[0]), int.Parse(linePointsArray[1]), int.Parse(linePointsArray[2]), int.Parse(linePointsArray[3]), int.Parse(linePointsArray[4]), int.Parse(linePointsArray[5]) };
                thisAnno.InitializeCallout(CalloutPoints);
                aCallOut = thisAnno.GenerateCallOutBox(aCallOut);

                m_thisAnnotationList.Add(thisAnno);

            }

            //same for lines

            //rects
            tempfields = str.Elements("rectobjects").ToList();
            var rects = tempfields.Elements("rectobject").ToList();
            if (rects.Count() > 0)
            {
                m_thisRectList = new List<iTextSharp.text.Rectangle>();
            }

            foreach (XElement thisXMLRect in rects)
            {
                //visibility

                var points = thisXMLRect.Element("points");
                var rgb = thisXMLRect.Element("rgb");
                string linewidth = thisXMLRect.Element("linewidth").Value.ToString();

                string[] rectPointsArray = points.Value.ToString().Split(',');
                string[] rectRGBArray = rgb.Value.ToString().Split(',');



                iTextSharp.text.Rectangle temprect = new iTextSharp.text.Rectangle(float.Parse(rectPointsArray[0]), float.Parse(rectPointsArray[1]), float.Parse(rectPointsArray[2]), float.Parse(rectPointsArray[3]));
                temprect.BorderColor = new BaseColor(int.Parse(rectRGBArray[0]), int.Parse(rectRGBArray[1]), int.Parse(rectRGBArray[2]));
                temprect.BorderWidth = float.Parse(linewidth);
                temprect.Border = iTextSharp.text.Rectangle.BOX;

                m_thisRectList.Add(temprect);
            }

            //labels
            tempfields = str.Elements("labelobjects").ToList();
            var labels = tempfields.Elements("labelobject").ToList();

            if (labels.Count() > 0)
            {
                m_thisLabelList = new List<csharppdf.LabelObjects>();
            }

            foreach (XElement thisXMLLabel in labels)
            {
                //visibility

                csharppdf.LabelObjects templabel = new csharppdf.LabelObjects();
                var points = thisXMLLabel.Element("points");
                string[] labelPointsArray = points.Value.ToString().Split(',');

                templabel.LabelText = thisXMLLabel.Element("labeltext").Value.ToString();
                templabel.LabelX = int.Parse(labelPointsArray[0]);
                templabel.LabelY = int.Parse(labelPointsArray[1]);
                templabel.LabelFontSize = int.Parse(thisXMLLabel.Element("labelfontsize").Value.ToString());

                m_thisLabelList.Add(templabel);
            }




        }

        private void PopulateFieldValuesFromReportItems(XElement inXML,DataTable inDT)
        {
            DataTable  xmlDT = new DataTable();
            xmlDT.ReadXml(new System.IO.StringReader(inXML.ToString()));

            foreach(DataRow thisDataRow in inDT.Rows )
            {
                gakkit;
            }
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
