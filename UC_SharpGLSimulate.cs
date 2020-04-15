using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SharpGL;
using System.Threading;
using SharpGL.SceneGraph.Assets;
using SharpGL.SceneGraph;

namespace UC_SharpGLSimulate
{
    public partial class UC_SharpGLSimulate : UserControl
    {
        public UC_SharpGLSimulate()
        {
            InitializeComponent();
            this.timer.Enabled = false;
            //string FilePath = @"C:\Users\whli\Desktop\2_刨花板_暖白_18.nc";
            string FilePath = @"E:\路径模拟开发\Monroe.cnc";

            this.CreateDrawing(FilePath, true);
            //this.CreateDrawing("(0,0,0)", false);

        }

        #region 全局变量

        /// <summary>
        /// 坐标点轨迹全局对象
        /// </summary>
        private List<Point> _trackPointList = new List<Point>();

        /// <summary>
        /// 记录最后一次状态
        /// </summary>
        private Point _point = new Point();

        /// <summary>
        /// G代码数据集合
        /// </summary>
        private string[] _nCTextArray;

        /// <summary>
        /// G代码执行当前的位置
        /// </summary>
        private int _currentNumber = 0;

        /// <summary>
        /// 判断是否为绝对值或者增量
        /// </summary>
        private bool _isAbs = true;

        /// <summary>
        /// 判断是否为文件路径
        /// </summary>
        private bool _isFilePath = true;

        /// <summary>
        /// 判断是否直线状态
        /// </summary>
        private bool _isFullLine = true;

        /// <summary>
        /// 传入的坐标
        /// </summary>
        private string _customPoint = string.Empty;
        #endregion

        #region 实体

        /// <summary>
        /// 坐标点对象、轨迹对象
        /// </summary>
        private class Point
        {
            /// <summary>
            /// 坐标类型，1：实线,2:虚线,3:圆弧
            /// </summary>
            public int PointType { get; set; } = 1;

            /// <summary>
            /// 是否为绝对值方式
            /// </summary>
            public bool IsAbs { get; set; }

            /// <summary>
            /// X坐标
            /// </summary>
            public float X { get; set; }

            /// <summary>
            /// Y坐标
            /// </summary>
            public float Y { get; set; }

            /// <summary>
            /// Z坐标
            /// </summary>
            public float Z { get; set; }

            /// <summary>
            /// 上一个X坐标
            /// </summary>
            public float L_X { get; set; }

            /// <summary>
            /// 上一个Y坐标
            /// </summary>
            public float L_Y { get; set; }

            /// <summary>
            /// 上一个Z坐标
            /// </summary>
            public float L_Z { get; set; }

            /// <summary>
            /// 圆弧
            /// </summary>
            public Circle Circle = new Circle();

            /// <summary>
            /// 圆弧坐标点轨迹全局对象
            /// </summary>
            public List<Circle> CricleList = new List<Circle>();

            /// <summary>
            /// 构造函数
            /// </summary>
            public Point(double xx = 0, double yy = 0, double zz = 0)
            {
                this.X = float.Parse(xx.ToString());
                this.Y = float.Parse(yy.ToString());
                this.Z = float.Parse(zz.ToString());
            }
        }



        /// <summary>
        /// 圆弧对象
        /// </summary>
        private class Circle
        {
            /// <summary>
            /// X坐标，可以表述圆心或其他
            /// </summary>
            public double X { get; set; } = 0;

            /// <summary>
            /// Y坐标，可以表述圆心或其他
            /// </summary>
            public double Y { get; set; } = 0;

            /// <summary>
            /// Z坐标，可以表述圆心或其他
            /// </summary>
            public double Z { get; set; } = 0;

            /// <summary>
            /// 圆半径，仅PointType=3有效
            /// </summary>
            public float R { get; set; }

            /// <summary>
            /// 圆弧方向，True代表顺时针(G02)，False代表逆时针(G03)，仅PointType=3有效
            /// </summary>
            public bool C_Direction { get; set; }

            /// <summary>
            /// 构造函数
            /// </summary>
            public Circle(double xx = 0, double yy = 0, double zz = 0)
            {
                this.X = xx;
                this.Y = yy;
                this.Z = zz;
            }

        }

        #endregion

