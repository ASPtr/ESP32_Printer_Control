#include "FS.h"

#include <SPI.h>
#include <TFT_eSPI.h>       // Hardware-specific library

TFT_eSPI tft = TFT_eSPI();  // Invoke custom library

// This is the file name used to store the calibration data
// You can change this to create new calibration files.
// The SPIFFS file name must start with "/".
#define CALIBRATION_FILE "/TouchCalData1"
// Set REPEAT_CAL to true instead of false to run calibration
// again, otherwise it will only be done once.
// Repeat calibration if you change the screen rotation.
#define REPEAT_CAL false

// long count = 0; // Loop count
// uint16_t x, y, last_x, last_y;
// boolean touch;

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

String keyLabel[12] = {"/lamp.xbm", "2", "3", "Light", "5", "6", "7", "Fan", "9", "10", "11", "Stop" };
uint8_t imageBits[512];

TFT_eSPI_Button key[12];

typedef enum { IDLE, PRESSED, JUSTPRESSED, HOLD, RELEASED } KeyState;
KeyState buttonState[12];
const char* status_str[5] = { "IDLE", "PRESSED", "JUSTPRESSED", "HOLD", "RELEASED" };
#define HOLDTIME 1000
#define CHECK_DELAY 100

void touch_calibrate();
void check_and_set_button_state();
void drawXBM(String filename);
void draw_button(uint8_t n, bool invert);

void setup()
{
  Serial.begin(115200);
  tft.begin();
  tft.setRotation(3);	// landscape
  tft.setSwapBytes(true); // Swap the colour byte order when rendering
  
  touch_calibrate();  // Calibrate the touch screen and retrieve the scaling factors

  tft.fillScreen(bk_color);
  tft.setTextFont(4);
  tft.setTextDatum(CC_DATUM);

  // drawXBM();
  // delay(10000);
  
// Draw the keys
  // for (uint8_t row = 0; row < 3; row++) {
  //   for (uint8_t col = 0; col < 4; col++) {
  //     uint8_t b = col + row * 4;

  //     // if (b < 3) tft.setFreeFont(LABEL1_FONT);
  //     // else tft.setFreeFont(LABEL2_FONT);

  //     key[b].initButton(&tft, KEY_X + col * (KEY_W + KEY_SPACING_X),
  //                       KEY_Y + row * (KEY_H + KEY_SPACING_Y), // x, y, w, h, outline, fill, text
  //                       KEY_W, KEY_H, fr_color, bk_color, fr_color,
  //                       "", KEY_TEXTSIZE);
  //     key[b].drawButton();
  //   }
  // }

  for (uint8_t n = 0; n < 12; n++) {
      draw_button(n, 0);
  }

}

void loop()
{
  static uint32_t tmr;
  if (millis() - tmr > CHECK_DELAY) {
  tmr = millis();

  check_and_set_button_state();

  for (uint8_t b = 0; b < 12; b++) {

    if (buttonState[b] == RELEASED){
      // key[b].drawButton();     // draw normal
      // Serial.print("Key "); Serial.print(keyLabel[b]); Serial.println(" Released");
      draw_button(b, 0);
    } 

    if (buttonState[b] == JUSTPRESSED) {
      // key[b].drawButton(true);  // draw invert
      draw_button(b, 1);
    }

    if (buttonState[b] != IDLE) {Serial.print("Key "); Serial.print(keyLabel[b]); Serial.println(status_str[buttonState[b]]);}
  }
  }
}


