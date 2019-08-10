Dim Pixels As Variant
Dim IntPixels(768) As Variant
Dim HQ() As Variant
Dim Block() As Variant

Dim tempBlock As Variant 'for row
Dim Block_color As Variant 'for row

Dim delta, maxT, minT As Double
Dim R_colour, G_colour, B_colour
Dim r, g, b As Byte
Dim MIN_TEMPSCALE_DELTA
Dim termoWidth, termoHeight
Dim imageWidth, imageHeight
Dim Multiplier
Dim x_HQ, y_HQ
Dim PaletteSteps

Private Type tRGBcolor
r As Byte
g As Byte
b As Byte
End Type

Dim pPalette() As tRGBcolor

Private Sub Form_Load()
Form1.ScaleMode = 3   ' Set ScaleMode to pixels.
Form1.Width = 320 * Screen.TwipsPerPixelX + 360
Form1.Height = (240 * Screen.TwipsPerPixelY + 35) * 2 + 700
Form1.Show

Main

End Sub

Public Sub Main()

Setup
'c = getColour(180)
'Iterpol
'InterpolateImage
flip
'temp

ds = DrawScale(3, imageHeight + 30, imageWidth, 20, minT, maxT)
rowInterpol
'blockInterpol
'newInterpol

'Test
'DrawHQ
'Draw

End Sub

Public Sub Setup()
termoWidth = 32
termoHeight = 24
Multiplier = 10
imageWidth = (termoWidth - 1) * Multiplier
imageHeight = (termoHeight - 1) * Multiplier
MIN_TEMPSCALE_DELTA = 0
ReDim HQ(imageWidth * imageHeight)
ReDim Block(Multiplier, Multiplier)
ReDim tempBlock(imageWidth, 2)
ReDim Block_color(imageWidth * Multiplier)

