#define DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Plot3D;

public class Graph3D : UserControl
{
    public enum eRaster { Raster, Off, MainAxis }
    public enum eNormalize { Separate, MaintainXY, MaintainXYZ }
    public enum eCoord { X, Y, Z }

    private enum eMouseAction { None, Move, Rho, Theta, Phi }

    private class cMouse
    {
        public eMouseAction me_Action;
        public Point mk_LastPos;
        public PointF mk_Offset;
        public TrackBar mi_TrackRho;
        public TrackBar mi_TrackTheta;
        public TrackBar mi_TrackPhi;
        public double md_Rho = VALUES_RHO[2];
        public double md_Theta = VALUES_THETA[2];
        public double md_Phi = VALUES_PHI[2];

        public void AssignTrackbar(eMouseAction e_Trackbar, TrackBar i_Trackbar, EventHandler i_OnScroll)
        {
            if (i_Trackbar != null)
            {
                double[] array = null;
                switch (e_Trackbar)
                {
                    case eMouseAction.Rho: array = VALUES_RHO; mi_TrackRho = i_Trackbar; break;
                    case eMouseAction.Theta: array = VALUES_THETA; mi_TrackTheta = i_Trackbar; break;
                    case eMouseAction.Phi: array = VALUES_PHI; mi_TrackPhi = i_Trackbar; break;
                }
                i_Trackbar.Minimum = (int)array[0];
                i_Trackbar.Maximum = (int)array[1];
                i_Trackbar.Value = (int)array[2];
                i_Trackbar.Scroll += i_OnScroll;
            }
        }

        public void OnTrackBarScroll()
        {
            if (mi_TrackRho != null) md_Rho = mi_TrackRho.Value;
            if (mi_TrackTheta != null) md_Theta = mi_TrackTheta.Value;
            if (mi_TrackPhi != null) md_Phi = mi_TrackPhi.Value;
        }

        public void OnMouseMove(int s32_DiffX, int s32_DiffY)
        {
            switch (me_Action)
            {
                case eMouseAction.Rho:
                    md_Rho += (double)s32_DiffY * VALUES_RHO[3];
                    SetRho(md_Rho);
                    break;
                case eMouseAction.Theta:
                    md_Theta -= (double)s32_DiffY * VALUES_THETA[3];
                    SetTheta(md_Theta);
                    break;
                case eMouseAction.Phi:
                    md_Phi -= (double)s32_DiffX * VALUES_PHI[3];
                    SetPhi(md_Phi);
                    break;
            }
        }

        public void SetRho(double d_Rho)
        {
            md_Rho = d_Rho;
            md_Rho = Math.Max(md_Rho, VALUES_RHO[0]);
            md_Rho = Math.Min(md_Rho, VALUES_RHO[1]);
            if (mi_TrackRho != null) mi_TrackRho.Value = (int)md_Rho;
        }

        public void SetTheta(double d_Theta)
        {
            md_Theta = d_Theta;
            md_Theta = Math.Max(md_Theta, VALUES_THETA[0]);
            md_Theta = Math.Min(md_Theta, VALUES_THETA[1]);
            if (mi_TrackTheta != null) mi_TrackTheta.Value = (int)md_Theta;
        }

        public void SetPhi(double d_Phi)
        {
            md_Phi = d_Phi;
            if (md_Phi > 360.0) md_Phi -= 360.0;
            if (md_Phi < 0.0) md_Phi += 360.0;
            if (mi_TrackPhi != null) mi_TrackPhi.Value = (int)md_Phi;
        }
    }

    public class cPoint3D
    {
        public double md_X;
        public double md_Y;
        public double md_Z;

        public cPoint3D() { }
        public cPoint3D(double X, double Y, double Z) { md_X = X; md_Y = Y; md_Z = Z; }
        public cPoint3D Clone() { return new cPoint3D(md_X, md_Y, md_Z); }
        public bool Equals(cPoint3D i_Point) { return md_X == i_Point.md_X && md_Y == i_Point.md_Y && md_Z == i_Point.md_Z; }

        public double GetValue(eCoord e_Coord)
        {
            return e_Coord switch
            {
                eCoord.X => md_X,
                eCoord.Y => md_Y,
                eCoord.Z => md_Z,
                _ => 0.0,
            };
        }

        public void SetValue(eCoord e_Coord, double d_Value)
        {
            switch (e_Coord)
            {
                case eCoord.X: md_X = d_Value; break;
                case eCoord.Y: md_Y = d_Value; break;
                case eCoord.Z: md_Z = d_Value; break;
            }
        }

        public override string ToString() { return $"{md_X:0.00}, {md_Y:0.00}, {md_Z:0.00}"; }
    }

    public class cPoint2D
    {
        public double md_X;
        public double md_Y;
        public PointF Coord => new PointF((float)md_X, (float)md_Y);
        public bool IsValid => !double.IsNaN(md_X) && Math.Abs(md_X) < 9999.9 && !double.IsNaN(md_Y) && Math.Abs(md_Y) < 9999.9;
        public override string ToString() { return $"{md_X:0.00}, {md_Y:0.00}"; }
    }

    private class cMinMax3D
    {
        public double md_MinX = double.PositiveInfinity;
        public double md_MaxX = double.NegativeInfinity;
        public double md_MinY = double.PositiveInfinity;
        public double md_MaxY = double.NegativeInfinity;
        public double md_MinZ = double.PositiveInfinity;
        public double md_MaxZ = double.NegativeInfinity;
        public cPoint3D mi_Center3D = new cPoint3D();

        public cMinMax3D(cPoint3D[,] i_Points3D)
        {
            for (int i = 0; i < i_Points3D.GetLength(0); i++)
                for (int j = 0; j < i_Points3D.GetLength(1); j++)
                {
                    cPoint3D cPoint3D2 = i_Points3D[i, j];
                    md_MinX = Math.Min(md_MinX, cPoint3D2.md_X);
                    md_MaxX = Math.Max(md_MaxX, cPoint3D2.md_X);
                    md_MinY = Math.Min(md_MinY, cPoint3D2.md_Y);
                    md_MaxY = Math.Max(md_MaxY, cPoint3D2.md_Y);
                    md_MinZ = Math.Min(md_MinZ, cPoint3D2.md_Z);
                    md_MaxZ = Math.Max(md_MaxZ, cPoint3D2.md_Z);
                }
        }

        public cMinMax3D(cScatter[] i_ScatterArr)
        {
            foreach (cScatter cScatter2 in i_ScatterArr)
            {
                cPoint3D mi_Point3D = cScatter2.mi_Point3D;
                md_MinX = Math.Min(md_MinX, mi_Point3D.md_X);
                md_MaxX = Math.Max(md_MaxX, mi_Point3D.md_X);
                md_MinY = Math.Min(md_MinY, mi_Point3D.md_Y);
                md_MaxY = Math.Max(md_MaxY, mi_Point3D.md_Y);
                md_MinZ = Math.Min(md_MinZ, mi_Point3D.md_Z);
                md_MaxZ = Math.Max(md_MaxZ, mi_Point3D.md_Z);
            }
        }
    }

