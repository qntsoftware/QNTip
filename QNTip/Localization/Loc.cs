namespace Qntip.Localization;

internal static class Loc
{
    private static string _lang = "TR";
    public static string Lang => _lang;

    public static void SetLang(string lang) => _lang = lang == "ENG" ? "ENG" : "TR";

    public static string T(string key)
    {
        if (!Strings.TryGetValue(key, out var pair)) return key;
        return _lang == "TR" ? pair.TR : pair.ENG;
    }

    private static readonly Dictionary<string, (string TR, string ENG)> Strings = new()
    {
        ["btn_stop"] = ("DURDUR", "STOP"),

        ["starting"] = ("BASLANGIC - ilk IP aliniyor", "START - fetching first IP"),
        ["loop_sep"] = ("DONGU {0} - SIGNAL NEWNYM gonderiliyor", "LOOP {0} - sending SIGNAL NEWNYM"),
        ["next_ip_in"] = ("Sonraki IP degisimi icin {0} sn", "Next IP change in {0} sec"),
        ["circuit_ok"] = ("Tor circuit yenilendi. Firefox yeni IP'den cikacak.", "Tor circuit refreshed. Firefox will exit from new IP."),
        ["newnym_fail"] = ("NEWNYM basarisiz", "NEWNYM failed"),
        ["shutting"] = ("Kapatiliyor...", "Shutting down..."),
        ["ctrl_c"] = ("Durdurmak icin Ctrl+C veya [S].", "Ctrl+C or [S] to stop."),
        ["proxy_title"] = ("PROXY AKTIF", "PROXY ACTIVE"),
        ["listening"] = ("Dinleniyor", "Listening"),
        ["forwarding"] = ("Yonlendirme", "Forwarding"),
        ["mode_lbl"] = ("Mod", "Mode"),
        ["ff_hint"] = ("Firefox proxy ayarini su sekilde yap:", "Set Firefox proxy as follows:"),

        ["ip_title"] = ("YENI IP ADRESI TESPIT EDILDI", "NEW IP ADDRESS DETECTED"),
        ["ip_label"] = ("IP Adresi", "IP Address"),
        ["country"] = ("Ulke", "Country"),
        ["region"] = ("Bolge", "Region"),
        ["city"] = ("Sehir", "City"),
        ["isp"] = ("ISP / Org", "ISP / Org"),

        ["err_fetch"] = ("IP bilgisi alinamadi", "Could not fetch IP info"),
        ["err_generic"] = ("Hata", "Error"),
        ["lang_changed"] = ("Dil degistirildi", "Language changed"),
    };
}