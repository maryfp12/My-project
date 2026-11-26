using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class RegistroPartidas
{
    public const string NombreArchivo = "partidas.csv";

    const int ColumnaAciertos = 2;

    static readonly string[] Encabezado =
    {
        "fecha", "resultado", "aciertos", "errores", "vencidos", "score_final", "segundos"
    };

    public static string Carpeta
    {
        get
        {
#if UNITY_EDITOR
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Datos");
#else
            return Application.persistentDataPath;
#endif
        }
    }

    public static string Ruta => Path.Combine(Carpeta, NombreArchivo);

    public static bool Guardar(bool gano, int aciertos, int errores, int vencidos, int score, float segundos)
    {
        var fila = new[]
        {
            DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            gano ? "ganó" : "perdió",
            aciertos.ToString(CultureInfo.InvariantCulture),
            errores.ToString(CultureInfo.InvariantCulture),
            vencidos.ToString(CultureInfo.InvariantCulture),
            score.ToString(CultureInfo.InvariantCulture),
            Mathf.RoundToInt(segundos).ToString(CultureInfo.InvariantCulture)
        };

        try
        {
            ArchivoCSV.AgregarFila(Ruta, Encabezado, fila);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"No se pudo guardar {NombreArchivo}: {e.Message}");
            return false;
        }
    }

    public static int MejorAciertos()
    {
        int mejor = 0;
        var filas = LeerSeguro();
        for (int i = 1; i < filas.Count; i++)
        {
            if (filas[i].Length > ColumnaAciertos && int.TryParse(filas[i][ColumnaAciertos], out int valor) && valor > mejor)
                mejor = valor;
        }
        return mejor;
    }

    public static int CantidadPartidas() => Mathf.Max(0, LeerSeguro().Count - 1);

    public static void AbrirCarpeta()
    {
        Directory.CreateDirectory(Carpeta);
        Application.OpenURL("file:///" + Carpeta.Replace('\\', '/'));
    }

    static List<string[]> LeerSeguro()
    {
        try
        {
            return ArchivoCSV.Leer(Ruta);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"No se pudo leer {NombreArchivo}: {e.Message}");
            return new List<string[]>();
        }
    }
}
