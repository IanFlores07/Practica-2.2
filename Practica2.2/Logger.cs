using System.Diagnostics;
using System.Text;

namespace ConversorNetflix;

public static class Logger {
    private const string Origen = "ConversorNetflix";   // Origen en el Visor de eventos
    private const string ArchivoLog = "registro.log";
    private static EventLog evento = new EventLog("Application");
    private static string idTransaccion = Guid.NewGuid().ToString();   // Agrupa los logs de esta ejecución

    // Registra el origen de eventos en Windows si no existe.
    public static void Inicializar() {
        try {
            // Un origen solo se registra una vez y requiere permisos.
            if (!EventLog.SourceExists(Origen)) {
                EventLog.CreateEventSource(Origen, "Application");
            }
            evento.Source = Origen;
        } catch (Exception) {
            // Sin permisos para el Visor de eventos: se usa solo el archivo de texto.
        }
    }

    // Compone la línea de log y la escribe en consola, archivo y EventLog.
    public static void EscribirLog(string mensaje, EventLogEntryType tipo, int id) {
        // Formato: fecha | origen | servicio | nivel | usuario | IP | proceso | evento | transacción | mensaje
        string linea =
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
            " | Origen=Local" +
            " | Servicio=" + Origen +
            " | Nivel=" + tipo +
            " | Usuario=" + Environment.UserName +
            " | IP=local" +
            " | Proceso=" + Environment.ProcessId +
            " | CodigoEvento=" + id +
            " | Transaccion=" + idTransaccion +
            " | Mensaje=" + mensaje;

        Console.WriteLine(linea);

        try {
            // UTF8 con BOM para que Windows muestre bien los acentos.
            File.AppendAllText(ArchivoLog, linea + Environment.NewLine, new UTF8Encoding(true));
        } catch (Exception ex) {
            Console.WriteLine("AVISO: No se pudo escribir en el archivo de log: " + ex.Message);
        }

        try {
            evento.WriteEntry(linea, tipo, id);
        } catch {
            // El log ya está guardado en el archivo de texto.
        }
    }

    // Registra el inicio de una exportación con sus rutas.
    public static void LogInicio(string origenFormato, string destinoFormato, string rutaOrigen, string rutaDestino) {
        EscribirLog("Inicio de exportación de " + origenFormato + " a " + destinoFormato +
            ". Origen: " + rutaOrigen + ". Destino: " + rutaDestino, EventLogEntryType.Information, 1000);
    }

    // Registra una película leída y sus datos.
    public static void LogElemento(string datos) {
        EscribirLog("Lectura de película: " + datos, EventLogEntryType.Information, 1001);
    }

    // Registra el fin de la exportación y la ruta del archivo creado.
    public static void LogFin(string rutaDestino) {
        EscribirLog("Fin de exportación. Archivo nuevo en: " + rutaDestino, EventLogEntryType.Information, 1002);
    }

    // Registra un error: la operación no se ha completado.
    public static void LogError(string mensaje) {
        EscribirLog("ERROR: " + mensaje, EventLogEntryType.Error, 3000);
    }
}
