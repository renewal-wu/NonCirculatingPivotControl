using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NonCirculatingPivotControl.Controls
{
    public class NonCirculatingPivot : StackPanel
    {
        protected TranslateTransform move = new TranslateTransform();
        protected TransformGroup Transforms = new TransformGroup();
        public UIElementCollection Items { get; protected set; }
        protected bool isTurnRight = false;
        protected bool isNeedMove = false;
        protected double screenWidth = Application.Current.Host.Content.ActualWidth;
        protected int minIndex = 0;
        protected int maxIndex = 3;
        protected double minMove = 50;
        private const bool defaultIsOffsetEnable = true;
        private const bool defaultIsNonSequential = true;

        private const double defaultAnimationSpeed = 100.0;
        private int _SelectedIndex;
        public virtual int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                _SelectedIndex = value > maxIndex ? maxIndex : value < minIndex ? minIndex : value;
                if (this.SelectionChanged != null)
                    SelectionChanged(this, new NonCirculatingPivotSelectionChangedArgs() { SelectedIndex = _SelectedIndex, SelectedItem = this.Items[_SelectedIndex] as NonCirculatingPivotItem });
                this.MoveToItemPosition(_SelectedIndex);
            }
        }
        public virtual NonCirculatingPivotItem SelectedItem
        {
            get
            {
                return this.Items[SelectedIndex] as NonCirculatingPivotItem;
            }
            set
            {
                if (this.Items.Contains(value))
                {
                    this.SelectedIndex = this.Items.IndexOf(value);
                }
            }
        }

        /// <summary>
        /// Item selection changed event.
        /// </summary>
        public event EventHandler<NonCirculatingPivotSelectionChangedArgs> SelectionChanged;

        public NonCirculatingPivot()
        {
            Transforms.Children.Add(move);
            this.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(NonSequentialPivot_ManipulationStarted);
            this.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(NonSequentialPivot_ManipulationDelta);
            this.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(NonSequentialPivot_ManipulationCompleted);
            this.Items = this.Children;
            this.Orientation = Orientation.Horizontal;
            this.Loaded += NonSequentialPivot_Loaded;
            this.LayoutUpdated += NonSequentialPivot_LayoutUpdated;
        }

        #region IsOffsetEnable relate
        public bool IsOffsetEnable
        {
            get { return (bool)GetValue(IsOffsetEnableProperty); }
            set { SetValue(IsOffsetEnableProperty, value); }
        }

        public static readonly DependencyProperty IsOffsetEnableProperty =
            DependencyProperty.Register("IsOffsetEnable", typeof(bool), typeof(NonCirculatingPivot), new PropertyMetadata(defaultIsOffsetEnable, OnIsOffsetEnableChanged));

        private static void OnIsOffsetEnableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NonCirculatingPivot _target = (NonCirculatingPivot)obj;
            _target.initItems();
        }
        #endregion

        #region animation speed
        public double AnimationSpeed
        {
            get { return (double)GetValue(AnimationSpeedProperty); }
            set { SetValue(AnimationSpeedProperty, value); }
        }

        public static readonly DependencyProperty AnimationSpeedProperty =
            DependencyProperty.Register("AnimationSpeed", typeof(double), typeof(NonCirculatingPivot), new PropertyMetadata(defaultAnimationSpeed));

        #endregion


        protected void NonSequentialPivot_Loaded(object sender, RoutedEventArgs e)
        {
            initItems();
            SelectedIndex = 0;
        }

        public void NonSequentialPivot_LayoutUpdated(object sender, EventArgs e)
        {
            initItems();
        }

        internal void initItems()
        {
            if (this.Items.Count > 0)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].GetType() != typeof(NonCirculatingPivotItem))
                        throw new ArgumentException("The NonSequentialPivot's child must be NonSequentialPivotItem.");
                    NonCirculatingPivotItem targetElement = (NonCirculatingPivotItem)this.Items[i];
                    targetElement.RenderTransform = Transforms;
                    if (i == 0)
                    {
                        targetElement.Margin = new Thickness(0, targetElement.Margin.Top, targetElement.Margin.Right, targetElement.Margin.Bottom);
                    }
                    else
                    {
                        NonCirculatingPivotItem previousElement = (NonCirculatingPivotItem)this.Items[i - 1];
                        targetElement.Margin = new Thickness(screenWidth - previousElement.ActualWidth - (this.IsOffsetEnable ? targetElement.Offest : 0), targetElement.Margin.Top, targetElement.Margin.Right, targetElement.Margin.Bottom);
                    }
                    targetElement.Tap -= targetElement_Tap;

                    if (targetElement.TapToMove)
                        targetElement.Tap += targetElement_Tap;
                }
            }

            maxIndex = this.Items.Count - 1;
        }

        protected virtual void targetElement_Tap(object sender, GestureEventArgs e)
        {
            this.SelectedIndex = this.Items.IndexOf((UIElement)sender);
            MoveToItemPosition(SelectedIndex);
        }

        protected virtual void NonSequentialPivot_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {

        }

        protected virtual void NonSequentialPivot_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            move.X += e.DeltaManipulation.Translation.X;

            if (e.DeltaManipulation.Translation.X >= 0)
                isTurnRight = true;
            else
                isTurnRight = false;

            Debug.WriteLine("Direction: " + (isTurnRight ? "Right" : "Left"));
        }

        protected virtual void NonSequentialPivot_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (Math.Abs(e.TotalManipulation.Translation.X) > minMove)
                isNeedMove = true;
            else
                isNeedMove = false;

            if (!isNeedMove)
            {
                MoveToItemPosition(SelectedIndex);
                return;
            }
            if ((isTurnRight && SelectedIndex == minIndex) || (!isTurnRight && SelectedIndex == maxIndex))
            {
                MoveToItemPosition(SelectedIndex);
            }
            else if (isTurnRight)
            {
                MoveToRightPosition();
            }
            else if (!isTurnRight)
            {
                MoveToLeftPosition();
            }
        }

        protected virtual void MoveToLeftPosition()
        {
            SelectedIndex++;
        }

        protected virtual void MoveToRightPosition()
        {
            SelectedIndex--;
        }

        protected virtual void MoveToItemPosition(int itemIndex)
        {
            double totalMovement = 0;
            if (itemIndex == 0)
                totalMovement = 0;
            else
            {
                for (int i = 1; i <= itemIndex; i++)
                {
                    NonCirculatingPivotItem item = (NonCirculatingPivotItem)this.Items[i];
                    totalMovement += (-screenWidth + item.Offest);
                }
            }
            var fade = new DoubleAnimation()
            {
                From = move.X,
                To = totalMovement,
                Duration = TimeSpan.FromMilliseconds(AnimationSpeed),
            };

            Storyboard.SetTarget(fade, move);
            Storyboard.SetTargetProperty(fade, new PropertyPath(TranslateTransform.XProperty));

            var sb = new Storyboard();
            sb.Children.Add(fade);

            sb.Begin();
        }
    }
}
