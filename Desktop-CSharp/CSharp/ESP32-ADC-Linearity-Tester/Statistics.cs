// Import system tools.
using System;
// Import list collection tools.
using System.Collections.Generic;
// Import data query tools.
using System.Linq;

namespace ESP32_ADC_Linearity_Tester
{
    // Define a public static class for math tools.
    public static class Statistics
    {
        // 1. Calculate the mean or average value.
        public static double Mean(List<int> values)
        {
            // Return 0 if the list is empty or null.
            if (values == null || values.Count == 0) return 0;
            // Find and return the average of the numbers.
            return values.Average();
        }

        // 2. Find the middle value or median.
        public static double Median(List<int> values)
        {
            // Return 0 if the list is empty or null.
            if (values == null || values.Count == 0) return 0;

            // Sort the list from small to large numbers.
            var sorted = values.OrderBy(n => n).ToList();
            // Get the total number of elements.
            int count = sorted.Count;

            // Check if the total number is even.
            if (count % 2 == 0)
                // Average the two middle numbers.
                return (sorted[(count / 2) - 1] + sorted[count / 2]) / 2.0;
            else
                // Return the single middle number.
                return sorted[count / 2];
        }

        // 3. Find the most frequent value or mode.
        public static int Mode(List<int> values)
        {
            // Return 0 if the list is empty or null.
            if (values == null || values.Count == 0) return 0;

            // Group identical numbers together, sort by count, and get the top key.
            return values.GroupBy(v => v)
                         .OrderByDescending(g => g.Count())
                         .First()
                         .Key;
        }

        // 4. Find the maximum value.
        public static int Max(List<int> values)
        {
            // If the list has items, return the max value. Else, return 0.
            return values.Count > 0 ? values.Max() : 0;
        }

        // 5. Find the minimum value.
        public static int Min(List<int> values)
        {
            // If the list has items, return the min value. Else, return 0.
            return values.Count > 0 ? values.Min() : 0;
        }

        // 6. Calculate the standard deviation of the sample.
        public static double StandardDeviation(List<int> values)
        {
            // Return 0 if there are fewer than two values.
            if (values == null || values.Count <= 1) return 0;

            // Get the average value of the list.
            double avg = Mean(values);
            // Calculate the sum of squared differences from the average.
            double sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));

            // Return the square root of the variance.
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }

        // 7. Calculate the range (Max - Min).
        public static int Range(List<int> values)
        {
            // Return 0 if the list is empty.
            if (values == null || values.Count == 0) return 0;
            // Return the difference between maximum and minimum values.
            return Max(values) - Min(values);
        }

        // 8. Calculate the variance (Noise Power).
        public static double Variance(List<int> values)
        {
            // Return 0 if there are fewer than two values.
            if (values == null || values.Count <= 1) return 0;
            // Get the standard deviation value.
            double stdDev = StandardDeviation(values);
            // Return the square of the standard deviation.
            return Math.Pow(stdDev, 2);
        }

        // 9. Calculate the Standard Error of the Mean (SEM).
        public static double StandardErrorOfMean(List<int> values)
        {
            // Return 0 if the list is empty or null.
            if (values == null || values.Count == 0) return 0;
            // Divide standard deviation by the square root of the sample count.
            return StandardDeviation(values) / Math.Sqrt(values.Count);
        }

        // 10. Calculate the Coefficient of Variation (CV percentage).
        public static double CoefficientOfVariation(List<int> values)
        {
            // Get the average value of the list.
            double avg = Mean(values);
            // Return 0 if the average is zero to avoid division error.
            if (avg == 0) return 0;
            // Return the percentage of standard deviation to the mean.
            return (StandardDeviation(values) / avg) * 100.0;
        }
    }
}
