@echo off
echo Cleaning .vs...
for /d /r %%d in (.vs) do (
  if exist "%%d" rd /s /q "%%d"
)
echo Done.