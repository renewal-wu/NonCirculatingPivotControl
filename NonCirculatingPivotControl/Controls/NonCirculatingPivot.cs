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
        protected bool isTurnBack = false;
        protected bool isNeedMove = false;
        protected double screenWidth = Application.Current.Host.Content.ActualWidth;
        protected double screenHeight = Application.Current.Host.Content.ActualHeight;
        protected int minIndex = 0;
        protected int maxIndex = 3;
        protected double minMove = 50;
        private const NonCirculatingPivotOffsetType defaultOffsetType = NonCirculatingPivotOffsetType.Stick;
        private const bool defaultIsNonSequential = true;
        private const bool defaultIsOnlyForward = false;
        private const double defaultAnimationSpeed = 100.0;
        private const Orientation defaultOrientation = Orientation.Vertical;

        private int _SelectedIndex;
        public virtual int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                if (_SelectedIndex == value)
                    return;
                else if (!(IsOnlyForward && value < _SelectedIndex))
                {
                    _SelectedIndex = value > maxIndex ? maxIndex : value < minIndex ? minIndex : value;
                    if (this.SelectionChanged != null)
                        SelectionChanged(this, new NonCirculatingPivotSelectionChangedArgs() { SelectedIndex = _SelectedIndex, SelectedItem = this.Items[_SelectedIndex] as NonCirculatingPivotItem });
                }
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

        /// <summary>
        /// Orientation changed
        /// </summary>
        public event EventHandler<NonCirculatingPivotOrientationChangedArgs> OrientationChanged;

        public NonCirculatingPivot()
        {
            Transforms.Children.Add(move);
            this.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(NonCirculatingPivot_ManipulationStarted);
            this.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(NonCirculatingPivot_ManipulationDelta);
            this.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(NonCirculatingPivot_ManipulationCompleted);
            this.Items = this.Children;
            this.Loaded += NonCirculatingPivot_Loaded;
            this.LayoutUpdated += NonCirculatingPivot_LayoutUpdated;
        }

        #region OffsetType relate
        public NonCirculatingPivotOffsetType OffsetType
        {
            get { return (NonCirculatingPivotOffsetType)GetValue(OffsetTypeProperty); }
            set { SetValue(OffsetTypeProperty, value); }
        }

        public static readonly DependencyProperty OffsetTypeProperty =
            DependencyProperty.Register("OffsetType", typeof(NonCirculatingPivotOffsetType), typeof(NonCirculatingPivot), new PropertyMetadata(defaultOffsetType, OnOffsetTypeChanged));

        private static void OnOffsetTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
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

        #region IsOnlyForward relate
        public bool IsOnlyForward
        {
            get { return (bool)GetValue(IsOnlyForwardProperty); }
            set { SetValue(IsOnlyForwardProperty, value); }
        }

        public static readonly DependencyProperty IsOnlyForwardProperty =
            DependencyProperty.Register("IsOnlyForward", typeof(bool), typeof(NonCirculatingPivot), new PropertyMetadata(defaultIsOnlyForward));
        #endregion

        #region Orientation relate
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public new static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(StackPanel), new PropertyMetadata(defaultOrientation, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((StackPanel)obj).Orientation = (Orientation)e.NewValue;
            NonCirculatingPivot _target = (NonCirculatingPivot)obj;
            if (_target.OrientationChanged != null)
                _target.OrientationChanged(_target, new NonCirculatingPivotOrientationChangedArgs() { newOrientation = _target.Orientation });
            _target.initItems();
        }
        #endregion

        protected void NonCirculatingPivot_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedIndex = 0;
            initItems();
        }

        public void NonCirculatingPivot_LayoutUpdated(object sender, EventArgs e)
        {
            initItems();
        }

        internal void initItems()
        {
            switch (this.Orientation)
            {
                case Orientation.Horizontal:
                    move.Y = 0;
                    break;
                case Orientation.Vertical:
                    move.X = 0;
                    break;
            }
            if (this.Items.Count > 0)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].GetType() != typeof(NonCirculatingPivotItem))
                        throw new ArgumentException("The NonCirculatingPivot's child must be NonCirculatingPivotItem.");
                    NonCirculatingPivotItem targetElement = (NonCirculatingPivotItem)this.Items[i];
                    targetElement.RenderTransform = Transforms;

                    switch (this.Orientation)
                    {
                        case Orientation.Horizontal:
                            if (i == 0)
                            {
                                targetElement.Margin = new Thickness(targetElement.Movement, 0, targetElement.Margin.Right, targetElement.Margin.Bottom);
                            }
                            else
                            {
                                NonCirculatingPivotItem previousElement = (NonCirculatingPivotItem)this.Items[i - 1];
                                double leftMargin = 0;
                                switch (this.OffsetType)
                                {
                                    case NonCirculatingPivotOffsetType.Normal:
                                        leftMargin = screenWidth - previousElement.ActualWidth - targetElement.Offest - previousElement.Movement;
                                        break;
                                    case NonCirculatingPivotOffsetType.None:
                                        leftMargin = screenWidth - previousElement.ActualWidth - previousElement.Movement;
                                        break;
                                    case NonCirculatingPivotOffsetType.Stick:
                                        leftMargin = previousElement.Movement;
                                        break;
                                }
                                targetElement.Margin = new Thickness(leftMargin, 0, targetElement.Margin.Right, targetElement.Margin.Bottom);
                            }
                            break;
                        case Orientation.Vertical:
                            if (i == 0)
                            {
                                targetElement.Margin = new Thickness(0, targetElement.Movement, targetElement.Margin.Right, targetElement.Margin.Bottom);
                            }
                            else
                            {
                                NonCirculatingPivotItem previousElement = (NonCirculatingPivotItem)this.Items[i - 1];
                                double topMargin = 0;
                                switch (this.OffsetType)
                                {
                                    case NonCirculatingPivotOffsetType.Normal:
                                        topMargin = screenHeight - previousElement.ActualHeight - targetElement.Offest - previousElement.Movement;
                                        break;
                                    case NonCirculatingPivotOffsetType.None:
                                        topMargin = screenHeight - previousElement.ActualHeight - previousElement.Movement;
                                        break;
                                    case NonCirculatingPivotOffsetType.Stick:
                                        topMargin = previousElement.Movement;
                                        break;
                                }
                                targetElement.Margin = new Thickness(0, topMargin, targetElement.Margin.Right, targetElement.Margin.Bottom);
                            }
                            break;
                    }
                    targetElement.Tap -= targetElement_Tap;

                    if (targetElement.TapToMove)
                        targetElement.Tap += targetElement_Tap;
                }
            }

            maxIndex = this.Items.Count - 1;

            if (SelectedIndex > maxIndex)
                SelectedIndex = maxIndex;
            else if (SelectedIndex < minIndex)
                SelectedIndex = minIndex;
            else
                MoveToItemPosition(SelectedIndex);
        }

        protected virtual void targetElement_Tap(object sender, GestureEventArgs e)
        {
            this.SelectedIndex = this.Items.IndexOf((UIElement)sender);
            MoveToItemPosition(SelectedIndex);
        }

        protected virtual void NonCirculatingPivot_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {

        }

        protected virtual void NonCirculatingPivot_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            switch (this.Orientation)
            {
                case Orientation.Horizontal:
                    if (e.DeltaManipulation.Translation.X == 0)
                        return;
                    move.X += e.DeltaManipulation.Translation.X;
                    if (e.DeltaManipulation.Translation.X > 0)
                        isTurnBack = true;
                    else
                        isTurnBack = false;
                    break;
                case Orientation.Vertical:
                    if (e.DeltaManipulation.Translation.Y == 0)
                        return;
                    move.Y += e.DeltaManipulation.Translation.Y;
                    if (e.DeltaManipulation.Translation.Y > 0)
                        isTurnBack = true;
                    else
                        isTurnBack = false;
                    break;
            }
            //Debug.WriteLine("Direction: " + (isTurnBack ? "Back" : "Forward"));
        }

        protected virtual void NonCirculatingPivot_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (Math.Abs(this.Orientation == Orientation.Horizontal ? e.TotalManipulation.Translation.X : e.TotalManipulation.Translation.Y) > minMove)
                isNeedMove = true;
            else
                isNeedMove = false;

            if (!isNeedMove)
            {
                MoveToItemPosition(SelectedIndex);
                return;
            }
            if ((isTurnBack && SelectedIndex == minIndex) || (!isTurnBack && SelectedIndex == maxIndex))
            {
                MoveToItemPosition(SelectedIndex);
            }
            else if (isTurnBack)
            {
                MoveToBackPosition();
            }
            else if (!isTurnBack)
            {
                MoveToForwardPosition();
            }
        }

        protected virtual void MoveToForwardPosition()
        {
            SelectedIndex++;
        }

        protected virtual void MoveToBackPosition()
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

                    switch (this.OffsetType)
                    {
                        case NonCirculatingPivotOffsetType.Normal:
                            totalMovement += ((this.Orientation == Orientation.Horizontal ? (-screenWidth) : (-screenHeight)) + item.Offest + item.Movement);
                            break;
                        case NonCirculatingPivotOffsetType.None:
                            totalMovement += ((this.Orientation == Orientation.Horizontal ? (-screenWidth) : (-screenHeight)) + item.Movement);
                            break;
                        case NonCirculatingPivotOffsetType.Stick:
                            totalMovement += (this.Orientation == Orientation.Horizontal ? -item.ActualWidth : -item.ActualHeight) - item.Movement;
                            break;
                    }
                }
            }

            DoubleAnimation fade = new DoubleAnimation()
            {
                From = this.Orientation == Orientation.Horizontal ? move.X : move.Y,
                To = totalMovement,
                EasingFunction = (itemIndex == 0 || itemIndex == this.Items.Count - 1) ? new BackEase() { EasingMode = EasingMode.EaseOut } as IEasingFunction : new CircleEase() { EasingMode = EasingMode.EaseOut } as IEasingFunction,
                Duration = (itemIndex == 0 || itemIndex == this.Items.Count - 1) ? TimeSpan.FromMilliseconds(AnimationSpeed * 3) : TimeSpan.FromMilliseconds(AnimationSpeed * 3)
            };

            Storyboard.SetTarget(fade, move);
            Storyboard.SetTargetProperty(fade, new PropertyPath(this.Orientation == Orientation.Horizontal ? TranslateTransform.XProperty : TranslateTransform.YProperty));

            Storyboard sb = new Storyboard();
            sb.Children.Add(fade);

            sb.Begin();
        }
    }
}
