$mainPageContent = Get-Content 'MainPage.xaml.cs' -Raw
$mainPageContent = $mainPageContent -replace '(\s+)ShowCaseViewWrapper\.ShowGuideView', '$1// ShowCaseViewWrapper.ShowGuideView // Commented out - ShowCaseView binding not available'
Set-Content 'MainPage.xaml.cs' -Value $mainPageContent -NoNewline

$guidedContent = Get-Content 'Pages\Guided.xaml.cs' -Raw
$guidedContent = $guidedContent -replace '(\s+)ShowCaseViewWrapper\.ShowGuideView', '$1// ShowCaseViewWrapper.ShowGuideView // Commented out - ShowCaseView binding not available'
Set-Content 'Pages\Guided.xaml.cs' -Value $guidedContent -NoNewline

Write-Host "Files updated successfully"
