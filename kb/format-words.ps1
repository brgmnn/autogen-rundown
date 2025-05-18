# Read the list of words
$words = Get-Content -Path "2000_Cleaned_Seven-Letter_Words.txt" | Sort-Object -Unique

# Prepare the output file
$outputPath = "formatted_words.txt"
if (Test-Path $outputPath) { Remove-Item $outputPath }

# Process and format the words
$quotedWords = $words | ForEach-Object { "`"$_`"" }

for ($i = 0; $i -lt $quotedWords.Count; $i += 7) {
    $lineWords = $quotedWords[$i..([math]::Min($i + 6, $quotedWords.Count - 1))]
    $line = $lineWords -join ", "
    Add-Content -Path $outputPath -Value $line
}