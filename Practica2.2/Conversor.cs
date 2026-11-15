using System.Text;
using System.Xml;

namespace ConversorNetflix;

public static class ConversorService {
    // Convierte un archivo CSV en XML aplicando un filtro.
    public static void CsvAXmlManual(string csvPath, string xmlPath, string filtroTipo, string valorFiltroStr, int valorFiltroAnio) {
        try {
            Logger.LogInicio("CSV", "XML", csvPath, xmlPath);

            string[] lineas = File.ReadAllLines(csvPath, Encoding.UTF8);
            if (lineas.Length <= 1) return;   // Solo cabecera: nada que exportar

            // Indent genera un XML legible con saltos de línea y sangría.
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter writer = XmlWriter.Create(xmlPath, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("Peliculas");

            for (int i = 1; i < lineas.Length; i++) {
                // El parser respeta comillas, así que las comas internas no separan.
                string[] campos = CsvUtils.SepararCamposCsv(lineas[i]);
                if (campos.Length < 6) continue;   // Línea incompleta o corrupta

                // Orden del CSV: 0 Title, 1 Genre, 2 Premiere, 3 Runtime, 4 IMDB, 5 Language.
                string title = campos[0];
                string genre = campos[1];
                string premiere = campos[2];
                string runtime = campos[3];
                string imdb = campos[4];
                string language = campos[5];

                if (!CsvUtils.CumpleFiltro(genre, premiere, language, filtroTipo, valorFiltroStr, valorFiltroAnio))
                    continue;

                Logger.LogElemento("Title=" + title + ", Genre=" + genre + ", Premiere=" + premiere +
                    ", Runtime=" + runtime + ", IMDB_Score=" + imdb + ", Language=" + language);

                // XmlWriter escapa automáticamente &, < y >.
                writer.WriteStartElement("Pelicula");
                EscribirCampo(writer, "Title", title);
                EscribirCampo(writer, "Genre", genre);
                EscribirCampo(writer, "Premiere", premiere);
                EscribirCampo(writer, "Runtime", runtime);
                EscribirCampo(writer, "IMDB_Score", imdb);
                EscribirCampo(writer, "Language", language);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            Logger.LogFin(xmlPath);
            Console.WriteLine("\nArchivo XML guardado con éxito en: " + xmlPath);
        } catch (FileNotFoundException) {
            Logger.LogError("No se encontró el archivo CSV en " + csvPath);
        } catch (Exception ex) {
            Logger.LogError("No se ha podido exportar a XML: " + ex.Message);
        }
    }

    // Escribe un campo XML simple: <nombre>valor</nombre>.
    private static void EscribirCampo(XmlWriter writer, string nombre, string valor) {
        writer.WriteStartElement(nombre);
        writer.WriteString(valor);
        writer.WriteEndElement();
    }

    // Convierte un archivo XML en CSV aplicando un filtro.
    public static void XmlACsvConXmlDocument(string xmlPath, string csvPath, string filtroTipo, string valorFiltroStr, int valorFiltroAnio) {
        try {
            Logger.LogInicio("XML", "CSV", xmlPath, csvPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            List<string> lineasCsv = new List<string> {
                "Title,Genre,Premiere,Runtime,IMDB Score,Language"
            };

            XmlNode root = xmlDoc.DocumentElement;
            if (root != null) {
                foreach (XmlNode peliculaNode in root.ChildNodes) {
                    if (peliculaNode.Name != "Pelicula") continue;   // Ignora nodos de texto/espacios

                    // Se comprueba null por si el XML no tiene todos los campos.
                    string title = "";
                    if (peliculaNode["Title"] != null) {
                        title = peliculaNode["Title"].InnerText;
                    }

                    string genre = "";
                    if (peliculaNode["Genre"] != null) {
                        genre = peliculaNode["Genre"].InnerText;
                    }

                    string premiere = "";
                    if (peliculaNode["Premiere"] != null) {
                        premiere = peliculaNode["Premiere"].InnerText;
                    }

                    string runtime = "";
                    if (peliculaNode["Runtime"] != null) {
                        runtime = peliculaNode["Runtime"].InnerText;
                    }

                    string imdb = "";
                    if (peliculaNode["IMDB_Score"] != null) {
                        imdb = peliculaNode["IMDB_Score"].InnerText;
                    }

                    string language = "";
                    if (peliculaNode["Language"] != null) {
                        language = peliculaNode["Language"].InnerText;
                    }

                    if (!CsvUtils.CumpleFiltro(genre, premiere, language, filtroTipo, valorFiltroStr, valorFiltroAnio))
                        continue;

                    Logger.LogElemento("Title=" + title + ", Genre=" + genre + ", Premiere=" + premiere +
                        ", Runtime=" + runtime + ", IMDB_Score=" + imdb + ", Language=" + language);

                    // Los textos se entrecomillan; runtime e IMDB son numéricos.
                    string fila = CsvUtils.EscaparCsv(title) + "," + CsvUtils.EscaparCsv(genre) + "," +
                        CsvUtils.EscaparCsv(premiere) + "," + runtime + "," + imdb + "," + CsvUtils.EscaparCsv(language);
                    lineasCsv.Add(fila);
                }
            }

            File.WriteAllLines(csvPath, lineasCsv, Encoding.UTF8);

            Logger.LogFin(csvPath);
            Console.WriteLine("\nArchivo CSV guardado con éxito en: " + csvPath);
        } catch (FileNotFoundException) {
            Logger.LogError("No se encontró el archivo XML en " + xmlPath);
        } catch (XmlException ex) {
            // XML mal formado: etiquetas sin cerrar, caracteres inválidos, etc.
            Logger.LogError("sintaxis no válida en el archivo XML: " + ex.Message);
        } catch (Exception ex) {
            Logger.LogError("no se ha podido exportar a CSV: " + ex.Message);
        }
    }
}