        /// <summary>
        /// 绘图事件
        /// </summary>
        private void GLC_Model_OpenGLDraw(object sender, RenderEventArgs args)
        {
            OpenGL gl = this.GLC_Model.OpenGL;//创建OpenGL对象

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();            //清除深度缓存

            //重置模型观察矩阵，我认为其实就是重置了三维坐标轴的位置，将其初始化为原点
            gl.Translate(-0.5f, -0.9f, -2.3f);  //最后一个参数为距离屏幕距离

            //图像旋转
            gl.Rotate(-50f, 1.0f, 0.0f, 0.0f); // 绕X轴旋转
            gl.Rotate(-10f, 0.0f, 1.0f, 0.0f);   // 绕Y轴旋转
            gl.Rotate(45, 0.0f, 0.0f, 1.0f);   // 绕Z轴旋转

            if (this._isFilePath == true)
            {
                ////在X,y平面上绘制网格
                //gl.Color(0f, 128f, 128f);
                //for (float i = 0f; i < 2.5; i += 0.2f)
                //{
                //    //设置类型为绘制线
                //    gl.Begin(OpenGL.GL_LINES);
                //    //x轴方向
                //    gl.Vertex(-0.4, i - 0.2, -0.15);
                //    gl.Vertex(4.4f, i - 0.2, -0.15);
                //    //y轴方向
                //    gl.Vertex(i * 2 - 0.4, -0.2, -0.15);
                //    gl.Vertex(i * 2 - 0.4, 2.2f, -0.15);
                //    gl.End();
                //}

                //////设置辅助线
                ////gl.Begin(OpenGL.GL_LINES);
                ////gl.Vertex(-0.4, -0.2, -0.15);
                ////gl.Vertex(-0.4, -0.2, 0);
                ////gl.Vertex(-0.4, 2.2, -0.15);
                ////gl.Vertex(-0.4, 2.2, 0);
                ////gl.Vertex(4.4, -0.2, 0);
                ////gl.Vertex(4.4, -0.2, 0);
                ////gl.Vertex(4.4, 2.2, -0.15);
                ////gl.Vertex(4.4, 2.2, 0);

                ////gl.Vertex(-0.4, -0.2, 0);
                ////gl.Vertex(4.4, -0.2, 0);
                ////gl.Vertex(-0.4, -0.2, 0);
                ////gl.Vertex(-0.4, 2.2, 0);
                ////gl.Vertex(4.4, 2.2, 0);
                ////gl.Vertex(-0.4, 2.2, 0);
                ////gl.Vertex(4.4, 2.2, 0);
                ////gl.Vertex(4.4, -0.2, 0);
                ////gl.End();

                //绘制坐标轴
                //var sv_X = OpenGLSceneGraphExtensions.Project(gl, new Vertex(0.1f, -0.05f, 0f));
                //gl.DrawText((int)(sv_X.X), (int)(sv_X.Y), 255, 0, 0, "宋体", 10, "X");

                //var sv_Y = OpenGLSceneGraphExtensions.Project(gl, new Vertex(-0.1f, 0.2f, 0f));
                //gl.DrawText((int)(sv_Y.X), (int)(sv_Y.Y), 0, 255, 0, "宋体", 10, "Y");

                //var sv_Z = OpenGLSceneGraphExtensions.Project(gl, new Vertex(0.05f, 0f, 0.2f));
                //gl.DrawText((int)(sv_Z.Y), (int)(sv_Z.Z), 0, 255, 0, "宋体", 10, "Z");

                //gl.LineWidth(1.2f);
                //gl.DrawText(360, 40, 255, 0, 0, "宋体", 10, "X");
                //gl.DrawText(290, 40, 0, 255, 0, "宋体", 10, "Y");
                //gl.DrawText(320, 40, 0, 0, 255, "宋体", 10, "Z");
                //var sv = OpenGLSceneGraphExtensions.Project(gl, new Vertex(0.05f, 5f, 0.0f));

                gl.LineWidth(3f);

                gl.Begin(OpenGL.GL_LINES);
                //X轴
                gl.Color(1f, 0f, 0f);
                gl.Vertex(0, 0, 0);
                gl.Vertex(0.2, 0, 0);

                //Y轴
                gl.Color(0f, 1f, 0f);
                gl.Vertex(0, 0, 0);
                gl.Vertex(0, 0.2, 0);

                //Z轴
                gl.Color(0f, 0f, 1f);
                gl.Vertex(0, 0, 0);
                gl.Vertex(0, 0, 0.3);
                gl.End();

                //绘制原点
                gl.Color(0f, 1.0f, 0f);
                gl.LineWidth(1f);
                gl.PointSize(4.0f);
                gl.Begin(OpenGL.GL_POINTS);
                gl.Vertex(0, 0, 0);
                gl.End();

                foreach (var item in this._trackPointList)
                {
                    //G1需要描绘实线轨迹图
                    if (item.PointType == 1)
                    {
                        gl.LineWidth(1.1f);
                        gl.Color(0f, 255f, 255f);
                        gl.Begin(OpenGL.GL_LINES);
                        gl.Vertex(item.L_X, item.L_Y, item.L_Z);//上一个位置
                        gl.Vertex(item.X, item.Y, item.Z);//当前位置
                        gl.End();
                    }
                    //虚线轨迹图
                    else if (item.PointType == 2)
                    {
                        gl.Enable(OpenGL.GL_LINE_STIPPLE);
                        gl.LineStipple(1, 0x0F0F);
                        gl.Begin(OpenGL.GL_LINES);
                        gl.LineWidth(3f);
                        gl.Color(255f, 255f, 255f);
                        gl.Vertex(item.L_X, item.L_Y, item.L_Z);//上一个位置
                        gl.Vertex(item.X, item.Y, item.Z);//当前位置
                        gl.End();
                        gl.Disable(OpenGL.GL_LINE_STIPPLE);
                    }
                    //画圆弧
                    else if (item.PointType == 3)
                    {
                        gl.Begin(OpenGL.GL_LINE_STRIP);
                        for (int i = 0; i < item.CricleList.Count(); i++)
                        {
                            gl.Vertex(item.CricleList[i].X + item.Circle.X - item.Circle.R, item.CricleList[i].Y + item.Circle.Y - item.Circle.R, item.CricleList[i].Z + item.Z);
                        }
                        gl.End();
                    }
                }
            }
            else
            {
                string var1 = this._customPoint.Substring(1, this._customPoint.Length - 2);
                var item = var1.Split(',');
                double X = Convert.ToDouble(item[0]);
                double Y = Convert.ToDouble(item[1]);
                double Z = Convert.ToDouble(item[2]);
                gl.Color(0f, 1.0f, 0f);
                gl.LineWidth(1f);
                gl.PointSize(4.0f);
                gl.Begin(OpenGL.GL_POINTS);
                gl.Vertex(X, Y, Z);
                gl.End();

            }
            gl.Flush();
            gl.Finish();
        }