    private class cLine
    {
        public eCoord me_Line;
        public eCoord me_Offset;
        public double md_Label;
        public Pen mi_Pen;
        public double md_Sort;
        public double md_Angle;
        public cPoint3D[] mi_Points3D = new cPoint3D[2] { new cPoint3D(), new cPoint3D() };
        public cPoint2D[] mi_Points2D = new cPoint2D[2] { new cPoint2D(), new cPoint2D() };
        public bool IsValid => mi_Points2D[0].IsValid && mi_Points2D[1].IsValid;
        public bool CoordEquals(cLine i_Line) { return mi_Points3D[0].Equals(i_Line.mi_Points3D[0]) && mi_Points3D[1].Equals(i_Line.mi_Points3D[1]); }
        public void CalcAngle2D()
        {
            double x = mi_Points2D[1].md_X - mi_Points2D[0].md_X;
            double y = mi_Points2D[1].md_Y - mi_Points2D[0].md_Y;
            md_Angle = Math.Atan2(y, x) * 180.0 / Math.PI;
            if (md_Angle < 0.0) md_Angle += 360.0;
        }
    }

    private class cPolygon
    {
        public PointF[] mk_Points;
        public double md_FactorZ;
        private bool mb_Valid;
        public bool IsValid => mb_Valid;

        public cPolygon(params cPoint2D[] i_Points2D)
        {
            mb_Valid = true;
            mk_Points = new PointF[i_Points2D.Length];
            for (int i = 0; i < i_Points2D.Length; i++)
            {
                if (i_Points2D[i].IsValid) mk_Points[i] = i_Points2D[i].Coord;
                else mb_Valid = false;
            }
        }
    }

    public class cScatter
    {
        public cPoint3D mi_Point3D;
        public PointF mk_Point;
        public Brush mi_Brush;
        public Pen mi_Pen;
        public double md_FactorZ;
        public cScatter mi_Previous;
        public bool mb_Combine;
        private bool mb_Valid;
        public bool IsValid => mb_Valid;

        public cScatter(double X, double Y, double Z, Brush i_Brush)
        {
            mi_Point3D = new cPoint3D(X, Y, Z);
            mi_Brush = i_Brush;
        }

        public void SetPoint2D(cPoint2D i_Point2D)
        {
            mk_Point = i_Point2D.Coord;
            mb_Valid = i_Point2D.IsValid;
            mk_Point.X -= 3f;
            mk_Point.Y -= 3f;
        }
    }

    private class cDrawObj
    {
        public cPolygon mi_Polygon;
        public cScatter mi_Scatter;
        public cLine mi_Line;
        public double md_Sort;
        private bool mb_Valid;
        public bool IsValid => mb_Valid;

        public cDrawObj(cPolygon i_Polygon, double d_Sort) { mi_Polygon = i_Polygon; mb_Valid = i_Polygon.IsValid; md_Sort = d_Sort; }
        public cDrawObj(cScatter i_Scatter, double d_Sort) { mi_Scatter = i_Scatter; mb_Valid = i_Scatter.IsValid; md_Sort = d_Sort; }
        public cDrawObj(cLine i_Line, double d_Sort) { mi_Line = i_Line; mb_Valid = i_Line.IsValid; md_Sort = d_Sort; }
    }

    private class cQuadrant
    {
        public double md_SortXY;
        public double md_SortXZ;
        public double md_SortYZ;
        public int ms32_Quadrant;
        public bool mb_BottomView;

        public cQuadrant(double d_Phi, cLine i_AxisX, cLine i_AxisY, cLine i_AxisZ)
        {
            int num = (int)d_Phi + 45;
            if (num > 360) num -= 360;
            num = Math.Min(3, num / 90);
            switch (num)
            {
                case 0: mb_BottomView = i_AxisX.md_Angle < 180.0; break;
                case 1: mb_BottomView = i_AxisY.md_Angle < 180.0; break;
                case 2: mb_BottomView = i_AxisX.md_Angle > 180.0; break;
                case 3: mb_BottomView = i_AxisY.md_Angle > 180.0; break;
            }
            if (mb_BottomView)
            {
                switch (num)
                {
                    case 0: ms32_Quadrant = ((i_AxisX.md_Angle + 180.0 < i_AxisZ.md_Angle) ? 1 : 0); break;
                    case 1: ms32_Quadrant = ((!(i_AxisY.md_Angle + 180.0 < i_AxisZ.md_Angle)) ? 1 : 2); break;
                    case 2: ms32_Quadrant = ((i_AxisX.md_Angle < i_AxisZ.md_Angle) ? 3 : 2); break;
                    case 3: ms32_Quadrant = ((!(i_AxisY.md_Angle < i_AxisZ.md_Angle)) ? 3 : 0); break;
                }
            }
            else
            {
                switch (num)
                {
                    case 0: ms32_Quadrant = ((i_AxisX.md_Angle > i_AxisZ.md_Angle) ? 1 : 0); break;
                    case 1: ms32_Quadrant = ((!(i_AxisY.md_Angle > i_AxisZ.md_Angle)) ? 1 : 2); break;
                    case 2: ms32_Quadrant = ((i_AxisX.md_Angle + 180.0 > i_AxisZ.md_Angle) ? 3 : 2); break;
                    case 3: ms32_Quadrant = ((!(i_AxisY.md_Angle + 180.0 > i_AxisZ.md_Angle)) ? 3 : 0); break;
                }
            }
            md_SortXY = (mb_BottomView ? 99999.9 : (-99999.9));
            md_SortXZ = ((ms32_Quadrant == 1 || ms32_Quadrant == 2) ? 99999.9 : (-99999.9));
            md_SortYZ = ((ms32_Quadrant == 0 || ms32_Quadrant == 1) ? 99999.9 : (-99999.9));
            i_AxisX.md_Sort = md_SortXZ;
            i_AxisY.md_Sort = md_SortYZ;
            i_AxisZ.md_Sort = 0.0;
        }
    }

    private class cTransform
    {
        private double md_Dist;
        private double md_sf;
        private double md_st;
        private double md_cf;
        private double md_ct;
        private double md_Rho;
        private double md_FactX;
        private double md_OffsX;
        private double md_FactY;
        private double md_OffsY;
        private double md_ViewportScale = 1.0;
        public double ZoomFactor = 1.0;
        public double FitScale = 1.0;

        public double md_NormalizeX;
        public double md_NormalizeY;
        public double md_NormalizeZ;

        public void SetCoeficients(cMouse i_Mouse)
        {
            md_Rho = i_Mouse.md_Rho;
            double num = i_Mouse.md_Theta * Math.PI / 180.0;
            double num2 = (i_Mouse.md_Phi - 180.0) * Math.PI / 180.0;
            md_sf = Math.Sin(num2);
            md_cf = Math.Cos(num2);
            md_st = Math.Sin(num);
            md_ct = Math.Cos(num);
            md_Dist = 0.5;
        }

        public void SetSize(Size k_Size)
        {
            if (k_Size.Width <= 0 || k_Size.Height <= 0) return;
            md_ViewportScale = Math.Max(0.1, Math.Min(k_Size.Width / 600.0, k_Size.Height / 420.0));
            double num = (double)k_Size.Width * 0.0254 / 96.0;
            double num2 = (double)k_Size.Height * 0.0254 / 96.0;
            md_FactX = (double)k_Size.Width / num;
            md_FactY = (double)(-k_Size.Height) / num2;
            md_OffsX = md_FactX * num / 2.0;
            md_OffsY = (0.0 - md_FactY) * num2 / 2.0;
        }

