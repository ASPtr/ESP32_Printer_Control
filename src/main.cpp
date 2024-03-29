#include "FS.h"
#include <SPI.h>
#include <TFT_eSPI.h>

TFT_eSPI tft = TFT_eSPI();

// This is the file name used to store the calibration data
// You can change this to create new calibration files.
// The SPIFFS file name must start with "/".
#define CALIBRATION_FILE "/TouchCalData1"
// Set REPEAT_CAL to true instead of false to run calibration
// again, otherwise it will only be done once.
// Repeat calibration if you change the screen rotation.
#define REPEAT_CAL false

uint16_t fr_color = TFT_YELLOW; // основной цвет
uint16_t bk_color = TFT_BLACK;  // цвет фона
// uint16_t fr_color = tft.color565(3, 230, 250);
// uint16_t bk_color = tft.color565(65, 0, 87);


// названия кнопок
// если начинается с "/", то рисуется иконка с соответствующем именем
// иначе выводится текстовый "лейбл"
String keyLabel[12] = {"/arrow_up.xbm",   "/home.xbm",      "/fil_in.xbm",  "BS+",
                       "/arrow_down.xbm", "/fan.xbm",       "/fil_out.xbm", "BS-",
                       "/hot.xbm",        "/exstruder.xbm", "/lampa.xbm",   "/pause.xbm" };

uint8_t imageBits[512];              // массив с данными XBM
int16_t imageWidth=0, imageHeight=0; // ширина и высота XBM

bool  holding;                       //признак удержания клавиши
uint8_t fan_speeed = 0;              //скорость вентилятора
boolean case_light = 1;              //флаг подсветки корпуса
boolean table_on = 0;                //флаг включения стола
boolean hotend_on = 0;               //флаг включения хотэнда
uint16_t target_hotend_temp = 220;   //температура хотэнда
uint16_t table_temp1 = 70;           //температура стола 1
uint16_t table_temp2 = 100;          //температура стола 2

// статусы кнопок
// (не нажата, нажата, только-что нажата, удерживается, отпущена)
typedef enum { IDLE, PRESSED, JUSTPRESSED, HOLD, RELEASED } KeyState;
KeyState buttonState[12];
const char* status_str[5] = { "IDLE", "PRESSED", "JUSTPRESSED", "HOLD", "RELEASED" }; //для отладки
#define HOLDTIME 1000                // время после которого считается удержание кнопки, мс
#define CHECK_DELAY 100              // время между опросами (антидребезг), мс
#define KEY_W 70                     // ширина и высота кнопки
#define KEY_H 70
bool ACTION;                         // какое-то действие с кнопками
bool MOTION_UP;                      // движение оси вверх
bool MOTION_DOWN;                    // движение оси вниз

// прототипы функций
void touch_calibrate();
void check_and_set_button_state();
void readXBM(String filename);
void draw_button(uint8_t n, bool invert);
void send_gcode(uint8_t num);

void setup()
{
  Serial.begin(115200);
  tft.begin();
  tft.setRotation(3);	        // ориентация экрана
  tft.setSwapBytes(true);     // Swap the colour byte order when rendering
  
  touch_calibrate();          // загрузка данных калибровки, либо перекалибровка

  tft.fillScreen(bk_color);
  tft.setTextFont(4);
  tft.setTextDatum(CC_DATUM);

  // рисум кнопки
  for (uint8_t n = 0; n < 12; n++) {
      draw_button(n, 0);
  }

}
//==============
// Главный цикл
//==============
void loop()
{
  static uint32_t tmr;
  if (millis() - tmr > CHECK_DELAY) {
  tmr = millis();

  // проверяем статус кнопок
  check_and_set_button_state();

  for (uint8_t b = 0; b < 12; b++) {
    if (buttonState[b] == RELEASED){
      draw_button(b, 0);      // если отпущена - перерисовываем
    } 
    if (buttonState[b] == JUSTPRESSED) {
      draw_button(b, 1);      // если нажата - рисуем с инверсией
    }

    if (ACTION && buttonState[b] != IDLE)  { send_gcode(b); }
  }
  }
}


