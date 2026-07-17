using UnityEngine;
using System.Collections.Generic;
using System;

// ============================================================================
// Shikaku_Solver
// ----------------------------------------------------------------------------
// STATIC class => sahne objesine bağlı değildir, sadece veri işler.
// Görevi: Bir LevelData'nın Shikaku kurallarına göre çözülüp çözülemeyeceğini
//         ve kaç farklı çözümü olduğunu bulmak (GDD 6.2 - Uniqueness Check).
// ============================================================================
public static class Shikaku_Solver
{
    // ------------------------------------------------------------------------
    // DAHİLİ YAPI: Bir dikdörtgenin sol-alt köşesi (x,y) ve boyutu (w,h)
    // Sadece bu sınıf içinde kullanılır, dışarı kapalı.
    // ------------------------------------------------------------------------
    private struct RectShape { public int x, y, w, h; }

    // ------------------------------------------------------------------------
    // DIŞA AÇIK YAPI: Bulunan bir çözümdeki dikdörtgeni temsil eder.
    // İleride "ipucu" özelliği yaparsak kullanırız.
    // ------------------------------------------------------------------------
    public struct SolutionRect { public int hintIndex; public int x, y, w, h; }

    // ========================================================================
    // ANA METOT: Solve()
    // Bir level çözülebiliyorsa ilk bulduğu çözümü (Liste) döner, yoksa null.
    // ========================================================================
    public static List<SolutionRect> Solve(LevelData level)
    {
        // 1. Guard: level ya da ipuçları yoksa çözülemez, direkt null.
        if (level == null || level.hints == null || level.hints.Length == 0)
            return null;

        // 2. Tahta boyutlarını local değişkene al (okunabilirlik + hız).
        int w = level.width;
        int h = level.height;

        // 3. level.hints => tüm numaralı hücrelerin listesi (x, y, number).
        var hints = level.hints;

        // 4. YENİ: Tüm ipucu konumlarını bir HashSet'te topla.
        //    Amaç: Aday dikdörtgen üretirken "başka bir sayıyı içeriyor mu?" diye
        //    hızlı kontrol edebilmek (Shikaku kuralı: 1 bölge = 1 sayı).
        var hintPositions = BuildHintPositionSet(hints);

        // 5. Her ipucu için OLASI dikdörtgen şekillerini (adayları) önceden hesapla
        //    ve bir dizi içinde tut.
        var candidates = new List<RectShape>[hints.Length];

        // 6. Tüm ipuçlarını dön:
        for (int i = 0; i < hints.Length; i++)
        {
            // i. ipucu için adayları bul (artık hintPositions da veriyoruz)
            candidates[i] = GetCandidateRects(hints[i], w, h, hintPositions);

            // 7. Eğer bu ipucu için hiç aday yoksa puzzle imkânsız -> null.
            if (candidates[i].Count == 0) return null;
        }

        // 8. Örtüşme takibi için grid matrisi: -1 = boş hücre.
        int[,] grid = new int[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                grid[x, y] = -1; // başlangıçta her yer boş

        // 9. Çözümü tutacak liste (her biri bir SolutionRect).
        var solution = new List<SolutionRect>();

        // 10. Backtracking'i 0. ipucudan başlat.
        //     Eğer true dönerse solution doludur ve geçerlidir.
        return Backtrack(0, hints, candidates, grid, solution) ? solution : null;
    }

    // ========================================================================
    // YENİ METOT: CountSolutions()
    // Kaç FARKLI geçerli çözüm olduğunu sayar (maxCount'a ulaşınca durur).
    // LevelGenerator bunu "tekil çözüm mü?" diye doğrulamak için kullanır.
    // ========================================================================
    public static int CountSolutions(LevelData level, int maxCount = 2)
    {
        // 1. Guard: level yoksa 0 çözüm.
        if (level == null || level.hints == null || level.hints.Length == 0) return 0;

        int w = level.width, h = level.height;
        var hints = level.hints;
        var hintPositions = BuildHintPositionSet(hints);

        // 2. Adayları yine hesapla.
        var candidates = new List<RectShape>[hints.Length];
        for (int i = 0; i < hints.Length; i++)
        {
            candidates[i] = GetCandidateRects(hints[i], w, h, hintPositions);
            if (candidates[i].Count == 0) return 0; // aday yoksa çözümsüz
        }

        // 3. Boş grid matrisi.
        int[,] grid = new int[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                grid[x, y] = -1;

        // 4. Sayacı tut ve recursive sayım başlat.
        int count = 0;
        CountBacktrack(0, hints, candidates, grid, ref count, maxCount);
        return count; // 0 = çözümsüz, 1 = tekil, 2+ = çoklu
    }

    // ========================================================================
    // YARDIMCI: BuildHintPositionSet()
    // İpucu koordinatlarını (x,y) tuple olarak HashSet'e ekler.
    // ========================================================================
    static HashSet<(int, int)> BuildHintPositionSet(TileHint[] hints)
    {
        var set = new HashSet<(int, int)>();
        foreach (var hint in hints) set.Add((hint.x, hint.y));
        return set;
    }

    // ========================================================================
    // BACKTRACK (ilk çözümü bulmak için)
    // ========================================================================
    private static bool Backtrack(int idx, TileHint[] hints, List<RectShape>[] candidates, int[,] grid, List<SolutionRect> sol)
    {
        // Tüm ipuçları yerleştirildi mi?
        if (idx == hints.Length)
        {
            // Tahtadaki tüm hücreler dolu mu? (-1 kalmamış mı)
            int w = grid.GetLength(0), h = grid.GetLength(1);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (grid[x, y] == -1) return false; // boş hücre var, geçersiz
            return true; // tam kaplama var, çözüm bulundu
        }

        // Şu anki ipucu için tüm aday dikdörtgenleri dene
        foreach (var shape in candidates[idx])
        {
            // Bu shape grid'e sığıyor mu (başkasıyla çakışmıyor mu)?
            if (CanPlace(shape, grid))
            {
                Place(shape, idx, grid, +1); // geçici yerleştir
                sol.Add(new SolutionRect { hintIndex = idx, x = shape.x, y = shape.y, w = shape.w, h = shape.h });

                // Bir sonrakine geç (recursive)
                if (Backtrack(idx + 1, hints, candidates, grid, sol))
                    return true; // ileride çözüm bulundu

                // Bulunamadıysa geri al (backtrack)
                sol.RemoveAt(sol.Count - 1);
                Place(shape, idx, grid, -1);
            }
        }
        return false; // bu ipucu için uygun aday yok
    }

    // ========================================================================
    // COUNTBACKTRACK (TÜM çözümleri saymak için, ilkinde durmaz)
    // ========================================================================
    private static void CountBacktrack(int idx, TileHint[] hints, List<RectShape>[] candidates, int[,] grid, ref int count, int maxCount)
    {
        if (count >= maxCount) return; // yeterince bulduk, aramayı kes

        if (idx == hints.Length)
        {
            // Tam kaplama mı?
            int w = grid.GetLength(0), h = grid.GetLength(1);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (grid[x, y] == -1) return;
            count++; // bir geçerli çözüm daha
            return;
        }

        foreach (var shape in candidates[idx])
        {
            if (count >= maxCount) return;
            if (CanPlace(shape, grid))
            {
                Place(shape, idx, grid, +1);
                CountBacktrack(idx + 1, hints, candidates, grid, ref count, maxCount);
                Place(shape, idx, grid, -1); // geri al
            }
        }
    }

    // ========================================================================
    // CANPLACE: Bir dikdörtgen grid'e sığıyor mu? (hiçbir hücre dolu mu?)
    // ========================================================================
    private static bool CanPlace(RectShape s, int[,] grid)
    {
        for (int x = s.x; x < s.x + s.w; x++)
            for (int y = s.y; y < s.y + s.h; y++)
                if (grid[x, y] != -1) return false; // çakışma
        return true;
    }

    // ========================================================================
    // PLACE: Dikdörtgeni grid'e işle (+1 yerleştir, -1 geri al)
    // ========================================================================
    private static void Place(RectShape s, int idx, int[,] grid, int sign)
    {
        for (int x = s.x; x < s.x + s.w; x++)
            for (int y = s.y; y < s.y + s.h; y++)
                grid[x, y] = sign > 0 ? idx : -1;
    }

    // ========================================================================
    // GETCANDIDATERECTS: Bir ipucu için alanı N olan tüm GEÇERLİ dikdörtgenleri bul
    // ========================================================================
    private static List<RectShape> GetCandidateRects(TileHint hint, int gridW, int gridH, HashSet<(int, int)> hintPositions)
    {
        var list = new List<RectShape>();
        int N = hint.number; // ipucu değeri = bölge alanı

        // w * h = N olacak şekilde tüm çarpanları dene
        for (int rw = 1; rw <= N; rw++)
        {
            if (N % rw != 0) continue; // tam bölen değil
            int rh = N / rw;

            // Boyut grid'i aşıyorsa atla (yeni eklenen güvenlik)
            if (rw > gridW || rh > gridH) continue;

            // hint (hx,hy) bu rect'in içinde olmalı => x sınırları
            int minX = Mathf.Max(0, hint.x - rw + 1);
            int maxX = Mathf.Min(hint.x, gridW - rw);
            int minY = Mathf.Max(0, hint.y - rh + 1);
            int maxY = Mathf.Min(hint.y, gridH - rh);

            // Tüm olası sol-alt köşeleri dene
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    // YENİ KONTROL: Bu rect BAŞKA bir ipucu hücresini içeriyor mu?
                    bool containsOtherHint = false;
                    foreach (var (hx, hy) in hintPositions)
                    {
                        if (hx == hint.x && hy == hint.y) continue; // kendisi
                        if (hx >= x && hx < x + rw && hy >= y && hy < y + rh)
                        {
                            containsOtherHint = true;
                            break;
                        }
                    }
                    if (containsOtherHint) continue; // kural ihlali, aday değil

                    list.Add(new RectShape { x = x, y = y, w = rw, h = rh });
                }
            }
        }
        return list;
    }
}