@echo off
echo Cleaning obj and bin...
for %%n in (obj bin) do (
  for /d /r %%d in (%%n) do (
    if exist "%%d" rd /s /q "%%d"
  )
)
echo Done.