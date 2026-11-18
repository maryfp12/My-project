using System.Collections.Generic;
using UnityEngine;

public static class BancoMensajes
{
    static readonly string[] Usuarios =
    {
        "valen_22", "juanes.play", "sofi.art", "dani_gamer", "lau.lectora", "tomi_fut",
        "mafe.music", "sebas.dev", "nati.foto", "andres_99", "isa.vlogs", "caro.skate"
    };

    static readonly (string texto, bool positivo, string consejo)[] Mensajes =
    {
        ("¡Qué buena tu presentación de hoy! Se nota que te preparaste mucho.", true,
            "Reconocer el esfuerzo de los demás hace las redes más amables."),
        ("Busqué esa noticia en dos medios confiables y era falsa. Mejor no la reenvíen.", true,
            "Verificar antes de compartir frena la desinformación."),
        ("No estoy de acuerdo contigo, pero respeto tu opinión. ¿Lo hablamos con calma?", true,
            "Se puede pensar distinto sin atacar a nadie."),
        ("Le pregunté a mi amiga si podía subir la foto donde sale ella.", true,
            "Pedir permiso antes de publicar a otra persona respeta su privacidad."),
        ("Hoy dejé el celular un rato y salí a montar bici.", true,
            "Equilibrar el tiempo en pantalla también es usar bien las redes."),
        ("Si alguien te está molestando por chat, cuéntale a un adulto de confianza. Cuenta conmigo.", true,
            "Apoyar a quien sufre ciberacoso marca la diferencia."),
        ("Activé la verificación en dos pasos en mi cuenta. ¡Se las recomiendo!", true,
            "Proteger tus cuentas evita que otros las usen."),
        ("Gracias por explicarme el tema de matemáticas, ya lo entendí.", true,
            "Las redes también sirven para ayudarnos entre todos."),
        ("Reporté esa cuenta porque estaba insultando a varias personas.", true,
            "Reportar contenido ofensivo ayuda a que la red sea más segura."),
        ("¡Feliz cumpleaños! Que la pases muy bien con tu familia.", true,
            "Un mensaje amable puede alegrarle el día a alguien."),
        ("Puse mi perfil en privado para que solo me vean mis amigos.", true,
            "Controlar quién ve tu información es cuidarte."),
        ("Ese video tiene un dato raro. Miremos la fuente antes de creerle.", true,
            "Dudar y revisar la fuente es pensamiento crítico."),
        ("Me equivoqué con lo que dije ayer en el grupo. Perdón, no estuvo bien.", true,
            "Reconocer un error y disculparse también es convivencia digital."),
        ("Les comparto este curso gratis de programación por si a alguien le sirve.", true,
            "Compartir contenido útil le suma valor a la comunidad."),
        ("No le doy mi contraseña a nadie, ni a mis mejores amigos.", true,
            "Tu contraseña es solo tuya: así nadie puede suplantarte."),

        ("Jajaja miren lo feo que salió Camilo en esta foto, compártanla.", false,
            "Burlarse de alguien y difundir su foto es ciberacoso."),
        ("¡URGENTE! Reenvía esto a 10 personas o te va a pasar algo malo.", false,
            "Las cadenas usan el miedo para manipularte. Ignóralas."),
        ("¡Ganaste un celular! Solo escribe tu dirección y el número de la tarjeta de tus papás.", false,
            "Es una estafa: nunca compartas datos personales ni bancarios."),
        ("Nadie te quiere en el grupo, mejor sálete.", false,
            "Excluir y humillar a alguien también es ciberacoso."),
        ("Mi contraseña es mi fecha de nacimiento y se la paso a quien me la pida.", false,
            "Una contraseña fácil o compartida pone en riesgo tu cuenta."),
        ("Voy a publicar en vivo dónde estoy y a qué hora llego sola a mi casa.", false,
            "Compartir tu ubicación en tiempo real con todos es peligroso."),
        ("Si no haces lo que te digo, le cuento a todos tu secreto.", false,
            "Eso es chantaje. Bloquea, guarda pruebas y cuéntale a un adulto."),
        ("Solo leí el título, pero ya lo compartí. Seguro es verdad.", false,
            "Compartir sin leer ni verificar riega noticias falsas."),
        ("Hagamos una cuenta falsa del profe para burlarnos de él.", false,
            "Suplantar a alguien es grave y puede tener consecuencias legales."),
        ("Llevo 9 horas seguidas en el celular, no he comido, pero no puedo parar.", false,
            "Si las redes te quitan comida o sueño, es momento de hacer una pausa."),
        ("Ese comentario tuyo es de lo más tonto, das pena.", false,
            "Insultar en redes duele igual que en persona."),
        ("Un desconocido me pidió que no le cuente a mis papás que hablamos.", false,
            "Si un desconocido te pide guardar secretos, cuéntale a un adulto de confianza."),
        ("Le tomé captura a su chat privado y lo voy a mandar al grupo del salón.", false,
            "Difundir conversaciones privadas rompe la confianza y la privacidad."),
        ("Descarga esta app de un enlace raro y te regala seguidores gratis.", false,
            "Los enlaces sospechosos pueden robar tu cuenta o tus datos."),
        ("Todos los de ese colegio son unos perdedores, ni los miren.", false,
            "Generalizar y discriminar a un grupo fomenta el odio en redes."),
    };

    public static string UsuarioAlAzar() => "@" + Usuarios[Random.Range(0, Usuarios.Length)];

    public static ListaMensajes Crear()
    {
        var lista = new ListaMensajes();
        foreach (var mensaje in Mensajes)
            lista.AgregarAlFinal(new NodoMensaje(mensaje.texto, mensaje.positivo, mensaje.consejo, UsuarioAlAzar()));
        return lista;
    }

    public static void GuardarCSV(string ruta)
    {
        var filas = new List<string[]> { new[] { "texto", "tipo", "consejo", "usuario" } };
        foreach (var mensaje in Mensajes)
            filas.Add(new[] { mensaje.texto, mensaje.positivo ? "positivo" : "negativo", mensaje.consejo, UsuarioAlAzar() });
        ArchivoCSV.Escribir(ruta, filas);
    }
}
