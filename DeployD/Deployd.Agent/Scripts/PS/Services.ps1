function StartService([string]$serviceName)
{
	$service = Get-Service -display $ServiceName -ErrorAction SilentlyContinue 
	if ( $service ) 
	{
		if ($service.Status -eq "Stopped")
		{
			Start-Service $service.Name
			do {
				Start-Sleep -m 100
			} while (!$service.Status -eq "Running")
			Write-Output $service.Name started
		}
	}
}

function StopIfInstalledAndStarted([string]$serviceName,[string]$pathToExecutable)
{
	$service = Get-Service -display $serviceName -ErrorAction SilentlyContinue
	if ($service)
	{
		if ( $service.Status -eq "Running" )
		{
			Write-Output Stopping $service.Name
			Stop-Service $serviceName
			do {
			Start-Sleep -m 100
			}
			while (!$service.Status -eq "Stopped")
			$serviceName + " stopped"
		}
	}
}

function InstallIfNotAlready([string]$serviceName,[string]$pathToExecutable)
{
	$service = Get-Service -display $serviceName -ErrorAction SilentlyContinue
	if (!$service)
	{
		$installArgs = $pathToExecutable
		[System.Configuration.Install.ManagedInstallerClass]::InstallHelper($installArgs)
	}
}