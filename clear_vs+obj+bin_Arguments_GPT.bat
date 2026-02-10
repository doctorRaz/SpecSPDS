@echo off
setlocal

set MODE=%1

if "%MODE%"=="" (
  echo Usage: clean.bat obj ^| bin ^| all
  goto :ALL
  REM exit /b 1
)

echo === Clean mode: %MODE% ===

if /i "%MODE%"=="obj" goto :OBJ
if /i "%MODE%"=="bin" goto :BIN
if /i "%MODE%"=="vs" goto :VS
if /i "%MODE%"=="all" goto :ALL

echo Unknown mode: %MODE%
exit /b 1

:OBJ
for /d /r %%d in (obj) do (
  if exist "%%d" (
    echo Deleting %%d
    rd /s /q "%%d"
  )
)
goto :END

:BIN
for %%n in (bin) do (
  for /d /r %%d in (%%n) do (
    if exist "%%d" (
      echo Deleting %%d
      rd /s /q "%%d"
    )
  )
)
goto :END

:VS
for %%n in (.vs) do (
  for /d /r %%d in (%%n) do (
    if exist "%%d" (
      echo Deleting %%d
      rd /s /q "%%d"
    )
  )
)
goto :END

:ALL
for %%n in (obj bin .vs) do (
  for /d /r %%d in (%%n) do (
    if exist "%%d" (
      echo Deleting %%d
      rd /s /q "%%d"
    )
  )
)


goto :END

:END
echo === Done ===
endlocal