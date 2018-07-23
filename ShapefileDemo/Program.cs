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
			// File source : https://www.naturalearthdata.com/
			string shpPath = @"\\Mac\Home\Downloads\ne_50m_admin_0_map_subunits\ne_50m_admin_0_map_subunits.shp";

			using (Shapefile shapefile = new Shapefile(shpPath))
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




			Console.WriteLine("Done");
			Console.WriteLine();
		}
	}
}