        public cPoint2D Project(cPoint3D i_Point3D, cPoint3D i_Center3D)
        {
            double num = (i_Point3D.md_X - i_Center3D.md_X) * md_NormalizeX;
            double num2 = (i_Point3D.md_Y - i_Center3D.md_Y) * md_NormalizeY;
            double num3 = (i_Point3D.md_Z - i_Center3D.md_Z) * md_NormalizeZ;
            double num4 = (0.0 - md_sf) * num + md_cf * num2;
            double num5 = (0.0 - md_cf) * md_ct * num - md_sf * md_ct * num2 + md_st * num3;
            double num6 = (0.0 - md_cf) * md_st * num - md_sf * md_st * num2 - md_ct * num3 + md_Rho;
            if (num6 <= 0.0) num6 = 0.01;
            cPoint2D cPoint2D2 = new cPoint2D();
            cPoint2D2.md_X = md_FactX * num4 * md_Dist / num6 * md_ViewportScale * FitScale * ZoomFactor + md_OffsX;
            cPoint2D2.md_Y = md_FactY * num5 * md_Dist / num6 * md_ViewportScale * FitScale * ZoomFactor + md_OffsY;
            return cPoint2D2;
        }

        public double Depth(cPoint3D i_Point3D, cPoint3D i_Center3D)
        {
            double num = (i_Point3D.md_X - i_Center3D.md_X) * md_NormalizeX;
            double num2 = (i_Point3D.md_Y - i_Center3D.md_Y) * md_NormalizeY;
            double num3 = (i_Point3D.md_Z - i_Center3D.md_Z) * md_NormalizeZ;
            return (0.0 - md_cf) * md_st * num - md_sf * md_st * num2 - md_ct * num3 + md_Rho;
        }

        public double ProjectXY(double X, double Y) { return X * md_cf + Y * md_sf; }
    }

    public delegate double delRendererFunction(double X, double Y);

    private static readonly double[] VALUES_RHO = new double[4] { 300.0, 1800.0, 1800.0, 2.0 };
    private static readonly double[] VALUES_THETA = new double[4] { 10.0, 170.0, 61.0, 0.25 };
    private static readonly double[] VALUES_PHI = new double[4] { 0.0, 360.0, 245.0, 0.4 };

    private const double AXIS_EXCESS = 1.1;
    private const int VERT_OFFSET = -30;
    private const int SCATTER_SIZE = 3;

    private eRaster me_Raster = eRaster.Off;
    private Pen[] mi_AxisPens = new Pen[3];
    private Pen[] mi_RasterPens = new Pen[3];
    private cTransform mi_Transform = new cTransform();
    private List<cDrawObj> mi_DrawObjects = new List<cDrawObj>();
    private cMouse mi_Mouse = new cMouse();
    private Point mk_Offset2D = default;
    private string[] ms_AxisLegends = new string[3];
    private SolidBrush[] mi_AxisBrushes = new SolidBrush[3];
    private Pen mi_PolyLinePen;
    private Pen mi_BorderPen;
    private SolidBrush mi_TopLegendBrush;
    private SolidBrush[] mi_SchemeBrushes;
    private Pen[] mi_SchemePens;
    private cPoint3D[,] mi_PolyArr;
    private cScatter[] mi_ScatterArr;
    private cMinMax3D mi_MinMax;
    private cQuadrant mi_Quadrant;
    private int ms32_Points;

    // ============================================================
    // TAMBAHAN: Mode edit titik
    // ============================================================
    public event Action<int, int, double> DataPointEdited;
    public event Action EditCommitted;
    public event EventHandler ZoomChanged;
    public event Action<int, int> DataPointSelected;

    private int mi_SelectedI = -1;
    private int mi_SelectedJ = -1;
    private bool mb_DraggingPoint = false;
    private double md_DragStartY;
    private double md_DragStartZ;
    private double md_DragPixelsPerZ = 1.0;
    private readonly List<double[]> mi_UndoStack = new List<double[]>();

    public double ZMin { get; set; } = double.NegativeInfinity;
    public double ZMax { get; set; } = double.PositiveInfinity;
    public double ZStep { get; set; } = 1.0;
    public bool EditModeEnabled { get; set; } = true;

    [Browsable(false)]
    public double ZoomPercent => mi_Transform.ZoomFactor * 100.0;

    public void ZoomIn() { ZoomAt(1.2, new Point(ClientSize.Width / 2, ClientSize.Height / 2)); }
    public void ZoomOut() { ZoomAt(1.0 / 1.2, new Point(ClientSize.Width / 2, ClientSize.Height / 2)); }

