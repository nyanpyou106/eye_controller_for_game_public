using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
// 後から追加したやつ
using System.Runtime.InteropServices;
using Tobii.StreamEngine;
using System.Diagnostics;
using System.Threading;

namespace 検証用
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // フラグ等の初期化
        public static double Dpi_Factor = 1;
        public string SingleOrDouble = "Single";
        OverLay overlay = new OverLay();
        public static Win32API.POINT gazingpoint = new Win32API.POINT();
        public const int GazingCircleAreaWidth_init = 100;
        public const int GazingCircleAreaHeight_init = 100;
        public int GazingCircleAreaWidth = 100;
        public int GazingCircleAreaHeight = 100;
        public int NeedTimeToGaze = 3000; // milliseconds

        public void GetDpiFactorAndShowOverLay(object sender, RoutedEventArgs e)
        {
            // 起動時にDPI倍率を取得し、注視点表示用のオーバーレイを表示するための関数
            // 本来は起動中常に調べて、いつ倍率を変えても正常にマウス操作が出来るようにしたいが、
            // GetDpiFactor()をTaskに渡したOnGazePoint()から上手く呼び出せないため保留
            Window mainwindow = System.Windows.Application.Current.MainWindow;
            Dpi_Factor = PresentationSource.FromVisual(mainwindow).CompositionTarget.TransformFromDevice.M11;

            // オーバーレイ用のウィンドウをMainWIndowの子に設定
            overlay.Owner = this;
            overlay.Show();
        }

        public class Win32API{
            // Win32APIを.NETから使うために必要な構造体の定義

            // POINT構造体の定義
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }

            // MOUSEINPUT構造体の定義
            [StructLayout(LayoutKind.Sequential)]
            public struct MOUSEINPUT
            {
                public int dx; //マウスの座標もしくは最後のイベントからの移動量　dwFlagsによって変わる
                public int dy; //マウスの座標もしくは最後のイベントからの移動量　dwFlagsによって変わる             
                public uint mouseData; //dwFlagsがMOUSEEVENTF_WHEEL,MOUSEEVENTF_XDOWN,MOUSEEVENTF_XUP以外の時は0
                public uint dwFlags; //マウス動作の内容
                public uint time;
                public UIntPtr dwExtraInfo; //使わないのでUIntPtr.Zeroを入れておく
            };

            // dwFlags用の各種定数の定義
            public class dwFlags_values
            {
                public const int MOUSEEVENTF_MOVE = 0x0001;
                public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
                public const int MOUSEEVENTF_LEFTUP = 0x0004;
                public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
                public const int MOUSEEVENTF_RIGHTUP = 0x0010;
                public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
                public const int MOUSEEVENTF_MIDDLEUP = 0x0040;
                public const int MOUSEEVENTF_XDOWN = 0x0080;
                public const int MOUSEEVENTF_XUP = 0x0100;
                public const int MOUSEEVENTF_WHEEL = 0x0800;
                public const int MOUSEEVENTF_HWHEEL = 0x1000;
                public const int MOUSEEVENTF_MOVE_NOCOALESCE = 0x2000;
                public const int MOUSEEVENTF_VIRTUALDESK = 0x4000;
                public const int MOUSEEVENTF_ABSOLUTE = 0x8000;
            }

            // C++のunionの定義
            [StructLayout(LayoutKind.Explicit)]
            public struct INPUT_UNION
            {
                [FieldOffset(0)] public MOUSEINPUT mouse;
                // keyboardやhardwareを使いたい場合はこれらも定義
                // [FieldOffset(0)] public KEYBDINPUT keyboard;
                // [FieldOffset(0)] public HARDWAREINPUT hardware;
            };

            // INPUT構造体の定義
            [StructLayout(LayoutKind.Sequential)]
            public struct INPUT
            {
                public int type; // 0:mouse 1:keyboard 2:hardware
                public INPUT_UNION ui;
            };

            // Win32APIの関数の読み込み
            [DllImport("User32.Dll")]
            public static extern void SetCursorPos(int X, int Y);
            [DllImport("User32.Dll")]
            public static extern void SendInput(
                uint nInputs, // pInputsの要素数
                INPUT pInputs, // INPUT構造体　マウス入力の内容
                int cbSize // INPUT構造体のバイト数
                );
            [DllImport("User32.Dll")]
            public static extern int GetCursorPos(
                out POINT lpPoint // POINT構造体　ここに現在のマウス座標が入るのでoutの指定が必要
                );
            [DllImport("User32.Dll")]
            public static extern int GetDpiForWindow(
                in int hwnd
                );

            // マウス関連関数

            public static POINT GetCurrentMouseCoordinate()
            {
                Win32API.POINT CURRENTCOORDINATE;
                var retval = Win32API.GetCursorPos(out CURRENTCOORDINATE);
                return CURRENTCOORDINATE;
            }

            public static void MouseLeftClick(int X, int Y, string SingleOrDouble)
            {
                Win32API.MOUSEINPUT MI_LEFTCLICK;
                MI_LEFTCLICK.dx = X;
                MI_LEFTCLICK.dy = Y;
                MI_LEFTCLICK.mouseData = 0;
                // マウス動作をビット和で設定
                MI_LEFTCLICK.dwFlags = Win32API.dwFlags_values.MOUSEEVENTF_LEFTDOWN | Win32API.dwFlags_values.MOUSEEVENTF_LEFTUP;
                MI_LEFTCLICK.time = 0;
                MI_LEFTCLICK.dwExtraInfo = UIntPtr.Zero;

                Win32API.INPUT_UNION IU_LEFTCLICK;
                IU_LEFTCLICK.mouse = MI_LEFTCLICK;

                Win32API.INPUT INPUT_LEFTCLICK;
                INPUT_LEFTCLICK.type = 0;
                INPUT_LEFTCLICK.ui = IU_LEFTCLICK;

                Win32API.SendInput(1, INPUT_LEFTCLICK, Marshal.SizeOf(INPUT_LEFTCLICK));
                if (SingleOrDouble == "Double")
                {
                    Win32API.SendInput(1, INPUT_LEFTCLICK, Marshal.SizeOf(INPUT_LEFTCLICK));
                }
            }
        }

        public double CalculateDistance(Win32API.POINT point1, Win32API.POINT point2)
        {
            double distance = Math.Pow((Math.Pow(point1.x - point2.x, 2) + Math.Pow(point1.y - point2.y, 2)), 0.5);
            return distance;
        }

        public void Move_Cursor_Smoothly()
        {
            // クリック地点から対象地点(X,Y)まで滑らかにカーソルを動かす
            // 実装してみたものの目的地到着時の処理に改善の余地あり。
            // 最終目的地からXかYが数pixelズレてしまう上に、現状それを無理やり直しているためカクっと動いてしまう。

            // 現在地と目的地の座標を取得
            Win32API.POINT currentpoint = new Win32API.POINT();
            currentpoint = Win32API.GetCurrentMouseCoordinate();
            Win32API.POINT destinationpoint = new Win32API.POINT();
            destinationpoint.x = Int32.Parse(x_coordinate.Text);
            destinationpoint.y = Int32.Parse(y_coordinate.Text);

            // 現在地と目的地の0.15倍の距離だけカーソルを動かしてそれを目的地に着くまで繰り返す
            // 移動のパラメータは実際に試してみていい感じだったもの
            while (true)
            {
                double Move_X = (destinationpoint.x - currentpoint.x) * 0.15;
                double Move_Y = (destinationpoint.y - currentpoint.y) * 0.15;
                int INT_Move_X = (int)Move_X;
                int INT_Move_Y = (int)Move_Y;

                if (INT_Move_X == 0)
                {
                    INT_Move_X = 1;
                }
                if (INT_Move_Y == 0)
                {
                    INT_Move_Y = 1;
                }
                currentpoint.x = currentpoint.x + INT_Move_X;
                currentpoint.y = currentpoint.y + INT_Move_Y;

                Win32API.SetCursorPos(currentpoint.x, currentpoint.y);
                currentX.Text = currentpoint.x.ToString();
                currentY.Text = currentpoint.y.ToString();

                if (destinationpoint.x == currentpoint.x | destinationpoint.y == currentpoint.y)
                {
                    Win32API.SetCursorPos(destinationpoint.x, destinationpoint.y);
                    break;
                }
                System.Threading.Thread.Sleep(20);
            }
        }

        private void Button_Click_ClickExecuteButton(object sender, RoutedEventArgs e)
        {
            // GUIの座標入力欄から移動先の座標を取得
            int X = Int32.Parse(x_coordinate.Text);
            int Y = Int32.Parse(y_coordinate.Text);
            Win32API.SetCursorPos(X, Y);
            Win32API.MouseLeftClick(X, Y, SingleOrDouble);
        }

        private void Button_Click_SingleClickButton(object sender, RoutedEventArgs e)
        {
            SingleOrDouble = "Single";
            SingleClickButton.Background = Brushes.Red;
            DoubleClickButton.Background = Brushes.LightGray;
        }

        private void Button_Click_DoubleClickButton(object sender, RoutedEventArgs e)
        {
            SingleOrDouble = "Double";
            SingleClickButton.Background = Brushes.LightGray;
            DoubleClickButton.Background = Brushes.Red;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Win32API.POINT currentpoint = new Win32API.POINT();
            currentpoint = Win32API.GetCurrentMouseCoordinate();
            x_coordinate.Text = currentpoint.x.ToString();
            y_coordinate.Text = currentpoint.y.ToString();
        }

        private void Button_Click_MoveSmoothlyButton(object sender, RoutedEventArgs e)
        {
            Move_Cursor_Smoothly();
        }

        // 視線捕捉サンプル
        public static class StreamSample
        {
            private static void OnGazePoint(ref tobii_gaze_point_t gazePoint, IntPtr userData)
            {
                // Check that the data is valid before using it
                if (gazePoint.validity == tobii_validity_t.TOBII_VALIDITY_VALID)
                {
                    //Debug.WriteLine($"Gaze point: {gazePoint.position.x}, {gazePoint.position.y}");
                    
                    // 画面解像度に合わせた座標に変換し、グローバル変数に記録
                    var w_height = System.Windows.SystemParameters.PrimaryScreenHeight;
                    var w_width = System.Windows.SystemParameters.PrimaryScreenWidth;
                    Win32API.SetCursorPos((int)(w_width * gazePoint.position.x / Dpi_Factor), (int)(w_height * gazePoint.position.y / Dpi_Factor));
                    gazingpoint.x = (int)(w_width * gazePoint.position.x / Dpi_Factor);
                    gazingpoint.y = (int)(w_height * gazePoint.position.y / Dpi_Factor);
                }
            }

            public static void StreamSampleMain()
            {
                // Create API context
                IntPtr apiContext;
                tobii_error_t result = Interop.tobii_api_create(out apiContext, null);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

                // Enumerate devices to find connected eye trackers
                List<String> urls;
                result = Interop.tobii_enumerate_local_device_urls(apiContext, out urls);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
                if (urls.Count == 0)
                {
                    Console.WriteLine("Error: No device found");
                    return;
                }

                // Connect to the first tracker found
                IntPtr deviceContext;
                result = Interop.tobii_device_create(apiContext, urls[0], Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_INTERACTIVE, out deviceContext);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

                // Subscribe to gaze data
                result = Interop.tobii_gaze_point_subscribe(deviceContext, OnGazePoint);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

                // This sample will collect 10000 gaze points
                for (int i = 0; i < 10000; i++)
                {
                    // Optionally block this thread until data is available. Especially useful if running in a separate thread.
                    Interop.tobii_wait_for_callbacks(new[] { deviceContext });
                    Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR || result == tobii_error_t.TOBII_ERROR_TIMED_OUT);

                    if (i % 10 == 0) //10回に1回だけ読み込む
                    {
                        // Process callbacks on this thread if data is available
                        Interop.tobii_device_process_callbacks(deviceContext);
                        Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
                    }
                }

                // Cleanup
                result = Interop.tobii_gaze_point_unsubscribe(deviceContext);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
                result = Interop.tobii_device_destroy(deviceContext);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
                result = Interop.tobii_api_destroy(apiContext);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
            }
        }

        private void Button_Click_TrackingStart(object sender, RoutedEventArgs e)
        {
            Task task = new Task(StreamSample.StreamSampleMain);
            task.Start();
        }

        private void MoveOverLay_Click(object sender, RoutedEventArgs e)
        {
            Thickness thickness = new Thickness(100, 100, 0, 0);
            overlay.MoveCat(thickness);
        }

        private void DrawCircle_Click(object sender, RoutedEventArgs e)
        {
            // マウスの位置に円を描画し続ける
            // まずメインスレッドをオブジェクトに入れておく
            var context = SynchronizationContext.Current;
            void DrawCircleContinuous()
            {
                while (true)
                {
                    Win32API.POINT currentpoint = new Win32API.POINT();
                    currentpoint = Win32API.GetCurrentMouseCoordinate();

                    // 描画処理だけメインスレッドで実行させるがここの書き方は正直よくわかってない
                    context.Post(__ =>
                    {
                        int true_x_coordinate = (int)(currentpoint.x * Dpi_Factor);
                        int true_y_coordinate = (int)(currentpoint.y * Dpi_Factor);
                        overlay.DrawCircle(true_x_coordinate, true_y_coordinate, GazingCircleAreaWidth_init, GazingCircleAreaHeight_init);
                    }, null);
                    Thread.Sleep(10);
                }
            }
            Task task = new Task(DrawCircleContinuous);
            task.Start();
        }

        private void DrawCircleAndClick_Click(object sender, RoutedEventArgs e)
        {
            // マウスの位置に円を描画し続ける
            // まずメインスレッドをオブジェクトに入れておく
            var context = SynchronizationContext.Current;

            void DrawCircleContinuous()
            {
                // 座標保存用変数とフラグの宣言
                Win32API.POINT pastpoint = new Win32API.POINT();
                pastpoint.x = -100;
                pastpoint.y = -100;
                Win32API.POINT currentpoint = new Win32API.POINT();
                long pasttime = 0;
                long nowtime = 0;
                bool IsNotMove = false;
                bool IsStarted = false;
                // 時間計測用のストップウォッチ
                var stopwatch = new System.Diagnostics.Stopwatch();

                while (true)
                {
                    currentpoint = Win32API.GetCurrentMouseCoordinate();
                    //currentpoint = gazingpoint;
                    // 比較して一定範囲内ならフラグをTrue　範囲外ならFalse
                    double distance = CalculateDistance(pastpoint, currentpoint) * Dpi_Factor;
                    Debug.WriteLine(distance);
                    if (distance < 10)
                    {
                        Debug.WriteLine("true");
                        IsNotMove = true;
                    }
                    else
                    {
                        Debug.WriteLine("false");
                        // 注視していない場合は初期値にリセット
                        IsNotMove = false;
                        GazingCircleAreaWidth = GazingCircleAreaWidth_init;
                        GazingCircleAreaHeight = GazingCircleAreaHeight_init;
                    }
                    Debug.WriteLine(IsNotMove);
                    // 描画処理だけメインスレッドで実行させるがここの書き方は正直よくわかってない
                    context.Post(__ =>
                    {
                        int true_x_coordinate = (int)(currentpoint.x * Dpi_Factor);
                        int true_y_coordinate = (int)(currentpoint.y * Dpi_Factor);
                        overlay.DrawCircle(true_x_coordinate, true_y_coordinate, GazingCircleAreaWidth, GazingCircleAreaHeight);
                    }, null);

                    // フラグがTrueならSW始動　Falseなら時間をリセット
                    if (IsNotMove == true & IsStarted == false)
                    {
                        Debug.WriteLine("start");
                        stopwatch.Start();
                        pasttime = stopwatch.ElapsedMilliseconds;
                        IsStarted = true;
                    }
                    else if(IsNotMove == false & IsStarted == true)
                    {
                        stopwatch.Stop();
                        stopwatch.Reset();
                        IsStarted = false;
                    }
                    // 経過時間を確認し、NeedTimeToGaze/GazingCircleAreaWidth_initごとに注視円を狭める　規定時間を超えたら左クリックしてリセット
                    nowtime = stopwatch.ElapsedMilliseconds;
                    Debug.WriteLine($"nowtime{nowtime}");
                    Debug.WriteLine($"pasttime{pasttime}");
                    Debug.WriteLine(nowtime - pasttime);
                    if ((nowtime - pasttime) > (NeedTimeToGaze / GazingCircleAreaWidth_init))
                    {
                        GazingCircleAreaWidth -= 1;
                        GazingCircleAreaHeight -= 1;
                        pasttime = nowtime;
                    }

                    if (GazingCircleAreaWidth < 0)
                    {
                        GazingCircleAreaWidth = 0;
                        GazingCircleAreaHeight = 0;
                    }
                    if (stopwatch.ElapsedMilliseconds > NeedTimeToGaze)
                    {
                        Debug.WriteLine("finish");
                        Win32API.MouseLeftClick(currentpoint.x, currentpoint.y, SingleOrDouble);
                        break;
                    }
                    Debug.WriteLine($"Width{GazingCircleAreaWidth}");
                    // 座標保存
                    pastpoint = currentpoint;
                    Thread.Sleep(10);
                }
            }
            Task task = new Task(DrawCircleContinuous);
            Task task2 = new Task(StreamSample.StreamSampleMain);
            task.Start();
            //task2.Start();
        }
    }
}
