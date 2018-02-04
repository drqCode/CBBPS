using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

using OfficeOpenXml;
using PredictionLogic;
using PredictionLogic.SimulationStatistics;
using Microsoft.Win32;
using PredictionLogic.Prediction;
using PredictionLogic.Prediction.BenchmarksAndReaders;
using System.Globalization;

namespace BranchPredictionSimulator
{
    public partial class WindowMain
    {
        private void btnSaveToText_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Title = "Save Results to Text";
            if (saveFileDialog.ShowDialog() == true)
            {
                saveResultsToText(saveFileDialog.FileName);
            }
        }

        private void saveResultsToText(string path)
        {
            StreamWriter streamWriter;

            streamWriter = new StreamWriter(path);
            foreach (KeyValuePair<PredictorInfo, int> predictorAndIndex in simulationResultsDictionary.PredictorsWithIndices)
            {
                streamWriter.WriteLine("Simulation with " + predictorAndIndex.Key.description + " : ");
                streamWriter.WriteLine();
                foreach (KeyValuePair<BenchmarkInfo, int> benchmarkAndIndex in simulationResultsDictionary.BenchmarksWithIndices)
                {
                    streamWriter.WriteLine("Benchmark: " + benchmarkAndIndex.Key.benchmarkName);
                    BenchmarkStatisticsResult result = simulationResultsDictionary.getResult(predictorAndIndex.Value, benchmarkAndIndex.Value);
                    if (result == null)
                    {
                        streamWriter.WriteLine("Simulation on this benchmark was not performed.");
                        streamWriter.WriteLine();
                    }
                    else
                    {
                        streamWriter.WriteLine("Total Branches: " + result.NumberOfBranches);
                        streamWriter.WriteLine("Correct predictions " + result.NumberOfCorrectPredictions);
                        streamWriter.WriteLine("Incorrect predictions: " + result.NumberOfIncorrectPredictions);
                        streamWriter.WriteLine("Accuracy: " + result.PercentAccuracy);
                        streamWriter.WriteLine();
                    }
                }
                streamWriter.WriteLine("Arithmetic Mean");
                streamWriter.WriteLine("Accuracy: " + simulationResultsDictionary.getResultCollectionForPredictor(predictorAndIndex.Key).ArithmeticMean.PercentAccuracy);
            }

            streamWriter.Close();
        }

        private void btnSaveToExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "MS Excel 2007 files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Title = "Save Results to Excel";
            if (saveFileDialog.ShowDialog() == true)
            {
                saveResultsToExcel(saveFileDialog.FileName);
            }
        }

        // Excel package from http://www.codeplex.com/Wikipage?ProjectName=ExcelPackage
        private void saveResultsToExcel(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FileInfo fileInfo = new FileInfo(path);
            using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
            {
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Simulation Results");

                // change the sheet view layout/page
                worksheet.View.PageLayoutView = false;

                worksheet.Cell(2, 2).Value = "Predictors used ↓  |  Benchmarks used →";
                worksheet.Column(2).Width = 40;

                int startRowIndex = 3;
                int startColumnIndex = 3;
                int rowIndex;
                int columnIndex;

                // row above
                columnIndex = startColumnIndex;
                foreach (KeyValuePair<BenchmarkInfo, int> benchmarkAndIndex in simulationResultsDictionary.BenchmarksWithIndices)
                {
                    worksheet.Cell(2, columnIndex).Value = benchmarkAndIndex.Key.benchmarkName;
                    worksheet.Column(columnIndex).Width = 18;
                    columnIndex++;
                }
                worksheet.Cell(2, columnIndex).Value = "Arithmetic Mean";
                worksheet.Column(columnIndex).Width = 17;

                rowIndex = startRowIndex;
                foreach (KeyValuePair<PredictorInfo, int> predictorAndIndex in simulationResultsDictionary.PredictorsWithIndices)
                {
                    worksheet.Cell(rowIndex, 2).Value = predictorAndIndex.Key.description;

                    columnIndex = startColumnIndex;
                    foreach (KeyValuePair<BenchmarkInfo, int> benchmarkAndIndex in simulationResultsDictionary.BenchmarksWithIndices)
                    {
                        var benchmarkStatisticsResult = simulationResultsDictionary.getResult(predictorAndIndex.Value, benchmarkAndIndex.Value);
                        if (benchmarkStatisticsResult != null)
                        {
                            if (!double.IsNaN(benchmarkStatisticsResult.Accuracy))
                            {
                                worksheet.Cell(rowIndex, columnIndex).Value = (benchmarkStatisticsResult.Accuracy * 100).ToString(CultureInfo.InvariantCulture);
                            }
                        }
                        else
                        {
                            worksheet.Cell(rowIndex, columnIndex).Value = " ";
                        }
                        columnIndex++;
                    }

                    // average formula
                    worksheet.Cell(rowIndex, columnIndex).Formula = "AVERAGE(" + worksheet.Cell(rowIndex, startColumnIndex).CellAddress + ":" + worksheet.Cell(rowIndex, columnIndex - 1).CellAddress + ")";
                    rowIndex++;
                }

                // lets set the header text 
                worksheet.HeaderFooter.oddHeader.CenteredText = "Predictor Simulation Results";
                // add the page number to the footer plus the total number of pages
                worksheet.HeaderFooter.oddFooter.RightAlignedText = string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
                // add the sheet name to the footer
                worksheet.HeaderFooter.oddFooter.CenteredText = ExcelHeaderFooter.SheetName;
                // add the file path to the footer
                worksheet.HeaderFooter.oddFooter.LeftAlignedText = ExcelHeaderFooter.FileName;

                // set some core property values
                excelPackage.Workbook.Properties.Title = "Predictor Simulation Results";
                excelPackage.Workbook.Properties.Author = "Andrei Marinică & Alexandru Dorobanțiu";
                excelPackage.Workbook.Properties.Subject = "Branch Prediction Simulation Results";
                excelPackage.Workbook.Properties.Keywords = "Branch Prediction Simulation";
                excelPackage.Workbook.Properties.Category = "Branch Prediction";
                excelPackage.Workbook.Properties.Comments = "Results";

                // save our new workbook and we are done!
                excelPackage.Save();
            }
        }
    }
}
