$scheduledRestartTime = 5400 # time in seconds

while ($true) {
	$a = $a + 1
	$process = Start-Process "dotnet.exe" -ArgumentList "run -c release" -PassThru 
	$wait = Wait-Process -Id $process.Id -Timeout $scheduledRestartTime
	
	echo "The bot was shutdown..."
	if ($process.ExitCode -eq 0) {
		echo "Possibly a graceful shutdown, exiting.."
		break
	} elseif (Get-Process -Id $process.Id) {
		echo "Process is running, killing!"
		$process.CloseMainWindow()
	} else {
		echo "Process exited with $($process.ExitCode)"
	}
}
