using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GenerateCorrectFile
{
    public partial class GenerateCorrectFileForm : Form
    {
        public GenerateCorrectFileForm()
        {
            InitializeComponent();
        }
        //定义变量
        CalData SysPara = new CalData();//系统数据
        bool WRFlag = false;
        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateCorrectFileForm_Load(object sender, EventArgs e)
        {
            if (File.Exists("SysPara.ini"))
            {
                SysPara = LoadXmlNoPath<CalData>.LoadPara("SysPara.ini");
            }
            else
            {
                SysPara = new CalData()
                {
                    XLength = 350,
                    XMerge = 5,
                    YLength = 350,
                    YMerge = 5
                };
            }
            //更新显示
            UpdateDisplay();
            //绑定事件
            XLengthnumericUpDown.ValueChanged += UpdateSysPara;
            MergeXnumericUpDown.ValueChanged += UpdateSysPara;
            YLengthnumericUpDown.ValueChanged += UpdateSysPara;
            MergeYnumericUpDown.ValueChanged += UpdateSysPara;
            XDeviatenumericUpDown.ValueChanged += UpdateSysPara;
            YDeviatenumericUpDown.ValueChanged += UpdateSysPara;
            CorrectMethodnumericUpDown.ValueChanged += UpdateSysPara;
            CorrectFunctioncomboBox.SelectedIndexChanged += UpdateSysPara;
            SourcetextBox.TextChanged += UpdateSysPara;
            ResulttextBox.TextChanged += UpdateSysPara;
        }
        /// <summary>
        /// 更新数据显示
        /// </summary>
        private void UpdateDisplay()
        {
            WRFlag = true;
            Thread.Sleep(30);
            XLengthnumericUpDown.Value = SysPara.XLength == 0 ? 350 : SysPara.XLength;
            MergeXnumericUpDown.Value = SysPara.XMerge == 0 ? 5 : SysPara.XMerge;
            SysPara.XAffinity = (int)(SysPara.XLength / SysPara.XMerge);
            SysPara.XCalibration = SysPara.XAffinity + 1;
            XCalibratetextBox.Text = SysPara.XCalibration.ToString();
            XAffinitytextBox.Text = SysPara.XAffinity.ToString();
            YLengthnumericUpDown.Value = SysPara.YLength == 0 ? 350 : SysPara.YLength;
            MergeYnumericUpDown.Value = SysPara.YMerge == 0 ? 5 : SysPara.YMerge;
            SysPara.YAffinity = (int)(SysPara.YLength / SysPara.YMerge);
            SysPara.YCalibration = SysPara.YAffinity + 1;
            YCalibratetextBox.Text = SysPara.YCalibration.ToString();
            YAffinitytextBox.Text = SysPara.YAffinity.ToString();
            XDeviatenumericUpDown.Value = SysPara.Cal_DeviateX;
            YDeviatenumericUpDown.Value = SysPara.Cal_DeviateY;
            CorrectMethodnumericUpDown.Value = (SysPara.CorrectMethod == 0) || (SysPara.CorrectMethod == 3) ? 3 : 4;
            CorrectFunctioncomboBox.SelectedIndex = SysPara.CorrectFunction;
            SourcetextBox.Text = SysPara.SourceFilePath;
            ResulttextBox.Text = SysPara.ResultFilePath;
            Thread.Sleep(30);
            WRFlag = false;
        }
        /// <summary>
        /// 保存数据更改
        /// </summary>
        private void UpdateSysPara(object sender, EventArgs e)
        {
            if (WRFlag) return;
            SysPara.XLength = XLengthnumericUpDown.Value;
            SysPara.XMerge = MergeXnumericUpDown.Value;
            SysPara.YLength = YLengthnumericUpDown.Value;
            SysPara.YMerge = MergeYnumericUpDown.Value;
            SysPara.Cal_DeviateX = XDeviatenumericUpDown.Value;
            SysPara.Cal_DeviateY = YDeviatenumericUpDown.Value;
            SysPara.CorrectMethod = (int)CorrectMethodnumericUpDown.Value;
            SysPara.CorrectFunction = CorrectFunctioncomboBox.SelectedIndex;
            SysPara.SourceFilePath = SourcetextBox.Text;
            SysPara.SourceFile = System.IO.Path.GetFileName(SysPara.SourceFilePath);
            SysPara.ResultFilePath = ResulttextBox.Text;
            SysPara.ResultFile = System.IO.Path.GetFileName(SysPara.ResultFilePath);
        }

        /// <summary>
        /// 加载源数据
        /// </summary>
        private void LoadSourceFile()
        {
            // 获取文件名       
            OpenFileDialog openfile = new OpenFileDialog
            {
                Filter = "csv 文件(*.csv)|*.csv"
            };
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                SysPara.SourceFile = System.IO.Path.GetFileName(openfile.FileName);
                SysPara.SourceFilePath = openfile.FileName;
                SourcetextBox.Text = SysPara.SourceFilePath;
            }
            else
            {
                SysPara.SourceFile = "";
                SysPara.SourceFilePath = "";
            }
        }
        /// <summary>
        /// 选择结果数据
        /// </summary>
        private void SelectResultFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "xml 文件   (*.xml)|*.xml";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SysPara.ResultFile = System.IO.Path.GetFileName(saveFileDialog.FileName);
                SysPara.ResultFilePath = saveFileDialog.FileName;
                ResulttextBox.Text = SysPara.ResultFilePath;
            }
        }
        /// <summary>
        /// 生成校准文件
        /// </summary>
        private void GenerateCorrectData()
        {
            if(string.IsNullOrEmpty(SysPara.SourceFilePath))//源数据文件目录为空
            {
                MessageBox.Show("源数据文件路径无效，请选择正确的源文件！！！");
                return;
            }
            if (!File.Exists(SysPara.SourceFilePath))
            {
                MessageBox.Show("源数据文件不存在！！！");
                return;
            }
            if (string.IsNullOrEmpty(SysPara.ResultFilePath))//结果数据文件为空
            {
                MessageBox.Show("结果数据文件路径无效，请选择正确的数据文件！！！");
                return;
            }
            //读取数据
            //建立变量
            List<Affinity_Matrix> Result = new List<Affinity_Matrix>();
            List<Correct_Data> OriginalDatas = new List<Correct_Data>();
            Correct_Data Temp_Correct_Data = new Correct_Data();
            Affinity_Matrix Temp_Affinity_Matrix = new Affinity_Matrix();
            //提取Csv数据
            DataTable Calibration_Data_Acquisition = OpenCSV(SysPara.SourceFilePath);
            Int16 i, j;
            decimal Xo, Yo, Xm, Ym;
            //2.5mm步距进行数据提取和整合，使用INC指令
            for (i = 0; i < Calibration_Data_Acquisition.Rows.Count; i++)
            {
                //清空Temp_Correct_Data
                Temp_Correct_Data = new Correct_Data();
                if ((decimal.TryParse(Calibration_Data_Acquisition.Rows[i][0].ToString(), out Xo)) && (decimal.TryParse(Calibration_Data_Acquisition.Rows[i][1].ToString(), out Yo)) && (decimal.TryParse(Calibration_Data_Acquisition.Rows[i][2].ToString(), out Xm)) && (decimal.TryParse(Calibration_Data_Acquisition.Rows[i][3].ToString(), out Ym)))
                {
                    //数据保存
                    Temp_Correct_Data.Xo = Xo;//理论X坐标
                    Temp_Correct_Data.Yo = Yo;//理论Y坐标
                    Temp_Correct_Data.Xm = Xm - SysPara.Cal_DeviateX;//平台X坐标
                    Temp_Correct_Data.Ym = Ym - SysPara.Cal_DeviateY;//平台Y坐标
                    //添加进入List
                    OriginalDatas.Add(Temp_Correct_Data);
                }
            }            
            //定义仿射变换数组 
            Mat mat = new Mat(new Size(3, 2), Emgu.CV.CvEnum.DepthType.Cv32F, 1); //2行 3列 的矩阵       
            //原坐标
            double[] temp_array;
            //数据处理
            if (SysPara.XCalibration * SysPara.YCalibration == OriginalDatas.Count)//矫正和差异数据完整
            {
                //定义点位数组 
                PointF[] srcTri = new PointF[SysPara.CorrectMethod];//标准数据
                PointF[] dstTri = new PointF[SysPara.CorrectMethod];//差异化数据
                //数据处理
                for (i = 0; i < SysPara.YCalibration - 1; i++)
                {
                    for (j = 0; j < SysPara.XCalibration - 1; j++)
                    {
                        switch (SysPara.CorrectMethod)
                        {
                            case 3:
                                //标准数据  定位坐标
                                srcTri[0] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration].Xo), (float)(OriginalDatas[j + i * SysPara.YCalibration].Yo));
                                srcTri[1] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration].Xo), (float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration].Yo));
                                srcTri[2] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration + 1].Xo), (float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration + 1].Yo));//计算仿射变换矩阵

                                //仿射数据  测量坐标
                                dstTri[0] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration].Xm), (float)(OriginalDatas[j + i * SysPara.YCalibration].Ym));
                                dstTri[1] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration].Xm), (float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration].Ym));
                                dstTri[2] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration + 1].Xm), (float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration + 1].Ym));

                                break;
                            case 4:
                                //标准数据  定位坐标
                                srcTri[0] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration].Xo), (float)(OriginalDatas[j + i * SysPara.YCalibration].Yo));
                                srcTri[1] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration].Xo), (float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration].Yo));
                                srcTri[2] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration + 1].Xo), (float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration + 1].Yo));//计算仿射变换矩阵
                                srcTri[3] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + 1].Xo), (float)(OriginalDatas[j + i * SysPara.YCalibration + 1].Yo));//计算仿射变换矩阵

                                //仿射数据  测量坐标
                                dstTri[0] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration].Xm), (float)(OriginalDatas[j + i * SysPara.YCalibration].Ym));
                                dstTri[1] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration].Xm), (float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration].Ym));
                                dstTri[2] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration + 1].Xm), (float)(OriginalDatas[j + i * SysPara.YCalibration + SysPara.XCalibration + 1].Ym));
                                dstTri[3] = new PointF((float)(OriginalDatas[j + i * SysPara.YCalibration + 1].Xm), (float)(OriginalDatas[j + i * SysPara.YCalibration + 1].Ym));

                                break;
                            default:
                                break;
                        }
                        
                        //计算仿射变换矩阵
                        if (SysPara.CorrectFunction == 0)
                        {
                            mat = CvInvoke.GetAffineTransform(srcTri, dstTri);
                        }
                        else if (SysPara.CorrectFunction == 1)
                        {
                            mat = CvInvoke.EstimateRigidTransform(srcTri, dstTri,true);
                        }
                        //提取矩阵数据
                        temp_array = mat.GetDoubleArray();
                        //获取仿射变换参数
                        Temp_Affinity_Matrix = Array_To_Affinity(temp_array);
                        //追加进入仿射变换List
                        Result.Add(new Affinity_Matrix(Temp_Affinity_Matrix));
                        //清除变量
                        Temp_Affinity_Matrix = new Affinity_Matrix();
                    }
                }
                //保存为文件
                Serialize_Affinity_Matrix(Result, SysPara.ResultFilePath);
            }
               
        }
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateCorrectFileForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveXmlNoPath<CalData>("SysPara.ini",SysPara);//保存数据
        }
        /// <summary>
        /// 保存配置参数 XML
        /// </summary>
        /// <param name="filename"></param>
        public bool SaveXmlNoPath<T>(string filename, T para)
        {
            //xml 序列化
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer bf = new XmlSerializer(typeof(T));
                bf.Serialize(fs, para);
                fs.Close();
            }
            return true;
        }
        // <summary>
        /// 读取配置参数
        /// </summary>
        /// <param name="filename"></param>
        /// /// <summary>
        /// 读取配置参数 返回实例化后的参数，如：类，结构体
        /// </summary>
        /// <param name="filename"></param>
        public class LoadXmlNoPath<T> where T : new()
        {
            public static T LoadPara(string filename)
            {
                T Result = new T();
                if (File.Exists(filename))
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        //xml 反序列化
                        XmlSerializer bf = new XmlSerializer(typeof(T));
                        Result = (T)bf.Deserialize(fs);
                        fs.Close();
                    }
                }
                return Result;
            }
        }
        /// <summary>
        /// 选择要保存的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectFilebutton_Click(object sender, EventArgs e)
        {
            SelectResultFile();
        }
        /// <summary>
        /// 生成校准文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateDatabutton_Click(object sender, EventArgs e)
        {
            GenerateCorrectData();
        }
        /// <summary>
        /// 加载源数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadDatabutton_Click(object sender, EventArgs e)
        {
            LoadSourceFile();
        }
        /// <summary>
        /// 将CSV文件的数据读取到DataTable中
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        /// <returns>返回读取了CSV数据的DataTable</returns>
        public DataTable OpenCSV(string filePath)
        {

            Encoding encoding = EncodingType.GetType(filePath); //获取编码格式
            DataTable dt = new DataTable();
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            StreamReader sr = new StreamReader(fs, encoding);
            //string fileContent = sr.ReadToEnd();
            //encoding = sr.CurrentEncoding;
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] aryLine = null;
            string[] tableHead = null;
            //标示列数
            int columnCount = 0;
            //标示是否是读取的第一行
            bool IsFirst = true;
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                //strLine = Common.ConvertStringUTF8(strLine, encoding);
                //strLine = Common.ConvertStringUTF8(strLine);
                if (IsFirst == true)
                {
                    tableHead = strLine.Split(',');
                    IsFirst = false;
                    columnCount = tableHead.Length;
                    //创建列
                    for (int i = 0; i < columnCount; i++)
                    {
                        DataColumn dc = new DataColumn(tableHead[i]);
                        dt.Columns.Add(dc);
                    }
                }
                else
                {
                    aryLine = strLine.Split(',');
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < columnCount; j++)
                    {
                        dr[j] = aryLine[j];
                    }
                    dt.Rows.Add(dr);
                }
            }
            if (aryLine != null && aryLine.Length > 0)
            {
                dt.DefaultView.Sort = tableHead[0] + " " + "asc";
            }
            sr.Close();
            fs.Close();
            return dt;
        }
        /// <summary>
        /// abstract affinity parameter from array
        /// </summary>
        /// <param name="temp_array"></param>
        /// <returns></returns>
        public Affinity_Matrix Array_To_Affinity(double[] temp_array)
        {
            Affinity_Matrix Result = new Affinity_Matrix
            {
                //获取仿射变换参数
                Stretch_X = Convert.ToDecimal(temp_array[0]),
                Distortion_X = Convert.ToDecimal(temp_array[1]),
                Delta_X = Convert.ToDecimal(temp_array[2]),//x方向偏移
                Stretch_Y = Convert.ToDecimal(temp_array[4]),
                Distortion_Y = Convert.ToDecimal(temp_array[3]),
                Delta_Y = Convert.ToDecimal(temp_array[5])//y方向偏移
            };
            //返回结果
            return Result;
        }
        /// <summary>
        /// List<Affinity_Matrix> 数据序列化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="txtFile"></param>
        public void Serialize_Affinity_Matrix(List<Affinity_Matrix> list, string File_Path)
        {
            using (FileStream fs = new FileStream(File_Path, FileMode.Create, FileAccess.ReadWrite))
            {
                //保存参数至文件 二进制
                //BinaryFormatter bf = new BinaryFormatter();
                //保存为xml
                XmlSerializer bf = new XmlSerializer(typeof(List<Affinity_Matrix>));
                bf.Serialize(fs, list);
            }
        }
    }
    //编码问题目前为止，基本上没人解决，就连windows的IE的自动识别有时还识别错编码呢。--yongfa365   
    //如果文件有BOM则判断，如果没有就用系统默认编码，缺点：没有BOM的非系统编码文件会显示乱码。   
    //调用方法： EncodingType.GetType(filename) 
    public class EncodingType
    {
        /// <summary>
        /// 获取文件编码格式
        /// </summary>
        /// <param name="FILE_NAME"></param>
        /// <returns></returns>
        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
            System.Text.Encoding r = GetType(fs);
            fs.Close();
            return r;
        }
        /// <summary> 
        /// 通过给定的文件流，判断文件的编码类型 
        /// </summary> 
        /// <param name=“fs“>文件流</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            Encoding reVal = Encoding.Default;
            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int.TryParse(fs.Length.ToString(), out int i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;
        }

        /// <summary> 
        /// 判断是否是不带 BOM 的 UTF8 格式 
        /// </summary> 
        /// <param name=“data“></param> 
        /// <returns></returns> 
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数 
            byte curByte; //当前分析的字节. 
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前 
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }

    }
    /// <summary>
    /// 计算用数据结构
    /// </summary>
    [Serializable]    
    public class CalData
    {
        public int CorrectMethod { get; set; }//校准方式
        public int CorrectFunction { get; set; }//校准函数
        public decimal Cal_DeviateX { get; set; }//X偏移值
        public decimal Cal_DeviateY { get; set; }//Y偏移值
        public decimal XLength { get; set; }//X长度
        public decimal XMerge { get; set; }//X间距
        public int XCalibration { get; set; }//X校准数据
        public int XAffinity { get; set; }//X结果数据
        public decimal YLength { get; set; }//Y长度
        public decimal YMerge { get; set; }//Y间距
        public int YCalibration { get; set; }//X校准数据
        public int YAffinity { get; set; }//X结果数据
        public string SourceFile { get; set; }//源数据文件名
        public string SourceFilePath { get; set; }//源数据文件路径
        public string ResultFile { get; set; }//结果数据文件名
        public string ResultFilePath { get; set; }//结果数据文件路径
    }
    /// <summary>
    /// 仿射变换参数
    /// </summary>
    [Serializable]
    public class Affinity_Matrix
    {
        //共有属性
        public decimal Stretch_X { get; set; }
        public decimal Distortion_X { get; set; }
        public decimal Delta_X { get; set; }
        public decimal Stretch_Y { get; set; }
        public decimal Distortion_Y { get; set; }
        public decimal Delta_Y { get; set; }

        //公开构造函数    
        public Affinity_Matrix()
        {
            this.Stretch_X = 0;
            this.Distortion_X = 0;
            this.Delta_X = 0;
            this.Stretch_Y = 0;
            this.Distortion_Y = 0;
            this.Delta_Y = 0;
        }
        //有参数
        public Affinity_Matrix(decimal stretch_x, decimal distortion_x, decimal delta_x, decimal stretch_y, decimal distortion_y, decimal delta_y)
        {
            this.Stretch_X = stretch_x;
            this.Distortion_X = distortion_x;
            this.Stretch_Y = stretch_y;
            this.Distortion_Y = distortion_y;
            this.Delta_X = delta_x;
            this.Delta_Y = delta_y;
        }
        public Affinity_Matrix(Affinity_Matrix Ini)
        {
            this.Stretch_X = Ini.Stretch_X;
            this.Distortion_X = Ini.Distortion_X;
            this.Delta_X = Ini.Delta_X;
            this.Stretch_Y = Ini.Stretch_Y;
            this.Distortion_Y = Ini.Distortion_Y;
            this.Delta_Y = Ini.Delta_Y;
        }
    }
    //矫正数据存储
    [Serializable]
    public class Correct_Data
    {
        //私有属性
        private decimal xo, yo;//x0,y0--基准坐标
        private decimal xm, ym;//x1,y1--轴实际坐标 

        //公开访问的属性
        public decimal Xo
        {
            get { return xo; }
            set { xo = value; }
        }
        public decimal Yo
        {
            get { return yo; }
            set { yo = value; }
        }
        public decimal Xm
        {
            get { return xm; }
            set { xm = value; }
        }
        public decimal Ym
        {
            get { return ym; }
            set { ym = value; }
        }


        //公开访问的方法
        //构造函数
        public Correct_Data(Correct_Data Ini)
        {
            this.xo = Ini.Xo;
            this.yo = Ini.Yo;
            this.xm = Ini.Xm;
            this.ym = Ini.Ym;
        }
        public Correct_Data(decimal xo, decimal yo, decimal xm, decimal ym)
        {
            this.xo = xo;
            this.yo = yo;
            this.xm = xm;
            this.ym = ym;
        }
        //初始化数据
        public Correct_Data()
        {
            this.xo = 0;
            this.yo = 0;
            this.xm = 0;
            this.ym = 0;
        }
    }
    public static class MatExtension
    {

        /*
         * Caution!
         * The following method may leak memory and cause unexcepted errors.
         * Plase use GetArray after calling GetImage methods.
         */
        public static double[] GetDoubleArray(this Mat mat)
        {
            double[] temp = new double[mat.Height * mat.Width];
            Marshal.Copy(mat.DataPointer, temp, 0, mat.Height * mat.Width);
            return temp;
        }
    }

 }
