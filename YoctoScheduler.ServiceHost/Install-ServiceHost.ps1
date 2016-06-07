<#
.SYNOPSIS
  <Overview of script>
.DESCRIPTION
  <Brief description of script>
.PARAMETER <Parameter_Name>
    <Brief description of parameter input required. Repeat this attribute if required>
.INPUTS
  <Inputs if any, otherwise state None>
.OUTPUTS
  <Outputs if any, otherwise state None - example: Log file stored in C:\Windows\Temp\<name>.log>
.NOTES
  Version:        1.0
  Author:         <Name>
  Creation Date:  <Date>
  Purpose/Change: Initial script development
  
.EXAMPLE
  <Example goes here. Repeat this attribute for more than one example>
#>

Param (
	[Parameter(Mandatory=$True,Position=1)]
	[string] $configFile,
	[Parameter(Mandatory=$False)]
	[string] $binaryFile
)

$sLogFile = ".\ServiceHostInstall_$(Get-Date -Format yyyyMMdd_hhmmss).log"

Function Log($Message) {
	$Message = "$(Get-Date -Format o) $Message"
	Write-Host $Message
	Add-Content $sLogFile $Message
}

if ($binaryFile -eq $null -or $binaryFile -eq "") {
	$binaryFile = $(Get-Item ".\YoctoScheduler.ServiceHost.exe").FullName
}

[Reflection.Assembly]::LoadFile($(Get-Item ".\Newtonsoft.Json.dll”).FullName) | Out-Null
[Reflection.Assembly]::LoadFile($binaryFile) | Out-Null

Log "Installation started"

$configFile = $(Get-Item $configFile).FullName
Log "Using configuration file $configFile"

$config = [YoctoScheduler.ServiceHost.ServiceConfiguration]::FromJson($(get-content $configFile))
Log "Instance name is $($config.InstanceName)"
Log "Binary file is $binaryFile"

$binaryPath = "$binaryFile -c `"$configFile`""
Log "Binary path is $binaryPath"

New-Service -Name $config.ServiceName -BinaryPathName $binaryPath

Log "Installation completed"