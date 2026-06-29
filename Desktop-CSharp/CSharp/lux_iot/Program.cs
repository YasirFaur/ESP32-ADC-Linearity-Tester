using lux_iot;
using System;
using System.IO.Ports;
using System.Threading;

class Program
{
    // Define the serial port component
    static SerialPort _serialPort;
    // Control variable to run or stop the loop
    static bool _isRunning = true;    

    static void Main()
    {
        //public LdrSensor ldr = new LdrSensor();
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
            Thread readThread = new Thread(ReadSerialLoop);
            // Start running the background thread
            readThread.Start();

            // Keep the main program alive until user quits
            while (_isRunning)
            {
                // Check if user pressed the 'Q' key
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                {
                    // Stop the background loop
                    _isRunning = false;
                }
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

    private static void ReadSerialLoop()
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
                    string rawFrame = _serialPort.ReadLine();
                    // Send the raw text to process function
                    ProcessFrame(rawFrame);
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

    private static void ProcessFrame(string frame)
    {
        //create a static instance of the LdrSensor class
        LdrSensor light_sensor = new LdrSensor(fixed_resistor: 50000.0, adcResolution: 4095);

        try
        {
            // Remove control characters and clean the text
            string clean_frame = frame.Replace("\x02", "").Replace("\x03", "").Trim();
            // Split data and checksum values by comma
            string[] parts = clean_frame.Split(',');
            // Stop if the data frame is incomplete
            if (parts.Length < 2) return;

            // Get the first part as sensor data
            string data_payload = parts[0];
            // Get the second part as hex checksum
            string receivedHex = parts[1];

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
            if (calculatedHex.Equals(receivedHex, StringComparison.OrdinalIgnoreCase))
            {
                
                //calculate the resistance using the sensor object
                double ldr_resistance = light_sensor.CalculateResistance(int.Parse(data_payload));

                // Print data when the checksum is correct
                Console.WriteLine($"{(int)ldr_resistance:D8}" + "," + data_payload);
            }
            else
            {
                // Print error message if the values do not match
                Console.WriteLine($"Data Error! Sent Checksum: {receivedHex}, Calculated: {calculatedHex}");
            }
        }
        catch
        {
            // Fail silently if packet parsing fails
        }
    }
}