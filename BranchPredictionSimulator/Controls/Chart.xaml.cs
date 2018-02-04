using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using PredictionLogic;
using BranchPredictionSimulator.SimulationResultStructures;
using PredictionLogic.Prediction.BenchmarksAndReaders;
using PredictionLogic.Prediction;

namespace BranchPredictionSimulator
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class Chart : UserControl
    {
        private ColumnDefinition AMColumnDefinition;
        private ColumnDefinition GMColumnDefinition;
        private ColumnDefinition HMColumnDefinition;
        private SimulationResultsDictionary currentDictionary = null;

        UniformGrid amGrid;
        UniformGrid gmGrid;
        UniformGrid hmGrid;
        List<UniformGrid> traceGrids = new List<UniformGrid>();

        FontSizeValueConverter fontSizeValueConverter = new FontSizeValueConverter();
        FontVisibilityValueConverter fontVisibilityValueConverter = new FontVisibilityValueConverter();

        private static readonly double opacityAboveLine = 1.0d;
        private static readonly double opacityBelowLine = 0.5d;
        private static readonly double opacityDefault = 1.0d;

        public Chart()
        {
            InitializeComponent();
            this.Width = double.NaN;
            this.Height = double.NaN;

            AMColumnDefinition = new ColumnDefinition { Width = (showAM ? SimpleStarLength : ZeroLength) };
            GMColumnDefinition = new ColumnDefinition { Width = (showGM ? SimpleStarLength : ZeroLength) };
            HMColumnDefinition = new ColumnDefinition { Width = (showHM ? SimpleStarLength : ZeroLength) };

            hideLevelLine();
        }

        private bool showAM = false;
        public bool ShowAM
        {
            get
            {
                return showAM;
            }
            set
            {
                if (showAM != value)
                {
                    AMColumnDefinition.Width = (value ? SimpleStarLength : ZeroLength);
                }
                showAM = value;
            }
        }

        private bool showGM = false;
        public bool ShowGM
        {
            get
            {
                return showGM;
            }
            set
            {
                if (showGM != value)
                {
                    GMColumnDefinition.Width = (value ? SimpleStarLength : ZeroLength);
                }
                showGM = value;
            }
        }

        private bool showHM = false;
        public bool ShowHM
        {
            get
            {
                return showHM;
            }
            set
            {
                if (showHM != value)
                {
                    HMColumnDefinition.Width = (value ? SimpleStarLength : ZeroLength);
                }
                showHM = value;
            }
        }

        private bool showLine = false;
        public bool ShowLine
        {
            get
            {
                return showLine;
            }
            set
            {
                if (value)
                {
                    LevelLineContainer.Visibility = Visibility.Visible;
                }
                else
                {
                    LevelLineContainer.Visibility = Visibility.Collapsed;
                    if (currentDictionary != null)
                    {
                        resetColumnsTransparency();
                    }
                }
                showLine = value;
            }
        }

        public void StartDrawData(SimulationResultsDictionary dictionary)
        {
            DataContainer.Children.Clear();
            DataContainer.ColumnDefinitions.Clear();

            if (currentDictionary != null)
            {
                currentDictionary.newValueReceived -= new simulationResultsDictionaryNewValueReceived(dictionary_NewValueReceived);
            }
            dictionary.newValueReceived += new simulationResultsDictionaryNewValueReceived(dictionary_NewValueReceived);
            currentDictionary = dictionary;

            int column = 0;
            int j;
            FrameworkElement caption;

            traceGrids.Clear();
            foreach (KeyValuePair<BenchmarkInfo, int> benchmarkAndIndex in dictionary.BenchmarksWithIndices)
            {
                UniformGrid traceGrid = new UniformGrid();
                traceGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                traceGrid.Columns = dictionary.Predictors.Count + 2;
                traceGrid.Rows = 1;
                traceGrid.Margin = new Thickness(2);
                traceGrid.Children.Add(new Border());

                foreach (KeyValuePair<PredictorInfo, int> predictorAndIndex in dictionary.PredictorsWithIndices)
                {
                    traceGrid.Children.Add(getNewIndividualColumn(0, predictorAndIndex.Value, predictorAndIndex.Key.description));
                }
                traceGrid.SetValue(Grid.ColumnProperty, column);

                DataContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = SimpleStarLength });
                DataContainer.Children.Add(traceGrid);
                traceGrids.Add(traceGrid);

                caption = getNewCaption(benchmarkAndIndex.Key.benchmarkName);
                caption.SetValue(Grid.ColumnProperty, column);
                caption.SetValue(Grid.RowProperty, 3);
                DataContainer.Children.Add(caption);

                column++;
            }

            amGrid = new UniformGrid();
            amGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            amGrid.Columns = dictionary.Predictors.Count + 2;
            amGrid.Rows = 1;
            amGrid.Margin = new Thickness(2);
            amGrid.Children.Add(new Border());

            j = 0;
            foreach (PredictorInfo predictor in dictionary.Predictors)
            {
                amGrid.Children.Add(getNewIndividualColumn(dictionary.getResultCollectionForPredictor(predictor).ArithmeticMean.Accuracy, j, predictor.description));
                j++;
            }

            amGrid.SetValue(Grid.ColumnProperty, column);
            DataContainer.ColumnDefinitions.Add(AMColumnDefinition);
            DataContainer.Children.Add(amGrid);

            caption = getNewCaption("Arithm. Mean");
            caption.SetValue(Grid.ColumnProperty, column);
            caption.SetValue(Grid.RowProperty, 3);
            DataContainer.Children.Add(caption);

            column++;

            gmGrid = new UniformGrid();
            gmGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            gmGrid.Columns = dictionary.Predictors.Count + 2;
            gmGrid.Rows = 1;
            gmGrid.Margin = new Thickness(2);
            gmGrid.Children.Add(new Border());

            j = 0;
            foreach (PredictorInfo predictor in dictionary.Predictors)
            {
                gmGrid.Children.Add(getNewIndividualColumn(dictionary.getResultCollectionForPredictor(predictor).GeometricMean.Accuracy, j, predictor.description));
                j++;
            }

            gmGrid.SetValue(Grid.ColumnProperty, column);
            DataContainer.ColumnDefinitions.Add(GMColumnDefinition);
            DataContainer.Children.Add(gmGrid);

            caption = getNewCaption("Geom. Mean");
            caption.SetValue(Grid.ColumnProperty, column);
            caption.SetValue(Grid.RowProperty, 3);
            DataContainer.Children.Add(caption);

            column++;

            hmGrid = new UniformGrid();
            hmGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            hmGrid.Columns = dictionary.Predictors.Count + 2;
            hmGrid.Rows = 1;
            hmGrid.Margin = new Thickness(2);
            hmGrid.Children.Add(new Border());

            j = 0;
            foreach (var pred in dictionary.Predictors)
            {
                hmGrid.Children.Add(getNewIndividualColumn(dictionary.getResultCollectionForPredictor(pred).HarmonicMean.Accuracy, j, pred.description));
                j++;
            }

            hmGrid.SetValue(Grid.ColumnProperty, column);
            DataContainer.ColumnDefinitions.Add(HMColumnDefinition);
            DataContainer.Children.Add(hmGrid);

            caption = getNewCaption("Harm. Mean");
            caption.SetValue(Grid.ColumnProperty, column);
            caption.SetValue(Grid.RowProperty, 3);
            DataContainer.Children.Add(caption);
        }

        void dictionary_NewValueReceived(object sender, SimulationResultsDictionaryNewValueReceivedEventArgs e)
        {
            UniformGrid uniformGrid = traceGrids[e.benchmarkIndex];
            setIndividualColumnValue(uniformGrid.Children[e.predictorIndex + 1] as FrameworkElement, e.value.Accuracy);
            setIndividualColumnValue(amGrid.Children[e.predictorIndex + 1] as FrameworkElement, e.correspondingCollection.ArithmeticMean.Accuracy);
            setIndividualColumnValue(gmGrid.Children[e.predictorIndex + 1] as FrameworkElement, e.correspondingCollection.GeometricMean.Accuracy);
            setIndividualColumnValue(hmGrid.Children[e.predictorIndex + 1] as FrameworkElement, e.correspondingCollection.HarmonicMean.Accuracy);
        }

        private FrameworkElement getNewIndividualColumn(double val, int index, string predictorName)
        {
            if (double.IsNaN(val))
            {
                val = 0;
            }
            string text = "" + Math.Round(val * 100, 2) + "%";

            Grid grid = new Grid();
            grid.Tag = predictorName;
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1 - val, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(val, GridUnitType.Star) });

            ToolTip tooltip = new ToolTip();
            tooltip.Content = new TextBlock { Text = text + " - " + predictorName };

            Border column = new Border();
            column.Background = brushes[index % brushes.Length];
            column.SetValue(Grid.RowProperty, 1);
            column.ToolTip = tooltip;

            grid.Children.Add(column);

            TextBlock tbShowNumbers = new TextBlock();
            tbShowNumbers.HorizontalAlignment = HorizontalAlignment.Center;
            tbShowNumbers.VerticalAlignment = VerticalAlignment.Bottom;
            tbShowNumbers.LayoutTransform = new RotateTransform(-90);
            tbShowNumbers.Text = text;
            tbShowNumbers.SetValue(Grid.RowProperty, 1);
            tbShowNumbers.Margin = new Thickness(0.1d, 0, 0.5d, 3d);
            tbShowNumbers.Foreground = WhiteBrush;
            Binding font_binding = new Binding("ActualWidth");
            font_binding.Mode = BindingMode.OneWay;
            font_binding.Source = grid;
            font_binding.Converter = fontSizeValueConverter;
            Binding font_visib = new Binding("ActualWidth");
            font_visib.Mode = BindingMode.OneWay;
            font_visib.Source = grid;
            font_visib.Converter = fontVisibilityValueConverter;

            tbShowNumbers.SetBinding(TextBlock.FontSizeProperty, font_binding);
            tbShowNumbers.SetBinding(TextBlock.VisibilityProperty, font_visib);

            grid.Children.Add(tbShowNumbers);

            return grid;
        }

        private void setIndividualColumnValue(FrameworkElement column, double val)
        {
            if (double.IsNaN(val))
            {
                val = 0;
            }
            string text = "" + Math.Round(val * 100, 2) + "%";
            Grid gd = column as Grid;
            gd.RowDefinitions[0].Height = new GridLength(1 - val, GridUnitType.Star);
            gd.RowDefinitions[1].Height = new GridLength(val, GridUnitType.Star);
            (((gd.Children[0] as Border).ToolTip as ToolTip).Content as TextBlock).Text = text + " - " + column.Tag;
            (gd.Children[1] as TextBlock).Text = text;
        }

        void updateColumnsTransparency(double level)
        {
            for (int predictorIndex = 0; predictorIndex < currentDictionary.NrPredictors; predictorIndex++)
            {
                for (int benchmarkIndex = 0; benchmarkIndex < currentDictionary.NrBenchmarks; benchmarkIndex++)
                {
                    UniformGrid uniformGrid = traceGrids[benchmarkIndex];
                    var result = currentDictionary.getResult(predictorIndex, benchmarkIndex);
                    if (result != null && result.Accuracy < level)
                    {
                        uniformGrid.Children[predictorIndex + 1].Opacity = opacityBelowLine;
                    }
                    else
                    {
                        uniformGrid.Children[predictorIndex + 1].Opacity = opacityAboveLine;
                    }
                }
            }

            foreach (KeyValuePair<PredictorInfo, int> predictorAndIndex in currentDictionary.PredictorsWithIndices)
            {
                var res = currentDictionary.getResultCollectionForPredictor(predictorAndIndex.Key);
                if (res.ArithmeticMean.Accuracy < level)
                {
                    this.amGrid.Children[predictorAndIndex.Value + 1].Opacity = opacityBelowLine;
                }
                else
                {
                    this.amGrid.Children[predictorAndIndex.Value + 1].Opacity = opacityAboveLine;
                }
                if (res.GeometricMean.Accuracy < level)
                {
                    this.gmGrid.Children[predictorAndIndex.Value + 1].Opacity = opacityBelowLine;
                }
                else
                {
                    this.gmGrid.Children[predictorAndIndex.Value + 1].Opacity = opacityAboveLine;
                }
                if (res.HarmonicMean.Accuracy < level)
                {
                    this.hmGrid.Children[predictorAndIndex.Value + 1].Opacity = opacityBelowLine;
                }
                else
                {
                    this.hmGrid.Children[predictorAndIndex.Value + 1].Opacity = opacityAboveLine;
                }
            }
        }

        void resetColumnsTransparency()
        {
            for (int predictorIndex = 0; predictorIndex < currentDictionary.NrPredictors; predictorIndex++)
            {
                for (int benchmarkIndex = 0; benchmarkIndex < currentDictionary.NrBenchmarks; benchmarkIndex++)
                {
                    traceGrids[benchmarkIndex].Children[predictorIndex + 1].Opacity = opacityDefault;
                }
            }

            foreach (var pred in currentDictionary.PredictorsWithIndices)
            {
                this.amGrid.Children[pred.Value + 1].Opacity = opacityDefault;
                this.gmGrid.Children[pred.Value + 1].Opacity = opacityDefault;
                this.hmGrid.Children[pred.Value + 1].Opacity = opacityDefault;
            }
        }

        private FrameworkElement getNewCaption(string name)
        {
            TextBlock caption = new TextBlock();
            caption.Text = name;
            caption.HorizontalAlignment = HorizontalAlignment.Center;
            caption.VerticalAlignment = VerticalAlignment.Center;
            caption.TextWrapping = TextWrapping.Wrap;
            caption.TextAlignment = TextAlignment.Center;
            caption.LayoutTransform = new RotateTransform(-90);
            return caption;
        }

        private static Brush[] brushes = {
                                             new SolidColorBrush(Colors.Red),
                                             new SolidColorBrush(Color.FromRgb(128, 128, 0)),
                                             new SolidColorBrush(Colors.LightGreen),
                                             new SolidColorBrush(Colors.Blue),
                                             new SolidColorBrush(Colors.Lime),
                                             new SolidColorBrush(Colors.Olive),
                                             new SolidColorBrush(Colors.OrangeRed),
                                             new SolidColorBrush(Colors.Violet),
                                             new SolidColorBrush(Colors.Black)
                                         };
        private static Brush WhiteBrush = new SolidColorBrush(Colors.White);

        private static GridLength SimpleStarLength = new GridLength(1, GridUnitType.Star);
        private static GridLength ZeroLength = new GridLength(0);

        static Chart()
        {
        }

        private void updateLevelLine(MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(LevelLineContainer);
            if (mousePosition.Y < 0 || mousePosition.Y > LevelLineContainer.ActualHeight)
            {
                hideLevelLine();
                return;
            }

            double level = 1 - mousePosition.Y / LevelLineContainer.ActualHeight;

            LevelLine.SetValue(Canvas.TopProperty, mousePosition.Y);
            LevelTextBorder.SetValue(Canvas.TopProperty, mousePosition.Y - LevelTextBorder.ActualHeight - 1);
            LevelTextBorder.SetValue(Canvas.LeftProperty, mousePosition.X);
            LevelText.Text = Math.Round(level * 100, 2).ToString() + " %";

            if (currentDictionary != null)
            {
                updateColumnsTransparency(level);
            }
        }

        private void showLevelLine()
        {
            LevelLine.Visibility = Visibility.Visible;
            LevelTextBorder.Visibility = Visibility.Visible;
        }

        private void hideLevelLine()
        {
            LevelLine.Visibility = Visibility.Hidden;
            LevelTextBorder.Visibility = Visibility.Hidden;
        }

        private void LevelLineContainer_MouseMove(object sender, MouseEventArgs e)
        {
            updateLevelLine(e);
        }

        private void LevelLineContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            updateLevelLine(e);
            showLevelLine();
        }

        private void LevelLineContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            hideLevelLine();
            if (currentDictionary != null)
            {
                resetColumnsTransparency();
            }
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class FontSizeValueConverter : IValueConverter
    {
        private static readonly double maxOuterWidth = 18;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double input = (double)value;
            if (input > maxOuterWidth)
            {
                return maxOuterWidth;
            }
            return input - 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(double), typeof(Visibility))]
    public class FontVisibilityValueConverter : IValueConverter
    {
        private static readonly double minOuterWidth = 10;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double input = (double)value;
            if (input < minOuterWidth)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
