using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf;

namespace csharppdf
{
    public class AnnotationObjects:IDisposable
    {
        bool m_isDisposed;

        //inputs are:
        //1.bounding box in pixels
        //2.bounding box in latlong 
        //3.client TP Polygon string as it comes out of ABS
        //4.text callout parameters
        //outputs are:
        //1.a PDF array for the polygon in page coordinates

        //client and request information
        private string m_tpPolygonData;
        private char m_tpPolygonSplitCharacter1;
        private char m_tpPolygonSplitCharacter2;
        private iTextSharp.text.BaseColor m_polygonBaseColor;
        private double m_BBoxMinLat;
        private double m_BBoxMinLon;
        private double m_BBoxMaxLat;
        private double m_BBoxMaxLon;
        private double m_LatDelta;
        private double m_LonDelta;

        //page information
        private double m_PageMapMinX;
        private double m_PageMapMinY;
        private double m_PageMapMaxX;
        private double m_PageMapMaxY;
        private double m_PageMapXDelta;
        private double m_PageMapYDelta;

        //callout
        private int[] m_calloutLinePointsArray;




        /// <summary>
        /// Input parameters for the polygon annotation.
        /// </summary>
        /// <param name="inTPPolygonData">ABS String of polygon data, points delmited by commas, and then the pipe character between points.</param>
        /// <param name="inPolygonBaseColor">Base Color of the polygon</param>
        /// <param name="inBBoxMinLat">BBox of the map request, minimum Latitude</param>
        /// <param name="inBBoxMinLon">BBox of the map request, minimum Longitude</param>
        /// <param name="inBBoxMaxLat">BBox of the map request, maximum Latitude</param>
        /// <param name="inBBoxMaxLon">BBox of the map request, maximum Longitude</param>
        /// <param name="inPageMapMinX">Map Page coordinates, Min X</param>
        /// <param name="inPageMapMinY">Map Page coordinates, Min Y</param>
        /// <param name="inPageMapMaxX">Map Page coordinates, Max X</param>
        /// <param name="inPageMapMaxY">Map Page coordinates, Max Y</param>
        /// <param name="m_tpPolygonSplitCharacter1">String value to separate the points out, currently the pipe character</param>
        /// <param name="m_tpPolygonSplitCharacter2">String value to separate out the x/y values, currently a comma</param>
        public void InitializePolygon(string inTPPolygonData,iTextSharp.text.BaseColor inPolygonBaseColor, double inBBoxMinLat, double inBBoxMinLon, double inBBoxMaxLat, double inBBoxMaxLon,
            double inPageMapMinX, double inPageMapMinY, double inPageMapMaxX, double inPageMapMaxY, char intpPolygonSplitCharacter1, char intpPolygonSplitCharacter2)
        {

            //gateway
            if (inBBoxMaxLat >90 || inBBoxMaxLat < -90) throw new Exception (string.Concat ("Error in InitializePolygon, inBBoxMaxLat out of range at ",inBBoxMaxLat.ToString ()));
            if (inBBoxMaxLon > 180 || inBBoxMaxLon < -180) throw new Exception(string.Concat("Error in InitializePolygon, inBBoxMaxLon out of range at ", inBBoxMaxLon.ToString()));
            if (inBBoxMinLat > 90 || inBBoxMinLat < -90) throw new Exception(string.Concat("Error in InitializePolygon, inBBoxMinLat out of range at ", inBBoxMinLat.ToString()));
            if (inBBoxMaxLon > 180 || inBBoxMaxLon < -180) throw new Exception(string.Concat("Error in InitializePolygon, inBBoxMaxLon out of range at ", inBBoxMaxLon.ToString()));
            if (inBBoxMaxLat < inBBoxMinLat) throw new Exception(string.Concat("Error in InitializePolygon, maxLat ", inBBoxMaxLat.ToString(), " is less than minLat ", inBBoxMinLat.ToString()));
            if (inBBoxMaxLon < inBBoxMinLon) throw new Exception(string.Concat("Error in InitializePolygon, maxLon ", inBBoxMaxLon.ToString(), " is less than minLon ", inBBoxMinLon.ToString()));
            //what about the map box coordinates?
            if (inPageMapMaxX < inPageMapMinX) throw new Exception(string.Concat("Error in InitializePolygon, MaxX ", inPageMapMaxX.ToString(), " is less than MinX ", inPageMapMinX.ToString()));

            m_tpPolygonData = inTPPolygonData;
            m_polygonBaseColor = inPolygonBaseColor;
            m_BBoxMinLat = inBBoxMinLat;
            m_BBoxMinLon = inBBoxMinLon;
            m_BBoxMaxLat = inBBoxMaxLat;
            m_BBoxMaxLon = inBBoxMaxLon;

            m_PageMapMaxX = inPageMapMaxX;
            m_PageMapMaxY = inPageMapMaxY;
            m_PageMapMinX = inPageMapMinX;
            m_PageMapMinY = inPageMapMinY;

            m_tpPolygonSplitCharacter1 = intpPolygonSplitCharacter1;
            m_tpPolygonSplitCharacter2 = intpPolygonSplitCharacter2;


            m_LatDelta = m_BBoxMaxLat - m_BBoxMinLat;
            m_LonDelta =Math.Abs (m_BBoxMaxLon - m_BBoxMinLon );

            m_PageMapXDelta = m_PageMapMaxX - m_PageMapMinX;
            m_PageMapYDelta = m_PageMapMaxY - m_PageMapMinY;


        }

