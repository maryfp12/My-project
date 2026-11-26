using System.IO;
using UnityEngine;

public static class ArchivoMensajes
{
    public const string NombreArchivo = "mensajes.csv";

    public static string Ruta => Path.Combine(Application.streamingAssetsPath, NombreArchivo);

    public static ListaMensajes Cargar()
    {
        if (!File.Exists(Ruta))
        {
            BancoMensajes.GuardarCSV(Ruta);
            Debug.Log($"No existía {NombreArchivo}; se creó con los mensajes de ejemplo en {Ruta}");
        }

        var lista = new ListaMensajes();
        var filas = ArchivoCSV.Leer(Ruta);
        int inicio = filas.Count > 0 && (filas[0].Length < 2 || LeerTipo(filas[0][1]) == null) ? 1 : 0;

        for (int i = inicio; i < filas.Count; i++)
        {
            var fila = filas[i];
            if (fila.Length < 2 || string.IsNullOrEmpty(fila[0])) continue;

            bool? positivo = LeerTipo(fila[1]);
            if (positivo == null)
            {
                Debug.LogWarning($"{NombreArchivo}, línea {i + 1}: el tipo \"{fila[1]}\" no es positivo ni negativo; se omitió.");
                continue;
            }

            string consejo = fila.Length > 2 ? fila[2] : "";
            string usuario = fila.Length > 3 && !string.IsNullOrEmpty(fila[3]) ? Arroba(fila[3]) : BancoMensajes.UsuarioAlAzar();
            lista.AgregarAlFinal(new NodoMensaje(fila[0], positivo.Value, consejo, usuario));
        }

        if (!lista.EstaVacia) return lista;
        Debug.LogWarning($"{NombreArchivo} no tiene mensajes válidos; se usan los de ejemplo.");
        return BancoMensajes.Crear();
    }

    static bool? LeerTipo(string valor)
    {
        switch (valor.Trim().ToLowerInvariant())
        {
            case "positivo":
            case "p":
            case "+":
            case "1":
            case "true":
                return true;
            case "negativo":
            case "n":
            case "-":
            case "0":
            case "false":
                return false;
            default:
                return null;
        }
    }

    static string Arroba(string usuario) => usuario.StartsWith("@") ? usuario : "@" + usuario;
}
