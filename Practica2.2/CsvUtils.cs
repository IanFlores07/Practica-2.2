using System.Text;
using System.Xml;

namespace ConversorNetflix;

public static class CsvUtils {
    // Devuelve los valores distintos de una columna del CSV.
    public static List<string> ObtenerValoresUnicos(string csvPath, int columnaIdx) {
        List<string> valores = new List<string>();
        string[] lineas = File.ReadAllLines(csvPath, Encoding.UTF8);

        for (int i = 1; i < lineas.Length; i++) {   // Empieza en 1 para saltar la cabecera
            string[] campos = SepararCamposCsv(lineas[i]);
            if (campos.Length > columnaIdx) {
                string valor = campos[columnaIdx].Trim();
                if (valor != "" && !valores.Contains(valor)) {   // Evita vacíos y duplicados
                    valores.Add(valor);
                }
            }
        }

        valores.Sort();
        return valores;
    }

    // Devuelve los valores distintos de un XPath del XML.
    public static List<string> ObtenerValoresUnicosXml(string xmlPath, string xpath) {
        List<string> valores = new List<string>();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);

        XmlNodeList nodos = xmlDoc.SelectNodes(xpath);
        foreach (XmlNode nodo in nodos) {
            string valor = nodo.InnerText.Trim();
            if (valor != "" && !valores.Contains(valor)) {
                valores.Add(valor);
            }
        }

        valores.Sort();
        return valores;
    }

    // Extrae el año de una fecha en texto.
    public static int ExtraerAnio(string fechaStr) {
        // "August 5, 2019" -> ["August", "5", "2019"]
        string[] partes = fechaStr.Replace(",", "").Split(' ');

        foreach (string parte in partes) {
            int anio;
            // Longitud 4 evita confundir el día o el mes con el año.
            if (int.TryParse(parte, out anio) && parte.Length == 4) {
                return anio;
            }
        }
        return 0;
    }

    // Comprueba si una película pasa el filtro activo.
    public static bool CumpleFiltro(string genre, string premiere, string language, string filtroTipo, string valorFiltroStr, int valorFiltroAnio) {
        if (filtroTipo == "genero")
            return genre.Trim().ToLower() == valorFiltroStr.ToLower();

        if (filtroTipo == "lenguaje")
            return language.Trim().ToLower() == valorFiltroStr.ToLower();

        if (filtroTipo == "anio")
            return ExtraerAnio(premiere) >= valorFiltroAnio;

        return true;   // filtroTipo == null: exportar todo
    }

    // Separa una línea CSV respetando comillas y comas internas.
    public static string[] SepararCamposCsv(string linea) {
        // Parser manual: Split(',') fallaría con comas dentro de comillas.
        List<string> campos = new List<string>();
        bool dentroDeComillas = false;
        string campoActual = "";

        for (int i = 0; i < linea.Length; i++) {
            char c = linea[i];

            if (c == '"') {
                // Dos comillas seguidas dentro de un campo equivalen a una comilla literal.
                if (dentroDeComillas && i + 1 < linea.Length && linea[i + 1] == '"') {
                    campoActual += '"';
                    i++;
                } else {
                    dentroDeComillas = !dentroDeComillas;
                }
            } else if (c == ',' && !dentroDeComillas) {
                campos.Add(campoActual);
                campoActual = "";
            } else {
                campoActual += c;
            }
        }

        campos.Add(campoActual);   // Último campo, sin coma final
        return campos.ToArray();
    }

    // Prepara un texto para escribirlo correctamente en CSV.
    public static string EscaparCsv(string valor) {
        // Inverso del parser: entrecomilla y duplica las comillas internas.
        return "\"" + valor.Replace("\"", "\"\"") + "\"";
    }
}
