using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems;
using ShapefileReader;

namespace ShapeFileReader
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			ReadShapefile_Catfood();
		}

		private void btnNTSTest_Click(object sender, EventArgs e)
		{
			ReadShapefile_NTS();
		}

		#region Catfood

		private void ReadShapefile_Catfood()
		{
			string v_basePath = @"C:\Users\ext.dev.xfi\Documents\GitHub\ShapeFileReader\sample files";



			try
			{
				string path = Path.Combine(v_basePath, "DEPARTEMENT.shp");
				//string path = Path.Combine(v_basePath, "Contours_45_UG_region.shp");



				using (Shapefile shapefile = new Shapefile(path))
				{

					lblBBox.Text = lblBBox.Tag.ToString() + " " + string.Format("Xmin={0}, Xmax={1}, Ymin={2}, Ymax={3}", shapefile.BoundingBox.Left, shapefile.BoundingBox.Right, shapefile.BoundingBox.Top, shapefile.BoundingBox.Bottom);
					lblShapeType.Text = lblShapeType.Tag.ToString() + " " + shapefile.Type.ToString();
					lblProj.Text = lblProj.Tag.ToString() + " " + shapefile.Projection;
					lblNumShapes.Text = lblNumShapes.Tag.ToString() + " " + shapefile.Count.ToString();

					ICoordinateSystem coordSys = GetCoordinateSystem(shapefile.Projection);

					// enumerate all shapes
					int n = 0;
					foreach (Shape shape in shapefile)
					{
						for (int i = 0; i < shape.DataRecord.FieldCount; i++)
						{
							string name = shape.DataRecord.GetName(i);
							string value = shape.DataRecord[i].ToString();
						}
						n++;
					}
					MessageBox.Show("Read " + n.ToString() + " shapes");
				}
			}
			catch (Exception v_ex)
			{
				MessageBox.Show("Exception: " + v_ex.Message);
			}
		}

		private ICoordinateSystem GetCoordinateSystem(string CoordSysWKT)
		{
			ICoordinateSystem fromCS = CoordinateSystemWktReader.Parse(CoordSysWKT) as ICoordinateSystem;
			return fromCS;

		}
		#endregion


		#region NTS

		private void ReadShapefile_NTS()
		{
			string v_basePath = @"C:\Users\ext.dev.xfi\Documents\GitHub\ShapeFileReader\sample files";



			try
			{
				string path = Path.Combine(v_basePath, "DEPARTEMENT.shp");
				//string path = Path.Combine(v_basePath, "Contours_45_UG_region.shp");


				var reader = new NetTopologySuite.IO.ShapefileReader(path);
				
				lblBBox.Text = lblBBox.Tag.ToString() + " " + reader.Header.Bounds.ToString();
				lblShapeType.Text = lblShapeType.Tag.ToString() + " " + reader.Header.ShapeType.ToString();
				lblProj.Text = lblProj.Tag.ToString() + " unknown";// +" " + shapefile.Projection;
					//ICoordinateSystem coordSys = GetCoordinateSystem(shapefile.Projection);

					// enumerate all shapes
				var geometries = reader.ReadAll();
				lblNumShapes.Text = lblNumShapes.Tag.ToString() + " " + geometries.Count.ToString();
				MessageBox.Show("Read " + geometries.Count.ToString() + " shapes");
				
			}
			catch (Exception v_ex)
			{
				MessageBox.Show("Exception: " + v_ex.Message);
			}
		}

		#endregion


	}
}
