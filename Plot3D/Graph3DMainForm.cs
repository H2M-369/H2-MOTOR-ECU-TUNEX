using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using _3D_Chart_Graph;

namespace Plot3D;

public class Graph3DMainForm : Form
{
    private Form1 form1Instance;
    private IContainer components = null;
    private TrackBar trackRho;
    private TrackBar trackTheta;
    private TrackBar trackPhi;
    private Label lblInfo;
    private ComboBox comboColors;
    private ComboBox comboDataSrc;
    private ComboBox comboRaster;
    private Label label2;
    private Label label3;
    private Label label4;
    private Button btnScreenshot;
    private Button btnReset;
    private Graph3D graph3D;
    private Panel settingsPanel;
    private Label lblZoom;

    // Mapping: index grid 3D → baris DataGridView di Form1
    // indexAwalBaris menyimpan offset baris, karena SetSurface() membalik urutan.
    private int lastGridRowCount = 0;
    private int lastGridColCount = 0;

    public Graph3DMainForm()
    {
        InitializeComponent();
        ConfigureGraphLayout();
        form1Instance = Application.OpenForms.OfType<Form1>().FirstOrDefault();
        ComboBox.ObjectCollection items = comboRaster.Items;
        items.AddRange(new object[] { "Raster", "Off" });
        comboRaster.SelectedIndex = 0;
    }

    public void UpdateGraphWithData(int[,] data)
    {
        SetSurface();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        graph3D.AssignTrackBars(trackRho, trackTheta, trackPhi);
        comboRaster.Sorted = false;
        foreach (Graph3D.eRaster value in Enum.GetValues(typeof(Graph3D.eRaster)))
            comboRaster.Items.Add(value);
        comboColors.Sorted = false;
        foreach (ColorSchema.eSchema value2 in Enum.GetValues(typeof(ColorSchema.eSchema)))
            comboColors.Items.Add(value2);
        comboColors.SelectedIndex = 10;
        comboDataSrc.SelectedIndex = 0;

        // Aktifkan mode edit
        graph3D.EditModeEnabled = true;
        graph3D.DataPointEdited += Graph3D_DataPointEdited;
        graph3D.EditCommitted += Graph3D_EditCommitted;
        AddEditControls();
        graph3D.ResetView();
    }

    private void comboColors_SelectedIndexChanged(object sender, EventArgs e)
    {
        Color[] schema = ColorSchema.GetSchema((ColorSchema.eSchema)comboColors.SelectedIndex);
        graph3D.SetColorScheme(schema, 3f);
    }

    private void comboRaster_SelectedIndexChanged(object sender, EventArgs e)
    {
        graph3D.Raster = (Graph3D.eRaster)comboRaster.SelectedIndex;
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
        graph3D.ResetView();
    }

