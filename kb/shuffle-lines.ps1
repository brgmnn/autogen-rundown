# Path to the input file
$inputFile = "six-letter-words-full.txt"

# Path to the output file
$outputFile = "shuffled_output.txt"

# Read all lines from the input file
$lines = Get-Content $inputFile

# Shuffle the lines
$shuffled = $lines | Get-Random -Count $lines.Count

# Write the shuffled lines to the output file
$shuffled | Set-Content $outputFile