    private void ZoomAt(double factor, Point anchor)
    {
        if (mb_DraggingPoint || mi_Mouse.me_Action != eMouseAction.None) return;
        double oldZoom = mi_Transform.ZoomFactor;
        double nextZoom = Math.Max(0.25, Math.Min(8.0, oldZoom * factor));
        if (Math.Abs(nextZoom - oldZoom) < 1e-10) return;
        double ratio = nextZoom / oldZoom;
        double centerX = ClientSize.Width / 2.0 + mk_Offset2D.X;
        double centerY = ClientSize.Height / 2.0 + mk_Offset2D.Y;
        mi_Mouse.mk_Offset = new PointF(
            (float)(anchor.X - centerX - (anchor.X - centerX - mi_Mouse.mk_Offset.X) * ratio),
            (float)(anchor.Y - centerY - (anchor.Y - centerY - mi_Mouse.mk_Offset.Y) * ratio));
        mi_Transform.ZoomFactor = nextZoom;
        mi_DrawObjects.Clear();
        Invalidate();
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetView()
    {
        if (mb_DraggingPoint || mi_Mouse.me_Action != eMouseAction.None) return;
        SetCoefficients(1350.0, 70.0, 230.0);
        mi_Transform.ZoomFactor = 1.0;
        mi_Transform.FitScale = 1.0;
        mi_Mouse.mk_Offset = PointF.Empty;
        mk_Offset2D = new Point(0, VERT_OFFSET);
        mi_Transform.SetSize(ClientSize);
        if (mi_PolyArr != null && mi_MinMax != null && ClientSize.Width > 0 && ClientSize.Height > 0)
        {
            double minX = double.PositiveInfinity, minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;
            foreach (cPoint3D point in mi_PolyArr)
            {
                cPoint2D projected = mi_Transform.Project(point, mi_MinMax.mi_Center3D);
                if (!projected.IsValid) continue;
                minX = Math.Min(minX, projected.md_X); maxX = Math.Max(maxX, projected.md_X);
                minY = Math.Min(minY, projected.md_Y); maxY = Math.Max(maxY, projected.md_Y);
            }
            if (!double.IsInfinity(minX))
            {
                double fit = Math.Min(Math.Max(1, ClientSize.Width - 120) / Math.Max(1, maxX - minX),
                                      Math.Max(1, ClientSize.Height - 160) / Math.Max(1, maxY - minY));
                mi_Transform.FitScale = fit;
                mi_Mouse.mk_Offset = new PointF(
                    (float)(-(minX + maxX - ClientSize.Width) * 0.5 * fit - mk_Offset2D.X),
                    (float)(-(minY + maxY - ClientSize.Height) * 0.5 * fit - mk_Offset2D.Y));
            }
        }
        mi_DrawObjects.Clear();
        Invalidate();
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }

    public int SelectedI => mi_SelectedI;
    public int SelectedJ => mi_SelectedJ;

    public void ClearSelection()
    {
        mi_SelectedI = mi_SelectedJ = -1;
        Invalidate();
    }

    public double? GetSelectedValue()
    {
        if (mi_SelectedI < 0 || mi_PolyArr == null) return null;
        return mi_PolyArr[mi_SelectedI, mi_SelectedJ].md_Z;
    }

    /// <summary>
    /// Update nilai Z dari luar (misalnya setelah form memproses).
    /// </summary>
    public void SetPointValue(int i, int j, double z, bool raiseEvent = false)
    {
        if (mi_PolyArr == null) return;
        if (i < 0 || i >= mi_PolyArr.GetLength(0)) return;
        if (j < 0 || j >= mi_PolyArr.GetLength(1)) return;

        mi_PolyArr[i, j].md_Z = z;
        RecomputeMinMaxZ();
        mi_DrawObjects.Clear();
        Invalidate();

        if (raiseEvent) DataPointEdited?.Invoke(i, j, z);
    }

    // ============================================================
    // Properties publik lama
    // ============================================================
    public eRaster Raster
    {
        get { return me_Raster; }
        set
        {
            Debug.Assert(!InvokeRequired);
            if (me_Raster != value)
            {
                me_Raster = value;
                mi_DrawObjects.Clear();
                Invalidate();
            }
        }
    }

    public Color PolygonLineColor
    {
        get { return mi_PolyLinePen != null ? mi_PolyLinePen.Color : Color.Empty; }
        set
        {
            Debug.Assert(!InvokeRequired);
            mi_PolyLinePen = (value.A == 0) ? null : new Pen(value);
            Invalidate();
        }
    }

    public Color BorderColor
    {
        get { return mi_BorderPen != null ? mi_BorderPen.Color : Color.Empty; }
        set
        {
            Debug.Assert(!InvokeRequired);
            mi_BorderPen = (value.A == 0) ? null : new Pen(value);
            Invalidate();
        }
    }

    public Color TopLegendColor
    {
        get { return mi_TopLegendBrush != null ? mi_TopLegendBrush.Color : Color.Empty; }
        set { Debug.Assert(!InvokeRequired); mi_TopLegendBrush = new SolidBrush(value); Invalidate(); }
    }

    public string AxisX_Legend { get { return ms_AxisLegends[0]; } set { ms_AxisLegends[0] = value; Invalidate(); } }
    public string AxisY_Legend { get { return ms_AxisLegends[1]; } set { ms_AxisLegends[1] = value; Invalidate(); } }
    public string AxisZ_Legend { get { return ms_AxisLegends[2]; } set { ms_AxisLegends[2] = value; Invalidate(); } }

    public Color AxisX_Color { get { return mi_AxisPens[0].Color; } set { SetAxisColor(eCoord.X, value); Invalidate(); } }
    public Color AxisY_Color { get { return mi_AxisPens[1].Color; } set { SetAxisColor(eCoord.Y, value); Invalidate(); } }
    public Color AxisZ_Color { get { return mi_AxisPens[2].Color; } set { SetAxisColor(eCoord.Z, value); Invalidate(); } }

    [ReadOnly(true), Browsable(false)]
    public int TotalPoints => ms32_Points;

    public void SetColorScheme(Color[] c_Colors, float f_LineWidth)
    {
        Debug.Assert(!InvokeRequired);
        mi_SchemeBrushes = new SolidBrush[c_Colors.Length];
        mi_SchemePens = new Pen[c_Colors.Length];
        for (int i = 0; i < mi_SchemeBrushes.Length; i++)
        {
            mi_SchemeBrushes[i] = new SolidBrush(c_Colors[i]);
            mi_SchemePens[i] = new Pen(mi_SchemeBrushes[i], f_LineWidth);
        }
        Invalidate();
    }

    public void AssignTrackBars(TrackBar i_Rho, TrackBar i_Theta, TrackBar i_Phi)
    {
        Debug.Assert(!InvokeRequired);
        mi_Mouse.AssignTrackbar(eMouseAction.Rho, i_Rho, OnTrackbarScroll);
        mi_Mouse.AssignTrackbar(eMouseAction.Theta, i_Theta, OnTrackbarScroll);
        mi_Mouse.AssignTrackbar(eMouseAction.Phi, i_Phi, OnTrackbarScroll);
    }

    public Graph3D()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.White;
        SetAxisColor(eCoord.X, Color.DarkBlue);
        SetAxisColor(eCoord.Y, Color.DarkGreen);
        SetAxisColor(eCoord.Z, Color.DarkRed);
        mi_PolyLinePen = new Pen(Color.Black, 1f);
        mi_BorderPen = new Pen(Color.FromArgb(255, 180, 180, 180), 1f);
        mi_TopLegendBrush = new SolidBrush(Color.FromArgb(255, 200, 200, 150));
        mi_Transform.SetCoeficients(mi_Mouse);
    }

    public void SetFunction(delRendererFunction f_Function, PointF k_Start, PointF k_End, double d_Density, eNormalize e_Normalize)
    {
        Debug.Assert(!InvokeRequired);
        int num = (int)((double)(k_End.X - k_Start.X) / d_Density + 1.0);
        int num2 = (int)((double)(k_End.Y - k_Start.Y) / d_Density + 1.0);
        cPoint3D[,] array = new cPoint3D[num, num2];
        for (int i = 0; i < num; i++)
        {
            double num3 = (double)k_Start.X + d_Density * (double)i;
            for (int j = 0; j < num2; j++)
            {
                double num4 = (double)k_Start.Y + d_Density * (double)j;
                double z = f_Function(num3, num4);
                array[i, j] = new cPoint3D(num3, num4, z);
            }
        }
        SetSurfacePoints(array, e_Normalize);
    }

    public void SetSurfacePoints(cPoint3D[,] i_Points3D, eNormalize e_Normalize)
    {
        Debug.Assert(!InvokeRequired);
        mi_ScatterArr = null;
        mi_PolyArr = i_Points3D;
        mi_UndoStack.Clear();
        mi_SelectedI = mi_SelectedJ = -1;
        ms32_Points = i_Points3D.Length;
        mi_MinMax = new cMinMax3D(i_Points3D);
        if (ms32_Points < 4) throw new Exception("Insufficient 3D points specified");
        NormalizeRanges(e_Normalize);
        mi_Mouse.mk_Offset = Point.Empty;
        mi_DrawObjects.Clear();
        Invalidate();
    }

    public void SetScatterPoints(cScatter[] i_Scatter, eNormalize e_Normalize)
    {
        Debug.Assert(!InvokeRequired);
        mi_PolyArr = null;
        mi_ScatterArr = i_Scatter;
        ms32_Points = i_Scatter.Length;
        mi_MinMax = new cMinMax3D(i_Scatter);
        NormalizeRanges(e_Normalize);
        mi_Mouse.mk_Offset = Point.Empty;
        mi_DrawObjects.Clear();
        Invalidate();
    }

    public void SetScatterLines(cScatter[] i_ScatterArr, eNormalize e_Normalize, float f_LineWidth)
    {
        cScatter mi_Previous = null;
        foreach (cScatter cScatter2 in i_ScatterArr)
        {
            cScatter2.mb_Combine = true;
            cScatter2.mi_Previous = mi_Previous;
            if (cScatter2.mi_Brush != null) cScatter2.mi_Pen = new Pen(cScatter2.mi_Brush, f_LineWidth);
            mi_Previous = cScatter2;
        }
        SetScatterPoints(i_ScatterArr, e_Normalize);
    }

