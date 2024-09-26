@ECHO OFF


ECHO.          :::     :::        :::::::::  :::::::::  :::            :::      ::::::::    
ECHO.       :+: :+:   :+:        :+:    :+: :+:    :+: :+:          :+: :+:   :+:    :+:    
ECHO.     +:+   +:+  +:+        +:+    +:+ +:+    +:+ +:+         +:+   +:+  +:+            
ECHO.   +#++:++#++: +#+        +#++:++#+  +#++:++#+  +#+        +#++:++#++: +#++:++#++      
ECHO.  +#+     +#+ +#+        +#+        +#+        +#+        +#+     +#+        +#+       
ECHO. #+#     #+# #+#        #+#        #+#        #+#        #+#     #+# #+#    #+#        
ECHO.###     ### ########## ###        ###        ########## ###     ###  ########          



ECHO. *****************************************************************************
ECHO. ***                  		  BEAST          	 	           ***
ECHO. ***                     PROGRAMLAMASI BASLATILIYOR                        ***
ECHO. ***                                                                       ***
ECHO. *****************************************************************************


ECHO.
ECHO.
ECHO.

REM Aşağıdaki satırlar, programlama işlemini başlatmak için J-Link komut satırını çalıştırır.
ECHO. -----------------------------------------------------------------------------
ECHO. PROGRAM YUKLENIYOR...
ECHO. -----------------------------------------------------------------------------


(
"C:\Program Files\SEGGER\JLink\JLink.exe" -device CY8C4146xxx-Sxxx -Speed 2000 -CommanderScript "C:\batch\JlinkCommandFile.jlink"
) > "output.txt"
ECHO. -----------------------------------------------------------------------------
REM Aşağıdaki satır, "Program & Verify" metnini içeren bir çıktı dosyasını arar.
REM Eğer bu metin bulunursa, program başarılıdır.

FINDSTR /C:"Program & Verify" "output.txt" > NUL
IF %ERRORLEVEL% EQU 0 (
    ECHO. @@@@@@@@@@@@@@@@@@@@@@@@@@@@ PROGRAM BASARILI @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ 
    ECHO. -----------------------------------------------------------------------------
    COLOR AE
    GOTO END
)
REM Aşağıdaki satır, "Cannot connect to J-Link" metnini içeren bir çıktı dosyasını arar.
REM Eğer bu metin bulunursa, programlayıcı bağlı değildir.

FINDSTR /C:"Cannot connect to J-Link" "output.txt" > NUL
IF %ERRORLEVEL% EQU 0 (
    ECHO. xxxxxxxxxxxxxxxxxxxxxxxxx PROGRAMLAYICI BULUNAMADI xxxxxxxxxxxxxxxxxxxxxxxxxx
    ECHO. -----------------------------------------------------------------------------
    COLOR CE
    GOTO END
)
REM Aşağıdaki satır, "Cannot connect to J-Link" metnini içeren bir çıktı dosyasını arar.
REM Eğer bu metin bulunursa, işlemcide bağlantı hatası vardır.

FINDSTR /C:"Target connection not established yet but required for command" "output.txt" > NUL
IF %ERRORLEVEL% EQU 0 (
    ECHO. xxxxxxxxxxxxxxxxxxxxxxxxxxxxx  BAGLANTI HATASI xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    ECHO. -----------------------------------------------------------------------------
    COLOR CE
    GOTO END
)


REM Aşağıdaki satır, belirli metinleri içermeyen bir çıktı dosyasının sonucunu görüntüler.
ECHO. TIME OUT.

:END
PAUSE