using ESP32_ADC_Linearity_Tester;
using System;
using System.IO.Ports;
using System.Threading;

class Program
{
    // Define the serial port component
    static SerialPort _serialPort;
    // Control variable to run or stop the loop
    static bool _isRunning = true;

    static List<int> adc_samples = new List<int>();// Store ADC samples for statistics
    const int sample_count = 5;// Number of samples to collect before calculating statistics
    static double voltage_step = 0.0; // Voltage step for each measurement (in volts)
    static bool is_waiting_for_input = true;

    static void Main()
    {        
        // Set port name, baud rate, and standard settings
        _serialPort = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
        // Set the end of line character for reading strings
        _serialPort.NewLine = "\r\n";

        // Wait maximum 1 second for incoming data before timeout
        _serialPort.ReadTimeout = 1000;

        try
        {
            // Open the connection with ESP32
            _serialPort.Open();
            // Show message to inform the user
            Console.WriteLine("Listening to ESP32... Press 'Q' to exit.");

            // Create a separate thread to handle data reading
            Thread read_thread = new Thread(Read_serial_loop);
            // Start running the background thread
            read_thread.Start();

            // Keep the main program alive until user quits
            while (_isRunning)
            {
                if (is_waiting_for_input)
                {
                    Console.Write("\n[1] Enter voltage from Avometer (or 'q' to quit): ");
                    string input = Console.ReadLine();

                    if (input?.ToLower() == "q") { _isRunning = false; break; }

                    if (double.TryParse(input, out voltage_step))
                    {
                        _serialPort.DiscardInBuffer();// Clear any old data in the buffer
                        adc_samples.Clear();// Clear any old samples from the list
                        Console.WriteLine($"[2] Collecting {sample_count} samples now...");                       
                        is_waiting_for_input = false;
                    }
                    else
                    {
                        Console.WriteLine("Invalid number, try again.");
                    }
                }
                Thread.Sleep(50);
            }

            // Close the serial port safely before close
            _serialPort.Close();
        }
        catch (Exception ex)
        {
            // Display error message if connection fails
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void Read_serial_loop()
    {
        // Continue reading while the flag is true
        while (_isRunning)
        {
            try
            {
                // Check if the port is still connected
                if (_serialPort.IsOpen)
                {
                    // Read one full line from the serial buffer
                    string raw_frame = _serialPort.ReadLine();
                    // Send the raw text to process function
                    Process_frame(raw_frame);
                }
            }
            catch (TimeoutException)
            {
                // Ignore timeout error and keep waiting for data
            }
            catch
            {
                // Catch any other sudden communication errors
            }
        }
    }

    private static void Process_frame(string frame)
    {        
        try
        {
            if (is_waiting_for_input) return;

            // Remove control characters and clean the text
            string clean_frame = frame.Replace("\x02", "").Replace("\x03", "").Trim();
            // Split data and checksum values by comma
            string[] parts = clean_frame.Split(',');
            // Stop if the data frame is incomplete
            if (parts.Length < 2) return;

            // Get the first part as sensor data
            string data_payload = parts[0];
            // Get the second part as hex checksum
            string received_hex = parts[1];

            // Initialize variable to calculate checksum
            byte calculatedChecksum = 0;
            // Loop through each character of data payload
            foreach (char c in data_payload)
            {
                // Apply XOR math logic on each byte
                calculatedChecksum ^= (byte)c;
            }

            // Convert calculated number to hex string format
            string calculatedHex = calculatedChecksum.ToString("X");

            // Compare calculated text with received text ignore case
            if (calculatedHex.Equals(received_hex, StringComparison.OrdinalIgnoreCase))
            {

                // Print data when the checksum is correct
                if (int.TryParse(data_payload, out int adc_value))
                {
                    // Add the new sample to our list
                    adc_samples.Add(adc_value);                    

                    // Print the live incoming raw value
                    //Console.WriteLine($"\tadc value: {adc_value:D4}");

                    // Check if we collected enough samples
                    if (adc_samples.Count >= sample_count)
                    {
                        // Get the current date and time
                        DateTime currentTimestamp = DateTime.Now;

                        // Calculate all valuable math and modeling metrics
                        double avg = Statistics.Mean(adc_samples);
                        double median = Statistics.Median(adc_samples);
                        int mode = Statistics.Mode(adc_samples);
                        int max = Statistics.Max(adc_samples);
                        int min = Statistics.Min(adc_samples);
                        double stdDev = Statistics.StandardDeviation(adc_samples);
                        int range = Statistics.Range(adc_samples);
                        double variance = Statistics.Variance(adc_samples);
                        double sem = Statistics.StandardErrorOfMean(adc_samples);
                        double cv = Statistics.CoefficientOfVariation(adc_samples);

                        // Save all calculated data and raw samples to the CSV file
                        Database.SaveToCsv(
                            currentTimestamp,
                            voltage_step,
                            adc_samples,
                            avg,
                            median,
                            mode,
                            max,
                            min,
                            stdDev,
                            range,
                            variance,
                            sem,
                            cv
                        );

                        // Print the statistical summary for the current voltage step
                        Console.WriteLine($"\n--- Statistical Summary (N={sample_count}) ---");
                        Console.WriteLine($"Average ADC : {avg:F2}");
                        Console.WriteLine($"Peak Noise  : {range} steps");
                        Console.WriteLine($"Std Dev     : {stdDev:F2}");
                        Console.WriteLine($"Noise Ratio : {cv:F3}%\n");                        

                        // Clear the list to prepare for new data or next voltage point
                        adc_samples.Clear();
                        is_waiting_for_input = true;
                    }
                }
            }
            else
            {
                // Print error message if the values do not match
                Console.WriteLine($"Data Error! Sent Checksum: {received_hex}, Calculated: {calculatedHex}");
            }
        }
        catch
        {
            // Fail silently if packet parsing fails
        }
    }
}