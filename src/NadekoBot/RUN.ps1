$scheduledRestartTime = 5400 # time in seconds

while ($true) {
	$process = Start-Process "dotnet.exe" -ArgumentList "run -c release" -PassThru -NoNewWindow
	$wait = Wait-Process -Id $process.Id -Timeout $scheduledRestartTime
	
	echo "The bot was shutdown..."
	if ($process.ExitCode -eq 0) {
		echo "Possibly a graceful shutdown, exiting.."
		break
	} elseif (Get-Process -Id $process.Id) {
		echo "Process is running, killing!"
		taskkill /T /F /PID $process.Id # kill process tree
	} else {
		echo "Process exited with $($process.ExitCode)"
	}
}