Pixels = Array( _
30.48, 28.58, 31.39, 30.27, 30.46, 28.58, 31.43, 31.08, 31.17, 29.64, 29.89, 29.92, 29.37, 29.34, 30.34, 28.88, 30.56, 29.47, 29.51, 29.77, 29.87, 29.62, 29.21, 30.38, 29.68, 28.24, 29.81, 28.34, 29.73, 29.56, 28.99, 28.95, _
30.12, 30.46, 30.56, 29.94, 29.27, 29.14, 29.88, 30.58, 29.35, 29.51, 30.17, 30.97, 30.06, 29.15, 29.65, 29.86, 29.61, 29.9, 29.27, 29.51, 28.7, 29.44, 28.77, 29.66, 28.78, 29.82, 29.25, 29.16, 28.48, 28.02, 28.91, 29.28, _
31.91, 29.39, 31.51, 30.26, 30.63, 28.71, 30.03, 29.88, 29.99, 28.99, 30.64, 29.37, 29.69, 28.96, 29.35, 30.94, 29.69, 28.41, 29.42, 28.94, 30.68, 29.06, 29.1, 29.87, 28.53, 28.79, 28.88, 28.68, 28.13, 28.38, 29.25, 29.02, _
29.45, 29.75, 29.85, 29.52, 29.51, 29.96, 29.16, 29.42, 29.19, 29.28, 29.14, 29.87, 29.09, 29.26, 28.52, 29.31, 29.34, 30.32, 29.69, 29.66, 29#, 29.35, 29.75, 29.22, 28.84, 29.04, 28.78, 27.98, 28.36, 27.9, 29.08, 30.76, _
29.73, 29.53, 28.87, 29.5, 29.46, 29.66, 30.88, 29.16, 29.22, 29.82, 30.05, 29.54, 29.64, 29.66, 29.85, 30.34, 30.05, 29.15, 28.96, 29.88, 30.06, 29.27, 29.68, 29.94, 28.99, 28.79, 29.71, 28.88, 29.29, 29.08, 30.08, 29.17, _
30.56, 29.89, 30.46, 30.25, 28.93, 30.34, 30.61, 29.73, 29.19, 29.38, 28.92, 29.13, 28.76, 28.93, 29.37, 29.41, 28.13, 30.38, 30.53, 31.39, 29.45, 30.02, 30.04, 30#, 29.81, 30.15, 29.74, 28.79, 30.6, 30.6, 30.19, 30.4, _
30.58, 29.34, 30.09, 31.53, 31.83, 30.37, 35.4, 34.85, 31.44, 30.36, 30.36, 29.65, 29.62, 28.65, 29.91, 30.29, 30.24, 30.32, 34.26, 34.97, 33.31, 30.9, 29.6, 29.8, 30.18, 29.86, 29.54, 30.18, 31.39, 29.48, 29.7, 31.27, _
29.98, 30.07, 30.2, 30.28, 31.38, 34.08, 35.72, 36.4, 33.41, 31.26, 30.07, 29.76, 29.35, 29.86, 30.11, 30.13, 29.01, 30.79, 34.28, 35.12, 35.15, 33.22, 30.66, 31.04, 30.44, 29.2, 29.25, 29.71, 30.53, 29.74, 30.68, 30.32, _
29.78, 29.88, 31.21, 29.86, 31.74, 31.76, 35.33, 36.35, 35.89, 34.27, 30.28, 30.07, 30.17, 28.91, 30.33, 30.17, 30.83, 30.84, 35.64, 36.44, 36.15, 34.61, 30.82, 29.33, 31.24, 29.27, 29.77, 29.9, 29#, 28.58, 28.17, 30.32, _
29.31, 30.61, 29.46, 30.57, 30.36, 32.18, 35.18, 35.38, 36.21, 34.8, 31#, 30.65, 29.09, 29.29, 30.01, 29.27, 29.35, 31.51, 35.18, 35.98, 35.88, 34.33, 30.84, 30.57, 29.78, 30.15, 29.45, 29.9, 28.69, 30.11, 29.31, 29.18, _
30.48, 29.98, 30.05, 30.32, 30.5, 30.4, 34.8, 35.92, 36.17, 35.2, 32.35, 31.9, 29.76, 29.13, 29.57, 29.11, 30.06, 30.43, 35.47, 35.79, 36.08, 35.12, 30.85, 29.53, 29.63, 28.48, 29.07, 29.99, 29.43, 28.59, 30.29, 30.41, _
28.95, 30.48, 28.96, 30.96, 29.01, 30.09, 33.37, 35.49, 35.32, 36.03, 34.43, 32.58, 30.13, 29.22, 30.09, 29.63, 29.35, 30.58, 35.35, 35.8, 35.67, 36.03, 31.03, 29.65, 29.12, 29.17, 28.6, 29#, 29.61, 30.28, 29.12, 30.34, _
30.78, 30.17, 30.53, 30.61, 29.21, 30.16, 32.21, 33.67, 36.49, 35.82, 35.95, 34.72, 31.31, 29.6, 29.75, 29.96, 30.24, 30.91, 35.95, 36.13, 36.37, 35.9, 32.06, 30.75, 30.04, 29.12, 30.18, 30.93, 30.05, 28.66, 30.22, 29.66, _
29.07, 29.62, 30.75, 30.3, 30.38, 29.47, 31.56, 33.4, 35.02, 35.75, 35.52, 34.61, 30.96, 30.45, 29.32, 30.48, 30.18, 31.39, 35.33, 35.53, 35.55, 36.79, 31.13, 29.78, 28.55, 30.22, 30.78, 30.25, 28.99, 29.01, 28.94, 28.9, _
30.73, 30.03, 30.94, 30.41, 30.07, 29.42, 31.49, 32.21, 36.06, 35.68, 37.3, 35.38, 32.73, 30.66, 30.02, 30.39, 30.36, 32.31, 36.62, 35.99, 36.61, 36.03, 31.08, 31.11, 30#, 29.67, 29.79, 30.55, 29.71, 29.67, 29.23, 30.6, _
29.6, 30.27, 29.26, 29.4, 29.48, 29.68, 30.22, 31.78, 35.99, 36.02, 36.78, 36.03, 32.93, 31.05, 30.41, 30.01, 30.17, 31.41, 36.62, 35.88, 35.9, 35.88, 31.66, 30.17, 29.59, 30.37, 30.21, 29.72, 28.42, 29.39, 30.92, 32.37, _
30.53, 29.39, 31.48, 29.62, 31.94, 30.03, 30.29, 31.23, 34.46, 36.17, 36.94, 36.2, 34.75, 34.11, 30.98, 30.85, 31.77, 32.61, 36.61, 35.89, 36#, 35.59, 31.73, 31.22, 30.85, 29.68, 30.72, 31.25, 32.28, 32.87, 34.38, 35.3, _
31.35, 30.84, 30.93, 30.15, 30.08, 30.45, 30.91, 31.07, 32.52, 35.76, 36.65, 36.71, 35.56, 34.17, 31.03, 30.91, 31.83, 33.87, 36.63, 36.42, 35.46, 36.52, 32.05, 31.81, 29.7, 30.57, 31.65, 32.75, 33.24, 34.02, 34.82, 35.49, _
30.53, 30.99, 31.67, 31.3, 30.87, 30.89, 30.83, 30.8, 32.87, 34.04, 35.78, 37.19, 36.84, 35.1, 32.43, 31.75, 32.87, 34.51, 36.18, 37.66, 36.19, 35.89, 33.05, 33.25, 33.52, 32.9, 34.44, 34.49, 34.5, 34.92, 36.09, 36.9, _
31.53, 31.05, 31.59, 30.29, 30.62, 30.77, 30.68, 31.72, 31.99, 33.44, 35.98, 36.47, 35.47, 35.59, 32.98, 30.84, 33.13, 34.83, 36.61, 35.84, 36.29, 36.43, 34.05, 33.93, 33.64, 33.96, 33.64, 33.65, 34.06, 35.73, 36.34, 35.03, _
31.49, 30.05, 30.98, 30.84, 32.15, 29.54, 30.73, 31.24, 32.22, 32.4, 36.37, 36.31, 36.25, 35.91, 35.79, 34.84, 35.46, 35.29, 36.58, 36.93, 36.17, 35.69, 35.56, 34.42, 34.91, 33.61, 35.05, 35.66, 34.96, 35.2, 35.74, 35.86, _
30.59, 31.61, 31.95, 31#, 30.99, 30.72, 31.14, 30.52, 31.27, 33.3, 36.06, 35.6, 35.58, 37.08, 36.06, 36.47, 35.73, 36.66, 36.3, 36.19, 36.65, 36.54, 36.09, 35.63, 34.54, 35.06, 34.59, 34.96, 34.24, 36.24, 36.66, 37.05, _
31.05, 29.24, 31.15, 31.73, 30.47, 30.51, 31.69, 31.19, 35.16, 35.13, 36.14, 35.7, 36.86, 35.85, 35.23, 36.43, 35.68, 36.22, 36.22, 37.09, 36.88, 36.1, 36.2, 35.16, 35.95, 35.3, 35.52, 35.02, 35.46, 34.68, 36.48, 38.09, _
30.6, 30.8, 30.72, 32.2, 29.85, 30.97, 32.41, 32.22, 34.84, 36.55, 36.55, 36.04, 35.48, 36.46, 36.29, 37.04, 36.12, 35.95, 36.45, 36.16, 35.37, 35.86, 35.77, 35.97, 36.02, 36.42, 35.97, 35.7, 33.96, 36.19, 35.79, 35.49, 0)

