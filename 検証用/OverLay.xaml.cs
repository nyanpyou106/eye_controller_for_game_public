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
using System.Windows.Shapes;

using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Interop;

namespace 検証用
{
    /// <summary>
    /// OverLay.xaml の相互作用ロジック
    /// </summary>
    public partial class OverLay : Window
    {
        public OverLay()
        {
            InitializeComponent();
        }

        public class Win32API
        {
            // ウインドウにマウスクリックを透過させる用の諸々
            public const int GWL_EXSTYLE = (-20);
            public const int WS_EX_TRANSPARENT = 0x00000020;

            [DllImport("user32")]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwLong);
        }

        public void SetClickTransparent(object sender, RoutedEventArgs e)
        {
            // ウィンドウのLoadedイベントに設定し、マウスクリックが背後に透過するように設定するための関数
            // 拡張ウィンドウスタイルを用いる
            // OverLayウィンドウのハンドルを取得
            var handle = new WindowInteropHelper(this).Handle;
            // 現在の設定の取得
            int extendStyle = Win32API.GetWindowLong(handle, Win32API.GWL_EXSTYLE);
            // ビット和で設定の追加
            extendStyle |= Win32API.WS_EX_TRANSPARENT;
            Win32API.SetWindowLong(handle, Win32API.GWL_EXSTYLE, extendStyle);
        }

        public void MoveCat(Thickness thickness)
        {
            Cat.Margin = thickness;
            Debug.Write("a");
        }

        public void DrawCircle(int X, int Y, int WIDTH, int HEIGHT)
        {
            // 描画されているものがあればまず消す
            OverLayCanvas.Children.Clear();
            // Create a red Ellipse.
            Ellipse myEllipse = new Ellipse();

            // Create a SolidColorBrush with a red color to fill the
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values.
            // Each value has a range of 0-255.
            mySolidColorBrush.Color = Color.FromArgb(100, 255, 255, 0);
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 1;
            myEllipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            myEllipse.Width = WIDTH;
            myEllipse.Height = HEIGHT;

            // Add the Ellipse to the Canvas.
            OverLayCanvas.Children.Add(myEllipse);
            // 円の半径分ズラした位置に描画
            Canvas.SetLeft(myEllipse, X-(int)(myEllipse.Width/2));
            Canvas.SetTop(myEllipse, Y-(int)(myEllipse.Height/2));
        }
    }
}