//=========================================================
// Начальная калибровка тача
// перекалибровка, сохранение и загрузка данных калибровки
//=========================================================
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

//==========================
// Проверка нажатия
// установка статуса кнопок
//==========================
void check_and_set_button_state() {

  static uint32_t hold_tmr;
  uint16_t t_x = 0, t_y = 0;                   // To store the touch coordinates

  ACTION = false;
  boolean pressed = tft.getTouch(&t_x, &t_y);             // Получаем координаты от тача
  uint8_t n = ((int)(t_y / 80) * 4) + (int)(t_x / 80);    // Вычисляем по ним номер кнопки

  for (uint8_t b = 0; b < 12; b++) {
    if (pressed && b == n) {
      switch (buttonState[b]){
            case IDLE:
                buttonState[b] = JUSTPRESSED;
                hold_tmr = millis();
                ACTION = true;
                break;

            case JUSTPRESSED:
                buttonState[b] = PRESSED;
                ACTION = true;
                break;
                
            case RELEASED:
                buttonState[b] = IDLE;
                break;

            case PRESSED:
                if (millis() - hold_tmr > HOLDTIME) {buttonState[b] = HOLD; ACTION = true;}
                break;

            case HOLD:
                break;
        }
    } else {
      switch (buttonState[b]){
            case PRESSED:
                buttonState[b] = RELEASED;
                ACTION = true;
                break;

            case RELEASED:
                buttonState[b] = IDLE;
                break;

            case JUSTPRESSED:
                buttonState[b] = RELEASED;
                ACTION = true;
                break;

            case HOLD:
                buttonState[b] = RELEASED;
                ACTION = true;
                break;

            case IDLE:
                break;
        }
    }
  }
}

//============================
// Чтение XBM файла из SPIFFS
//============================
void readXBM(String filename){
  if (!SPIFFS.exists(filename)) {
    // Serial.println("File not found");
    return;
  }
  File imagefile = SPIFFS.open(filename);
  String xbm;
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
        } 
        if (xbm.indexOf("_height ")>0) {
          xbm.remove(0,xbm.lastIndexOf(" "));
          imageHeight=xbm.toInt();
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
}

//===============================
// Отрисовка кнопки по её номеру
//===============================
void draw_button(uint8_t n, bool invert) {
  const uint16_t w = 2;   // толщина контура кнопок
  const uint16_t r = 16;  // радиус скругления
  uint16_t x = (n % 4) * 80;
  uint16_t y = (int)(n / 4) * 80;

  tft.fillRoundRect(x + ((80 - KEY_W) / 2), y + ((80 - KEY_H) / 2), KEY_W, KEY_H, r, !invert ? fr_color : bk_color);
  tft.fillRoundRect(x + ((80 - KEY_W) / 2) + w, y + ((80 - KEY_H) / 2) + w, KEY_W - w * 2, KEY_H - w * 2, r - w, invert ? fr_color : bk_color);
  if (keyLabel[n].substring(0,1) != "/") {
    tft.setTextColor(!invert ? fr_color : bk_color);
    tft.drawString(keyLabel[n], x + 40, y + 40);
  }
  else {
    readXBM(keyLabel[n]);
    tft.drawXBitmap(x + 40 - imageWidth / 2, y + 40 - imageHeight / 2, imageBits, imageWidth, imageHeight, !invert ? fr_color : bk_color);
  }
}

