using System;
using System.Collections.Generic;
using System.Text;

namespace ESP32_ADC_Linearity_Tester
{
    public static class Database
    {
        // Define the CSV file name.
        private static readonly string FileName = "adc_test_report.csv";

        // Save all statistical results into a CSV file.
        public static void SaveToCsv(
            DateTime timestamp,
            double voltage,
            List<int> samples,
            double mean,
            double median,
            int mode,
            int max,
            int min,
            double stdDev,
            int range,
            double variance,
            double sem,
            double cv)
        {
            try
            {
                // Check if the file does not exist to write the header.
                if (!File.Exists(FileName))
                {
                    string header = "Timestamp,Voltage(V),RawSamples,Mean,Median,Mode,Max,Min,StdDev,Range,Variance,SEM,CV(%)";
                    File.WriteAllText(FileName, header + Environment.NewLine);
                }

                // Convert the list of raw samples into a single string separated by semicolons.
                string rawSamplesText = string.Join(";", samples);

                // Create a formatted string row with all data fields.
                string dataRow = $"{timestamp:yyyy-MM-dd HH:mm:ss},{voltage:F3},\"{rawSamplesText}\"," +
                                 $"{mean:F2},{median:F1},{mode},{max},{min},{stdDev:F2}," +
                                 $"{range},{variance:F2},{sem:F3},{cv:F3}%";

                // Append the new data row to the CSV file.
                File.AppendAllText(FileName, dataRow + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Print error message if file writing fails.
                Console.WriteLine($"Database Error: {ex.Message}");
            }
        }
    }
}