        /// <summary>
        /// Input the parameters required for the callout box.
        /// </summary>
        /// <param name="inCalloutText">Text that shows in the box.</param>
        /// <param name="inCalloutBaseColor">Color for the box.</param>
        /// <param name="inCalloutLinePointsArray">Int array of points that make up the line, format is all comma delimited, x, y.</param>
        /// <param name="inCalloutRectangle">ITextSharp rectangle for the box itself.</param>
        public void InitializeCallout(int[] inCalloutLinePointsArray)
        {
            m_calloutLinePointsArray = inCalloutLinePointsArray;

        }

        public PdfAnnotation GenerateCallOutBox(PdfAnnotation inInitialCallOut)
        {
            PdfAnnotation retVal;
            PdfDictionary aDictionary = (PdfDictionary)inInitialCallOut;

            aDictionary.Put(new PdfName("IT"), new PdfName("FreeTextCallout"));
            aDictionary.Put(new PdfName("CL"), new PdfArray(m_calloutLinePointsArray));

            aDictionary.Put(new PdfName("LE"), new PdfName("OpenArrow"));

            retVal= (PdfAnnotation)aDictionary;

            return retVal;
        }

        public PdfArray  GeneratePolygonPDFArray()
        {
     
            string[] tpPolygonPoints = m_tpPolygonData.Split(new char[] {m_tpPolygonSplitCharacter1  });
            
            List<System.Drawing.Point> pixelPointList = new List<System.Drawing.Point>();

            int[] polyAnnoPoints = new int[tpPolygonPoints.Count() * 2];
            int pointListCounter = 0;
            double testX;
            double testY;

            foreach (string thisString in tpPolygonPoints)
            {
                string[] tpXY = thisString.Split(new char[] { m_tpPolygonSplitCharacter2  });
                testX = returnPixelRatioPoint(m_PageMapXDelta, m_LonDelta, Convert.ToDouble(tpXY[0]) - m_BBoxMinLon, m_PageMapMinX);
                testY = returnPixelRatioPoint(m_PageMapYDelta, m_LatDelta, Convert.ToDouble(tpXY[1]) - m_BBoxMinLat , m_PageMapMinY);


                polyAnnoPoints[pointListCounter] = Convert.ToInt16(testX);
                pointListCounter += 1;
                polyAnnoPoints[pointListCounter] = Convert.ToInt16 (testY);
                pointListCounter += 1;

                

            }

            iTextSharp.text.pdf.PdfArray polyAnnoPointsPDFArray = new iTextSharp.text.pdf.PdfArray(polyAnnoPoints);

            return polyAnnoPointsPDFArray;
        }

        private double returnPixelRatioPoint(double inPixelDelta, double inDegreeDelta, double inDegreePoint, double inStartPoint)
        {
            double retval = 0;

            retval = (inPixelDelta / inDegreeDelta) * inDegreePoint;
            if (retval < 0)
            {
                retval = 0 - retval;
            }
            //add the start point
            retval = retval + inStartPoint;

            return retval;
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
