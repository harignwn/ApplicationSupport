
public class Planning
{
    public int Id { get; set; }

    // Produksi asli
    public int Senin { get; set; }
    public int Selasa { get; set; }
    public int Rabu { get; set; }
    public int Kamis { get; set; }
    public int Jumat { get; set; }
    public int Sabtu { get; set; }
    public int Minggu { get; set; }

    // Produksi hasil koreksi
    public int SeninFixed { get; set; }
    public int SelasaFixed { get; set; }
    public int RabuFixed { get; set; }
    public int KamisFixed { get; set; }
    public int JumatFixed { get; set; }
    public int SabtuFixed { get; set; }
    public int MingguFixed { get; set; }

    public DateTime TanggalInput { get; set; } = DateTime.Now;
}
