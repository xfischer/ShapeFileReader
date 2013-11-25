namespace ShapeFileReader
{
	partial class Form1
	{
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.lblShapeType = new System.Windows.Forms.Label();
			this.lblBBox = new System.Windows.Forms.Label();
			this.lblProj = new System.Windows.Forms.Label();
			this.lblNumShapes = new System.Windows.Forms.Label();
			this.btnNTSTest = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 12);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Catfood test";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// lblShapeType
			// 
			this.lblShapeType.AutoSize = true;
			this.lblShapeType.Location = new System.Drawing.Point(130, 22);
			this.lblShapeType.Name = "lblShapeType";
			this.lblShapeType.Size = new System.Drawing.Size(68, 13);
			this.lblShapeType.TabIndex = 1;
			this.lblShapeType.Tag = "ShapeType :";
			this.lblShapeType.Text = "ShapeType :";
			// 
			// lblBBox
			// 
			this.lblBBox.AutoSize = true;
			this.lblBBox.Location = new System.Drawing.Point(130, 50);
			this.lblBBox.Name = "lblBBox";
			this.lblBBox.Size = new System.Drawing.Size(41, 13);
			this.lblBBox.TabIndex = 2;
			this.lblBBox.Tag = "BBox  :";
			this.lblBBox.Text = "BBox  :";
			// 
			// lblProj
			// 
			this.lblProj.AutoSize = true;
			this.lblProj.Location = new System.Drawing.Point(130, 78);
			this.lblProj.Name = "lblProj";
			this.lblProj.Size = new System.Drawing.Size(60, 13);
			this.lblProj.TabIndex = 3;
			this.lblProj.Tag = "Projection :";
			this.lblProj.Text = "Projection :";
			// 
			// lblNumShapes
			// 
			this.lblNumShapes.AutoSize = true;
			this.lblNumShapes.Location = new System.Drawing.Point(130, 104);
			this.lblNumShapes.Name = "lblNumShapes";
			this.lblNumShapes.Size = new System.Drawing.Size(74, 13);
			this.lblNumShapes.TabIndex = 4;
			this.lblNumShapes.Tag = "NumShapes : ";
			this.lblNumShapes.Text = "NumShapes : ";
			// 
			// btnNTSTest
			// 
			this.btnNTSTest.Location = new System.Drawing.Point(12, 45);
			this.btnNTSTest.Name = "btnNTSTest";
			this.btnNTSTest.Size = new System.Drawing.Size(75, 23);
			this.btnNTSTest.TabIndex = 5;
			this.btnNTSTest.Text = "NTS test";
			this.btnNTSTest.UseVisualStyleBackColor = true;
			this.btnNTSTest.Click += new System.EventHandler(this.btnNTSTest_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(551, 339);
			this.Controls.Add(this.btnNTSTest);
			this.Controls.Add(this.lblNumShapes);
			this.Controls.Add(this.lblProj);
			this.Controls.Add(this.lblBBox);
			this.Controls.Add(this.lblShapeType);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label lblShapeType;
		private System.Windows.Forms.Label lblBBox;
		private System.Windows.Forms.Label lblProj;
		private System.Windows.Forms.Label lblNumShapes;
		private System.Windows.Forms.Button btnNTSTest;
	}
}

