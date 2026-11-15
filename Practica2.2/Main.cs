using ConversorNetflix;

string rutaCsv = "NetflixOriginals.csv";
string rutaXml = "NetflixOriginals.xml";

Logger.Inicializar();

while (true) {
    Console.WriteLine("\n   CONVERSOR NETFLIX ORIGINALS\n");
    Console.WriteLine("Ruta CSV: " + rutaCsv);
    Console.WriteLine("Ruta XML: " + rutaXml + "\n");
    Console.WriteLine("1. Cambiar rutas de archivos (Path)");
    Console.WriteLine("2. Exportar de CSV a XML");
    Console.WriteLine("3. Exportar de XML a CSV");
    Console.WriteLine("4. Salir");
    Console.Write("Seleccione una opción: ");

    string opcion = Console.ReadLine().Trim();

    if (opcion == "1") {
        // Si se pulsa Enter sin escribir, se conserva la ruta anterior.
        Console.Write("Ingrese la nueva ruta para el CSV [" + rutaCsv + "]: ");
        string nuevaCsv = Console.ReadLine().Trim();
        if (nuevaCsv != "") rutaCsv = nuevaCsv;

        Console.Write("Ingrese la nueva ruta para el XML [" + rutaXml + "]: ");
        string nuevaXml = Console.ReadLine().Trim();
        if (nuevaXml != "") rutaXml = nuevaXml;

    } else if (opcion == "2") {
        if (!File.Exists(rutaCsv)) {
            Logger.LogError("No existe el archivo CSV en " + rutaCsv);
            continue;
        }
        MenuExportacion("CSV", "XML", rutaCsv, rutaXml);

    } else if (opcion == "3") {
        if (!File.Exists(rutaXml)) {
            Logger.LogError("No existe el archivo XML en " + rutaXml);
            continue;
        }
        MenuExportacion("XML", "CSV", rutaXml, rutaCsv);

    } else if (opcion == "4") {
        Console.WriteLine("Saliendo del programa...");
        break;

    } else {
        Console.WriteLine("Opción no válida.");
    }
}

// Muestra el submenú y lanza la exportación con el filtro elegido.
void MenuExportacion(string origenFormato, string destinoFormato, string rutaOrigen, string rutaDestino) {
    Console.WriteLine("\n    SUBMENÚ: Exportar de " + origenFormato + " a " + destinoFormato + "    ");
    Console.WriteLine("1. Exportar todo el documento");
    Console.WriteLine("2. Exportar por género");
    Console.WriteLine("3. Exportar por año (año seleccionado y superiores)");
    Console.WriteLine("4. Exportar por lenguaje");
    Console.WriteLine("5. Volver al menú principal");
    Console.Write("Seleccione una opción: ");

    string opcion = Console.ReadLine().Trim();

    // filtroTipo = null significa exportar todo.
    string filtroTipo = null;
    string valorFiltroStr = null;
    int valorFiltroAnio = 0;

    if (opcion == "1") {
        filtroTipo = null;

    } else if (opcion == "2") {
        filtroTipo = "genero";

        // Los valores se leen del archivo de origen, no de una lista fija.
        List<string> generos;
        if (origenFormato == "CSV") {
            generos = CsvUtils.ObtenerValoresUnicos(rutaOrigen, 1);
        } else {
            generos = CsvUtils.ObtenerValoresUnicosXml(rutaOrigen, "//Genre");
        }

        Console.WriteLine("\nGÉNEROS DISPONIBLES EN EL " + origenFormato + ":");
        foreach (string g in generos) {
            Console.WriteLine("  - " + g);
        }
        Console.Write("\nEscriba el género que desea exportar: ");
        valorFiltroStr = Console.ReadLine().Trim();

    } else if (opcion == "3") {
        filtroTipo = "anio";

        // TryParse valida sin lanzar excepción.
        Console.Write("\nIngrese el año límite inferior (ej. 2018): ");
        if (!int.TryParse(Console.ReadLine().Trim(), out valorFiltroAnio)) {
            Console.WriteLine("Año no válido. Cancelando exportación.");
            return;
        }

    } else if (opcion == "4") {
        filtroTipo = "lenguaje";

        List<string> idiomas;
        if (origenFormato == "CSV") {
            idiomas = CsvUtils.ObtenerValoresUnicos(rutaOrigen, 5);
        } else {
            idiomas = CsvUtils.ObtenerValoresUnicosXml(rutaOrigen, "//Language");
        }

        Console.WriteLine("\nIDIOMAS DISPONIBLES EN EL " + origenFormato + ":");
        foreach (string idioma in idiomas) {
            Console.WriteLine("  - " + idioma);
        }
        Console.Write("\nEscriba el idioma que desea exportar: ");
        valorFiltroStr = Console.ReadLine().Trim();

    } else if (opcion == "5") {
        return;

    } else {
        Console.WriteLine("Opción no válida.");
        return;
    }

    // Ambos conversores comparten la misma firma.
    if (origenFormato == "CSV") {
        ConversorService.CsvAXmlManual(rutaOrigen, rutaDestino, filtroTipo, valorFiltroStr, valorFiltroAnio);
    } else {
        ConversorService.XmlACsvConXmlDocument(rutaOrigen, rutaDestino, filtroTipo, valorFiltroStr, valorFiltroAnio);
    }
}
