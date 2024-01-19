Get-Content dotnet-result.txt | ForEach-Object {
    (($_.TrimStart() -replace ',','') -replace "`e\[[0-9]+m",'') -replace '\s+',' '
}