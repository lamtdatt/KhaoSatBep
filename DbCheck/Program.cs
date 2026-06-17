using Npgsql;

const string connString = "Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;" +
    "Username=postgres.fevxnjjimsiqcipzrpxk;Password=30042008@@AZHIHI;" +
    "SSL Mode=Require;Trust Server Certificate=true;Timeout=15;Command Timeout=30";

await using var conn = new NpgsqlConnection(connString);
await conn.OpenAsync();
Console.WriteLine("Ket noi Supabase thanh cong!\n");

// Liệt kê tất cả biên bản
Console.WriteLine("=== DANH SACH BIEN BAN HIEN CO ===");
var ids = new List<int>();
await using (var cmd = new NpgsqlCommand(@"
    SELECT b.""Id"", b.""SoBienBan"", b.""LoaiBienBan"", b.""TrangThai"", b.""NgayKiemTra"", u.""HoTen""
    FROM ""BienBans"" b
    JOIN ""Users"" u ON u.""Id"" = b.""NguoiTaoId""
    ORDER BY b.""Id""", conn))
await using (var reader = await cmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        int id = reader.GetInt32(0);
        ids.Add(id);
        Console.WriteLine($"  ID={id} | {reader.GetString(1)} | {reader.GetString(2)} | {reader.GetString(3)} | {reader.GetDateTime(4):yyyy-MM-dd} | NguoiTao: {reader.GetString(5)}");
    }
}

if (ids.Count == 0)
{
    Console.WriteLine("  (Khong co bien ban nao trong DB)");
    return;
}

Console.WriteLine($"\nTong cong: {ids.Count} bien ban.");
Console.WriteLine("Ban co muon XOA TAT CA bien ban tren khong? (y/n)");
var confirm = Console.ReadLine();
if (confirm?.ToLower() != "y")
{
    Console.WriteLine("Da huy. Khong co gi thay doi.");
    return;
}

// Xóa theo thứ tự FK: child tables trước, rồi mới xóa BienBans
Console.WriteLine("\nDang xoa...");
await using (var cmd = new NpgsqlCommand(@"
    DELETE FROM ""ChuKys"";
    DELETE FROM ""ThanhPhanKiemTras"";
    DELETE FROM ""ChiTietBienBans"";
    DELETE FROM ""DinhLuongSuatAns"";
    DELETE FROM ""BienBans"";
", conn))
{
    await cmd.ExecuteNonQueryAsync();
}

Console.WriteLine($"Da xoa xong {ids.Count} bien ban va toan bo du lieu lien quan!");

// Kiểm tra lại
Console.WriteLine("\n=== KIEM TRA SAU KHI XOA ===");
await using (var cmd2 = new NpgsqlCommand(@"SELECT COUNT(*) FROM ""BienBans""", conn))
{
    var count = await cmd2.ExecuteScalarAsync();
    Console.WriteLine($"So bien ban con lai trong DB: {count}");
}
Console.WriteLine("Hoan tat!");