    private void NormalizeRanges(eNormalize e_Normalize)
    {
        if (mi_MinMax.md_MaxX == mi_MinMax.md_MinX) { mi_MinMax.md_MinX--; mi_MinMax.md_MaxX++; }
        if (mi_MinMax.md_MaxY == mi_MinMax.md_MinY) { mi_MinMax.md_MinY--; mi_MinMax.md_MaxY++; }
        if (mi_MinMax.md_MaxZ == mi_MinMax.md_MinZ) { mi_MinMax.md_MinZ--; mi_MinMax.md_MaxZ++; }
        double num = mi_MinMax.md_MaxX - mi_MinMax.md_MinX;
        double num2 = mi_MinMax.md_MaxY - mi_MinMax.md_MinY;
        double num3 = ((me_Raster != eRaster.Off) ? (Math.Max(0.0, mi_MinMax.md_MaxZ) - Math.Min(0.0, mi_MinMax.md_MinZ)) : (mi_MinMax.md_MaxZ - mi_MinMax.md_MinZ));
        switch (e_Normalize)
        {
            case eNormalize.MaintainXY:
                { double num5 = (num + num2) / 2.0; num = num5; num2 = num5; break; }
            case eNormalize.MaintainXYZ:
                { double num4 = (num + num2 + num3) / 3.0; num = num4; num2 = num4; num3 = num4; break; }
        }
        mi_Transform.md_NormalizeX = 250.0 / num;
        mi_Transform.md_NormalizeY = 250.0 / num2;
        mi_Transform.md_NormalizeZ = 250.0 / num3;
        mi_MinMax.mi_Center3D.md_X = (mi_MinMax.md_MaxX + mi_MinMax.md_MinX) / 2.0;
        mi_MinMax.mi_Center3D.md_Y = (mi_MinMax.md_MaxY + mi_MinMax.md_MinY) / 2.0;
        if (me_Raster == eRaster.Off) mi_MinMax.mi_Center3D.md_Z = (mi_MinMax.md_MaxZ + mi_MinMax.md_MinZ) / 2.0;
        else mi_MinMax.mi_Center3D.md_Z = (Math.Max(0.0, mi_MinMax.md_MaxZ) + Math.Min(0.0, mi_MinMax.md_MinZ)) / 2.0;
    }

    public void SetCoefficients(double d_Rho, double d_Theta, double d_Phi)
    {
        Debug.Assert(!InvokeRequired);
        mi_Mouse.SetRho(d_Rho);
        mi_Mouse.SetTheta(d_Theta);
        mi_Mouse.SetPhi(d_Phi);
        mi_Transform.SetCoeficients(mi_Mouse);
        mi_DrawObjects.Clear();
        Invalidate();
    }

    private void CreateCoordinateSystem(Graphics i_Graph)
    {
        mk_Offset2D = new Point(0, VERT_OFFSET);
        if (me_Raster == eRaster.Off) return;

        List<cLine> list = new List<cLine>();
        for (int i = 0; i < 3; i++)
        {
            cLine cLine2 = new cLine();
            cLine2.mi_Pen = mi_AxisPens[i];
            switch ((eCoord)i)
            {
                case eCoord.X:
                    cLine2.mi_Points3D[0].md_X = Math.Min(0.0, mi_MinMax.md_MinX * AXIS_EXCESS);
                    cLine2.mi_Points3D[0].md_Y = Math.Min(0.0, mi_MinMax.md_MinY * AXIS_EXCESS);
                    cLine2.mi_Points3D[1].md_Y = Math.Min(0.0, mi_MinMax.md_MinY * AXIS_EXCESS);
                    cLine2.me_Line = eCoord.X;
                    cLine2.me_Offset = eCoord.X;
                    break;
                case eCoord.Y:
                    cLine2.mi_Points3D[0].md_Y = Math.Min(0.0, mi_MinMax.md_MinY * AXIS_EXCESS);
                    cLine2.mi_Points3D[0].md_X = Math.Min(0.0, mi_MinMax.md_MinX * AXIS_EXCESS);
                    cLine2.mi_Points3D[1].md_X = Math.Min(0.0, mi_MinMax.md_MinX * AXIS_EXCESS);
                    cLine2.me_Line = eCoord.Y;
                    cLine2.me_Offset = eCoord.Z;
                    break;
            }
            cLine2.mi_Points2D[0] = mi_Transform.Project(cLine2.mi_Points3D[0], mi_MinMax.mi_Center3D);
            cLine2.mi_Points2D[1] = mi_Transform.Project(cLine2.mi_Points3D[1], mi_MinMax.mi_Center3D);
            list.Add(cLine2);
        }
        mi_Quadrant = new cQuadrant(mi_Mouse.md_Phi, list[0], list[1], list[2]);
        if (me_Raster >= eRaster.Raster)
        {
            for (int j = 0; j < 3; j++)
            {
                eCoord eCoord2 = (eCoord)j;
                eCoord eCoord3 = (eCoord)((j + 1) % 3);
                for (int k = 0; k < 2; k++)
                {
                    cLine cLine3 = list[(int)eCoord2];
                    cLine cLine4 = list[(int)eCoord3];
                    double value = cLine4.mi_Points3D[0].GetValue(eCoord3);
                    double value2 = cLine4.mi_Points3D[1].GetValue(eCoord3);
                    double num = CalculateInterval(value2 - value);
                    for (int l = -11; l < 11; l++)
                    {
                        double num2 = num * (double)l;
                        if (num2 < value || num2 > value2) continue;
                        cLine cLine5 = new cLine();
                        cLine5.mi_Pen = mi_RasterPens[(int)eCoord3];
                        cLine5.me_Line = eCoord2;
                        cLine5.me_Offset = eCoord3;
                        cLine5.md_Label = num2;
                        cLine5.mi_Points3D[0] = cLine3.mi_Points3D[0].Clone();
                        cLine5.mi_Points3D[1] = cLine3.mi_Points3D[1].Clone();
                        cLine5.mi_Points3D[0].SetValue(eCoord3, num2);
                        cLine5.mi_Points3D[1].SetValue(eCoord3, num2);
                        if (!cLine5.CoordEquals(list[0]) && !cLine5.CoordEquals(list[1]) && !cLine5.CoordEquals(list[2]))
                        {
                            if ((eCoord2 == eCoord.X && eCoord3 == eCoord.Z) || (eCoord2 == eCoord.Z && eCoord3 == eCoord.X))
                                cLine5.md_Sort = mi_Quadrant.md_SortXZ;
                            else if ((eCoord2 == eCoord.Z && eCoord3 == eCoord.Y) || (eCoord2 == eCoord.Y && eCoord3 == eCoord.Z))
                                cLine5.md_Sort = mi_Quadrant.md_SortYZ;
                            else
                            {
                                cLine5.md_Sort = mi_Quadrant.md_SortXY;
                                cLine cLine6 = list[2];
                                cLine5.mi_Points3D[0].md_Z = cLine6.mi_Points3D[0].md_Z;
                                cLine5.mi_Points3D[1].md_Z = cLine6.mi_Points3D[0].md_Z;
                            }
                            list.Add(cLine5);
                        }
                    }
                    eCoord eCoord4 = eCoord2;
                    eCoord2 = eCoord3;
                    eCoord3 = eCoord4;
                }
            }
        }
        foreach (cLine item in list)
        {
            item.mi_Points2D[0] = mi_Transform.Project(item.mi_Points3D[0], mi_MinMax.mi_Center3D);
            item.mi_Points2D[1] = mi_Transform.Project(item.mi_Points3D[1], mi_MinMax.mi_Center3D);
            AddDrawObject(new cDrawObj(item, item.md_Sort));
        }
    }

