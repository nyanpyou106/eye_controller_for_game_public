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

using System.Diagnostics;

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

        public void MoveCat()
        {
            Cat.VerticalAlignment = 0;
            Debug.Write("a");
        }
    }
}
