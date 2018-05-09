uint8_t my_header[] = {0x34, 0x75}; // some random header
uint8_t my_data[] = "Hello!";       // some data

void setup()
{
  Serial.begin(9600);
}

void loop()
{
  Serial.write(my_header, sizeof(my_header));

  // Send first 3 bytes of data, wait 100ms and send rest bytes
  Serial.write(my_data, 3);
  delay(100);
  Serial.write(my_data+3, sizeof(my_data)-3);
  delay(500);

  delay(500);
}
