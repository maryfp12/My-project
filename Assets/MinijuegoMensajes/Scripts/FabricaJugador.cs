using UnityEngine;

public static class FabricaJugador
{
    public static JugadorMovimiento CrearNino(Vector3 posicion, Quaternion rotacion, float altura, GameObject personaje, Texture2D textura, Transform camara, out PoseNino pose)
    {
        var raiz = new GameObject("Jugador");
        raiz.tag = "Player";
        raiz.transform.SetPositionAndRotation(posicion, rotacion);
        var control = raiz.AddComponent<CharacterController>();
        control.height = altura;
        control.radius = 0.35f;
        control.center = new Vector3(0f, altura * 0.5f, 0f);
        control.stepOffset = 0.3f;
        var jugador = raiz.AddComponent<JugadorMovimiento>();

        var modelo = personaje != null ? personaje : Resources.Load<GameObject>("Nino/Walking");
        if (modelo != null)
        {
            var instancia = Object.Instantiate(modelo, raiz.transform);
            instancia.name = "Nino";
            foreach (var componente in instancia.GetComponentsInChildren<MonoBehaviour>()) componente.enabled = false;
            foreach (var otro in instancia.GetComponentsInChildren<CharacterController>()) otro.enabled = false;
            foreach (var animador in instancia.GetComponentsInChildren<Animator>()) animador.enabled = false;
            foreach (var camaraExtra in instancia.GetComponentsInChildren<Camera>()) camaraExtra.enabled = false;
            foreach (var oido in instancia.GetComponentsInChildren<AudioListener>()) oido.enabled = false;
            foreach (var colisionador in instancia.GetComponentsInChildren<Collider>()) colisionador.enabled = false;
            AplicarTextura(instancia, textura != null ? textura : personaje == null ? Resources.Load<Texture2D>("Nino/texture_diffuse") : null);

            pose = instancia.AddComponent<PoseNino>();
            pose.Preparar(altura);
            pose.ConfigurarCaminata(BuscarCaminata(modelo, personaje == null));
        }
        else
        {
            var cuerpo = ConstructorEscena.Bloque(PrimitiveType.Capsule, "Cuerpo", raiz.transform, new Vector3(0f, altura * 0.5f, 0f), new Vector3(0.7f, altura * 0.5f, 0.7f), Estilo.Azul);
            pose = cuerpo.AddComponent<PoseNino>();
        }

        jugador.Configurar(camara, pose);
        return jugador;
    }

    static void AplicarTextura(GameObject instancia, Texture2D textura)
    {
        if (textura == null) return;
        var material = ConstructorEscena.NuevoMaterial(Color.white, 0.12f);
        material.mainTexture = textura;
        foreach (var r in instancia.GetComponentsInChildren<Renderer>())
        {
            var materiales = r.sharedMaterials;
            for (int i = 0; i < materiales.Length; i++) materiales[i] = material;
            r.sharedMaterials = materiales;
        }
    }

    static AnimationClip BuscarCaminata(GameObject modelo, bool esNinoPorDefecto)
    {
        if (esNinoPorDefecto)
        {
            foreach (var clip in Resources.LoadAll<AnimationClip>("Nino/Walking"))
                if (!clip.name.StartsWith("__preview__")) return clip;
            return null;
        }

        var animador = modelo.GetComponentInChildren<Animator>();
        if (animador == null || animador.runtimeAnimatorController == null) return null;
        var clips = animador.runtimeAnimatorController.animationClips;
        return clips.Length > 0 ? clips[0] : null;
    }
}
