using System;
using UnityEngine;

namespace CNRMods
{
    // Runtime reconstruction of the two vanilla SingleMode pickup visuals.
    // The mesh and textures below are the game's original PropBulletPreb / PropBloodPref assets,
    // embedded so multiplayer maps do not need to load the SingleMode sharedassets first.
    internal static class CNRVanillaPickupVisuals
    {
        private static Mesh _ammoMesh;
        private static Texture2D _ammoTexture;
        private static Texture2D _healthTexture;
        private static Material _ammoMaterial;
        private static Material _healthMaterial;

        private const string AmmoVertices = "OuUAPk3cKz5N3Ks9OuUAvk3cKz5N3Ks9OuUAvk3cKz5N3Cs+OuUAPk3cKz5N3Cs+OuUAPk3cKz5N3Cu+OuUAvk3cKz5N3Cu+OuUAvk3cKz5N3Ku9OuUAPk3cKz5N3Ku9OuUAPk3cKz5N3Ks+OuUAvk3cKz5N3Ks+OuUAvk3cKz7XV8E+OuUAPk3cKz7XV8E+OuUAPk3cKz7XV8G+OuUAvk3cKz7XV8G+OuUAvk3cKz5N3Ku+OuUAPk3cKz5N3Ku+OuUAPk3cKz5N3Cs+TdyrPU3cKz5N3Cs+TdyrPU3cKz5N3Ks+OuUAPk3cKz5N3Ks+TdyrvU3cKz5N3Cs+OuUAvk3cKz5N3Cs+OuUAvk3cKz5N3Ks+TdyrvU3cKz5N3Ks+TdyrPTrlgD7DYJY+TdyrvTrlgD7DYJY+TdyrvTrlgD5N3Ks+TdyrPTrlgD5N3Ks+TdwrPcNglj5g01Y+TdwrvcNglj5g01Y+TdwrvcNglj7DYJY+TdwrPcNglj7DYJY+TdyrPTrlgD5N3Cs+TdyrvTrlgD5N3Cs+TdyrvTrlgD5g01Y+TdyrPTrlgD5g01Y+OuUAPk3cKz5N3Ku9TdyrPU3cKz5N3Ku9TdyrPU3cKz5N3Ks9OuUAPk3cKz5N3Ks9TdyrvU3cKz5N3Ku9OuUAvk3cKz5N3Ku9OuUAvk3cKz5N3Ks9TdyrvU3cKz5N3Ks9TdyrPTrlgD5N3Cs9TdyrvTrlgD5N3Cs9TdyrvTrlgD5N3Ks9TdyrPTrlgD5N3Ks9TdwrPcNglj5N3Cu9TdwrvcNglj5N3Cu9TdwrvcNglj5N3Cs9TdwrPcNglj5N3Cs9TdyrPTrlgD5N3Ku9TdyrvTrlgD5N3Ku9TdyrvTrlgD5N3Cu9TdyrPTrlgD5N3Cu9OuUAPk3cKz5N3Ku+TdyrPU3cKz5N3Ku+TdyrPU3cKz5N3Cu+OuUAPk3cKz5N3Cu+TdyrvU3cKz5N3Ku+OuUAvk3cKz5N3Ku+OuUAvk3cKz5N3Cu+TdyrvU3cKz5N3Cu+TdyrPTrlgD5g01a+TdyrvTrlgD5g01a+TdyrvTrlgD5N3Cu+TdyrPTrlgD5N3Cu+TdwrPcNglj7DYJa+TdwrvcNglj7DYJa+TdwrvcNglj5g01a+TdwrPcNglj5g01a+TdyrPTrlgD5N3Ku+TdyrvTrlgD5N3Ku+TdyrvTrlgD7DYJa+TdyrPTrlgD7DYJa+TdyrPTrlgD5g01Y+TdwrPTrlgD5g01Y+TdwrPTrlgD7DYJY+TdyrPTrlgD7DYJY+TdwrvTrlgD5g01Y+TdyrvTrlgD5g01Y+TdyrvTrlgD7DYJY+TdwrvTrlgD7DYJY+TdyrPTrlgD5N3Cu9TdwrPTrlgD5N3Cu9TdwrPTrlgD5N3Cs9TdyrPTrlgD5N3Cs9TdwrvTrlgD5N3Cu9TdyrvTrlgD5N3Cu9TdyrvTrlgD5N3Cs9TdwrvTrlgD5N3Cs9TdyrPTrlgD7DYJa+TdwrPTrlgD7DYJa+TdwrPTrlgD5g01a+TdyrPTrlgD5g01a+TdwrvTrlgD7DYJa+TdyrvTrlgD7DYJa+TdyrvTrlgD5g01a+TdwrvTrlgD5g01a+OuUAPsNglr7XV8E+OuUAvsNglr7XV8E+OuUAvsNglr7XV8G+OuUAPsNglr7XV8G+OuUAPk3cKz7XV8G+OuUAPk3cKz7XV8E+OuUAPsNglr7XV8E+OuUAPsNglr7XV8G+TdyrPTrlgD5N3Cs+TdyrPTrlgD5N3Ks+TdyrPU3cKz5N3Ks+TdyrPU3cKz5N3Cs+TdyrPTrlgD5N3Ku9TdyrPTrlgD5N3Ks9TdyrPU3cKz5N3Ks9TdyrPU3cKz5N3Ku9TdyrPTrlgD5N3Ku+TdyrPTrlgD5N3Cu+TdyrPU3cKz5N3Cu+TdyrPU3cKz5N3Ku+TdwrPcNglj5g01Y+TdwrPcNglj7DYJY+TdwrPTrlgD7DYJY+TdwrPTrlgD5g01Y+TdwrPcNglj5N3Cu9TdwrPcNglj5N3Cs9TdwrPTrlgD5N3Cs9TdwrPTrlgD5N3Cu9TdwrPcNglj7DYJa+TdwrPcNglj5g01a+TdwrPTrlgD5g01a+TdwrPTrlgD7DYJa+OuUAvsNglr7XV8G+OuUAvsNglr7XV8E+OuUAvk3cKz7XV8E+OuUAvk3cKz7XV8G+TdyrvU3cKz5N3Cs+TdyrvU3cKz5N3Ks+TdyrvTrlgD5N3Ks+TdyrvTrlgD5N3Cs+TdyrvU3cKz5N3Ku9TdyrvU3cKz5N3Ks9TdyrvTrlgD5N3Ks9TdyrvTrlgD5N3Ku9TdyrvU3cKz5N3Ku+TdyrvU3cKz5N3Cu+TdyrvTrlgD5N3Cu+TdyrvTrlgD5N3Ku+TdwrvTrlgD5g01Y+TdwrvTrlgD7DYJY+TdwrvcNglj7DYJY+TdwrvcNglj5g01Y+TdwrvTrlgD5N3Cu9TdwrvTrlgD5N3Cs9TdwrvcNglj5N3Cs9TdwrvcNglj5N3Cu9TdwrvTrlgD7DYJa+TdwrvTrlgD5g01a+TdwrvcNglj5g01a+TdwrvcNglj7DYJa+OuUAPsNglr7XV8G+OuUAvsNglr7XV8G+OuUAvk3cKz7XV8G+OuUAPk3cKz7XV8G+TdyrPU3cKz5N3Cs+TdyrvU3cKz5N3Cs+TdyrvTrlgD5N3Cs+TdyrPTrlgD5N3Cs+TdyrPU3cKz5N3Ku9TdyrvU3cKz5N3Ku9TdyrvTrlgD5N3Ku9TdyrPTrlgD5N3Ku9TdyrPU3cKz5N3Ku+TdyrvU3cKz5N3Ku+TdyrvTrlgD5N3Ku+TdyrPTrlgD5N3Ku+TdwrPTrlgD5g01Y+TdwrvTrlgD5g01Y+TdwrvcNglj5g01Y+TdwrPcNglj5g01Y+TdwrPTrlgD5N3Cu9TdwrvTrlgD5N3Cu9TdwrvcNglj5N3Cu9TdwrPcNglj5N3Cu9TdwrPTrlgD7DYJa+TdwrvTrlgD7DYJa+TdwrvcNglj7DYJa+TdwrPcNglj7DYJa+OuUAPk3cKz7XV8E+OuUAvk3cKz7XV8E+OuUAvsNglr7XV8E+OuUAPsNglr7XV8E+TdyrPTrlgD5N3Ks+TdyrvTrlgD5N3Ks+TdyrvU3cKz5N3Ks+TdyrPU3cKz5N3Ks+TdyrPTrlgD5N3Ks9TdyrvTrlgD5N3Ks9TdyrvU3cKz5N3Ks9TdyrPU3cKz5N3Ks9TdyrPTrlgD5N3Cu+TdyrvTrlgD5N3Cu+TdyrvU3cKz5N3Cu+TdyrPU3cKz5N3Cu+TdwrPcNglj7DYJY+TdwrvcNglj7DYJY+TdwrvTrlgD7DYJY+TdwrPTrlgD7DYJY+TdwrPcNglj5N3Cs9TdwrvcNglj5N3Cs9TdwrvTrlgD5N3Cs9TdwrPTrlgD5N3Cs9TdwrPcNglj5g01a+TdwrvcNglj5g01a+TdwrvTrlgD5g01a+TdwrPTrlgD5g01a+";
        private const string AmmoUvs = "AAAMPwAA4D4AACQ/AADgPgAAJD8AAAA/AAAMPwAAAD8AANg+AADgPgAABD8AAOA+AAAEPwAAAD8AANg+AAAAPwAA2D4AAEg/AAAEPwAASD8AAAQ/AABQPwAA2D4AAFA/AACYPgAASD8AAMg+AABIPwAAyD4AAFA/AACYPgAAUD8AABA+AADgPgAAID4AAOA+AAAgPgAAED8AABA+AAAQPwAAcD4AAOA+AACAPgAA4D4AAIA+AAAQPwAAcD4AABA/AABUPwAASD8AAGQ/AABIPwAAZD8AAFA/AABUPwAAUD8AAGw/AAAoPwAAdD8AACg/AAB0PwAAOD8AAGw/AAA4PwAAPD8AAEg/AABMPwAASD8AAEw/AABQPwAAPD8AAFA/AACQPgAA4D4AAJg+AADgPgAAmD4AABA/AACQPgAAED8AAKg+AADgPgAAsD4AAOA+AACwPgAAED8AAKg+AAAQPwAAgDwAAGg/AACgPQAAaD8AAKA9AABwPwAAgDwAAHA/AABcPwAAKD8AAGQ/AAAoPwAAZD8AADg/AABcPwAAOD8AAAw/AABIPwAAHD8AAEg/AAAcPwAAUD8AAAw/AABQPwAAwD4AAOA+AADIPgAA4D4AAMg+AAAQPwAAwD4AABA/AABAPgAA4D4AAFA+AADgPgAAUD4AABA/AABAPgAAED8AACQ/AABIPwAAND8AAEg/AAA0PwAAUD8AACQ/AABQPwAAdD8AAOA+AAB8PwAA4D4AAHw/AAAAPwAAdD8AAAA/AABsPwAASD8AAHw/AABIPwAAfD8AAFA/AABsPwAAUD8AAOA9AABIPwAAAD4AAEg/AAAAPgAAWD8AAOA9AABYPwAAgD0AAEg/AACgPQAASD8AAKA9AABYPwAAgD0AAFg/AACAPAAASD8AAAA9AABIPwAAAD0AAFg/AACAPAAAWD8AACA+AABIPwAAMD4AAEg/AAAwPgAAWD8AACA+AABYPwAAgD4AAEg/AACIPgAASD8AAIg+AABYPwAAgD4AAFg/AABQPgAASD8AAGA+AABIPwAAYD4AAFg/AABQPgAAWD8AAIA8AAAAPQAA4D0AAAA9AADgPQAAGD8AAIA8AAAYPwAA6D4AAAA9AAA8PwAAAD0AADw/AADAPgAA6D4AAMA+AADgPQAAKD8AADA+AAAoPwAAMD4AADg/AADgPQAAOD8AACw/AADgPgAAPD8AAOA+AAA8PwAAAD8AACw/AAAAPwAAUD4AACg/AACIPgAAKD8AAIg+AAA4PwAAUD4AADg/AAD4PgAAaD8AAAQ/AABoPwAABD8AAHA/AAD4PgAAcD8AAEw/AABoPwAAVD8AAGg/AABUPwAAcD8AAEw/AABwPwAAPD8AAGg/AABEPwAAaD8AAEQ/AABwPwAAPD8AAHA/AAAQPgAAAD0AANg+AAAAPQAA2D4AAMA+AAAQPgAAwD4AAJg+AAAoPwAAuD4AACg/AAC4PgAAOD8AAJg+AAA4PwAALD8AACg/AAA8PwAAKD8AADw/AAA4PwAALD8AADg/AABEPwAAKD8AAFQ/AAAoPwAAVD8AADg/AABEPwAAOD8AAHA+AABoPwAAiD4AAGg/AACIPgAAcD8AAHA+AABwPwAAMD4AAGg/AABQPgAAaD8AAFA+AABwPwAAMD4AAHA/AADgPQAAaD8AABA+AABoPwAAED4AAHA/AADgPQAAcD8AAGQ/AAAAPQAAfD8AAAA9AAB8PwAAwD4AAGQ/AADAPgAA+D4AACg/AAAMPwAAKD8AAAw/AAA4PwAA+D4AADg/AAAUPwAAKD8AACQ/AAAoPwAAJD8AADg/AAAUPwAAOD8AAMg+AAAoPwAA6D4AACg/AADoPgAAOD8AAMg+AAA4PwAA2D4AAGg/AADoPgAAaD8AAOg+AABwPwAA2D4AAHA/AAC4PgAAaD8AAMg+AABoPwAAyD4AAHA/AAC4PgAAcD8AAJg+AABoPwAAqD4AAGg/AACoPgAAcD8AAJg+AABwPwAARD8AAAA9AABcPwAAAD0AAFw/AADAPgAARD8AAMA+AABcPwAA4D4AAGw/AADgPgAAbD8AAAA/AABcPwAAAD8AAEQ/AADgPgAAVD8AAOA+AABUPwAAAD8AAEQ/AAAAPwAAgDwAACg/AACgPQAAKD8AAKA9AAA4PwAAgDwAADg/AAAsPwAAaD8AADQ/AABoPwAAND8AAHA/AAAsPwAAcD8AAAw/AABoPwAAFD8AAGg/AAAUPwAAcD8AAAw/AABwPwAAHD8AAGg/AAAkPwAAaD8AACQ/AABwPwAAHD8AAHA/";
        private const string AmmoTriangles = "AAABAAIAAAACAAMABAAFAAYABAAGAAcACAAJAAoACAAKAAsADAANAA4ADAAOAA8AEAARABIAEAASABMAFAAVABYAFAAWABcAGAAZABoAGAAaABsAHAAdAB4AHAAeAB8AIAAhACIAIAAiACMAJAAlACYAJAAmACcAKAApACoAKAAqACsALAAtAC4ALAAuAC8AMAAxADIAMAAyADMANAA1ADYANAA2ADcAOAA5ADoAOAA6ADsAPAA9AD4APAA+AD8AQABBAEIAQABCAEMARABFAEYARABGAEcASABJAEoASABKAEsATABNAE4ATABOAE8AUABRAFIAUABSAFMAVABVAFYAVABWAFcAWABZAFoAWABaAFsAXABdAF4AXABeAF8AYABhAGIAYABiAGMAZABlAGYAZABmAGcAaABpAGoAaABqAGsAbABtAG4AbABuAG8AcABxAHIAcAByAHMAdAB1AHYAdAB2AHcAeAB5AHoAeAB6AHsAfAB9AH4AfAB+AH8AgACBAIIAgACCAIMAhACFAIYAhACGAIcAiACJAIoAiACKAIsAjACNAI4AjACOAI8AkACRAJIAkACSAJMAlACVAJYAlACWAJcAmACZAJoAmACaAJsAnACdAJ4AnACeAJ8AoAChAKIAoACiAKMApAClAKYApACmAKcAqACpAKoAqACqAKsArACtAK4ArACuAK8AsACxALIAsACyALMAtAC1ALYAtAC2ALcAuAC5ALoAuAC6ALsAvAC9AL4AvAC+AL8AwADBAMIAwADCAMMAxADFAMYAxADGAMcAyADJAMoAyADKAMsAzADNAM4AzADOAM8A0ADRANIA0ADSANMA1ADVANYA1ADWANcA";
        private const string AmmoTexturePng = "iVBORw0KGgoAAAANSUhEUgAAAEAAAAAgCAIAAAAt/+nTAAAA6ElEQVR4nN2WsQrCMBCG/xwZfACHzt18CUeXPo5rQYSuPk4XR/c66tbHEEepBRFCQkKakj/f1PwHuYbLfxc13Acwo6ptBWC8TYt6jwzpr70jKiBHQI56PaYrFMrz7SorwQHqr1tm5/wrM259WUVAjoAcZWujuSm29kpfAQE5gjI8EDrA80HbArtNQzHgBOToZbdrDs1qE7qQCgjIEZTaRm231kdJx2jkKr0CMbTn4++7O108lRmbHnyAmJ9IQWvk0msmS1EBATkCcgTkCMjR7rBpIB8lHZ2Ri76NqnSDLOYZMno/T+g98AF2jYf6kSEd3gAAAABJRU5ErkJggg==";
        private const string HealthTexturePng = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAAPElEQVR4nGP8//8/AymAiSTVDDg18FqBEC1tGEgNjNBgxeFFFPD5GCU2oAGIhWAjB38oMZGqAYenqWgDAJLVEJ88m3gsAAAAAElFTkSuQmCC";

