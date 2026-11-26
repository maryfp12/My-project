public class NodoMensaje
{
    public string Texto { get; }
    public bool EsPositivo { get; }
    public string Consejo { get; }
    public string Usuario { get; }
    public bool Acertado { get; set; }
    public NodoMensaje Siguiente { get; set; }

    public NodoMensaje(string texto, bool esPositivo, string consejo, string usuario)
    {
        Texto = texto;
        EsPositivo = esPositivo;
        Consejo = consejo;
        Usuario = usuario;
    }
}
