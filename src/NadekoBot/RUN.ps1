while ($true){
	Start-Process "dotnet.exe" "run -c release"
	$a = Get-Process nadekobot
	$a.waitforexit()
}
