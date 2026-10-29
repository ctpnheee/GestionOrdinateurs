namespace GestionOrdinateurs.API;

public class JournalService
{
    private readonly object verrou = new object();
    private readonly string dossierLogs;

    public JournalService()
    {
        dossierLogs = "Logs";
    }

    public JournalService(string dossierLogs)
    {
        this.dossierLogs = dossierLogs;
    }

    private void EcrireDansFichier(string chemin, string message)
    {
        string? dossier = Path.GetDirectoryName(chemin);

        if (dossier != null)
            Directory.CreateDirectory(dossier);

        // Le verrou évite que plusieurs tâches écrivent en même temps dans le fichier.
        lock (verrou)
        {
            using var writer = new StreamWriter(
                chemin,
                append: true);

            writer.WriteLine($"{DateTime.Now:dd/MM/yyyy} - {message}");
        }
    }
    public void Ecrire(string message)
    {
        EcrireDansFichier(
            Path.Combine(dossierLogs, "Reparations", "reparations.log"),
            message);
    }

    public void EcrireOrdinateur(string message)
    {
        EcrireDansFichier(
            Path.Combine(dossierLogs, "Ordinateurs", "ordinateurs.log"),
            message);
    }
}
