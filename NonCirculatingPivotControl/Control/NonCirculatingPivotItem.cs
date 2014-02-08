using System;
using System.Windows;
using System.Windows.Controls;

namespace NonCirculatingPivotControl.Controls
{
    public class NonCirculatingPivotItem : Grid
    {
        private const double defaultOffset = 50.0;

        public NonCirculatingPivotItem()
        {

        }

        #region TapToMove
        public bool TapToMove
        {
            get { return (bool)GetValue(TapToMoveProperty); }
            set { SetValue(TapToMoveProperty, value); }
        }

        public static readonly DependencyProperty TapToMoveProperty =
            DependencyProperty.Register("TapToMove", typeof(bool), typeof(NonCirculatingPivotItem), new PropertyMetadata(true, OnTapToMoveChanged));

        private static void OnTapToMoveChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NonCirculatingPivotItem _target = (NonCirculatingPivotItem)obj;
            NonCirculatingPivot parent = (NonCirculatingPivot)_target.Parent;
            if (parent == null)
                return;
            if (parent.GetType() != typeof(NonCirculatingPivot))
                throw new Exception("NonCirculatingPivotItem's parent must be NonCirculatingPivot.");

            ((NonCirculatingPivot)_target.Parent).initItems();
        }
        #endregion

        #region offset
        public double Offest
        {
            get { return (double)GetValue(OffestProperty); }
            set { SetValue(OffestProperty, value); }
        }

        public static readonly DependencyProperty OffestProperty =
            DependencyProperty.Register("Offest", typeof(double), typeof(NonCirculatingPivotItem), new PropertyMetadata(defaultOffset, OnOffestChanged));

        private static void OnOffestChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NonCirculatingPivotItem _target = (NonCirculatingPivotItem)obj;
            NonCirculatingPivot parent = (NonCirculatingPivot)_target.Parent;
            if (parent == null)
                return;
            if (parent.GetType() != typeof(NonCirculatingPivot))
                throw new Exception("NonCirculatingPivotItem's parent must be NonCirculatingPivot.");
        }
        #endregion
    }
}