    private void CreatePolygons()
    {
        cPoint2D[,] array = new cPoint2D[mi_PolyArr.GetLength(0), mi_PolyArr.GetLength(1)];
        for (int i = 0; i < mi_PolyArr.GetLength(0); i++)
            for (int j = 0; j < mi_PolyArr.GetLength(1); j++)
                array[i, j] = mi_Transform.Project(mi_PolyArr[i, j], mi_MinMax.mi_Center3D);

        for (int k = 0; k < mi_PolyArr.GetLength(0) - 1; k++)
        {
            for (int l = 0; l < mi_PolyArr.GetLength(1) - 1; l++)
            {
                cPolygon cPolygon2 = new cPolygon(array[k, l], array[k, l + 1], array[k + 1, l + 1], array[k + 1, l]);
                double md_Z = mi_PolyArr[k, l].md_Z;
                double md_Z2 = mi_PolyArr[k, l + 1].md_Z;
                double md_Z3 = mi_PolyArr[k + 1, l + 1].md_Z;
                double md_Z4 = mi_PolyArr[k + 1, l].md_Z;
                double num = (md_Z + md_Z2 + md_Z3 + md_Z4) / 4.0;
                cPolygon2.md_FactorZ = (num - mi_MinMax.md_MinZ) / (mi_MinMax.md_MaxZ - mi_MinMax.md_MinZ);
                double d_Sort = mi_Transform.ProjectXY(k + 1, l + 1);
                AddDrawObject(new cDrawObj(cPolygon2, d_Sort));
            }
        }
    }

    private void CreateScatterDots()
    {
        cScatter[] array = mi_ScatterArr;
        foreach (cScatter cScatter2 in array)
        {
            cScatter2.SetPoint2D(mi_Transform.Project(cScatter2.mi_Point3D, mi_MinMax.mi_Center3D));
            if (cScatter2.mi_Brush == null)
                cScatter2.md_FactorZ = (cScatter2.mi_Point3D.md_Z - mi_MinMax.md_MinZ) / (mi_MinMax.md_MaxZ - mi_MinMax.md_MinZ);
            double d_Sort = mi_Transform.ProjectXY(cScatter2.mi_Point3D.md_X + 1.0, cScatter2.mi_Point3D.md_Y + 1.0);
            AddDrawObject(new cDrawObj(cScatter2, d_Sort));
        }
    }

    private void AddDrawObject(cDrawObj i_DrawObj)
    {
        int i;
        for (i = 0; i < mi_DrawObjects.Count && !(mi_DrawObjects[i].md_Sort > i_DrawObj.md_Sort); i++) { }
        mi_DrawObjects.Insert(i, i_DrawObj);
    }

    private Brush GetSchemeBrush(double d_FactorZ)
    {
        if (mi_SchemeBrushes == null || double.IsNaN(d_FactorZ)) return Brushes.Goldenrod;
        d_FactorZ = Math.Min(1.0, d_FactorZ);
        d_FactorZ = Math.Max(0.0, d_FactorZ);
        int num = (int)(d_FactorZ * (double)(mi_SchemeBrushes.Length - 1));
        return mi_SchemeBrushes[num];
    }

    private Pen GetSchemePen(double d_FactorZ)
    {
        if (mi_SchemePens == null || double.IsNaN(d_FactorZ)) return Pens.Goldenrod;
        d_FactorZ = Math.Min(1.0, d_FactorZ);
        d_FactorZ = Math.Max(0.0, d_FactorZ);
        int num = (int)(d_FactorZ * (double)(mi_SchemePens.Length - 1));
        return mi_SchemePens[num];
    }

    private void SetAxisColor(eCoord e_Coord, Color c_Color)
    {
        mi_AxisBrushes[(int)e_Coord] = new SolidBrush(c_Color);
        mi_AxisPens[(int)e_Coord] = new Pen(c_Color, 3f);
        mi_RasterPens[(int)e_Coord] = new Pen(BrightenColor(c_Color), 1f);
    }

    private Color BrightenColor(Color c_Color)
    {
        int red = c_Color.R + (255 - c_Color.R) / 2;
        int green = c_Color.G + (255 - c_Color.G) / 2;
        int blue = c_Color.B + (255 - c_Color.B) / 2;
        return Color.FromArgb(255, red, green, blue);
    }

    private double CalculateInterval(double d_Range)
    {
        double num = Math.Pow(10.0, Math.Floor(Math.Log10(d_Range)));
        if (d_Range / num >= 5.0) return num;
        if (d_Range / (num / 2.0) >= 5.0) return num / 2.0;
        return num / 5.0;
    }

