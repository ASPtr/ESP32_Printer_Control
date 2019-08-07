#include "FS.h"

#include <SPI.h>
#include <TFT_eSPI.h>       // Hardware-specific library

TFT_eSPI tft = TFT_eSPI();  // Invoke custom library

// Include the header files that contain the icons
#include "bmp.h"
// #include "press.h"

// This is the file name used to store the calibration data
// You can change this to create new calibration files.
// The SPIFFS file name must start with "/".
#define CALIBRATION_FILE "/TouchCalData1"
// Set REPEAT_CAL to true instead of false to run calibration
// again, otherwise it will only be done once.
// Repeat calibration if you change the screen rotation.
#define REPEAT_CAL false

long count = 0; // Loop count
uint16_t x, y, last_x, last_y;
boolean touch;

uint16_t fr_color = tft.color565(3, 230, 250);
uint16_t bk_color = tft.color565(65, 0, 87);

// Keypad start position, key sizes and spacing
#define KEY_X 40 // Centre of key
#define KEY_Y 40
#define KEY_W 70 // Width and height
#define KEY_H 70
#define KEY_SPACING_X 10 // X and Y gap
#define KEY_SPACING_Y 10
#define KEY_TEXTSIZE 1   // Font size multiplier

char keyLabel[12][5] = {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };

TFT_eSPI_Button key[12];

void touch_calibrate();

void setup()
{
  Serial.begin(115200);
  tft.begin();
  tft.setRotation(3);	// landscape

  // Calibrate the touch screen and retrieve the scaling factors
  touch_calibrate();

  tft.fillScreen(bk_color);

  // Swap the colour byte order when rendering
  tft.setSwapBytes(true);

  // // Draw the icons
  // for (size_t y = 0; y < 3; y++)
  // {
  //   for (size_t x = 0; x < 4; x++)
  //   {
  //     tft.pushImage(x*80, y*80, offWidth, offHeight, off);

      tft.setTextFont(4);
  //     tft.setTextDatum(CC_DATUM);
  //     // tft.setTextPadding(tft.textWidth("-123"));
  //     tft.setTextColor(TFT_YELLOW, TFT_BLACK);
  //     tft.drawNumber(x+y*4+1, x*80+40, y*80+40);
  //   }
    
  // }
  
// Draw the keys
  for (uint8_t row = 0; row < 3; row++) {
    for (uint8_t col = 0; col < 4; col++) {
      uint8_t b = col + row * 4;

      // if (b < 3) tft.setFreeFont(LABEL1_FONT);
      // else tft.setFreeFont(LABEL2_FONT);

      key[b].initButton(&tft, KEY_X + col * (KEY_W + KEY_SPACING_X),
                        KEY_Y + row * (KEY_H + KEY_SPACING_Y), // x, y, w, h, outline, fill, text
                        KEY_W, KEY_H, fr_color, bk_color, fr_color,
                        keyLabel[b], KEY_TEXTSIZE);
      key[b].drawButton();
    }
  }

}

void loop()
{
  uint16_t t_x = 0, t_y = 0; // To store the touch coordinates
   
  

  // Pressed will be set true is there is a valid touch on the screen
  boolean pressed = tft.getTouch(&t_x, &t_y);
  if (pressed){
    x = (int)(t_x / 80) * 80; y = (int)(t_y / 80) * 80;
    tft.pushImage(x, y, pressWidth, pressHeight, press);
    last_x = x; last_y = y;
    touch = true;
    Serial.println("Touch");
    // Serial.println(t_y);
    // Serial.println(x);
    // Serial.println(y);
  }
  if (!pressed && touch){
    tft.pushImage(last_x, last_y, offWidth, offHeight, off);
    touch = false;
    Serial.print("Realise");// Serial.println(SPIFFS.totalBytes());
    // Serial.print("Used:  "); Serial.println(SPIFFS.usedBytes());
    tft.drawNumber(last_y+last_x*4+1, last_x+40, last_y+40);
  }
}


void touch_calibrate()
{
  uint16_t calData[5];
  uint8_t calDataOK = 0;

  // check file system exists
  if (!SPIFFS.begin()) {
    Serial.println("Formating file system");
    SPIFFS.format();
    SPIFFS.begin();
  }

  // check if calibration file exists and size is correct
  if (SPIFFS.exists(CALIBRATION_FILE)) {
    if (REPEAT_CAL)
    {
      // Delete if we want to re-calibrate
      SPIFFS.remove(CALIBRATION_FILE);
    }
    else
    {
      File f = SPIFFS.open(CALIBRATION_FILE, "r");
      if (f) {
        if (f.readBytes((char *)calData, 14) == 14)
          calDataOK = 1;
        f.close();
      }
    }
  }

  if (calDataOK && !REPEAT_CAL) {
    // calibration data valid
    tft.setTouch(calData);
  } else {
    // data not valid so recalibrate
    tft.fillScreen(TFT_BLACK);
    tft.setCursor(20, 0);
    tft.setTextFont(2);
    tft.setTextSize(1);
    tft.setTextColor(TFT_WHITE, TFT_BLACK);

    tft.println("Touch corners as indicated");

    tft.setTextFont(1);
    tft.println();

    if (REPEAT_CAL) {
      tft.setTextColor(TFT_RED, TFT_BLACK);
      tft.println("Set REPEAT_CAL to false to stop this running again!");
    }

    tft.calibrateTouch(calData, TFT_MAGENTA, TFT_BLACK, 15);

    tft.setTextColor(TFT_GREEN, TFT_BLACK);
    tft.println("Calibration complete!");

    // store data
    File f = SPIFFS.open(CALIBRATION_FILE, "w");
    if (f) {
      f.write((const unsigned char *)calData, 14);
      f.close();
    }
  }
}