        /// <summary>
        /// 定时器事件，定时添加G代码
        /// </summary>
        private void timer_Tick(object sender, EventArgs e)
        {
            this.timer.Enabled = false;
            this.lblFinished.Visible = false;

            if (this._currentNumber >= this._nCTextArray.Length)
            {
                this.lblFinished.Visible = true;
                this.timer.Enabled = false;
                return;
            }

            //往全局数据添加数据
            this.AddDrawingData();
            this._currentNumber += 1;
            this.timer.Enabled = true;
        }

        /// <summary>
        /// 往全局数组添加数据
        /// </summary>
        /// <returns></returns>
        private void AddDrawingData()
        {
            var item = this._nCTextArray[this._currentNumber].Replace("\r\n", "");
            //设置是否有绝对值或增量
            if (item.Contains("G90") || item.Contains("G91"))
            {
                this._isAbs = item.Contains("G90") ? true : false;
            }
            //判断直线的状态
            if (item.Contains("G1") || item.Contains("G01"))
            {
                this._isFullLine = true;
            }
            //判断虚线的状态
            if (item.Contains("G0") || item.Contains("G00"))
            {
                this._isFullLine = false;
            }

            Point Point = new Point();
            //虚线
            if (this._isFullLine == false && (item.Contains("X") || item.Contains("Y") || item.Contains("Z")) && !item.Contains("G2") && !item.Contains("G02"))
            {
                Point.PointType = 2;
                Point.IsAbs = this.IsAbs(item);
                //先获取上一次的坐标
                Point.L_X = this._point.X;
                Point.L_Y = this._point.Y;
                Point.L_Z = this._point.Z;
                //判断是否为绝对值，如果是直接赋值，否则是增量将上一次的坐标添加上去
                Point.X = this._isAbs == true ? this.GetCoordinate(item, 1) : this.GetCoordinate(item, 1) + Point.L_X;
                Point.Y = this._isAbs == true ? this.GetCoordinate(item, 2) : this.GetCoordinate(item, 2) + Point.L_Y;
                Point.Z = this._isAbs == true ? this.GetCoordinate(item, 3) : this.GetCoordinate(item, 3) + Point.L_Z;
                this._trackPointList.Add(Point);
                //记录上一次一次的状态
                this._point.X = Point.X;
                this._point.Y = Point.Y;
                this._point.Z = Point.Z;
            }//实线
            else if (this._isFullLine == true && (item.Contains("X") || item.Contains("Y") || item.Contains("Z")) && !item.Contains("G2") && !item.Contains("G02"))
            {
                Point.PointType = 1;
                Point.IsAbs = this.IsAbs(item);
                //先获取上一次的坐标
                Point.L_X = this._point.X;
                Point.L_Y = this._point.Y;
                Point.L_Z = this._point.Z;
                //判断是否为绝对值，如果是直接赋值，否则是增量将上一次的坐标添加上去
                Point.X = this._isAbs == true ? this.GetCoordinate(item, 1) : this.GetCoordinate(item, 1) + Point.L_X;
                Point.Y = this._isAbs == true ? this.GetCoordinate(item, 2) : this.GetCoordinate(item, 2) + Point.L_Y;
                Point.Z = this._isAbs == true ? this.GetCoordinate(item, 3) : this.GetCoordinate(item, 3) + Point.L_Z;
                this._trackPointList.Add(Point);
                //记录上一次一次的状态
                this._point.X = Point.X;
                this._point.Y = Point.Y;
                this._point.Z = Point.Z;
            }
            //画圆
            else if (item.Contains("G02") || item.Contains("G2") || item.Contains("G03") || item.Contains("G3"))
            {
                Point.PointType = 3;
                Point.IsAbs = this.IsAbs(item);
                //先获取上一次的坐标
                Point.L_X = this._point.X;
                Point.L_Y = this._point.Y;
                Point.L_Z = this._point.Z;
                //判断是否为绝对值，如果是直接赋值，否则是增量将上一次的坐标添加上去
                Point.X = this._isAbs == true ? this.GetCoordinateForCircle(item, 1) : this.GetCoordinateForCircle(item, 1) + Point.L_X;
                Point.Y = this._isAbs == true ? this.GetCoordinateForCircle(item, 2) : this.GetCoordinateForCircle(item, 2) + Point.L_Y;
                Point.Z = this._isAbs == true ? this.GetCoordinateForCircle(item, 3) : this.GetCoordinateForCircle(item, 3) + Point.L_Z;
                Point.Circle.R = this.GetCoordinateForCircle(item, 4);
                //记录上一次一次的状态
                this._point.X = Point.X;
                this._point.Y = Point.Y;
                this._point.Z = Point.Z;

                //圆心坐标
                double Circle_X = 0;
                double Circle_Y = 0;
                double Circle_Z = 0;

                
                Point p1 = new Point(Point.L_X, Point.L_Y, Point.L_Z);//起点
                Point p2 = new Point(Point.X, Point.Y, Point.Z);//终点

                //利用两点间到圆心距离相等的条件求出圆心坐标,给出0.00001的误差,这种方法效率较低,不建议使用
                for (double x1 = Point.L_X - Point.Circle.R * 2; x1 <= Point.L_X + Point.Circle.R * 2; x1 += 0.001)
                {
                    for (double y1 = Point.Y - Point.Circle.R * 2; y1 <= Point.Y + Point.Circle.R * 2; y1 += 0.001)
                    {
                        if (((Math.Pow((Point.L_X - x1), 2) + Math.Pow((Point.L_Y - y1), 2)) - Math.Pow(Point.Circle.R, 2)) < 0.00001 && (Math.Pow((Point.X - x1), 2) + Math.Pow((Point.Y - y1), 2) - Math.Pow(Point.Circle.R, 2)) < 0.00001)
                        {
                            Circle_X = x1;
                            Circle_Y = y1;
                            break;
                        }
                    }
                }

                //在画圆弧中，起点为上一个点，终点为当前点
                //求出两点到圆心的斜率，弧度并转化为角度
                int N = 1000;
                float Start_Angel = float.Parse(Math.Atan(Math.Round((Convert.ToDouble(Point.L_Y) - Circle_Y) / (Convert.ToDouble(Point.L_X) - Circle_X), 5)).ToString());
                Start_Angel = float.Parse((180 * Start_Angel / Math.PI).ToString());//开始角度（弧度）
                float End_Angel = float.Parse(Math.Atan(Math.Round((Convert.ToDouble(Point.Y) - Circle_Y) / (Convert.ToDouble(Point.X) - Circle_X), 5)).ToString());
                End_Angel = float.Parse((180 * End_Angel / Math.PI).ToString());//结束角度（弧度）
                float Radius = Point.Circle.R;
                //Start_Angel = 0;
                //End_Angel =360;

                if (Start_Angel < End_Angel)
                {
                    float Diff = End_Angel - Start_Angel;
                    for (int i = 0; i < N; i++)
                    Point.CricleList.Add(new Circle(Radius * Math.Cos((Start_Angel + Diff / N * i) / 360.0 * 2 * Math.PI), Radius * Math.Sin((Start_Angel + Diff / N * i) / 360.0 * 2 * Math.PI)));
                    Point.CricleList.Add(new Circle(Radius * Math.Cos(End_Angel / 360.0 * 2 * Math.PI), Radius * Math.Sin(End_Angel / 360.0 * 2 * Math.PI)));//将圆弧终点加上
                }
                else
                {
                    float Diff = End_Angel - Start_Angel + 360.0f;
                    for (int i = 0; i < N; i++)
                    Point.CricleList.Add(new Circle(Radius * Math.Cos((Start_Angel + Diff / N * i) / 360.0 * 2 * Math.PI), Radius * Math.Cos((Start_Angel + Diff / N * i) / 360.0 * 2 * Math.PI)));
                    Point.CricleList.Add(new Circle(Radius * Math.Cos(End_Angel / 360.0 * 2 * Math.PI), Radius * Math.Sin(End_Angel / 360.0 * 2 * Math.PI)));//将圆弧终点加上
                }
                Point.Circle.X = Circle_X;
                Point.Circle.Y = Circle_Y;
                this._trackPointList.Add(Point);
            }
        }