    private string FormatLabel(double d_Label)
    {
        return d_Label.ToString("0.000", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e) { Draw(e.Graphics); }

    public Bitmap GetScreenshot()
    {
        Bitmap bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
        using (Graphics i_Graph = Graphics.FromImage(bitmap)) { Draw(i_Graph); }
        return bitmap;
    }

    private void Draw(Graphics i_Graph)
    {
        if (mi_DrawObjects.Count == 0)
        {
            CreateCoordinateSystem(i_Graph);
            if (mi_PolyArr != null) CreatePolygons();
            if (mi_ScatterArr != null) CreateScatterDots();
        }
        i_Graph.Clear(BackColor);
        int num = 4;
        int num2 = ClientSize.Height - Font.Height - 4;
        for (int num3 = 2; num3 >= 0; num3--)
        {
            if (!string.IsNullOrEmpty(ms_AxisLegends[num3]))
            {
                string s = $"{(eCoord)num3}: {ms_AxisLegends[num3]}";
                i_Graph.DrawString(s, Font, mi_AxisBrushes[num3], num, num2);
                num2 -= Font.Height;
            }
        }
        if (mi_TopLegendBrush != null)
        {
            string[] array = new string[3] { "Rotation:", "Elevation:", "Distance:" };
            string[] array2 = new string[3]
            {
                $"{(int)mi_Mouse.md_Phi:+#;-#;0}°",
                $"{(int)mi_Mouse.md_Theta:+#;-#;0}°",
                $"{(int)mi_Mouse.md_Rho}"
            };
            SizeF sizeF = i_Graph.MeasureString(array[1], Font);
            num = 4;
            num2 = 3;
            for (int i = 0; i < 3; i++)
            {
                i_Graph.DrawString(array[i], Font, mi_TopLegendBrush, num, num2);
                i_Graph.DrawString(array2[i], Font, mi_TopLegendBrush, (float)num + sizeF.Width, num2);
                num2 += Font.Height;
            }
        }
        i_Graph.TranslateTransform(mi_Mouse.mk_Offset.X + mk_Offset2D.X, mi_Mouse.mk_Offset.Y + mk_Offset2D.Y);
        SmoothingMode smoothingMode = SmoothingMode.Invalid;
        foreach (cDrawObj mi_DrawObject in mi_DrawObjects)
        {
            if (!mi_DrawObject.IsValid) continue;

            if (mi_DrawObject.mi_Polygon != null)
            {
                if (smoothingMode != SmoothingMode.None)
                {
                    smoothingMode = SmoothingMode.None;
                    i_Graph.SmoothingMode = SmoothingMode.None;
                }
                cPolygon mi_Polygon = mi_DrawObject.mi_Polygon;
                Brush schemeBrush = GetSchemeBrush(mi_Polygon.md_FactorZ);
                i_Graph.FillPolygon(schemeBrush, mi_Polygon.mk_Points);
                if (mi_PolyLinePen != null) i_Graph.DrawPolygon(mi_PolyLinePen, mi_Polygon.mk_Points);
            }
            else if (mi_DrawObject.mi_Scatter != null)
            {
                if (smoothingMode != SmoothingMode.AntiAlias)
                {
                    smoothingMode = SmoothingMode.AntiAlias;
                    i_Graph.SmoothingMode = SmoothingMode.AntiAlias;
                }
                cScatter mi_Scatter = mi_DrawObject.mi_Scatter;
                if (mi_Scatter.mb_Combine)
                {
                    if (mi_Scatter.mi_Previous != null)
                    {
                        Pen pen = mi_Scatter.mi_Pen ?? GetSchemePen(mi_Scatter.md_FactorZ);
                        i_Graph.DrawLine(pen, mi_Scatter.mi_Previous.mk_Point, mi_Scatter.mk_Point);
                    }
                }
                else
                {
                    Brush brush = mi_Scatter.mi_Brush ?? GetSchemeBrush(mi_Scatter.md_FactorZ);
                    i_Graph.FillEllipse(brush, mi_Scatter.mk_Point.X, mi_Scatter.mk_Point.Y, 6f, 6f);
                }
            }
            else
            {
                if (smoothingMode != SmoothingMode.AntiAlias)
                {
                    smoothingMode = SmoothingMode.AntiAlias;
                    i_Graph.SmoothingMode = SmoothingMode.AntiAlias;
                }
                cLine mi_Line = mi_DrawObject.mi_Line;
                i_Graph.DrawLine(mi_Line.mi_Pen, mi_Line.mi_Points2D[0].Coord, mi_Line.mi_Points2D[1].Coord);
            }
        }

        // ====== GAMBAR MARKER TITIK TERPILIH ======
        DrawSelectionMarker(i_Graph);

        if (mi_BorderPen != null)
        {
            i_Graph.ResetTransform();
            Rectangle clientRectangle = ClientRectangle;
            i_Graph.DrawRectangle(mi_BorderPen, clientRectangle.X, clientRectangle.Y, clientRectangle.Width - 1, clientRectangle.Height - 1);
        }
    }

    // ============================================================
    // TAMBAHAN: Hit-test, marker, recompute Z
    // ============================================================
    private bool HitTestPoint(Point mouse, out int iOut, out int jOut)
    {
        iOut = jOut = -1;
        if (mi_PolyArr == null || mi_MinMax == null) return false;

        float ox = mi_Mouse.mk_Offset.X + mk_Offset2D.X;
        float oy = mi_Mouse.mk_Offset.Y + mk_Offset2D.Y;

        const float RADIUS = 12f;
        double bestDepth = double.PositiveInfinity;
        float bestDist = float.MaxValue;
        for (int i = 0; i < mi_PolyArr.GetLength(0); i++)
        {
            for (int j = 0; j < mi_PolyArr.GetLength(1); j++)
            {
                var p2 = mi_Transform.Project(mi_PolyArr[i, j], mi_MinMax.mi_Center3D);
                if (!p2.IsValid) continue;
                float dx = (float)p2.md_X + ox - mouse.X;
                float dy = (float)p2.md_Y + oy - mouse.Y;
                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d > RADIUS) continue;
                // pilih titik yang paling dekat ke kamera; jarak piksel sebagai pemutus seri
                double depth = mi_Transform.Depth(mi_PolyArr[i, j], mi_MinMax.mi_Center3D);
                if (depth < bestDepth - 1.0 || (Math.Abs(depth - bestDepth) <= 1.0 && d < bestDist))
                {
                    bestDepth = depth; bestDist = d; iOut = i; jOut = j;
                }
            }
        }
        return iOut >= 0;
    }

    private double ClampZ(double z)
    {
        if (z < ZMin) z = ZMin;
        if (z > ZMax) z = ZMax;
        double step = ZStep > 0 ? ZStep : 0.001;
        z = Math.Round(z / step) * step;
        return Math.Round(z, 6);
    }

    private double GetKeyStep()
    {
        double step = ZStep > 0 ? ZStep : 0.001;
        double range = (mi_MinMax != null) ? (mi_MinMax.md_MaxZ - mi_MinMax.md_MinZ) : 0.0;
        double mult = Math.Max(1.0, Math.Round(range / 200.0 / step));
        return step * mult;
    }

    private void ApplyPointZ(int i, int j, double z, bool renormalize)
    {
        mi_PolyArr[i, j].md_Z = z;
        if (renormalize) RecomputeMinMaxZ();
        mi_DrawObjects.Clear();
        DataPointEdited?.Invoke(i, j, z);
        Invalidate();
    }

    public void SelectPoint(int i, int j)
    {
        if (mi_PolyArr == null) return;
        if (i < 0 || j < 0 || i >= mi_PolyArr.GetLength(0) || j >= mi_PolyArr.GetLength(1)) return;
        if (i == mi_SelectedI && j == mi_SelectedJ) return;
        mi_SelectedI = i;
        mi_SelectedJ = j;
        Invalidate();
    }

    public void Undo()
    {
        if (mi_PolyArr == null || mi_UndoStack.Count == 0) return;
        double[] last = mi_UndoStack[mi_UndoStack.Count - 1];
        mi_UndoStack.RemoveAt(mi_UndoStack.Count - 1);
        int i = (int)last[0], j = (int)last[1];
        if (i >= mi_PolyArr.GetLength(0) || j >= mi_PolyArr.GetLength(1)) return;
        mi_SelectedI = i; mi_SelectedJ = j;
        ApplyPointZ(i, j, last[2], true);
        EditCommitted?.Invoke();
    }

    public void ClearUndo()
    {
        mi_UndoStack.Clear();
    }

    protected override bool IsInputKey(Keys keyData)
    {
        switch (keyData & Keys.KeyCode)
        {
            case Keys.Up:
            case Keys.Down:
            case Keys.Left:
            case Keys.Right:
                return true;
        }
        return base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!EditModeEnabled || mi_PolyArr == null) return;

        if (e.Control && e.KeyCode == Keys.Z)
        {
            Undo();
            e.Handled = true;
            return;
        }
        if (mi_SelectedI < 0) return;

