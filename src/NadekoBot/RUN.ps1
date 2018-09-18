while ($true) {
	$a = $a + 1
	$process = Start-Process "dotnet.exe" -ArgumentList "run -c release" -PassThru 
	$wait = Wait-Process -Id $process.Id 
	echo "The bot crashed..."
	if ($process.ExitCode -eq 0) {
		echo "Possibly a graceful shutdown, exiting.."
		break
	} else {
		echo "Process exited with $($process.ExitCode)"
	}
}
