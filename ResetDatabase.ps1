# Veritabanını sıfırlayıp yeniden oluşturma scripti
# PowerShell script

Write-Host "Veritabanı migration'ları siliniyor..." -ForegroundColor Yellow
dotnet ef database drop --force

Write-Host "Yeni migration oluşturuluyor..." -ForegroundColor Yellow
dotnet ef migrations add ResetDatabase

Write-Host "Veritabanı oluşturuluyor ve seed ediliyor..." -ForegroundColor Yellow
dotnet ef database update

Write-Host "Tamamlandı! API'yi çalıştırabilirsiniz." -ForegroundColor Green