        int rows = mi_PolyArr.GetLength(0), cols = mi_PolyArr.GetLength(1);
        switch (e.KeyCode)
        {
            case Keys.Up:
            case Keys.Down:
            {
                double step = GetKeyStep() * (e.Shift ? 10.0 : 1.0);
                double oldZ = mi_PolyArr[mi_SelectedI, mi_SelectedJ].md_Z;
                double newZ = ClampZ(oldZ + (e.KeyCode == Keys.Up ? step : -step));
                if (newZ != oldZ)
                {
                    mi_UndoStack.Add(new double[] { mi_SelectedI, mi_SelectedJ, oldZ });
                    ApplyPointZ(mi_SelectedI, mi_SelectedJ, newZ, false);
                }
                e.Handled = true;
                break;
            }
            case Keys.Left:
                mi_SelectedJ = Math.Max(0, mi_SelectedJ - 1);
                DataPointSelected?.Invoke(mi_SelectedI, mi_SelectedJ);
                Invalidate();
                e.Handled = true;
                break;
            case Keys.Right:
                mi_SelectedJ = Math.Min(cols - 1, mi_SelectedJ + 1);
                DataPointSelected?.Invoke(mi_SelectedI, mi_SelectedJ);
                Invalidate();
                e.Handled = true;
                break;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (EditModeEnabled && mi_PolyArr != null && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
        {
            RecomputeMinMaxZ();
            mi_DrawObjects.Clear();
            Invalidate();
            EditCommitted?.Invoke();
        }
    }

    private void DrawSelectionMarker(Graphics g)
    {
        if (mi_SelectedI < 0 || mi_PolyArr == null) return;
        if (mi_SelectedI >= mi_PolyArr.GetLength(0) || mi_SelectedJ >= mi_PolyArr.GetLength(1)) return;

        var p2 = mi_Transform.Project(mi_PolyArr[mi_SelectedI, mi_SelectedJ], mi_MinMax.mi_Center3D);
        if (!p2.IsValid) return;

        float cx = (float)p2.md_X;
        float cy = (float)p2.md_Y;

        using (var pen = new Pen(Color.Yellow, 2f))
        using (var brush = new SolidBrush(Color.FromArgb(120, 255, 255, 0)))
        {
            g.FillEllipse(brush, cx - 7, cy - 7, 14, 14);
            g.DrawEllipse(pen, cx - 7, cy - 7, 14, 14);
        }

        // Tampilkan nilai Z sebagai label kecil
        string label = mi_PolyArr[mi_SelectedI, mi_SelectedJ].md_Z.ToString("F3");
        using (var bg = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
        using (var fg = new SolidBrush(Color.Yellow))
        {
            SizeF sz = g.MeasureString(label, Font);
            g.FillRectangle(bg, cx + 10, cy - 10, sz.Width + 6, sz.Height + 4);
            g.DrawString(label, Font, fg, cx + 13, cy - 8);
        }
    }

    private void RecomputeMinMaxZ()
    {
        if (mi_PolyArr == null) return;
        double minZ = double.PositiveInfinity, maxZ = double.NegativeInfinity;
        for (int i = 0; i < mi_PolyArr.GetLength(0); i++)
            for (int j = 0; j < mi_PolyArr.GetLength(1); j++)
            {
                double z = mi_PolyArr[i, j].md_Z;
                if (z < minZ) minZ = z;
                if (z > maxZ) maxZ = z;
            }
        if (mi_MinMax != null)
        {
            mi_MinMax.md_MinZ = minZ;
            mi_MinMax.md_MaxZ = maxZ;
            NormalizeRanges(eNormalize.Separate);
        }
    }

    // ============================================================
    // Mouse events (MODIFIED)
    // ============================================================
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        mi_Mouse.mk_LastPos = e.Location;
        Focus();

        // Klik kiri tanpa modifier → coba pilih titik untuk edit
        if (EditModeEnabled && e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.None)
        {
            if (HitTestPoint(e.Location, out int ii, out int jj))
            {
                mi_SelectedI = ii;
                mi_SelectedJ = jj;
                mb_DraggingPoint = true;
                md_DragStartY = e.Y;
                md_DragStartZ = mi_PolyArr[ii, jj].md_Z;
                {
                    cPoint3D pt = mi_PolyArr[ii, jj];
                    double dz = Math.Max(1e-6, (mi_MinMax.md_MaxZ - mi_MinMax.md_MinZ) / 50.0);
                    var a0 = mi_Transform.Project(pt, mi_MinMax.mi_Center3D);
                    var a1 = mi_Transform.Project(new cPoint3D(pt.md_X, pt.md_Y, pt.md_Z + dz), mi_MinMax.mi_Center3D);
                    double sy = (a1.md_Y - a0.md_Y) / dz;
                    md_DragPixelsPerZ = (Math.Abs(sy) > 1e-9) ? sy : -1.0;
                }
                Focus();
                Cursor = Cursors.SizeNS;
                DataPointSelected?.Invoke(ii, jj);
                Invalidate();
                return;
            }
            else
            {
                if (mi_SelectedI >= 0)
                {
                    mi_SelectedI = mi_SelectedJ = -1;
                    Invalidate();
                }
            }
        }

        switch (Control.ModifierKeys)
        {
            case Keys.None:
                if (e.Button == MouseButtons.Right)
                {
                    Cursor = Cursors.NoMoveHoriz;
                    mi_Mouse.me_Action = eMouseAction.Phi;
                }
                break;
            case Keys.Shift:
                if (e.Button == MouseButtons.Left)
                {
                    Cursor = Cursors.NoMove2D;
                    mi_Mouse.me_Action = eMouseAction.Move;
                }
                break;
            case Keys.Control:
                if (e.Button == MouseButtons.Left)
                {
                    Cursor = Cursors.SizeNS;
                    mi_Mouse.me_Action = eMouseAction.Rho;
                }
                break;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (mb_DraggingPoint)
        {
            mb_DraggingPoint = false;
            Cursor = Cursors.Arrow;
            if (mi_PolyArr != null && mi_SelectedI >= 0)
            {
                double finalZ = mi_PolyArr[mi_SelectedI, mi_SelectedJ].md_Z;
                if (finalZ != md_DragStartZ)
                    mi_UndoStack.Add(new double[] { mi_SelectedI, mi_SelectedJ, md_DragStartZ });
                RecomputeMinMaxZ();
                mi_DrawObjects.Clear();
                Invalidate();
                if (finalZ != md_DragStartZ) EditCommitted?.Invoke();
            }
        }
        mi_Mouse.me_Action = eMouseAction.None;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (!mb_DraggingPoint)
        {
            mi_Mouse.me_Action = eMouseAction.None;
            Cursor = Cursors.Arrow;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (e is HandledMouseEventArgs handled) handled.Handled = true;
        ZoomAt(Math.Exp(e.Delta * 0.0015), e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int num = e.X - mi_Mouse.mk_LastPos.X;
        int num2 = e.Y - mi_Mouse.mk_LastPos.Y;
        mi_Mouse.mk_LastPos = e.Location;

        // Drag titik untuk ubah Z (mengikuti pergerakan mouse di layar)
        if (mb_DraggingPoint && mi_SelectedI >= 0 && mi_PolyArr != null)
        {
            double newZ = md_DragStartZ + (e.Y - md_DragStartY) / md_DragPixelsPerZ;
            newZ = ClampZ(newZ);
            if (newZ != mi_PolyArr[mi_SelectedI, mi_SelectedJ].md_Z)
                ApplyPointZ(mi_SelectedI, mi_SelectedJ, newZ, false);
            return;
        }

        // Kursor tangan saat mouse berada di atas titik yang bisa diedit
        if (EditModeEnabled && mi_Mouse.me_Action == eMouseAction.None && mi_PolyArr != null)
        {
            Cursor = HitTestPoint(e.Location, out _, out _) ? Cursors.Hand : Cursors.Default;
        }

        switch (mi_Mouse.me_Action)
        {
            case eMouseAction.Move:
                mi_Mouse.mk_Offset.X += num;
                mi_Mouse.mk_Offset.Y += num2;
                Invalidate();
                break;
            case eMouseAction.Rho:
            case eMouseAction.Theta:
            case eMouseAction.Phi:
                mi_Mouse.OnMouseMove(num, num2);
                mi_Transform.SetCoeficients(mi_Mouse);
                mi_DrawObjects.Clear();
                Invalidate();
                break;
        }
    }

    private void OnTrackbarScroll(object sender, EventArgs e)
    {
        mi_Mouse.OnTrackBarScroll();
        mi_Transform.SetCoeficients(mi_Mouse);
        mi_DrawObjects.Clear();
        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        mi_Transform.SetSize(ClientSize);
        mi_DrawObjects.Clear();
        Invalidate();
    }
}