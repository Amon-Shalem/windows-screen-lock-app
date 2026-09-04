using System.Windows;

namespace CustomScreenLocker
{
    public partial class SecondaryBlankWindow : Window
    {
        public SecondaryBlankWindow(double left, double top, double width, double height)
        {
            InitializeComponent();
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
        }
    }
}
