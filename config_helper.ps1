# --------------------------------------------------------
# config_helper.ps1
# 后端脚本，由批处理脚本调用
# --------------------------------------------------------

# 1. 接收从 .bat 文件传来的参数 (Small, Standard, 或 Large)
param (
    [string]$Size
)

# 2. 定义尺寸配置映射
$ConfigMap = @{
    "Small"    = @{ Font = 16; Width = 1027; Height = 671 }
    "Standard" = @{ Font = 22; Width = 1415; Height = 922 }
    "Large"    = @{ Font = 26; Width = 1666; Height = 1089 }
}

# 3. 检查传入的参数是否有效
if (-not $ConfigMap.ContainsKey($Size)) {
    Write-Error "错误：接收到无效的尺寸参数 '$Size'。"
    Read-Host "按 Enter 键退出"
    exit 1
}

# 4. 获取选定的配置
$Settings = $ConfigMap[$Size]
Write-Host "正在应用 '$Size' 尺寸配置..."
Write-Host "  - 字体/行高: $($Settings.Font)pt"
Write-Host "  - 窗口宽度: $($Settings.Width)px"
Write-Host "  - 窗口高度: $($Settings.Height)px"

# 5. 定义文件路径
$ConfigFile = ".\emuera.config"
$DefaultConfigFile = ".\CSV\_default.config"

# 6. 检查配置文件是否存在，如果不存在则复制
if (-not (Test-Path $ConfigFile)) {
    Write-Warning "'$ConfigFile' 未找到。"
    if (-not (Test-Path $DefaultConfigFile)) {
        Write-Error "'$DefaultConfigFile' 也未找到! 无法创建配置文件。"
        Read-Host "按 Enter 键退出"
        exit 1
    }
    
    Write-Host "正在从 '$DefaultConfigFile' 复制默认配置..."
    Copy-Item -Path $DefaultConfigFile -Destination $ConfigFile
    Write-Host "已创建 '$ConfigFile'."
} else {
    Write-Host "已找到 '$ConfigFile'，准备更新..."
}

# 7. 修改配置文件
Write-Host "正在以 UTF-8 with BOM 编码读写配置文件..."

try {
    # 读取文件内容
    $Content = Get-Content -Path $ConfigFile -Raw -Encoding "utf8"

    # 1. 删除旧的配置行
    $Content = $Content -replace "(?m)^(\s*ウィンドウ幅:).*", ""
    $Content = $Content -replace "(?m)^(\s*ウィンドウ高さ:).*", ""
    $Content = $Content -replace "(?m)^(\s*フォントサイズ:).*", ""
    $Content = $Content -replace "(?m)^(\s*一行の高さ:).*", ""

    # 2. 添加新的配置行
    $Content += "`r`nウィンドウ幅:$($Settings.Width)"
    $Content += "`r`nウィンドウ高さ:$($Settings.Height)"
    $Content += "`r`nフォントサイズ:$($Settings.Font)"
    $Content += "`r`n一行の高さ:$($Settings.Font)"

    # 3. 将修改后的内容写回文件
    Set-Content -Path $ConfigFile -Value $Content -Encoding "utf8"
    
    Write-Host "配置更新成功！"

} catch {
    Write-Error "处理文件时发生错误: $($_.Exception.Message)"
    Read-Host "按 Enter 键退出"
    exit 1
}