        public static GameObject CreateAmmoPack(string objectName, Vector3 groundPosition)
        {
            GameObject go = new GameObject(objectName);
            go.transform.position = groundPosition + new Vector3(0f, 0.30f, 0f);
            go.transform.rotation = Quaternion.identity;

            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = GetAmmoMesh();
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = GetAmmoMaterial();
            return go;
        }

        public static GameObject CreateHealthPack(string objectName, Vector3 groundPosition)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = objectName;
            go.transform.position = groundPosition + new Vector3(0f, 0.30f, 0f);
            go.transform.localScale = new Vector3(0.50f, 0.50f, 0.50f);
            go.transform.rotation = Quaternion.identity;

            Collider collider = go.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.Destroy(collider);
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = GetHealthMaterial();
            return go;
        }

        private static Mesh GetAmmoMesh()
        {
            if (_ammoMesh != null) return _ammoMesh;

            float[] vf = DecodeFloats(AmmoVertices);
            float[] uf = DecodeFloats(AmmoUvs);
            int[] triangles = DecodeUInt16(AmmoTriangles);
            int vertexCount = vf.Length / 3;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i] = new Vector3(vf[i * 3], vf[i * 3 + 1], vf[i * 3 + 2]);
                uvs[i] = new Vector2(uf[i * 2], uf[i * 2 + 1]);
            }

            Mesh mesh = new Mesh();
            mesh.name = "danjia";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _ammoMesh = mesh;
            return _ammoMesh;
        }

        private static Material GetAmmoMaterial()
        {
            if (_ammoMaterial != null) return _ammoMaterial;
            _ammoMaterial = MakeMaterial(GetAmmoTexture(), new Color(0.8f, 0.8f, 0.8f, 1f));
            _ammoMaterial.name = "danjia-tex";
            return _ammoMaterial;
        }

        private static Material GetHealthMaterial()
        {
            if (_healthMaterial != null) return _healthMaterial;
            _healthMaterial = MakeMaterial(GetHealthTexture(), Color.white);
            _healthMaterial.name = "bloodpak";
            return _healthMaterial;
        }

        private static Material MakeMaterial(Texture2D texture, Color color)
        {
            Shader shader = Shader.Find("Diffuse");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            Material material = new Material(shader);
            material.mainTexture = texture;
            material.color = color;
            return material;
        }

        private static Texture2D GetAmmoTexture()
        {
            if (_ammoTexture == null)
            {
                _ammoTexture = DecodeTexture(AmmoTexturePng, "danjia-tex");
            }
            return _ammoTexture;
        }

        private static Texture2D GetHealthTexture()
        {
            if (_healthTexture == null)
            {
                _healthTexture = DecodeTexture(HealthTexturePng, "xuebao");
            }
            return _healthTexture;
        }

        private static Texture2D DecodeTexture(string base64, string name)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture.name = name;
            texture.LoadImage(bytes);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply(false, false);
            return texture;
        }

        private static float[] DecodeFloats(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            float[] values = new float[bytes.Length / 4];
            for (int i = 0; i < values.Length; i++) values[i] = BitConverter.ToSingle(bytes, i * 4);
            return values;
        }

        private static int[] DecodeUInt16(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            int[] values = new int[bytes.Length / 2];
            for (int i = 0; i < values.Length; i++) values[i] = BitConverter.ToUInt16(bytes, i * 2);
            return values;
        }
    }
}
