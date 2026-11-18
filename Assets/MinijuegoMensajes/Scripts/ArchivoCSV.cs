using System.Collections.Generic;
using System.IO;
using System.Text;

public static class ArchivoCSV
{
    public const char Separador = ';';

    static readonly Encoding Utf8ConBom = new UTF8Encoding(true);

    public static List<string[]> Leer(string ruta)
    {
        var filas = new List<string[]>();
        if (!File.Exists(ruta)) return filas;

        var lineas = File.ReadAllLines(ruta, Encoding.UTF8);
        char separador = Separador;
        bool primera = true;
        foreach (var linea in lineas)
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;
            if (primera)
            {
                separador = DetectarSeparador(linea);
                primera = false;
            }
            filas.Add(DividirLinea(linea, separador));
        }
        return filas;
    }

    public static void Escribir(string ruta, IEnumerable<string[]> filas)
    {
        CrearCarpeta(ruta);
        var texto = new StringBuilder();
        foreach (var fila in filas) texto.AppendLine(UnirLinea(fila));
        File.WriteAllText(ruta, texto.ToString(), Utf8ConBom);
    }

    public static void AgregarFila(string ruta, string[] encabezado, string[] fila)
    {
        CrearCarpeta(ruta);
        bool nuevo = !File.Exists(ruta) || new FileInfo(ruta).Length == 0;
        using (var escritor = new StreamWriter(ruta, true, Utf8ConBom))
        {
            if (nuevo) escritor.WriteLine(UnirLinea(encabezado));
            escritor.WriteLine(UnirLinea(fila));
        }
    }

    static char DetectarSeparador(string linea)
    {
        int puntoYComa = 0, coma = 0;
        foreach (char c in linea)
        {
            if (c == ';') puntoYComa++;
            else if (c == ',') coma++;
        }
        return coma > puntoYComa ? ',' : Separador;
    }

    static string[] DividirLinea(string linea, char separador)
    {
        var campos = new List<string>();
        var actual = new StringBuilder();
        bool entreComillas = false;

        for (int i = 0; i < linea.Length; i++)
        {
            char c = linea[i];
            if (entreComillas)
            {
                if (c != '"') actual.Append(c);
                else if (i + 1 < linea.Length && linea[i + 1] == '"')
                {
                    actual.Append('"');
                    i++;
                }
                else entreComillas = false;
            }
            else if (c == '"') entreComillas = true;
            else if (c == separador)
            {
                campos.Add(actual.ToString().Trim());
                actual.Clear();
            }
            else actual.Append(c);
        }
        campos.Add(actual.ToString().Trim());
        return campos.ToArray();
    }

    static string UnirLinea(string[] campos)
    {
        var partes = new string[campos.Length];
        for (int i = 0; i < campos.Length; i++) partes[i] = Escapar(campos[i]);
        return string.Join(Separador.ToString(), partes);
    }

    static string Escapar(string campo)
    {
        if (string.IsNullOrEmpty(campo)) return "";
        if (campo.IndexOfAny(new[] { Separador, '"', '\n', '\r' }) < 0) return campo;
        return "\"" + campo.Replace("\"", "\"\"") + "\"";
    }

    static void CrearCarpeta(string ruta)
    {
        var carpeta = Path.GetDirectoryName(ruta);
        if (!string.IsNullOrEmpty(carpeta)) Directory.CreateDirectory(carpeta);
    }
}
