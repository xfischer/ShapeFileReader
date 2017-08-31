/* ------------------------------------------------------------------------
 * (c)copyright 2009-2012 Catfood Software and contributors - http://catfood.net
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Catfood.Shapefile;
using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Win32.SafeHandles;
using static Catfood.Shapefile.Unzip;

namespace ShapefileDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            string zipPath = @"c:\\Users\\sliu\\desktop\\yourfile.zip";
            string csvPath = @"c:\\Users\\sliu\\desktop\\yourfile.csv";

            Stream strCsv = File.OpenRead(csvPath);


            if (csvPath.Contains(".csv"))
            {
                StreamReader reader = new StreamReader(strCsv);
                string[] columnsInit = reader.ReadLine().Split(',');
                string[] values = reader.ReadLine().Split(',');
                List<string> columns = new List<string>();

                Decimal value;

                for(int i = 0; i < columnsInit.Length; i++)
                {
                   if(values[i] == "")
                   {
                      values[i] = "0";  
                   }
                   
                   if(!columnsInit[i].ToLower().Contains("lat") && !columnsInit[i].ToLower().Contains("lon") && Decimal.TryParse(values[i], out value))
                   {
                        columns.Add(columnsInit[i]);
                   }
                }

                reader.Dispose();

            }


            Stream fs = File.OpenRead(zipPath);
            Stream strShp = new MemoryStream();
            Stream strShx = new MemoryStream();
            Stream strDbf = new MemoryStream();

            if (zipPath.Contains(".zip"))
            {
                Unzip unzip = new Unzip(fs);

                Entry[] entries = unzip.Entries;

                foreach (Entry entry in entries)
                {
                    if (entry.Name.Contains(".shp"))
                    {
                        strShp = unzip.Extract(entry);
                        if (strShp.CanSeek)
                        {
                            strShp.Seek(0, SeekOrigin.Begin);
                        }
                    }
                    if (entry.Name.Contains(".shx"))
                    {
                        strShx = unzip.Extract(entry);
                        if (strShx.CanSeek)
                        {
                            strShx.Seek(0, SeekOrigin.Begin);
                        }
                    }
                    if (entry.Name.Contains(".dbf"))
                    {
                        strDbf = unzip.Extract(entry);
                        if (strDbf.CanSeek)
                        {
                            strDbf.Seek(0, SeekOrigin.Begin);
                        }
                    }
                }



                //Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(fs);
                //foreach (ZipEntry e in zip)
                //{
                //    if (e.FileName.Contains(".shp"))
                //    {
                //        e.Extract(strShp);
                //        if (strShp.CanSeek)
                //        {
                //            strShp.Seek(0, SeekOrigin.Begin);
                //        }
                //    }
                //    if (e.FileName.Contains(".shx"))
                //    {
                //        e.Extract(strShx);
                //        if (strShx.CanSeek)
                //        {
                //            strShx.Seek(0, SeekOrigin.Begin);
                //        }
                //    }
                //    if (e.FileName.Contains(".dbf"))
                //    {
                //        e.Extract(strDbf);
                //        if (strDbf.CanSeek)
                //        {
                //            strDbf.Seek(0, SeekOrigin.Begin);
                //        }
                //    }
                //}

                if (strShp != null && strShx != null && strDbf != null)
                {
                    using (Shapefile shapefile = new Shapefile(strShp, strShx, strDbf))
                    {

                        //Console.WriteLine("ShapefileDemo Dumping {0}", args[0]);
                        Console.WriteLine();

                        // a shapefile contains one type of shape (and possibly null shapes)
                        Console.WriteLine("Type: {0}, Shapes: {1:n0}", shapefile.Type, shapefile.Count);

                        // a shapefile also defines a bounding box for all shapes in the file
                        Console.WriteLine("Bounds: {0},{1} -> {2},{3}",
                            shapefile.BoundingBox.Left,
                            shapefile.BoundingBox.Top,
                            shapefile.BoundingBox.Right,
                            shapefile.BoundingBox.Bottom);
                        Console.WriteLine();

                        // enumerate all shapes
                        foreach (Shape shape in shapefile)
                        {
                            Console.WriteLine("----------------------------------------");
                            Console.WriteLine("Shape {0:n0}, Type {1}", shape.RecordNumber, shape.Type);

                            // each shape may have associated metadata
                            string[] metadataNames = shape.GetMetadataNames();
                            if (metadataNames != null)
                            {
                                Console.WriteLine("Field data:");
                                foreach (string metadataName in metadataNames)
                                {
                                    Console.WriteLine("{0}={1}", metadataName, shape.GetMetadata(metadataName));
                                }
                                Console.WriteLine();
                            }

                            // cast shape based on the type
                            switch (shape.Type)
                            {
                                case ShapeType.Point:
                                    // a point is just a single x/y point
                                    ShapePoint shapePoint = shape as ShapePoint;
                                    Console.WriteLine("Point={0},{1}", shapePoint.Point.X, shapePoint.Point.Y);
                                    break;

                                case ShapeType.Polygon:
                                    // a polygon contains one or more parts - each part is a list of points which
                                    // are clockwise for boundaries and anti-clockwise for holes 
                                    // see http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
                                    ShapePolygon shapePolygon = shape as ShapePolygon;
                                    foreach (PointD[] part in shapePolygon.Parts)
                                    {
                                        Console.WriteLine("Polygon part:");
                                        foreach (PointD point in part)
                                        {
                                            Console.WriteLine("{0}, {1}", point.X, point.Y);
                                        }
                                        Console.WriteLine();
                                    }
                                    break;

                                default:
                                    // and so on for other types...
                                    break;
                            }

                            Console.WriteLine("----------------------------------------");
                            Console.WriteLine();
                        }

                    }

                    strShp.Dispose();
                    strShx.Dispose();
                    strDbf.Dispose();
                }

            }

            

            // Pass the path to the shapefile in as the command line argument
            //args = new string[] { "C:\\Users\\sliu\\Desktop\\TestData\\Home West 1998 Corn\\Home West 1998 Corn.shp" };

            //if ((args.Length == 0) || (!File.Exists(args[0])))
            //{
            //    Console.WriteLine("Usage: ShapefileDemo <shapefile.shp>");
            //    return;
            //}

           
            // construct shapefile with the path to the .shp file
            //using (Shapefile shapefile = new Shapefile(args[0]))
            //{
            //    strShp.Close();
            //    strShx.Close();
            //    strDbf.Close();
            //    Console.WriteLine("ShapefileDemo Dumping {0}", args[0]);
            //    Console.WriteLine();

            //    // a shapefile contains one type of shape (and possibly null shapes)
            //    Console.WriteLine("Type: {0}, Shapes: {1:n0}", shapefile.Type, shapefile.Count);

            //    // a shapefile also defines a bounding box for all shapes in the file
            //    Console.WriteLine("Bounds: {0},{1} -> {2},{3}",
            //        shapefile.BoundingBox.Left,
            //        shapefile.BoundingBox.Top,
            //        shapefile.BoundingBox.Right,
            //        shapefile.BoundingBox.Bottom);
            //    Console.WriteLine();

            //    // enumerate all shapes
            //    foreach (Shape shape in shapefile)
            //    {
            //        Console.WriteLine("----------------------------------------");
            //        Console.WriteLine("Shape {0:n0}, Type {1}", shape.RecordNumber, shape.Type);

            //        // each shape may have associated metadata
            //        string[] metadataNames = shape.GetMetadataNames();
            //        if (metadataNames != null)
            //        {
            //            Console.WriteLine("Field data:");
            //            foreach (string metadataName in metadataNames)
            //            {
            //                Console.WriteLine("{0}={1} ({2})", metadataName, shape.GetMetadata(metadataName), shape.DataRecord.GetDataTypeName(shape.DataRecord.GetOrdinal(metadataName)));
            //            }
            //            Console.WriteLine();
            //        }

            //        // cast shape based on the type
            //        switch (shape.Type)
            //        {
            //            case ShapeType.Point:
            //                // a point is just a single x/y point
            //                ShapePoint shapePoint = shape as ShapePoint;
            //                Console.WriteLine("Point={0},{1}", shapePoint.Point.X, shapePoint.Point.Y);
            //                break;

            //            case ShapeType.Polygon:
            //                // a polygon contains one or more parts - each part is a list of points which
            //                // are clockwise for boundaries and anti-clockwise for holes 
            //                // see http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
            //                ShapePolygon shapePolygon = shape as ShapePolygon;
            //                foreach (PointD[] part in shapePolygon.Parts)
            //                {
            //                    Console.WriteLine("Polygon part:");
            //                    foreach (PointD point in part)
            //                    {
            //                        Console.WriteLine("{0}, {1}", point.X, point.Y);
            //                    }
            //                    Console.WriteLine();
            //                }
            //                break;

            //            default:
            //                // and so on for other types...
            //                break;
            //        }

            //        Console.WriteLine("----------------------------------------");
            //        Console.WriteLine();
            //    }

            //}

            Console.WriteLine("Done");
            Console.WriteLine();
        }
    }
}
