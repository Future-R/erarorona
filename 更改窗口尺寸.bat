@echo off
chcp 65001 > nul
set NLM=^


set BR=^^^%NLM%%NLM%^%NLM%%NLM%
:MENU
cls
echo ===================================
echo  窗口尺寸设置
echo ===================================
echo,
echo  请选择一个窗口尺寸:
echo  [1] 小%BR% [2] 标准（推荐） %BR% [3] 大  （2k及以上）
echo,
CHOICE /C 123 /N /M "请按下数字键: "
SET RESULT=%ERRORLEVEL%
IF %RESULT%==3 (
    SET SIZE_CHOICE=Large
    SET SIZE_CHOICE_CN=大
)
IF %RESULT%==2 (
    SET SIZE_CHOICE=Standard
    SET SIZE_CHOICE_CN=标准
)
IF %RESULT%==1 (
    SET SIZE_CHOICE=Small
    SET SIZE_CHOICE_CN=小
)

echo.
echo 您选择了: %SIZE_CHOICE_CN%
echo 正在应用设置...
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0\config_helper.ps1" -Size "%SIZE_CHOICE%"

echo.
echo ===================================
echo 设置完成！按任意键启动游戏！
echo ===================================
echo.
pause>nul
start "" "%~dp0\Emuera开始游戏.exe"
exit