minT = 300
maxT = -40
 For i = 0 To 768 - 1
    If maxT < Pixels(i) Then maxT = Pixels(i)
    If minT > Pixels(i) Then minT = Pixels(i)
Next
delta = maxT - minT

If delta < MIN_TEMPSCALE_DELTA Then
    minT = minT - (MIN_TEMPSCALE_DELTA - delta) / 2
    maxT = maxT + (MIN_TEMPSCALE_DELTA - delta) / 2
End If

End Sub

Public Sub Draw()
imageWidth = termoWidth * Multiplier
imageHeight = termoHeight * Multiplier
DrawWidth = 1
k = 0
For y = imageHeight + 8 To imageHeight + imageHeight + 6 Step Multiplier
    For x = 1 To imageWidth - 1 Step Multiplier
        col = 180# * (Pixels(k) - minT) / (maxT - minT)
        getColour (col)
        Form1.Line (x, y)-(x + Multiplier - 1, y + Multiplier - 1), RGB(R_colour, G_colour, B_colour), BF
        k = k + 1
    Next
Next
End Sub

Public Sub DrawHQ()
PaletteSteps = (maxT - minT) * 10
getIronPalette (PaletteSteps)
For i = 0 To imageHeight * imageWidth - 1
    plotPalette (i)
'    plot (i)
Next
End Sub

