@ECHO OFF
setlocal
SET RhinoCommonsBoo=%Develop%\Ayende\rhino-commons-fork\SharedLibs\Boo

copy "%RhinoCommonsBoo%\*" "%~dp0"