    private void btnScreenshot_Click(object sender, EventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Title = "Save as PNG image";
        saveFileDialog.Filter = "PNG Image|*.png";
        saveFileDialog.DefaultExt = ".png";
        if (DialogResult.Cancel == saveFileDialog.ShowDialog(this)) return;
        Bitmap screenshot = graph3D.GetScreenshot();
        try { screenshot.Save(saveFileDialog.FileName, ImageFormat.Png); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand); }
    }

    private void SetCallback()
    {
        Graph3D.delRendererFunction f_Function = (double X, double Y) =>
        {
            double num = 0.15 * Math.Sqrt(X * X + Y * Y);
            return (num < 1E-10) ? 120.0 : (120.0 * Math.Sin(num) / num);
        };
        graph3D.SetFunction(f_Function, new PointF(-120f, -80f), new PointF(120f, 80f), 5.0, Graph3D.eNormalize.MaintainXYZ);
    }

    private void SetFormula()
    {
        string functionBody = "12 * sin(x) * cos(y) / (sqrt(sqrt(x * x + y * y)) + 0.2)";
        try
        {
            Graph3D.delRendererFunction f_Function = FunctionCompiler.Compile(functionBody);
            graph3D.SetFunction(f_Function, new PointF(-10f, -10f), new PointF(10f, 10f), 0.5, Graph3D.eNormalize.MaintainXYZ);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand); }
    }

    private void comboDataSrc_SelectedIndexChanged(object sender, EventArgs e)
    {
        graph3D.AxisX_Legend = null;
        graph3D.AxisY_Legend = null;
        graph3D.AxisZ_Legend = null;
        if (comboDataSrc.SelectedIndex == 0) SetSurface();
        lblInfo.Text = "Points: " + graph3D.TotalPoints;
    }

    private void SetSurface()
    {
        if (form1Instance == null)
        {
            MessageBox.Show("Form1 instance tidak tersedia.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return;
        }

        double[,] gridData = form1Instance.GetGridData();
        if (gridData.GetLength(0) == 0 || gridData.GetLength(1) == 0)
        {
            MessageBox.Show("Data grid kosong!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return;
        }

        lastGridRowCount = gridData.GetLength(0);
        lastGridColCount = gridData.GetLength(1);

        // Buat array titik 3D. Ukuran = (rows-1) x (cols-1)
        // Karena baris 0 & kolom 0 adalah header.
        Graph3D.cPoint3D[,] array = new Graph3D.cPoint3D[gridData.GetLength(0) - 1, gridData.GetLength(1) - 1];

        for (int num = gridData.GetLength(0) - 1; num >= 1; num--)
        {
            for (int i = 1; i < gridData.GetLength(1); i++)
            {
                array[gridData.GetLength(0) - num - 1, i - 1] = new Graph3D.cPoint3D(
                    (gridData.GetLength(0) - num - 1) * 10,
                    (gridData.GetLength(1) - i) * 500,
                    gridData[num, i]
                );
            }
        }

        graph3D.AxisX_Legend = "X Axis";
        graph3D.AxisY_Legend = "Y Axis";
        graph3D.AxisZ_Legend = "Z Axis";
        double zMin = double.PositiveInfinity, zMax = double.NegativeInfinity;
        for (int r = 1; r < gridData.GetLength(0); r++)
            for (int c = 1; c < gridData.GetLength(1); c++)
            {
                zMin = Math.Min(zMin, gridData[r, c]);
                zMax = Math.Max(zMax, gridData[r, c]);
            }
        double zRange = Math.Max(zMax - zMin, 1.0);
        int decimals = form1Instance.GetGridDecimals();
        graph3D.ZStep = Math.Pow(10, -decimals);
        graph3D.ZMin = (zMin >= 0) ? 0 : zMin - zRange;
        graph3D.ZMax = zMax + zRange;

        graph3D.SetSurfacePoints(array, Graph3D.eNormalize.Separate);
    }

    private void Graph3D_EditCommitted()
    {
        if (form1Instance != null)
        {
            form1Instance.CommitGraphEdit();
        }
    }

    public void SelectGridCell(int gridRow, int gridCol, int gridRowCount)
    {
        int i = gridRowCount - 1 - gridRow;
        int j = gridCol - 1;
        graph3D.SelectPoint(i, j);
    }

    private void AddEditControls()
    {
        Button btnUndo = new Button();
        btnUndo.Text = "Undo (Ctrl+Z)";
        btnUndo.Location = new Point(12, 240);
        btnUndo.Size = new Size(208, 28);
        btnUndo.UseVisualStyleBackColor = true;
        btnUndo.ForeColor = Color.Black;
        btnUndo.Click += (s, e) => graph3D.Undo();
        settingsPanel.Controls.Add(btnUndo);
        btnUndo.BringToFront();

        CheckBox chkEdit = new CheckBox();
        chkEdit.Text = "Edit titik (drag / panah)";
        chkEdit.ForeColor = Color.White;
        chkEdit.AutoSize = true;
        chkEdit.Checked = true;
        chkEdit.Location = new Point(12, 280);
        chkEdit.CheckedChanged += (s, e) => graph3D.EditModeEnabled = chkEdit.Checked;
        settingsPanel.Controls.Add(chkEdit);
        chkEdit.BringToFront();
    }

    // ============================================================
    // Handler edit titik: tulis balik ke Form1
    // ============================================================
    private void Graph3D_DataPointEdited(int i, int j, double newZ)
    {
        if (form1Instance == null) return;

        // Mapping:
        //   Di SetSurface():  i = gridData.GetLength(0) - num - 1  → num = gridData.GetLength(0) - i - 1
        //   dan              j = i_kolom - 1                       → i_kolom = j + 1
        //
        // gridData sendiri: rowCount = dataGridView1.RowCount, colCount = dataGridView1.ColumnCount.
        // gridData[num, k] ↔ dataGridView1[k, num]
        int gridLength0 = lastGridRowCount;  // = dataGridView1.RowCount
        int dgvRow = gridLength0 - i - 1;    // baris di DataGridView
        int dgvCol = j + 1;                  // kolom di DataGridView

        // Lewat method publik di Form1
        form1Instance.SetCellValueFromGraph(dgvRow, dgvCol, newZ);

        lblInfo.Text = $"Edit[{i},{j}] = {newZ:F3}  → DGV[{dgvRow},{dgvCol}]";
    }

    private void SetScatterPlot(bool b_Lines)
    {
        List<Graph3D.cScatter> list = new List<Graph3D.cScatter>();
        for (double num = -22.0; num < 22.0; num += 0.1)
        {
            double num2 = Math.Sin(num) * num;
            double num3 = Math.Cos(num) * num;
            double num4 = num;
            if (num4 > 0.0) num4 /= 3.0;
            list.Add(new Graph3D.cScatter(num2, num3, num4, null));
        }
        if (b_Lines) graph3D.SetScatterLines(list.ToArray(), Graph3D.eNormalize.Separate, 3f);
        else graph3D.SetScatterPoints(list.ToArray(), Graph3D.eNormalize.Separate);
    }

    private void SetValentine()
    {
        List<Graph3D.cScatter> list = new List<Graph3D.cScatter>();
        double num = 0.0;
        double num2 = 0.0;
        for (double num3 = 0.0; num3 <= 4.1469023027385274; num3 += 0.025)
        {
            num = Math.Cos(num3) * 1.5 - 1.5;
            num2 = Math.Sin(num3) * 3.0 + 6.0;
            list.Add(new Graph3D.cScatter(num, 0.0 - num, num2, Brushes.Black));
            list.Add(new Graph3D.cScatter(0.0 - num, num, num2, Brushes.Black));
        }
        double num4 = num / 70.0;
        double num5 = num2 / 70.0;
        while (num2 >= 0.0)
        {
            list.Add(new Graph3D.cScatter(num, 0.0 - num, num2, Brushes.Black));
            list.Add(new Graph3D.cScatter(0.0 - num, num, num2, Brushes.Black));
            num -= num4;
            num2 -= num5;
        }
        graph3D.SetScatterPoints(list.ToArray(), Graph3D.eNormalize.MaintainXYZ);
    }

    private void ConfigureGraphLayout()
    {
        SuspendLayout();
        settingsPanel = new Panel
        {
            Dock = DockStyle.Left, Width = 236, AutoScroll = true,
            BackColor = BackColor, ForeColor = Color.White
        };
        Control[] settings = { label2, comboDataSrc, label3, comboColors, label4, comboRaster,
                               lblInfo, btnReset, btnScreenshot, trackRho, trackTheta, trackPhi };
        foreach (Control control in settings)
        {
            settingsPanel.Controls.Add(control);
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            if (control is ComboBox || control is Button) control.ForeColor = Color.Black;
        }
        label2.Location = new Point(12, 12);
        comboDataSrc.Location = new Point(12, 30); comboDataSrc.Width = 208;
        label3.Location = new Point(12, 62);
        comboColors.Location = new Point(12, 80); comboColors.Width = 208;
        label4.Location = new Point(12, 112);
        comboRaster.Location = new Point(12, 130); comboRaster.Width = 208;
        btnReset.Location = new Point(12, 166); btnReset.Size = new Size(208, 28);
        btnReset.Text = "Reset View";
        btnScreenshot.Location = new Point(12, 202); btnScreenshot.Size = new Size(208, 28);
        trackRho.Location = new Point(12, 334); trackRho.Size = new Size(45, 180);
        trackTheta.Location = new Point(84, 334); trackTheta.Size = new Size(45, 180);
        trackPhi.Location = new Point(156, 334); trackPhi.Size = new Size(45, 180);
        settingsPanel.Controls.Add(new Label { Text = "Distance", AutoSize = true, Location = new Point(12, 314) });
        settingsPanel.Controls.Add(new Label { Text = "Elevation", AutoSize = true, Location = new Point(84, 314) });
        settingsPanel.Controls.Add(new Label { Text = "Rotation", AutoSize = true, Location = new Point(156, 314) });
        lblInfo.AutoSize = false; lblInfo.Location = new Point(12, 526); lblInfo.Size = new Size(208, 64);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 42, Padding = new Padding(6),
            WrapContents = false, AutoScroll = true, BackColor = BackColor, ForeColor = Color.White
        };
        var tooltips = new ToolTip();
        if (components == null) components = new Container();
        components.Add(tooltips);
        var btnSettings = new Button { Text = "Settings", Size = new Size(85, 28), UseVisualStyleBackColor = true, ForeColor = Color.Black };
        btnSettings.Click += (s, e) => settingsPanel.Visible = !settingsPanel.Visible;
        var btnZoomIn = new Button { Text = "+", Size = new Size(36, 28), UseVisualStyleBackColor = true, ForeColor = Color.Black };
        btnZoomIn.Click += (s, e) => graph3D.ZoomIn();
        var btnZoomOut = new Button { Text = "−", Size = new Size(36, 28), UseVisualStyleBackColor = true, ForeColor = Color.Black };
        btnZoomOut.Click += (s, e) => graph3D.ZoomOut();
        var btnFit = new Button { Text = "Reset View", Size = new Size(100, 28), UseVisualStyleBackColor = true, ForeColor = Color.Black };
        btnFit.Click += (s, e) => graph3D.ResetView();
        lblZoom = new Label { Text = "100%", AutoSize = false, Size = new Size(70, 28), TextAlign = ContentAlignment.MiddleLeft };
        graph3D.ZoomChanged += (s, e) => lblZoom.Text = graph3D.ZoomPercent.ToString("0") + "%";
        tooltips.SetToolTip(btnZoomIn, "Zoom in");
        tooltips.SetToolTip(btnZoomOut, "Zoom out");
        tooltips.SetToolTip(btnFit, "Fit graph and reset position");
        tooltips.SetToolTip(btnSettings, "Show / hide settings");
        toolbar.Controls.AddRange(new Control[] { btnSettings, btnZoomOut, btnZoomIn, btnFit, lblZoom });

        graph3D.Anchor = AnchorStyles.None;
        graph3D.Dock = DockStyle.Fill;
        Controls.Add(settingsPanel);
        Controls.Add(toolbar);
        // WinForms docks controls in reverse z-order: top toolbar, sidebar, then fill.
        Controls.SetChildIndex(toolbar, 2);
        Controls.SetChildIndex(settingsPanel, 1);
        Controls.SetChildIndex(graph3D, 0);
        ClientSize = new Size(1280, 800);
        WindowState = FormWindowState.Maximized;
        ResumeLayout(true);
    }

    private void Graph3DMainForm_Load(object sender, EventArgs e) { }
    private void graph3D_Load(object sender, EventArgs e) { }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Plot3D.Graph3DMainForm));
        this.trackRho = new System.Windows.Forms.TrackBar();
        this.trackTheta = new System.Windows.Forms.TrackBar();
        this.trackPhi = new System.Windows.Forms.TrackBar();
        this.lblInfo = new System.Windows.Forms.Label();
        this.comboColors = new System.Windows.Forms.ComboBox();
        this.comboDataSrc = new System.Windows.Forms.ComboBox();
        this.comboRaster = new System.Windows.Forms.ComboBox();
        this.label2 = new System.Windows.Forms.Label();
        this.label3 = new System.Windows.Forms.Label();
        this.label4 = new System.Windows.Forms.Label();
        this.btnScreenshot = new System.Windows.Forms.Button();
        this.btnReset = new System.Windows.Forms.Button();
        this.graph3D = new Plot3D.Graph3D();
        ((System.ComponentModel.ISupportInitialize)this.trackRho).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.trackTheta).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.trackPhi).BeginInit();
        base.SuspendLayout();

        this.trackRho.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        this.trackRho.Location = new System.Drawing.Point(9, 211);
        this.trackRho.Name = "trackRho";
        this.trackRho.Orientation = System.Windows.Forms.Orientation.Vertical;
        this.trackRho.Size = new System.Drawing.Size(45, 203);
        this.trackRho.TabIndex = 10;
        this.trackRho.TickFrequency = 20;
        this.trackRho.TickStyle = System.Windows.Forms.TickStyle.None;

        this.trackTheta.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        this.trackTheta.Location = new System.Drawing.Point(53, 211);
        this.trackTheta.Name = "trackTheta";
        this.trackTheta.Orientation = System.Windows.Forms.Orientation.Vertical;
        this.trackTheta.Size = new System.Drawing.Size(45, 203);
        this.trackTheta.TabIndex = 11;
        this.trackTheta.TickFrequency = 20;
        this.trackTheta.TickStyle = System.Windows.Forms.TickStyle.None;

        this.trackPhi.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
        this.trackPhi.Location = new System.Drawing.Point(98, 211);
        this.trackPhi.Name = "trackPhi";
        this.trackPhi.Orientation = System.Windows.Forms.Orientation.Vertical;
        this.trackPhi.Size = new System.Drawing.Size(45, 203);
        this.trackPhi.TabIndex = 12;
        this.trackPhi.TickFrequency = 20;
        this.trackPhi.TickStyle = System.Windows.Forms.TickStyle.None;

        this.lblInfo.AutoSize = true;
        this.lblInfo.Location = new System.Drawing.Point(101, 147);
        this.lblInfo.Name = "lblInfo";
        this.lblInfo.Size = new System.Drawing.Size(25, 13);
        this.lblInfo.TabIndex = 9;
        this.lblInfo.Text = "Info";

        this.comboColors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboColors.FormattingEnabled = true;
        this.comboColors.Location = new System.Drawing.Point(102, 80);
        this.comboColors.MaxDropDownItems = 30;
        this.comboColors.Name = "comboColors";
        this.comboColors.Size = new System.Drawing.Size(121, 21);
        this.comboColors.TabIndex = 2;
        this.comboColors.SelectedIndexChanged += new System.EventHandler(this.comboColors_SelectedIndexChanged);

        this.comboDataSrc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboDataSrc.FormattingEnabled = true;
        this.comboDataSrc.Items.AddRange(new object[1] { "Graph" });
        this.comboDataSrc.Location = new System.Drawing.Point(102, 40);
        this.comboDataSrc.MaxDropDownItems = 30;
        this.comboDataSrc.Name = "comboDataSrc";
        this.comboDataSrc.Size = new System.Drawing.Size(121, 21);
        this.comboDataSrc.TabIndex = 1;
        this.comboDataSrc.SelectedIndexChanged += new System.EventHandler(this.comboDataSrc_SelectedIndexChanged);

        this.comboRaster.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboRaster.FormattingEnabled = true;
        this.comboRaster.Location = new System.Drawing.Point(102, 119);
        this.comboRaster.MaxDropDownItems = 30;
        this.comboRaster.Name = "comboRaster";
        this.comboRaster.Size = new System.Drawing.Size(121, 21);
        this.comboRaster.TabIndex = 3;
        this.comboRaster.SelectedIndexChanged += new System.EventHandler(this.comboRaster_SelectedIndexChanged);

        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(100, 25);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(70, 13);
        this.label2.TabIndex = 15;
        this.label2.Text = "Data Source:";

        this.label3.AutoSize = true;
        this.label3.Location = new System.Drawing.Point(100, 64);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(76, 13);
        this.label3.TabIndex = 16;
        this.label3.Text = "Color Scheme:";

        this.label4.AutoSize = true;
        this.label4.Location = new System.Drawing.Point(100, 104);
        this.label4.Name = "label4";
        this.label4.Size = new System.Drawing.Size(98, 13);
        this.label4.TabIndex = 17;
        this.label4.Text = "Coordinate System:";

        this.btnScreenshot.Location = new System.Drawing.Point(102, 194);
        this.btnScreenshot.Name = "btnScreenshot";
        this.btnScreenshot.Size = new System.Drawing.Size(121, 23);
        this.btnScreenshot.TabIndex = 5;
        this.btnScreenshot.Text = "Save Screenshot";
        this.btnScreenshot.UseVisualStyleBackColor = true;
        this.btnScreenshot.Click += new System.EventHandler(this.btnScreenshot_Click);

        this.btnReset.Location = new System.Drawing.Point(102, 165);
        this.btnReset.Name = "btnReset";
        this.btnReset.Size = new System.Drawing.Size(121, 23);
        this.btnReset.TabIndex = 4;
        this.btnReset.Text = "Reset Position";
        this.btnReset.UseVisualStyleBackColor = true;
        this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

        this.graph3D.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        this.graph3D.AxisX_Color = System.Drawing.Color.DarkBlue;
        this.graph3D.AxisX_Legend = null;
        this.graph3D.AxisY_Color = System.Drawing.Color.DarkGreen;
        this.graph3D.AxisY_Legend = null;
        this.graph3D.AxisZ_Color = System.Drawing.Color.DarkRed;
        this.graph3D.AxisZ_Legend = null;
        this.graph3D.BackColor = System.Drawing.Color.Black;
        this.graph3D.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
        this.graph3D.Location = new System.Drawing.Point(7, 6);
        this.graph3D.Name = "graph3D";
        this.graph3D.PolygonLineColor = System.Drawing.Color.Black;
        this.graph3D.Raster = Plot3D.Graph3D.eRaster.Off;
        this.graph3D.Size = new System.Drawing.Size(986, 668);
        this.graph3D.TabIndex = 21;
        this.graph3D.TopLegendColor = System.Drawing.Color.Gray;
        this.graph3D.Load += new System.EventHandler(this.graph3D_Load);

        base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
        base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.Black;
        base.ClientSize = new System.Drawing.Size(1000, 680);
        base.Controls.Add(this.graph3D);
        base.Controls.Add(this.btnReset);
        base.Controls.Add(this.btnScreenshot);
        base.Controls.Add(this.label4);
        base.Controls.Add(this.label3);
        base.Controls.Add(this.label2);
        base.Controls.Add(this.comboRaster);
        base.Controls.Add(this.comboDataSrc);
        base.Controls.Add(this.comboColors);
        base.Controls.Add(this.lblInfo);
        base.Controls.Add(this.trackPhi);
        base.Controls.Add(this.trackTheta);
        base.Controls.Add(this.trackRho);
        base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        base.MaximizeBox = true;
        base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        this.MinimumSize = new System.Drawing.Size(560, 480);
        base.Name = "Graph3DMainForm";
        base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Graph3D ";
        base.Load += new System.EventHandler(this.Graph3DMainForm_Load);
        ((System.ComponentModel.ISupportInitialize)this.trackRho).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.trackTheta).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.trackPhi).EndInit();
        base.ResumeLayout(false);
        base.PerformLayout();
    }
}