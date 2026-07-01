<p align="center">
  <img src="images/ESP32 ADC Calibration Curve with 4th-Degree Polynomial Model.png" alt="ADC Linearity Graph" width="600">
</p>

# ESP32 ADC Linearity Tester

An electronics project to analyze and map the non-linear behavior of the ESP32 Internal ADC using two methods: direct voltage sweep (0-3.3V) and voltage divider resistance conversion.

## Project Structure
- **Firmware-ESP32/**: Contains Arduino code for raw data transmission packed with safe framing (`\x02`, `\x03`) and XOR checksum validation.
- **Desktop-CSharp/**: Contains the static multi-layered C# application designed to handle real-time serial stream filtering, hardware buffer management, and automated statistical logging.

## Key Features Built
- **Real-Time Data Sync**: Integrated dynamic flow control (`DiscardInBuffer`) to accurately align physical Avometer readings with live microcontroller registers.
- **Statistical Analytics Layer**: A dedicated standalone `Statistics` module computing critical math metrics for signal modeling (Mean, Median, Mode, Range, StdDev, Variance, SEM, and CV%).
- **Automated Data Logger**: A decoupled `Database` module for structured CSV generation (`adc_test_report.csv`) saving multi-sampled telemetry for direct polynomial calibration.

## Current Calibration Model
The project successfully extracted the true structural polynomial signature of the tested ESP32 ADC node:
$$y = 26.512x^4 - 149.48x^3 + 321.98x^2 + 915.38x - 67.233$$