Public Sub DrawHQ2()
DrawWidth = 1
k = 0
For y = 1 To imageHeight
    For x = 1 To imageWidth
        col = Map(HQ(k), minT, maxT, 240, 0) - 40
        A = HSVtoRGB(r, g, b, col, 100, 100)
        Form1.PSet (x, y), RGB(r, g, b)
        k = k + 1
    Next
Next
End Sub



Public Function getColour(j As Double)
 
If j >= 0 And j < 30 Then
    R_colour = 0
    G_colour = 0
    B_colour = 20 + (120# / 30#) * j
End If

If j >= 30 And j < 60 Then
    R_colour = (120# / 30) * (j - 30#)
    G_colour = 0
    B_colour = 140 - (60# / 30#) * (j - 30#)
End If

If j >= 60 And j < 90 Then
    R_colour = 120 + (135# / 30#) * (j - 60#)
    G_colour = 0
    B_colour = 80 - (70# / 30#) * (j - 60#)
End If

If j >= 90 And j < 120 Then
    R_colour = 255
    G_colour = 0 + (60# / 30#) * (j - 90#)
    B_colour = 10 - (10# / 30#) * (j - 90#)
End If

If j >= 120 And j < 150 Then
    R_colour = 255
    G_colour = 60 + (175# / 30#) * (j - 120#)
    B_colour = 0
End If

If j >= 150 And j <= 180 Then
    R_colour = 255
    G_colour = 235 + (20# / 30#) * (j - 150#)
    B_colour = 0 + 255# / 30# * (j - 150#)
End If

End Function



Function HQ_offset(x, y)
HQ_offset = (y * imageWidth + x) * Multiplier
End Function

Public Function HSVtoRGB(ByRef r, ByRef g, ByRef b, H, S, V)
'/* Вход:
'* 0 <= hue < 360 градусов - оттенок. Основные цвета:
'* 0 - красный, 60 - желтый,
'* 120 - зеленый, 180 - голубой
'* 240 - синий, 300 - пурпурный
'* hue == 360 - неопределён !!!
'* Остальные цвета между ними
'* 0 <= sat <= 100 - Saturation - насыщенность
'* 0 <= val <= 100 - Value - светлота
'*
'* Выход:
'* 0 <= r,g,b <= 255 - значения красного, зеленого, синего
'*/
  
Dim Hi
Dim Vmin, Vinc, Vdec
Dim A

If S = 0 Then
   r = V
   g = V
   b = V
Else
'   /* Хроматический цвет */
    While (H >= 360)
      H = H - 360
    Wend
    While (H <= 0)
      H = H + 360
    Wend
    S = constrain(S, 0, 100)
    V = constrain(V, 0, 100)
    
    Hi = Int(H / 60) Mod 6
    Vmin = ((100 - S) * V) / 100
    A = (V - Vmin) * ((H Mod 60) / 60)
    Vinc = Vmin + A
    Vdec = V - A
    
Select Case (Hi)
    Case 0
    r = V: g = Vinc: b = Vmin
    Case 1
    r = Vdec: g = V: b = Vmin
    Case 2
    r = Vmin: g = V: b = Vinc
    Case 3
    r = Vmin: g = Vdec: b = V
    Case 4
    r = Vinc: g = Vmin: b = V
    Case 5
    r = V: g = Vmin: b = Vdec
End Select
End If

r = r * (255 / 100)
g = g * (255 / 100)
b = b * (255 / 100)

End Function

Function constrain(fun, min, max)
constrain = fun
If fun < min Then constrain = min
If fun > max Then constrain = max
End Function

Function Map(x, in_min, in_max, out_min, out_max)
Map = (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min
End Function

Public Sub Test()
DrawWidth = 1
k = 0
offset = 3
Form1.Line (offset, offset)-(imageWidth + 1 + offset, imageHeight + 1 + offset), vbRed, B
For y = 1 + offset To imageHeight + offset
    For x = 1 + offset To imageWidth + offset
        If HQ(k) = Empty Then col = vbBlack
        If HQ(k) <> Empty Then col = vbYellow
        Form1.PSet (x, y), col
        k = k + 1
    Next
Next
End Sub

Sub plot(indx) 'рисует точку по смещению в массиве
DrawWidth = 1
offset = 3
sz = UBound(HQ)
y = indx \ imageWidth 'целочисленное деление
x = indx Mod imageWidth 'остаток
        col = 180 * (HQ(indx) - minT) / (maxT - minT)
        getColour (col)
        Form1.PSet (x + 1 + offset, y + 1 + offset), RGB(R_colour, G_colour, B_colour)
End Sub

Sub newInterpol()
DrawWidth = 1
offset = 3
'Form1.Line (offset, offset)-(imageWidth + 1 + offset, imageHeight + 1 + offset), vbBlack, B

i_pix = 0
'Интерполяция строк
For y = 0 To termoHeight - 1
    For x = 0 To termoWidth - 2
    
        i_pix = (y * termoWidth) + x 'Termo_offset
        'координаты начала блока по индексу теплограммы
        y_HQ = (indx \ termoWidth) * Multiplier      'целочисленное деление
        x_HQ = (indx Mod termoWidth) * Multiplier    'остаток
        
        tempStart = Pixels(i_pix)   'первая точка
        tempEnd = Pixels(i_pix + 1) 'вторая точка (между ними интерполируем)
        
        For step = 0 To Multiplier - 1
            idx = HQ_offset(x, y) + step
            HQ(idx) = tempStart + ((tempEnd - tempStart) / (Multiplier - 1)) * step
'            plot (idx)
        Next
    Next
Next

'Заполняем последний столбец
idx = imageWidth - 1
For y = 1 To termoHeight
    HQ(idx) = Pixels(y * termoWidth - 1)
'    plot (idx)
    idx = idx + imageWidth * Multiplier
Next

'Растягиваем точки по вертикали
For y = 0 To termoHeight - 2
    For x = 0 To imageWidth - 1
        tempStart = HQ(y * Multiplier * imageWidth + x)
        tempEnd = HQ((y * Multiplier * imageWidth + x) + Multiplier * imageWidth)
        For step = 1 To Multiplier - 1
            idx = (y * Multiplier * imageWidth + x) + step * imageWidth
            HQ(idx) = tempStart + ((tempEnd - tempStart) / (Multiplier - 1)) * step
'            plot (idx)
        Next
    Next
Next

DrawHQ

End Sub

'==============================================================================
' Процедура отрисовки цветовой шкалы
'==============================================================================
Function DrawScale(x, y, Width, Height, minTemp, maxTemp)

getIronPalette (Width)

For i = 0 To Width

Form1.Line (x + i, y)-(x + i, y + Height), RGB(pPalette(i).r, pPalette(i).g, pPalette(i).b), BF

Next

'    ' Вывод максимального значения в шкале (по горизонтали - по центру)
'    TextWidth = dispcolor_getFormatStrWidth(FONTID_6X8M, "%.0f°C", maxTemp)
'    dispcolor_printf(X + (Width - TextWidth) / 2, Y + 2, FONTID_6X8M, BLACK, "%.0f°C", maxTemp)
'    ' Вывод минимального значения в шкале (по горизонтали - по центру)
'    TextWidth = dispcolor_getFormatStrWidth(FONTID_6X8M, "%.0f°C", minTemp)
'    dispcolor_printf(X + (Width - TextWidth) / 2, Y + Height - 10, FONTID_6X8M, WHITE, "%.0f°C", minTemp)

End Function

'==============================================================================
' Функция возвращает указатель на массив точек палитры (steps - кол-во шагов палитры)
'==============================================================================
Sub getIronPalette(steps)

If (steps Mod 4) <> 0 Then steps = (steps \ 4) * 4 + 4

partSize = steps / 4

Dim KeyColors(5) As tRGBcolor

'KeyColors(0).r = &H0: KeyColors(0).g = &H0: KeyColors(0).b = &H0
'KeyColors(1).r = &H20: KeyColors(1).g = &H0: KeyColors(1).b = &H8C
'KeyColors(2).r = &HCC: KeyColors(2).g = &H0: KeyColors(2).b = &H77
'KeyColors(3).r = &HFF: KeyColors(3).g = &HD7: KeyColors(3).b = &H0
'KeyColors(4).r = &HFF: KeyColors(4).g = &HFF: KeyColors(4).b = &HFF

'0x00, 0x00, 0x00, ' Чёрный
'0x20, 0x00, 0x8C, ' Тёмно-синий
'0xCC, 0x00, 0x77, ' Фиолетовый
'0xFF, 0xD7, 0x00, ' Золотой
'0xFF, 0xFF, 0xFF  ' Белый

KeyColors(0).r = 0: KeyColors(0).g = 0: KeyColors(0).b = 20
KeyColors(1).r = 55: KeyColors(1).g = 0: KeyColors(1).b = 120
KeyColors(2).r = 255: KeyColors(2).g = 0: KeyColors(2).b = 10
KeyColors(3).r = 255: KeyColors(3).g = 220: KeyColors(3).b = 35
KeyColors(4).r = 255: KeyColors(4).g = 255: KeyColors(4).b = 255

ReDim pPalette(steps)

For part = 0 To 3

    For step = 0 To partSize - 1

        n = step / (partSize - 1)

        pPalette(k).r = KeyColors(part).r * (1# - n) + KeyColors(part + 1).r * n
        pPalette(k).g = KeyColors(part).g * (1# - n) + KeyColors(part + 1).g * n
        pPalette(k).b = KeyColors(part).b * (1# - n) + KeyColors(part + 1).b * n

        k = k + 1
    Next
Next

End Sub

Sub plotPalette(indx) 'рисует точку по смещению в массиве
DrawWidth = 1
offset = 3

y = indx \ imageWidth 'целочисленное деление
x = indx Mod imageWidth 'остаток

colorIdx = HQ(indx) * 10 - (minT * 10)

If colorIdx < 0 Then colorIdx = 0
If colorIdx >= PaletteSteps Then colorIdx = PaletteSteps - 1

Form1.PSet (x + 1 + offset, y + 1 + offset), RGB(pPalette(colorIdx).r, pPalette(colorIdx).g, pPalette(colorIdx).b)

End Sub


'Sub blockInterpol()
'
'For y = 0 To termoHeight - 2
'    For x = 0 To termoWidth - 2
'        point_1 = (y * termoWidth) + x      'LeftUp
'        point_2 = point_1 + 1               'RU
'        point_3 = point_1 + termoWidth      'LD
'        point_4 = point_2 + termoWidth      'RD
'
'        point_1_HQ = y * Multiplier * imageWidth + x * Multiplier     'LeftUp
'        point_2_HQ = point_1_HQ + Multiplier               'RU
'        point_3_HQ = point_1_HQ + Multiplier * imageWidth      'LD
'        point_4_HQ = point_2_HQ + Multiplier * imageWidth      'RD
'
'        For i = point_1_HQ To point_2_HQ
'            For j = 0 To Multiplier
'                HQ(i + j * imageWidth) = 24
'                k = k + 1
'            Next
'
'        Next
'
'    Next
'Next

''Заполняем последний столбец
'idx = imageWidth - 1
'For Y = 1 To termoHeight
'    HQ(idx) = Pixels(Y * termoWidth - 1)
''    plot (idx)
'    idx = idx + imageWidth * Multiplier
'Next

''Растягиваем точки по вертикали
'For Y = 0 To termoHeight - 2
'    For X = 0 To imageWidth - 1
'        tempStart = HQ(Y * Multiplier * imageWidth + X)
'        tempEnd = HQ((Y * Multiplier * imageWidth + X) + Multiplier * imageWidth)
'        For step = 1 To Multiplier - 1
'            idx = (Y * Multiplier * imageWidth + X) + step * imageWidth
'            HQ(idx) = tempStart + ((tempEnd - tempStart) / (Multiplier - 1)) * step
''            plot (idx)
'        Next
'    Next
'Next

'DrawHQ
'Test
'
'End Sub

'        i_pix = (Y * termoWidth) + X 'Termo_offset
'        'координаты начала блока по индексу теплограммы
'        y_HQ = (indx \ termoWidth) * Multiplier      'целочисленное деление
'        x_HQ = (indx Mod termoWidth) * Multiplier    'остаток
'
'        tempStart = Pixels(i_pix)   'первая точка
'        tempEnd = Pixels(i_pix + 1) 'вторая точка (между ними интерполируем)
'
'        For step = 0 To Multiplier - 1
'            idx = HQ_offset(X, Y) + step
'            HQ(idx) = tempStart + ((tempEnd - tempStart) / (Multiplier - 1)) * step
''            plot (idx)
'        Next

Function index_by_coord_Termo(x, y)
index_by_coord_Termo = y * termoWidth + x
End Function

Function coord_by_index_Termo(indx, ByRef x, ByRef y)
y = indx \ termoWidth 'целочисленное деление
x = indx Mod termoWidth 'остаток
End Function
Function index_by_coord_HQ(x, y)

End Function

Function coord_by_index_HQ(indx, ByRef x, ByRef y)
y = indx \ imageWidth 'целочисленное деление
x = indx Mod imageWidth 'остаток
End Function

Function termo_index_to_HQ_coord()

End Function

Sub temp()
i = index_by_coord_Termo(31, 23)
r = coord_by_index_HQ(311, c, d)
r2 = coord_by_index_Termo(767, e, f)
End Sub

Sub blockInterpol()
'Dim sw As Boolean
DrawWidth = 1
offset = 3
Form1.Line (offset, offset)-(imageWidth + 1 + offset, imageHeight + 1 + offset), vbBlack, B

'Вычисляем палитру для текущего кадра
PaletteSteps = (maxT - minT) * 10
getIronPalette (PaletteSteps)


For y = 0 To termoHeight - 2
    For x = 0 To (termoWidth - 2)
        'Интерполяция верхней строки блока
        i_pix = (y * termoWidth) + x    'Termo_offset
        tempStart = Pixels(i_pix)       'первая точка
        tempEnd = Pixels(i_pix + 1)     'вторая точка
        k = ((tempEnd - tempStart) / (Multiplier - 1))
            For step = 0 To Multiplier - 1
                Block(step, 0) = tempStart + k * step: n = n + 1
            Next
        'Интерполяция нижней строки блока
        i_pix = ((y + 1) * termoWidth) + x  'Termo_offset
        tempStart = Pixels(i_pix)       'первая точка
        tempEnd = Pixels(i_pix + 1)     'вторая точка
        k = ((tempEnd - tempStart) / (Multiplier - 1))
            For step = 0 To Multiplier - 1
                Block(step, Multiplier - 1) = tempStart + k * step: n = n + 1
            Next
        
        'Интерполяция вертикальных линий
        For vert = 0 To Multiplier - 1
        
        tempStart = Block(vert, 0)       'первая точка
        tempEnd = Block(vert, Multiplier - 1)     'вторая точка
        k = ((tempEnd - tempStart) / (Multiplier - 1))
            For step = 1 To Multiplier - 2
                Block(vert, step) = tempStart + k * step: n = n + 1
            Next
        Next
        
        'Отрисовка блока
        r = drawBlock(x, y)
    Next
Next

End Sub

Function drawBlock(ByVal x, ByVal y)
'Отрисовка блока по координатам Termo
DrawWidth = 1
offset = 3

sdvig_x = x * Multiplier
sdvig_y = y * Multiplier

For j = 0 To Multiplier - 1
    yy = j + sdvig_y
    
    For i = 0 To Multiplier - 1
        xx = i + sdvig_x
        
        colorIdx = Block(i, j) * 10 - (minT * 10)
        If colorIdx < 0 Then colorIdx = 0
        If colorIdx >= PaletteSteps Then colorIdx = PaletteSteps - 1
        
        Form1.PSet (xx + 1 + offset, yy + 1 + offset), RGB(pPalette(colorIdx).r, pPalette(colorIdx).g, pPalette(colorIdx).b)
        
    Next
Next
End Function

'=========================================
' Процедура интерполяции термограммы
'=========================================
Sub rowInterpol()
DrawWidth = 1
offset = 3
Form1.Line (offset, offset)-(imageWidth + 1 + offset, imageHeight + 1 + offset), vbBlack, B


  'Вычисляем палитру для текущего кадра
  PaletteSteps = (maxT - minT) * 10
  getIronPalette (PaletteSteps)

  For y = 0 To (termoHeight - 2)
  
    For x = 0 To (termoWidth - 2)
    
      'Интерполяция верхней строки блока
      w = imageWidth
      i_pix = (y * termoWidth) + x               'Termo_offset
      tempStart = IntPixels(i_pix)                  'первая точка
      tempEnd = IntPixels(i_pix + 1)                'вторая точка
      k = ((tempEnd - tempStart) / (Multiplier - 1))
      For step = 0 To Multiplier - 1
        tempBlock(x * Multiplier + step, 0) = tempStart + k * step
        colorIdx = tempBlock(x * Multiplier + step, 0) * 10 - (minT * 10)
        Block_color(x * Multiplier + step) = colorIdx: n = n + 1
      Next
'      'Интерполяция нижней строки блока
      i_pix = ((y + 1) * termoWidth) + x
      tempStart = IntPixels(i_pix)
      tempEnd = IntPixels(i_pix + 1)
      k = ((tempEnd - tempStart) / (Multiplier - 1))
      For step = 0 To Multiplier - 1

        tempBlock(x * Multiplier + step, 1) = tempStart + k * step
        colorIdx = tempBlock(x * Multiplier + step, 1) * 10 - (minT * 10)
        Block_color((x * Multiplier + step) + (imageWidth * (Multiplier - 1))) = colorIdx: n = n + 1
      Next

      

    Next
    
    'Интерполяция вертикальных линий
       For vert = 0 To imageWidth - 1
      

         tempStart = tempBlock(vert, 0)           'первая точка
         tempEnd = tempBlock(vert, 1)  'вторая точка
         k = ((tempEnd - tempStart) / (Multiplier - 1))
         For step = 1 To Multiplier - 2
      
           colorIdx = (tempStart + k * step) * 10 - (minT * 10)
           Block_color((imageWidth * step) + vert) = colorIdx: n = n + 1
         Next
      Next
      
    'Отрисовка блока
    r = drawRow(y)
  Next
End Sub

Function drawRow(y)
'Отрисовка блока по координатам Termo
DrawWidth = 1
offset = 3

'Block_color (imageWidth * Multiplier)

For j = 0 To Multiplier - 1
    For i = 0 To imageWidth - 1
        colorIdx = Block_color(i + imageWidth * j)
'        Form1.PSet (i + 1 + offset, j + 1 + offset + (y * Multiplier)), vbYellow
        Form1.PSet (i + 1 + offset, j + 1 + offset + (y * Multiplier)), RGB(pPalette(colorIdx).r, pPalette(colorIdx).g, pPalette(colorIdx).b)

    Next
Next
End Function

Sub flip()

For col = 0 To termoHeight - 1
    For row = 0 To termoWidth - 1
      
'        n = row + col * termoHeight
'        k = (termoWidth - row - 1) + col * termoHeight

            i = (termoWidth - row - 1) + col * termoWidth

            IntPixels(i) = Pixels(row + col * termoWidth)

    Next
Next
    
End Sub




