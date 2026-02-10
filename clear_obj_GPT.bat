@echo off
echo Cleaning obj...
for /d /r %%d in (obj) do (
  if exist "%%d" rd /s /q "%%d"
)
echo Done. 