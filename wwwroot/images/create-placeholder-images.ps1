# Placeholder resimler olusturma scripti
$imagesPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateImage = Join-Path $imagesPath "zeytinyagi1.jpg"

$missingImages = @(
    "pekmez1.jpg", "pekmez2.jpg", "pekmez3.jpg", "pekmez4.jpg", "pekmez5.jpg", "pekmez6.jpg", "pekmez7.jpg",
    "tozbiber1.jpg", "tozbiber2.jpg", "tozbiber3.jpg", "tozbiber4.jpg", "tozbiber5.jpg", "tozbiber6.jpg", "tozbiber7.jpg",
    "yesilzeytin1.jpg", "yesilzeytin2.jpg", "yesilzeytin3.jpg", "yesilzeytin4.jpg", "yesilzeytin5.jpg", "yesilzeytin6.jpg", "yesilzeytin7.jpg",
    "siyahzeytin1.jpg", "siyahzeytin2.jpg", "siyahzeytin3.jpg", "siyahzeytin4.jpg", "siyahzeytin5.jpg", "siyahzeytin6.jpg", "siyahzeytin7.jpg",
    "incir1.jpg", "incir2.jpg", "incir3.jpg", "incir4.jpg", "incir5.jpg", "incir6.jpg", "incir7.jpg",
    "kurubiber1.jpg", "kurubiber2.jpg", "kurubiber3.jpg", "kurubiber4.jpg", "kurubiber5.jpg", "kurubiber6.jpg", "kurubiber7.jpg"
)

Write-Host "Placeholder resimler olusturuluyor..." -ForegroundColor Cyan

if (Test-Path $templateImage) {
    foreach ($imageName in $missingImages) {
        $targetPath = Join-Path $imagesPath $imageName
        if (-not (Test-Path $targetPath)) {
            Copy-Item $templateImage $targetPath -Force
            Write-Host "Olusturuldu: $imageName" -ForegroundColor Green
        }
    }
    Write-Host "Tum placeholder resimler olusturuldu!" -ForegroundColor Green
} else {
    Write-Host "Hata: Template resim bulunamadi: $templateImage" -ForegroundColor Red
}
