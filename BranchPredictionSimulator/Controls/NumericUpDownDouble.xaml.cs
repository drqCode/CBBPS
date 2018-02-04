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
    /// Interaction logic for NumericUpDownDouble.xaml
    /// </summary>
    public partial class NumericUpDownDouble : UserControl
    {
        public NumericUpDownDouble()
        {
            Increment = 0.01d;
            NrDigits = 2;
            MinValue = 0;
            MaxValue = 1;

            InitializeComponent();

            tb_UP.Text = Math.Round(Value, NrDigits).ToString();
        }

        public double Increment { get; set; }
        public int NrDigits { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(NumericUpDownDouble), new PropertyMetadata(0.0d, ValueChangedCallback));

        public double Value
        {
            get
            {
                return (double)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownDouble sender = obj as NumericUpDownDouble;
            double newValue = (double)e.NewValue;
            if (newValue > sender.MaxValue)
            {
                sender.Value = (double)e.OldValue;
            }
            else
            {
                if (newValue < sender.MinValue)
                {
                    sender.Value = (double)e.OldValue;
                }
                else
                {
                    sender.tb_UP.Text = Math.Round(sender.Value, sender.NrDigits).ToString();
                }
            }
        }

        private void tb_UP_LostFocus(object sender, RoutedEventArgs e)
        {
            tb_UP.Text = Math.Round(Value, NrDigits).ToString();
        }

        private void tb_UP_TextChanged(object sender, TextChangedEventArgs e)
        {
            double newValue;
            if (double.TryParse(tb_UP.Text, out newValue) && newValue >= MinValue && newValue <= MaxValue)
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
