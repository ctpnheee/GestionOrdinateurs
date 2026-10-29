namespace GestionOrdinateurs.API;

public class StockService
{
    public int StockSSD = 10;
    private readonly object verrou = new object();


    // Sans verrou, le résultat n'est pas stable, car plusieurs tâches peuvent
    // modifier le stock en même temps. Avec le verrou, une seule tâche modifie
    // le stock à la fois, donc le résultat final est toujours 0.
    public async void ConsommerSSD()
    {
        Task.Delay(10).Wait();

        // Le verrou permet à une seule tâche de modifier le stock à la fois.
        lock (verrou)
        {
            StockSSD--;
        }
    }
}
