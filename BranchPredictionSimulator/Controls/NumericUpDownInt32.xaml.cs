using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BranchPredictionSimulator
{
    /// <summary>
    /// Interaction logic for NumericUpDownInt32.xaml
    /// </summary>
    public partial class NumericUpDownInt32 : UserControl
    {
        public NumericUpDownInt32()
        {
            Increment = 1;
            MinValue = int.MinValue;
            MaxValue = int.MaxValue;

            InitializeComponent();
            tb_UP.DataContext = this;
            tb_UP.Text = Value.ToString();
        }

        public int Increment { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDownInt32), new PropertyMetadata((int)0, ValueChangedCallback));

        public int Value
        {
            get
            {
                return (int)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownInt32 sender = obj as NumericUpDownInt32;
            int newValue = (int)e.NewValue;
            if (newValue > sender.MaxValue)
            {
                sender.Value = (int)e.OldValue;
            }
            else
            {
                if (newValue < sender.MinValue)
                {
                    sender.Value = (int)e.OldValue;
                }
                else
                {
                    sender.tb_UP.Text = sender.Value.ToString();
                }
            }
        }

        public static readonly DependencyProperty TextBoxWidthProperty = DependencyProperty.Register("TextBoxWidth", typeof(double), typeof(NumericUpDownInt32), new PropertyMetadata(30.0d));

        public double TextBoxWidth
        {
            get
            {
                return (double)GetValue(TextBoxWidthProperty);
            }
            set
            {
                SetValue(TextBoxWidthProperty, value);
            }
        }


        private void tb_UP_LostFocus(object sender, RoutedEventArgs e)
        {
            tb_UP.Text = Value.ToString();
        }

        private void tb_UP_TextChanged(object sender, TextChangedEventArgs e)
        {
            int newValue;
            if (int.TryParse(tb_UP.Text, out newValue) && newValue >= MinValue && newValue <= MaxValue)
            {
                Value = newValue;
            }
        }

        private void tb_inc_UP_Click(object sender, RoutedEventArgs e)
        {
            Value += Increment;
        }

        private void tb_dec_UP_Click(object sender, RoutedEventArgs e)
        {
            Value -= Increment;
        }

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(true))
            {
                tb_UP.IsEnabled = true;
                tb_inc_UP.IsEnabled = true;
                tb_dec_UP.IsEnabled = true;
                poly_up.Fill = textForegroundEnabled;
                poly_down.Fill = textForegroundEnabled;
            }
            else
            {
                tb_UP.IsEnabled = false;
                tb_inc_UP.IsEnabled = false;
                tb_dec_UP.IsEnabled = false;
                poly_up.Fill = textForegroundDisabled;
                poly_down.Fill = textForegroundDisabled;
            }
        }

        private static Brush textForegroundEnabled = new SolidColorBrush(Colors.Black);
        private static Brush textForegroundDisabled = new SolidColorBrush(Color.FromRgb(128, 128, 128));
    }
}
