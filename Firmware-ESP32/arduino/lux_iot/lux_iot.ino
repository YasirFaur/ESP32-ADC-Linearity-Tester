// Define the analog pin number for the sensor (VP is Pin 36)
#define LDR_PIN 36

void setup() {
  // Start serial communication at 115200 speed
  Serial.begin(115200);
  
  // Set the voltage range to 3.3V for accurate reading
  analogSetAttenuation(ADC_11db);
  /*
  The internal reference voltage of the ESP32 ADC is only 1.1V. 
  This means it cannot measure signals above 1.1V by default.

  Using analogSetAttenuation(ADC_11db) reduces the input signal inside the chip. 
  This allows the ESP32 to safely and accurately measure the full range from 0V up to 3.3V, giving you the maximum reading of 4095.
  */
}

void loop() {
  // Read the analog signal from the sensor (values 0 to 4095)
  int adcValue = analogRead(LDR_PIN);
  
  // Convert the integer number to text (String)
  String dataStr = String(adcValue);
  
  // Create a variable to store the error check value
  byte checksum = 0;
  
  // Loop through every character in the text to calculate checksum
  for (unsigned int i = 0; i < dataStr.length(); i++) {
    // Use XOR logic on each character to make the security code
    checksum ^= dataStr[i];

    /*
    In C#, the application extracts the text for example "100" from the frame. 
    Then, it repeats the exact same XOR logic on its characters.
    If the calculated result equals 49, C# compares it with the received checksum byte. 
    If they match, the program knows the data is 100% safe and correct. 
    Any cable noise will change the code, and C# will reject the bad frame.
    */
  }

  // Send the Start of Text byte (STX = 0x02) to C#
  Serial.write(0x02);
  
  // Send the actual sensor data text
  Serial.print(dataStr);
  
  // Send a comma as a separator character
  Serial.print(",");
  
  // Send the checksum value in Hexadecimal format
  Serial.print(checksum, HEX);
  
  // Send the End of Text byte (ETX = 0x03) to C#
  Serial.write(0x03);
  
  // Print a new line to clean up the display
  Serial.println();

  // Wait for 500 milliseconds (half a second) before next reading
  delay(500);
}