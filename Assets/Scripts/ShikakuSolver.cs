using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

// static olması sadece veri işlediğini gösterir. Bulmaca çözücü level datanın kurallarına uygun bir şekilde bulmacaları çözer.
public static class Shikaku_Solver
{
  //Bir dikdörtgenin sol-alt köşesi ve boyutu
  private struct RectShape { public int x, y, w, h; }
  
  //Çözümdeki bir dikdörtgeni temsil eder. Hint sisteminde kullanılır.

  public struct SolutionRect { public int hintIndex; public int x, y, w, h; }

    //Ana çözüm metodu. Level çözülebiliyorsa çözümü listelenir. Yoksa null
    public static List<SolutionRect> Solve(LevelData level)
    {
        //level null mı veya içinde hiç hints yok mu iki türlüde null
        if (level == null || level.hints == null || level.hints.Length == 0)
            return null;
        //level boyutlarının değişkenlerini al.
        int w = level.width;
        int h = level.height;
        // level.hints = tahtadaki TÜM numaralı hücrelerin listesi (x, y, number bilgisi)
        var hints = level.hints;
        // her ipucu için olası dikdörtgenleri şekillerini önceden hesaplar.
        var candidates = new List<RectShape>[hints.Length];

        //Tüm ipuçlarını döndüren kısım
        for ( int i = 0; i < hints.Length; i++)
        {
            // i. ipucu için aday dikdörtgenleri bul
            candidates[i] = GetCandidateRects(hints[i], w, h);

            //eğer ipucu için hiç aday yoksa puzzle imkansızdır.
            if (candidates[i].Count == 0 ) return null;

        }
        //puzzle örtüşüyor mu çözüm ile grid matrisi oluştur.

        int[,] grid = new int[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                grid[x, y] = -1; // Başlangıçta her yer boş.

        //Çözümü tutacak Liste(her biri bir SolutionRect)
        var solution = new List<SolutionRect>();

        //Backtracking başladığı yer. 0. ipucudan itibaren 
        //Eğer true dönerse solution listesi doludur ve geçerlidir, değilse null döner.
        return Backtrack(0, hints, candidates, grid, solution) ? solution : null;

    }

    //Backtracking çekirdeği : sırayla ipuçlarını dener.
    private static bool Backtrack(int idx, TileHint[] hints, List<RectShape>[] candidates, int[,] grid, List<SolutionRect> sol)
    {
        //Tüm ipuçlarını geri döndürür. Idx sona geldi
        if (idx == hints.Length)
        {
            // Tahtadaki tüm hücrelerin dolu olup olmadığını kontrol et.
            int w = grid.GetLength(0), h = grid.GetLength(1);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (grid[x, y] == -1) return false; // Tahtada boş hücre var false ver.
            //tahta iyi geçerli çözüm bulundu.
            return true;
        }

        //Şu anki idx. ipucu için tüm dikdörtgenleri dene.
        foreach (var shape in candidates[idx])
        {
            //Bu shape(dikdörtgen) grid üzerine sığıyor mu başka bir dikdörtgen ile çakışıyor mu?
            if (CanPlace(shape, grid))
            {
                //geçici olarak yerleştir.
                Place(shape, idx, grid, +1);

                //Çözüm listesine ekle
                sol.Add(new SolutionRect { hintIndex = idx, x = shape.x, y = shape.y, w = shape.w, h = shape.h });


                //Bir sonraki ipucuya geç  (recursive)
                if (Backtrack(idx + 1, hints, candidates, grid, sol))
                    return true; //çözüm bulundu, geri dönme

                //Çözüm bulunmadıysa bu adımı geri al
                sol.RemoveAt(sol.Count - 1);
                Place(shape, idx, grid, -1);//Gridi eski haline getiren kısım.

            }
        }
        return false; //Bu ipucu için hiçbir aday işe yaramadı demek.
    }
    //Bir dikdörtgen grid'e sığıyor mu?
    private static bool CanPlace(RectShape s, int[,] grid)
    {
        for (int x = s.x; x < s.x + s.w; x++)
            for (int y = s.y; y < s.y + s.h; y++)
                if (grid[x, y] != -1) return false; // Çakışma var demektir.
        return true;
    }

    //Dikdörtgeni grid'e işlediğimiz yer (+1 yerleştir, -1 geri al.)
    private static void Place(RectShape s, int idx, int[,] grid, int sign)
    {
        for (int x = s.x; x < s.x + s.w; x++)
            for (int y = s.y; y < s.y + s.h; y++)
                grid[x, y] = sign > 0 ? idx : -1;
    }

    //Bir ipucu için alanı N olan tüm geçerli dikdörtgenleri bul
    private static List<RectShape> GetCandidateRects(TileHint hint, int gridW, int gridH)
    {
        var list = new List<RectShape>();
        int N = hint.number; //İpucu sayısı yani bölge alanı

        // w * h = N olacak şekilde tüm çarpanları deniyoruz mesela işte 6 ise 2 * 3 ya da 6 *1
        for (int rw = 1; rw <= N; rw++)
        {
            if (N % rw != 0) continue; //Tam böleni değilse sayı atlıyorsun
            int rh = N / rw; //Bu da sanırsam tam bölen sayısı işte

            //hintimiz (hx, hy) bu rectanguların içinde olmalı.
            // x koordinatı : hint.x - rw + 1 ile hint.x arasında olabilir.
            int minX = Mathf.Max(0, hint.x - rw + 1);
            int maxX = Mathf.Min(hint.x, gridW - rw);
            int minY = Mathf.Max(0, hint.y - rh + 1);
            int maxY = Mathf.Min(hint.y, gridH - rh);

            //Tüm olası sol - alt köşeleri dene ve listeye ekle.
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    list.Add(new RectShape { x = x, y = y, w = rw, h = rh });


        }

        return list;
    
    }

}