        /// <summary>
        /// 计算两个坐标点的距离
        /// </summary>
        /// <param name="p1">坐标p1</param>
        /// <param name="p2">坐标p2</param>
        /// <returns></returns>
        private double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X-p2.X)* (p1.X - p2.X)+(p1.Y-p2.Y)* (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z));
        }


        /// <summary>
        /// 传一个路径或者一个坐标，坐标格式（0,0,0）
        /// </summary>
        /// <param name="Parameter">传入的参数，文件路径或者坐标</param>
        /// <param name="Type">显示的类型，True代表模拟加工，False代表全部画出，仅Parameter为路径时有效</param>
        /// <returns></returns>
        public void CreateDrawing(string Parameter, bool Type)
        {
            try
            {
                //判断是否为文件路径,并绘画轨迹图
                if (File.Exists(Parameter))
                {
                    this._isFilePath = true;
                    FileStream fs = new FileStream(Parameter, FileMode.Open, FileAccess.Read);
                    StreamReader sw = new StreamReader(fs, false);
                    string NCText = sw.ReadToEnd();
                    //添加G代码集合
                    this._nCTextArray = NCText.Split('\n');
                    //模拟加工
                    if (Type == true)
                    {
                        this.GLC_Model.OpenGLDraw += new SharpGL.RenderEventHandler(this.GLC_Model_OpenGLDraw);
                        this.timer.Enabled = true;
                    }
                    //全部画出
                    else
                    {
                        this.timer.Enabled = false;
                        this.lblFinished.Text = "Loading...";
                        for (this._currentNumber = 0; this._currentNumber <= this._nCTextArray.Length - 1; this._currentNumber++)
                        {
                            this.AddDrawingData();
                        }
                        this.GLC_Model.OpenGLDraw += new SharpGL.RenderEventHandler(this.GLC_Model_OpenGLDraw);
                        this.lblFinished.Text = "Finished!";
                    }
                }
                //判断是否为坐标，只绘画一个点的状态
                else
                {
                    string var1 = Parameter.Substring(1, Parameter.Length - 2);
                    bool result = true;
                    foreach (var item in var1.Split(','))
                    {
                        result = this.IsNumber(item);
                        if (result == false)
                        {
                            MessageBox.Show("传入格式错误，请检查!");
                            return;
                        }
                    }
                    //坐标形式
                    this._customPoint = Parameter;
                    this._isFilePath = false;
                    this.GLC_Model.OpenGLDraw += new SharpGL.RenderEventHandler(this.GLC_Model_OpenGLDraw);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 判断是否为绝对值
        /// </summary>
        /// <param name="Value">G代码</param>
        /// <returns></returns>
        private bool IsAbs(string Value)
        {
            return Value.Contains("G90") ? true : false;
        }

        /// <summary>
        /// 根据传入的G代码获取对应的坐标值,Type:1代表X，2代表Y，3代表Z
        /// </summary>
        /// <param name="Value">G代码</param>
        /// <param name="Type">类型，1代表X，2代表Y，3代表Z</param>
        /// <returns></returns>
        private float GetCoordinate(string Value, int Type)
        {
            float floatvar = 0;
            string ReturnValue = "";
            string[] ValueArray = Value.Split(' ');

            //取出XYZ的值
            foreach (var item in ValueArray)
            {
                if (Type == 1)
                {
                    if (item.Contains("X"))
                    {
                        ReturnValue = item.Replace("X", "");
                        break;
                    }
                }
                else if (Type == 2)
                {
                    if (item.Contains("Y"))
                    {
                        ReturnValue = item.Replace("Y", "");
                        break;
                    }
                }
                else if (Type == 3)
                {
                    if (item.Contains("Z"))
                    {
                        ReturnValue = item.Replace("Z", "");
                        break;
                    }
                }
            }

            //如果没有数据的话代表坐标没变,就使用上一次的数据
            if (string.IsNullOrEmpty(ReturnValue))
            {
                if (Type == 1)
                {
                    floatvar = float.Parse(this._point.X.ToString());
                }
                else if (Type == 2)
                {
                    floatvar = float.Parse(this._point.Y.ToString());

                }
                else if (Type == 3)
                {
                    floatvar = float.Parse(this._point.Z.ToString());
                }
            }
            else
            {
                double var1 = Convert.ToDouble(ReturnValue);
                double var2 = Math.Round(var1 / 600, 5);  //因为放大缩小的原因，这里需要对读出来的值进行缩小600倍
                floatvar = float.Parse(var2.ToString());
            }
            return floatvar;
        }

        /// <summary>
        /// 根据传入的G代码获取对应的坐标值,Type:1代表X，2代表Y，3代表Z，如果没有则返回0,仅适用于画圆时使用
        /// </summary>
        /// <param name="Value">G代码</param>
        /// <param name="Type">类型，1代表X，2代表Y，3代表Z</param>
        /// <returns></returns>
        private float GetCoordinateForCircle(string Value, int Type)
        {
            float floatvar = 0;
            string ReturnValue = "0";
            string[] ValueArray = Value.Split(' ');
            foreach (var item in ValueArray)
            {
                if (Type == 1)
                {
                    if (item.Contains("X"))
                    {
                        ReturnValue = item.Replace("X", "");
                        break;
                    }
                }
                else if (Type == 2)
                {
                    if (item.Contains("Y"))
                    {
                        ReturnValue = item.Replace("Y", "");
                        break;
                    }
                }
                else if (Type == 3)
                {
                    if (item.Contains("Z"))
                    {
                        ReturnValue = item.Replace("Z", "");
                        break;
                    }
                }
                else if (Type == 4)
                {
                    if (item.Contains("R"))
                    {
                        ReturnValue = item.Replace("R", "");
                        break;
                    }
                }
            }

            double var1 = Convert.ToDouble(ReturnValue);
            double var2 = Math.Round(var1 / 600, 5);  //因为放大缩小的原因，这里需要对读出来的值进行缩小600倍
            floatvar = float.Parse(var2.ToString());
            return floatvar;
        }

        /// <summary>
        /// 判断某一个字符串是否为数字类型
        /// </summary>
        /// <param name="Text">字符串</param>
        /// <returns></returns>
        private bool IsNumber(string Text)
        {
            try
            {
                int result = int.Parse(Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
