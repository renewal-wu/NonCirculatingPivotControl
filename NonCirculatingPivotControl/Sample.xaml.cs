using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using NonCirculatingPivotControl.Controls;

namespace NonCirculatingPivotControl
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.nonCirculatingPivot.SelectionChanged += nonCirculatingPivot_SelectionChanged;
        }

        private void nonCirculatingPivot_SelectionChanged(object sender, Controls.NonCirculatingPivotSelectionChangedArgs e)
        {
            Debug.WriteLine(e.SelectedItem.Tag);
        }

        private void btnSelectionChangeByIndex_Click(object sender, RoutedEventArgs e)
        {
            this.nonCirculatingPivot.SelectedIndex = 0;
        }

        private void btnSelectionChangeByItem_Click(object sender, RoutedEventArgs e)
        {
            this.nonCirculatingPivot.SelectedItem = this.nonCirculatingPivot.Items[1] as NonCirculatingPivotItem;
        }

        private void btnChangeItemOffset_Click(object sender, RoutedEventArgs e)
        {
            ((NonCirculatingPivotItem)this.nonCirculatingPivot.Items[2]).Offest = 100d;
        }

        private void btnAddItemProgrammatically_Click(object sender, RoutedEventArgs e)
        {
            NonCirculatingPivotItem newItem = new NonCirculatingPivotItem();
            newItem.Children.Add(new Image() { Source = new BitmapImage(new Uri("Assets/new.png", UriKind.Relative)) });
            this.nonCirculatingPivot.Children.Add(newItem);
        }

        private void btnChangeOrientation_Click(object sender, RoutedEventArgs e)
        {
            this.nonCirculatingPivot.Orientation = System.Windows.Controls.Orientation.Vertical;
        }

        private void btnChangeOffsetType_Click(object sender, RoutedEventArgs e)
        {
            switch (this.nonCirculatingPivot.OffsetType)
            {
                case NonCirculatingPivotOffsetType.None:
                    this.nonCirculatingPivot.OffsetType = NonCirculatingPivotOffsetType.Normal;
                    break;
                case NonCirculatingPivotOffsetType.Normal:
                    this.nonCirculatingPivot.OffsetType = NonCirculatingPivotOffsetType.Stick;
                    break;
                case NonCirculatingPivotOffsetType.Stick:
                    this.nonCirculatingPivot.OffsetType = NonCirculatingPivotOffsetType.None;
                    break;
            }
            Debug.WriteLine("this.nonCirculatingPivot.OffsetType is " + this.nonCirculatingPivot.OffsetType.ToString());
        }
    }
}