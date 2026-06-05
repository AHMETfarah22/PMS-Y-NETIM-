$body = @{
    FirstName = "Ahmet"
    LastName = "Yilmaz"
    IdentityNumber = "12345678901"
    Phone = "5551234567"
    Email = "ahmet@example.com"
    RoomNumber = "101"
    BedNumber = 1
    CheckInDate = "2026-05-31"
    CheckOutDate = "2026-06-01"
    Notes = "Test rez"
} | ConvertTo-Json

$result = Invoke-RestMethod -Uri "http://localhost:5262/api/reservation/book" -Method Post -Body $body -ContentType "application/json"
$result | ConvertTo-Json | Out-File "test_book_result.txt"
