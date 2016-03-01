# PipEditor
This is an editor for PIP files which are used in my Pipdweno project located [here](https://github.com/MattiusMatt/pipdweno "PipDweno").
##Disclaimer
This has not been designed for anyone else to use... thats my way of saying "it's a bit crap" and has massive issues I can't be bothered fixing, like the fact that if you open a second pip it just falls on its arse!

##Structure
Note: I use Little Endian format for the byte storage and each entry is terminated with a CRLF after the Data, SubScreens and SubMenus are Pipe Separated Values | that are automagically positioned

    Type = 1 byte
      1 = TEXT
      2 = IMAGE
      3 = LINE
      4 = RECT
      5 = FILLRECT
      6 = SUBSCREENS
      7 = SUBMENUS
    StartX = 2 bytes
    StartY = 2 bytes
    EndX = 2 bytes
    Size = 1 byte
    Colour = 2 bytes
    BackColour = 2 bytes
    Data = ASCII
    
    TEXT = [Type][StartX][StartY][Size][Colour][BackColour][Data][CRLF]
    IMAGE = [Type][StartX][StartY][Data][CRLF]
    LINE = [Type][StartX][StartY][EndX][EndY][Colour]
    RECT = [Type][StartX][StartY][EndX][EndY][Colour]
    FILLRECT = [Type][StartX][StartY][EndX][EndY][Colour]
    SUBSCREENS = [Type][StartX][StartY][Data][CRLF]
    SUBMENUS = [Type][StartX][StartY][Data][CRLF]
