using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;

namespace Sts2ConceptMap.Localization;

/// <summary>
/// Localization table for concept names. Mirrors the sibling-mod pattern
/// (LocManager.Instance.Language → per-language dictionary, ENG fallback).
/// Keys: name_surprise + name_&lt;conceptKey&gt; for all 16 concepts.
/// </summary>
public static class Strings
{
    public static string Get(string key)
    {
        var lang = GetCurrentLanguage();
        if (_tables.TryGetValue(lang, out var t) && t.TryGetValue(key, out var v)) return v;
        if (_tables.TryGetValue("ENG", out var eng) && eng.TryGetValue(key, out var ev)) return ev;
        return key;
    }

    private static string GetCurrentLanguage()
    {
        try { return (LocManager.Instance?.Language ?? "ENG").ToUpperInvariant(); }
        catch { return "ENG"; }
    }

    private static readonly Dictionary<string, Dictionary<string, string>> _tables = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ENG"] = new() {
            ["name_surprise"]="Surprise", ["name_wanderer"]="Wanderer's Path", ["name_market"]="Market Street",
            ["name_woods"]="Quiet Woods", ["name_trove"]="Treasure Trail", ["name_story"]="Storyteller's Path",
            ["name_balanced"]="Balanced", ["name_crossroads"]="Crossroads", ["name_hunt"]="Hunting Grounds",
            ["name_merchant"]="Merchant's Run", ["name_wild"]="Wildlands", ["name_trial"]="Trial Road",
            ["name_gauntlet"]="Elite Gauntlet", ["name_fire"]="Trial by Fire", ["name_grinder"]="The Meatgrinder",
            ["name_famine"]="Famine", ["name_dread"]="Dread Domain" },
        ["KOR"] = new() {
            ["name_surprise"]="서프라이즈", ["name_wanderer"]="방랑자의 길", ["name_market"]="시장 거리",
            ["name_woods"]="고요한 숲", ["name_trove"]="보물의 길", ["name_story"]="이야기의 장",
            ["name_balanced"]="균형", ["name_crossroads"]="갈림길", ["name_hunt"]="사냥터",
            ["name_merchant"]="상인의 길", ["name_wild"]="변덕의 땅", ["name_trial"]="시험의 길",
            ["name_gauntlet"]="정예 시련장", ["name_fire"]="시련의 불길", ["name_grinder"]="분쇄기",
            ["name_famine"]="기근의 땅", ["name_dread"]="공포의 영역" },
        ["JPN"] = new() {
            ["name_surprise"]="おまかせ", ["name_wanderer"]="放浪者の道", ["name_market"]="市場通り",
            ["name_woods"]="静かな森", ["name_trove"]="宝の道", ["name_story"]="物語の道",
            ["name_balanced"]="バランス", ["name_crossroads"]="分かれ道", ["name_hunt"]="狩場",
            ["name_merchant"]="商人の道", ["name_wild"]="気まぐれの地", ["name_trial"]="試練の道",
            ["name_gauntlet"]="精鋭の試練場", ["name_fire"]="炎の試練", ["name_grinder"]="肉挽き場",
            ["name_famine"]="飢餓の地", ["name_dread"]="恐怖の領域" },
        ["ZHS"] = new() {
            ["name_surprise"]="随机", ["name_wanderer"]="漫游者之路", ["name_market"]="集市街",
            ["name_woods"]="静谧森林", ["name_trove"]="寻宝之路", ["name_story"]="故事之路",
            ["name_balanced"]="均衡", ["name_crossroads"]="十字路口", ["name_hunt"]="狩猎场",
            ["name_merchant"]="商人之路", ["name_wild"]="狂野之地", ["name_trial"]="试炼之路",
            ["name_gauntlet"]="精英试炼场", ["name_fire"]="烈火试炼", ["name_grinder"]="绞肉机",
            ["name_famine"]="饥荒之地", ["name_dread"]="恐惧领域" },
        ["ZHT"] = new() {
            ["name_surprise"]="隨機", ["name_wanderer"]="漫遊者之路", ["name_market"]="集市街",
            ["name_woods"]="靜謐森林", ["name_trove"]="尋寶之路", ["name_story"]="故事之路",
            ["name_balanced"]="均衡", ["name_crossroads"]="十字路口", ["name_hunt"]="狩獵場",
            ["name_merchant"]="商人之路", ["name_wild"]="狂野之地", ["name_trial"]="試煉之路",
            ["name_gauntlet"]="精英試煉場", ["name_fire"]="烈火試煉", ["name_grinder"]="絞肉機",
            ["name_famine"]="饑荒之地", ["name_dread"]="恐懼領域" },
        ["FRA"] = new() {
            ["name_surprise"]="Surprise", ["name_wanderer"]="Chemin du Vagabond", ["name_market"]="Rue du Marché",
            ["name_woods"]="Bois Tranquilles", ["name_trove"]="Piste au Trésor", ["name_story"]="Voie du Conteur",
            ["name_balanced"]="Équilibré", ["name_crossroads"]="Carrefour", ["name_hunt"]="Terrain de Chasse",
            ["name_merchant"]="Tournée du Marchand", ["name_wild"]="Terres Sauvages", ["name_trial"]="Route des Épreuves",
            ["name_gauntlet"]="Gantelet d'Élites", ["name_fire"]="Épreuve du Feu", ["name_grinder"]="Le Hachoir",
            ["name_famine"]="Famine", ["name_dread"]="Domaine de l'Effroi" },
        ["DEU"] = new() {
            ["name_surprise"]="Überraschung", ["name_wanderer"]="Wandererpfad", ["name_market"]="Marktstraße",
            ["name_woods"]="Stiller Wald", ["name_trove"]="Schatzpfad", ["name_story"]="Erzählerpfad",
            ["name_balanced"]="Ausgewogen", ["name_crossroads"]="Kreuzweg", ["name_hunt"]="Jagdgründe",
            ["name_merchant"]="Händlerrunde", ["name_wild"]="Wildland", ["name_trial"]="Prüfungspfad",
            ["name_gauntlet"]="Elite-Spießroute", ["name_fire"]="Feuerprobe", ["name_grinder"]="Der Fleischwolf",
            ["name_famine"]="Hungersnot", ["name_dread"]="Schreckensreich" },
        ["ESP"] = new() {
            ["name_surprise"]="Sorpresa", ["name_wanderer"]="Senda del Errante", ["name_market"]="Calle del Mercado",
            ["name_woods"]="Bosque Tranquilo", ["name_trove"]="Ruta del Tesoro", ["name_story"]="Senda del Narrador",
            ["name_balanced"]="Equilibrado", ["name_crossroads"]="Encrucijada", ["name_hunt"]="Cotos de Caza",
            ["name_merchant"]="Ronda del Mercader", ["name_wild"]="Tierras Salvajes", ["name_trial"]="Camino de Pruebas",
            ["name_gauntlet"]="Reto de Élites", ["name_fire"]="Prueba de Fuego", ["name_grinder"]="La Picadora",
            ["name_famine"]="Hambruna", ["name_dread"]="Dominio del Terror" },
        ["SPA"] = new() {
            ["name_surprise"]="Sorpresa", ["name_wanderer"]="Senda del Errante", ["name_market"]="Calle del Mercado",
            ["name_woods"]="Bosque Tranquilo", ["name_trove"]="Ruta del Tesoro", ["name_story"]="Senda del Narrador",
            ["name_balanced"]="Equilibrado", ["name_crossroads"]="Encrucijada", ["name_hunt"]="Cotos de Caza",
            ["name_merchant"]="Ronda del Mercader", ["name_wild"]="Tierras Salvajes", ["name_trial"]="Camino de Pruebas",
            ["name_gauntlet"]="Reto de Élites", ["name_fire"]="Prueba de Fuego", ["name_grinder"]="La Picadora",
            ["name_famine"]="Hambruna", ["name_dread"]="Dominio del Terror" },
        ["ITA"] = new() {
            ["name_surprise"]="Sorpresa", ["name_wanderer"]="Sentiero del Viandante", ["name_market"]="Via del Mercato",
            ["name_woods"]="Bosco Silenzioso", ["name_trove"]="Pista del Tesoro", ["name_story"]="Sentiero del Narratore",
            ["name_balanced"]="Equilibrato", ["name_crossroads"]="Bivio", ["name_hunt"]="Terreni di Caccia",
            ["name_merchant"]="Giro del Mercante", ["name_wild"]="Terre Selvagge", ["name_trial"]="Via delle Prove",
            ["name_gauntlet"]="Guanto delle Élite", ["name_fire"]="Prova del Fuoco", ["name_grinder"]="Il Tritacarne",
            ["name_famine"]="Carestia", ["name_dread"]="Dominio del Terrore" },
        ["RUS"] = new() {
            ["name_surprise"]="Сюрприз", ["name_wanderer"]="Путь странника", ["name_market"]="Рыночная улица",
            ["name_woods"]="Тихий лес", ["name_trove"]="Тропа сокровищ", ["name_story"]="Путь рассказчика",
            ["name_balanced"]="Баланс", ["name_crossroads"]="Перепутье", ["name_hunt"]="Охотничьи угодья",
            ["name_merchant"]="Путь торговца", ["name_wild"]="Дикие земли", ["name_trial"]="Путь испытаний",
            ["name_gauntlet"]="Строй элит", ["name_fire"]="Испытание огнём", ["name_grinder"]="Мясорубка",
            ["name_famine"]="Голод", ["name_dread"]="Владения ужаса" },
        ["PTB"] = new() {
            ["name_surprise"]="Surpresa", ["name_wanderer"]="Caminho do Andarilho", ["name_market"]="Rua do Mercado",
            ["name_woods"]="Bosque Silencioso", ["name_trove"]="Trilha do Tesouro", ["name_story"]="Caminho do Contador",
            ["name_balanced"]="Equilibrado", ["name_crossroads"]="Encruzilhada", ["name_hunt"]="Terreno de Caça",
            ["name_merchant"]="Rota do Mercador", ["name_wild"]="Terras Selvagens", ["name_trial"]="Estrada das Provações",
            ["name_gauntlet"]="Desafio de Elites", ["name_fire"]="Provação de Fogo", ["name_grinder"]="O Moedor",
            ["name_famine"]="Fome", ["name_dread"]="Domínio do Pavor" },
        ["POR"] = new() {
            ["name_surprise"]="Surpresa", ["name_wanderer"]="Caminho do Andarilho", ["name_market"]="Rua do Mercado",
            ["name_woods"]="Bosque Silencioso", ["name_trove"]="Trilha do Tesouro", ["name_story"]="Caminho do Contador",
            ["name_balanced"]="Equilibrado", ["name_crossroads"]="Encruzilhada", ["name_hunt"]="Terreno de Caça",
            ["name_merchant"]="Rota do Mercador", ["name_wild"]="Terras Selvagens", ["name_trial"]="Estrada das Provações",
            ["name_gauntlet"]="Desafio de Elites", ["name_fire"]="Provação de Fogo", ["name_grinder"]="O Moedor",
            ["name_famine"]="Fome", ["name_dread"]="Domínio do Pavor" },
        ["POL"] = new() {
            ["name_surprise"]="Niespodzianka", ["name_wanderer"]="Ścieżka Wędrowca", ["name_market"]="Ulica Targowa",
            ["name_woods"]="Cichy Las", ["name_trove"]="Szlak Skarbów", ["name_story"]="Ścieżka Gawędziarza",
            ["name_balanced"]="Zrównoważony", ["name_crossroads"]="Rozdroże", ["name_hunt"]="Tereny Łowieckie",
            ["name_merchant"]="Trasa Kupca", ["name_wild"]="Dzikie Ziemie", ["name_trial"]="Droga Prób",
            ["name_gauntlet"]="Szpaler Elit", ["name_fire"]="Próba Ognia", ["name_grinder"]="Maszynka do Mięsa",
            ["name_famine"]="Głód", ["name_dread"]="Domena Grozy" },
        ["TUR"] = new() {
            ["name_surprise"]="Sürpriz", ["name_wanderer"]="Gezginin Yolu", ["name_market"]="Pazar Sokağı",
            ["name_woods"]="Sessiz Orman", ["name_trove"]="Hazine İzi", ["name_story"]="Anlatıcının Yolu",
            ["name_balanced"]="Dengeli", ["name_crossroads"]="Kavşak", ["name_hunt"]="Av Sahası",
            ["name_merchant"]="Tüccar Turu", ["name_wild"]="Vahşi Topraklar", ["name_trial"]="Sınav Yolu",
            ["name_gauntlet"]="Seçkin Çilesi", ["name_fire"]="Ateş Sınavı", ["name_grinder"]="Kıyma Makinesi",
            ["name_famine"]="Kıtlık", ["name_dread"]="Dehşet Diyarı" },
        ["THA"] = new() {
            ["name_surprise"]="สุ่ม", ["name_wanderer"]="เส้นทางนักพเนจร", ["name_market"]="ถนนตลาด",
            ["name_woods"]="ป่าเงียบสงบ", ["name_trove"]="เส้นทางสมบัติ", ["name_story"]="เส้นทางนักเล่าเรื่อง",
            ["name_balanced"]="สมดุล", ["name_crossroads"]="ทางแยก", ["name_hunt"]="เขตล่าสัตว์",
            ["name_merchant"]="เส้นทางพ่อค้า", ["name_wild"]="ดินแดนเถื่อน", ["name_trial"]="เส้นทางบททดสอบ",
            ["name_gauntlet"]="สนามประลองชนชั้นยอด", ["name_fire"]="บททดสอบแห่งไฟ", ["name_grinder"]="เครื่องบดเนื้อ",
            ["name_famine"]="ความอดอยาก", ["name_dread"]="อาณาเขตสยองขวัญ" },
    };
}
