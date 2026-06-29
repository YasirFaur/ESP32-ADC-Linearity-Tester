using System;
using System.Collections.Generic;
using System.Text;

namespace lux_iot
{
    public class LdrSensor
    {
        // The value of the fixed resistor in Ohms.
        public double FixedResistor { get; private set; }

        // The maximum steps of the ADC (4095 for ESP32).
        public int AdcResolution { get; private set; }

        // Constructor to set configuration with default values.
        public LdrSensor(double fixed_resistor = 50000.0, int adcResolution = 4095)
        {
            // Save the fixed resistor value.
            FixedResistor = fixed_resistor;
            // Save the ADC resolution value.
            AdcResolution = adcResolution;
        }

        // Calculate the real LDR resistance from ADC input.
        public double CalculateResistance(int adc_value)
        {
            if (adc_value <= 0) adc_value = 1;
            if (adc_value >= AdcResolution) return 0;

            // Correct equation for: LDR to 3.3V, FixedResistor to GND
            double resistance = FixedResistor * ((double)AdcResolution / adc_value - 1.0);

            return (int)resistance;
        }
    }
}
