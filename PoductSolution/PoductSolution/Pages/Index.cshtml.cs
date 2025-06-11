using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Humanizer;
using System.Xml.Linq;

// Model untuk halaman Index (input rencana produksi mobil)
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    // Constructor: Inject DbContext untuk akses database
    public IndexModel(AppDbContext context) => _context = context;

    // Property terikat ke form input produksi (untuk 7 hari)
    [BindProperty]
    public int[] InputProduksi { get; set; } = new int[7];

    // Menyimpan hasil output produksi yang sudah diperbaiki
    public int[]? OutputProduksi { get; set; }

    // Menyimpan pesan error jika validasi gagal
    public string ErrorMessage { get; set; }

    // Nama-nama hari untuk ditampilkan di form dan tabel
    public List<string> DayNames { get; } = ["Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu", "Minggu"];

    // Method yang dipanggil ketika form disubmit (POST)
    //    public async Task<IActionResult> OnPostAsync() digunakan dalam Razor Pages karena:
    //1. OnPostAsync() adalah handler method khusus untuk HTTP POST
    //Metode OnPostAsync() akan otomatis dijalankan saat user submit form(<form method = "post" >).


    //     2. Kenapa async?
    //Karena kita melakukan operasi asinkron
    //Pemanggilan ini asinkron karena mengakses database dan bisa memakan waktu — jika tidak await, UI bisa freeze atau tidak responsif.Karena itu, metode kita harus:


    public async Task<IActionResult> OnPostAsync()
    {
        var input = InputProduksi.ToArray();

        // Validasi: jika semua input bernilai 0
        if (input.All(i => i == 0))
        {
            ErrorMessage = "Semua hari kosong. Harap isi minimal satu hari produksi.";
            OutputProduksi = null;
            return Page(); // Tampilkan kembali halaman dengan pesan error
        }

        // Proses perataan rencana produksi
        var output = FixPlanning(input);

        // Simpan data input & output ke database (tabel Planning)
        var plan = new Planning
        {
            Senin = input[0],
            Selasa = input[1],
            Rabu = input[2],
            Kamis = input[3],
            Jumat = input[4],
            Sabtu = input[5],
            Minggu = input[6],

            SeninFixed = output[0],
            SelasaFixed = output[1],
            RabuFixed = output[2],
            KamisFixed = output[3],
            JumatFixed = output[4],
            SabtuFixed = output[5],
            MingguFixed = output[6]
        };

        _context.Plannings.Add(plan);
        await _context.SaveChangesAsync();

        // Tampilkan hasil di halaman
        OutputProduksi = output;
        ErrorMessage = null;
        return Page();
    }

    // Fungsi untuk meratakan distribusi produksi ke hari aktif
    private int[] FixPlanning(int[] data)
    {
        //data adalah array berisi jumlah produksi per hari dalam seminggu(int[7] untuk Senin sampai Minggu).

        var result = (int[])data.Clone();
        //Membuat salinan dari data supaya tidak mengubah array asli.


        // Ambil hanya hari yang memiliki nilai > 0 (hari aktif)
        var hariAktif = data.Select((val, idx) => new { val, idx })
                            .Where(x => x.val > 0)
                            .ToList();
        //Select((val, idx) => new { val, idx }): mengambil nilai dan index-nya.
        //Where(x => x.val > 0): filter hanya hari yang memiliki nilai produksi > 0.

        // Hitung total unit dan jumlah hari aktif
        int total = hariAktif.Sum(x => x.val);
        int jumlahHariAktif = hariAktif.Count;

        // Hitung rata-rata dan sisa
        int rata = total / jumlahHariAktif;
        int sisa = total % jumlahHariAktif;

        // Siapkan array hasil (default 0 semua)
        // Membuat array hasil berisi 7 elemen semua bernilai 0.
        //Ini yang nanti dikembalikan sebagai output perencanaan baru.
        var hasil = Enumerable.Repeat(0, 7).ToArray();

        // Set semua hari aktif dengan nilai rata-rata
        foreach (var h in hariAktif)
            hasil[h.idx] = rata;

        // Tambahkan sisa ke hari-hari yang sebelumnya punya nilai besar
        // Jika ada sisa, diberikan ke hari yang awalnya punya nilai produksi terbesar.
        //Tujuannya agar pembagian lebih adil dan mendekati rencana awal.
        foreach (var idx in hariAktif.OrderByDescending(x => x.val).Take(sisa))
        {
            hasil[idx.idx]++;
        }

        return hasil;
    }
}

//Tujuan
//Menyamakan beban produksi di hari aktif :	Mengisi dengan rata-rata
//Tidak mengubah total produksi	 : Hitung ulang dengan total, rata, dan sisa
//Prioritaskan hari sibuk	: Tambahkan sisa ke hari dengan nilai awal besar