void send_gcode(uint8_t num) {
  // Serial.print("Key "); Serial.print(keyLabel[num]); Serial.println(status_str[buttonState[num]]);

  switch (num + 1){
    case 1:   //клавиша "1"
              //стол вверх на 1мм, при удержании - постоянно
      switch (buttonState[num]){
        case JUSTPRESSED:
            break;
        case RELEASED:
            if (holding) {
              MOTION_UP = false;
            }
            else {
              { Serial.println("G91"); Serial.println("G01 Z-1 F600"); Serial.println("G90"); }
            }
            holding = false;
            break;
        case HOLD:
            holding = true;
            MOTION_UP = true;
            break;
      }
    break;

    case 2:
      switch (buttonState[num]){
        case PRESSED:
            break;
        case RELEASED:
            break;
        case JUSTPRESSED:
            break;
        case HOLD:
            break;
      }
    break;
    
    case 3:
      switch (buttonState[num]){
        case PRESSED:
            break;
        case RELEASED:
            break;
        case JUSTPRESSED:
            break;
        case HOLD:
            break;
      }
    break;

    case 4:   //клавиша "4"
              //"BABYSTEPPING+" увеличивает толщину слоя на 0.05мм
      switch (buttonState[num]){
        case JUSTPRESSED:
            Serial.println("M290 Z0.05");
            break;
        case RELEASED:
            break;
        case HOLD:
            break;
      }
    break;

    case 5:   //клавиша "5"
              //стол вниз на 1мм, при удержании - постоянно
      switch (buttonState[num]){
        case JUSTPRESSED:
            break;
        case RELEASED:
            if (holding) {
              MOTION_DOWN = false;
            }
            else {
              { Serial.println("G91"); Serial.println("G01 Z1 F600"); Serial.println("G90"); }
            }
            holding = false;
            break;
        case HOLD:
            holding = true;
            MOTION_DOWN = true;
            break;
      }
    break;

    case 6:   //клавиша "12"
              //скорость вентилятора обдува детали 0%-->25%-->50%-->75%-->100%-->0%
              //удержарие - 100%
      switch (buttonState[num]){
        case JUSTPRESSED:
            fan_speeed++;
            if (fan_speeed == 5) fan_speeed = 0;
            switch (fan_speeed) {
                case 0:
                    Serial.println("M107");
                    Serial.println("M117 FAN OFF");
                    break;
                case 1:
                    Serial.println("M106 S64");
                    Serial.println("M117 FAN 25%");
                    break;
                case 2:
                    Serial.println("M106 S128");
                    Serial.println("M117 FAN 50%");
                    break;
                case 3:
                    Serial.println("M106 S192");
                    Serial.println("M117 FAN 75%");
                    break;
                case 4:
                    Serial.println("M106");
                    Serial.println("M117 FAN 100%");
                    break;
            }
            break;
        case RELEASED:
            break;
        case HOLD:
            Serial.println("M106");
            Serial.println("M117 FAN 100%");
            fan_speeed = 0;
            break;
      }
    break;

    case 7:
      switch (buttonState[num]){
        case PRESSED:
            break;
        case RELEASED:
            break;
        case JUSTPRESSED:
            break;
        case HOLD:
            break;
      }
    break;

    case 8:   //клавиша "8"
              //"BABYSTEPPING-" уменьшает толщину слоя на 0.05мм
      switch (buttonState[num]){
        case JUSTPRESSED:
            Serial.println("M290 Z-0.05");
            break;
        case RELEASED:
            break;
        case HOLD:
            break;
      }
    break;

    case 9:
      switch (buttonState[num]){
        case PRESSED:
            break;
        case RELEASED:
            break;
        case JUSTPRESSED:
            break;
        case HOLD:
            break;
      }
    break;

    case 10:
      switch (buttonState[num]){
        case PRESSED:
            break;
        case RELEASED:
            break;
        case JUSTPRESSED:
            break;
        case HOLD:
            break;
      }
    break;

    case 11:    //клавиша "11"
                //подсветка корпуса вкл/выкл (на 100%), при удержании вкл. на 50%
      switch (buttonState[num]){
        case JUSTPRESSED:
            break;
        case RELEASED:
            if (!holding) {
              if (case_light) {
                Serial.println("M355 S0");
                case_light = false;
              }
              else {
                Serial.println("M355 S1 P255");
                case_light = true;
              }
            }
            holding = false;
            break;
        case HOLD:
            Serial.println("M355 S1 P128");
            case_light = true; 
            holding = true;
            break;
      }
    break;

    case 12:
      switch (buttonState[num]){
        case PRESSED:
            break;
        case RELEASED:
            break;
        case JUSTPRESSED:
            break;
        case HOLD:
            break;
      }
    break;

  }
}