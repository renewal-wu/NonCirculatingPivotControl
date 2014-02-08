﻿using System;
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
            ((NonCirculatingPivotItem)this.nonCirculatingPivot.Items[2]).Offest = 70d;
        }

        private void btnAddItemProgrammatically_Click(object sender, RoutedEventArgs e)
        {
            NonCirculatingPivotItem newItem = new NonCirculatingPivotItem();
            newItem.Children.Add(new Image() { Source = new BitmapImage(new Uri("Assets/new.png", UriKind.Relative)) });
            this.nonCirculatingPivot.Children.Add(newItem);
        }
    }
}