void touch_calibrate() {
  
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

void check_and_set_button_state() {
  uint16_t t_x = 0, t_y = 0;                   // To store the touch coordinates
  boolean pressed = tft.getTouch(&t_x, &t_y);  // Pressed will be set true is there is a valid touch on the screen
  static uint32_t hold_tmr;

  // / Check if any key coordinate boxes contain the touch coordinates
  for (uint8_t b = 0; b < 12; b++) {
    if (pressed && key[b].contains(t_x, t_y)) {
      switch (buttonState[b]){
            case IDLE:
                buttonState[b] = JUSTPRESSED;
                hold_tmr = millis();
                break;

            case JUSTPRESSED:
                buttonState[b] = PRESSED;
                break;
                
            case RELEASED:
                buttonState[b] = IDLE;
                break;

            case PRESSED:
                if (millis() - hold_tmr > HOLDTIME) {buttonState[b] = HOLD;}
                break;

            case HOLD:
                break;
        }
    } else {
      switch (buttonState[b]){
            case PRESSED:
                buttonState[b] = RELEASED;
                break;

            case RELEASED:
                buttonState[b] = IDLE;
                break;

            case JUSTPRESSED:
                buttonState[b] = RELEASED;
                break;

            case HOLD:
                buttonState[b] = RELEASED;
                break;

            case IDLE:
                break;
        }
    }
  }
}

void drawXBM(String filename){
  // SPIFFS.open("lamp.xbm", "r")
  // xbm_data = file.read()
  // SPIFFS.close()
  // tft.drawXBitmap(x, y, logo, logoWidth, logoHeight, TFT_WHITE);

  // const char *filename = "/lamp.xbm";
  // if (!filename) return;
  // Serial.print("Reading ");
  // Serial.println(filename);
  if (!SPIFFS.exists(filename)) {
    // Serial.println("File not found");
    return;
  }
  File imagefile = SPIFFS.open(filename);
  String xbm;
  int16_t imageWidth=0, imageHeight=0;
  // uint8_t imageBits[imagefile.size()/4]; //This seems inefficient
  uint16_t pos = 0;
  const char CR = 10;
  const char comma = 44;
  while(imagefile.available()) {
    char next = imagefile.read();
    if (next == CR) {
      if (xbm.indexOf("#define") == 0) {
        if (xbm.indexOf("_width ")>0) {
          xbm.remove(0,xbm.lastIndexOf(" "));
          imageWidth = xbm.toInt();
          // if (imageWidth > 320) {
          //   Serial.println("Image too large for screen");
          //   return;
          // }
        } 
        if (xbm.indexOf("_height ")>0) {
          xbm.remove(0,xbm.lastIndexOf(" "));
          imageHeight=xbm.toInt();
          // if (imageHeight > 240) {
          //   Serial.println("Image too large for screen");
          //   return;
          // }
        }
      }
      xbm = "";
    } else if (next == comma) {
      imageBits[pos++] = (uint8_t) strtol(xbm.c_str(), NULL, 16);
      xbm = "";
    } else {xbm += next;}
  }
  imageBits[pos++] = (int) strtol(xbm.c_str(), NULL, 16); //turn the string into a character
  imageBits[pos]=0;
  // tft.drawXBitmap(10, 10, imageBits, imageWidth, imageHeight, TFT_YELLOW);
}

void draw_button(uint8_t n, bool invert) {
  const uint16_t w = 2;
  const uint16_t r = 10;
  uint16_t x = (n % 4) * 80;
  uint16_t y = (int)(n / 4) * 80;

  tft.fillRoundRect(x + ((80 - KEY_W) / 2), y + ((80 - KEY_H) / 2), KEY_W, KEY_H, r, !invert ? fr_color : bk_color);
  tft.fillRoundRect(x + ((80 - KEY_W) / 2) + w, y + ((80 - KEY_H) / 2) + w, KEY_W - w * 2, KEY_H - w * 2, r - w, invert ? fr_color : bk_color);
  if (keyLabel[n].substring(0, 1) != "/") {
    tft.drawString(keyLabel[n], x + 40, y + 40, fr_color);
  }
  else {
    drawXBM(keyLabel[n]);
    tft.drawXBitmap(x + 40 - 48 / 2, y + 40 - 48 / 2, imageBits, 48, 48, !invert ? fr_color : bk_color);
  }
}